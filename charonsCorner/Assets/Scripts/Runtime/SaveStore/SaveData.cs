using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class SaveData
    {
        private SaveStore saveStore;

        [SerializeField] private SerializedDictionary<string, string> values = new();

        public SaveData()
        {
            values = new();
        }

        public void SetSaveStore(SaveStore store) => saveStore = store;

        private string GetValue(string key, string defaultValue = null)
        {
            if (values.TryGetValue(key, out string value))
                return value;
            return defaultValue;
        }

        private void SetValue(string key, string value)
        {
            if (values.ContainsKey(key))
                values[key] = value;
            else
                values.Add(key, value);

            saveStore.Save(); // Automatically save after setting a value
        }

        public bool HasKey(string key) => values.ContainsKey(key);
        public void DeleteKey(string key)
        {
            if (values.ContainsKey(key))
                values.Remove(key);
        }
        public void DeleteAll() => values.Clear();

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
    }
}
