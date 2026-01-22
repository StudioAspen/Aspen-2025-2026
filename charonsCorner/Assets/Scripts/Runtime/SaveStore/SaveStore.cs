using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace CharonsCorner.Runtime
{
    public class SaveStore
    {
        public string FileName { get; private set; }
        /// <summary>
        /// Whether to obfuscate the save data. Prevents easy modification of the save file.
        /// </summary>
        public bool Obfuscate { get; private set; }
        public string FilePath => Path.Combine(Application.persistentDataPath, $"{FileName}");

        private Dictionary<string, string> _data = new();

        /// <summary>
        /// Constructs a new SaveStore with the specified file name and obfuscation setting.
        /// </summary>
        public SaveStore(string fileName, bool obfuscate)
        {
            FileName = fileName;
            Obfuscate = obfuscate;

            _data = Load();
        }

        /// <summary>
        /// Loads the save data from the save file.
        /// </summary>
        public Dictionary<string, string> Load()
        {
            if (!File.Exists(FilePath))
                return InitNewSave();

            string json = File.ReadAllText(FilePath);

            if(Obfuscate && !TryDecrypt(ref json))
                return InitNewSave();

            if (!TryParseJson(json, out Dictionary<string, string> data))
                return InitNewSave();

            return data;
        }

        /// <summary>
        /// Saves the current data to the save file.
        /// </summary>
        public void Save()
        {
            Save(_data);
        }

        /// <summary>
        /// Saves the provided data to the save file.
        /// </summary>
        private void Save(Dictionary<string, string> data)
        {
            string json = JsonUtility.ToJson(new DictionaryWrapper<string, string>(data), true);

            if (Obfuscate)
                json = EncryptionUtilities.Encrypt(json);

            File.WriteAllText(FilePath, json);

            this._data = data;
        }

        /// <summary>
        /// Initializes a new empty save data instance and saves it to the store.
        /// </summary>
        private Dictionary<string, string> InitNewSave()
        {
            Dictionary<string, string> newData = new();
            Save(newData);
            return newData;
        }

        private bool TryDecrypt(ref string json)
        {
            try
            {
                json = EncryptionUtilities.Decrypt(json);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Decryption failed. Resetting save file '{FilePath}' due to potential corruption.\nError: {e.Message}");
                return false;
            }
        }

        private bool TryParseJson(string json, out Dictionary<string, string> data)
        {
            try
            {
                data = JsonUtility.FromJson<DictionaryWrapper<string, string>>(json).Dictionary;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to parse save data from '{FilePath}'. Resetting save file due to potential corruption.\nError: {e.Message}");
                data = null;
                return false;
            }
        }

        private string GetValue(string key, string defaultValue = null)
        {
            if (_data.TryGetValue(key, out string value))
                return value;
            return defaultValue;
        }

        private void SetValue(string key, string value)
        {
            if (_data.ContainsKey(key))
                _data[key] = value;
            else
                _data.Add(key, value);

            Save(); // Automatically save after setting a value
        }

        public bool HasKey(string key) => _data.ContainsKey(key);
        public void DeleteKey(string key)
        {
            if (_data.ContainsKey(key))
            {
                _data.Remove(key);
                Save(); // Automatically save after deleting a key
            }
        }
        public void DeleteAll()
        {
            _data.Clear();
            Save(); // Automatically save after clearing all values
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            string value = GetValue(key, defaultValue.ToString());
            return float.TryParse(value, out float result) ? result : defaultValue;
        }
        public void SetFloat(string key, float value) => SetValue(key, value.ToString());

        public int GetInt(string key, int defaultValue = 0)
        {
            string value = GetValue(key, defaultValue.ToString());
            return int.TryParse(value, out int result) ? result : defaultValue;
        }
        public void SetInt(string key, int value) => SetValue(key, value.ToString());

        public string GetString(string key, string defaultValue = "") => GetValue(key, defaultValue);
        public void SetString(string key, string value) => SetValue(key, value == null ? string.Empty : value);

        public bool GetBool(string key, bool defaultValue = false) => GetInt(key, defaultValue ? 1 : 0) == 1;
        public void SetBool(string key, bool value) => SetInt(key, value ? 1 : 0);

        public Vector3 GetVector3(string key, Vector3 defaultValue = default)
        {
            string value = GetValue(key);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            string[] parts = value.Trim('(', ')').Split(',');
            if (parts.Length != 3)
                return defaultValue;

            if (float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y) &&
                float.TryParse(parts[2], out float z))
                return new Vector3(x, y, z);

            return defaultValue;
        }
        public void SetVector3(string key, Vector3 value) => SetValue(key, $"({value.x},{value.y},{value.z})");

        public List<T> GetList<T>(string key, List<T> defaultValue = null)
        {
            string value = GetValue(key);
            if (string.IsNullOrEmpty(value))
                return defaultValue ?? new List<T>();

            try
            {
                return JsonUtility.FromJson<ListWrapper<T>>(value).List;
            }
            catch
            {
                return defaultValue ?? new List<T>();
            }
        }
        public void SetList<T>(string key, List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                DeleteKey(key);
                return;
            }

            string json = JsonUtility.ToJson(new ListWrapper<T>(list));
            SetValue(key, json);
        }

        public Dictionary<K, V> GetDictionary<K, V>(string key, Dictionary<K, V> defaultValue = null)
        {
            string value = GetValue(key);
            if (string.IsNullOrEmpty(value))
                return defaultValue ?? new Dictionary<K, V>();

            try
            {
                return JsonUtility.FromJson<DictionaryWrapper<K, V>>(value).Dictionary;
            }
            catch
            {
                return defaultValue ?? new Dictionary<K, V>();
            }
        }
        public void SetDictionary<K, V>(string key, Dictionary<K, V> dict)
        {
            if (dict == null || dict.Count == 0)
            {
                DeleteKey(key);
                return;
            }

            string json = JsonUtility.ToJson(new DictionaryWrapper<K, V>(new SerializedDictionary<K, V>(dict)));
            SetValue(key, json);
        }
    }

    [System.Serializable]
    public class ListWrapper<T>
    {
        public List<T> List;

        public ListWrapper(List<T> list)
        {
            List = list;
        }
    }

    [System.Serializable]
    public class DictionaryWrapper<K, V>
    {
        public SerializedDictionary<K, V> Dictionary;

        public DictionaryWrapper(Dictionary<K, V> dict)
        {
            Dictionary = new SerializedDictionary<K, V>(dict);
        }
    }
}
