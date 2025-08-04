using System;
using System.IO;
using UnityEngine;

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

        public SaveData Data { get; private set; }

        /// <summary>
        /// Constructs a new SaveStore with the specified file name and obfuscation setting.
        /// </summary>
        public SaveStore(string fileName, bool obfuscate)
        {
            FileName = fileName;
            Obfuscate = obfuscate;

            Data = Load();
        }

        /// <summary>
        /// Loads the save data from the save file.
        /// </summary>
        public SaveData Load()
        {
            if (!File.Exists(FilePath))
                return InitNewSave();

            string json = File.ReadAllText(FilePath);

            if(Obfuscate && !TryDecrypt(ref json))
                return InitNewSave();

            if (!TryParseJson(json, out SaveData data))
                return InitNewSave();

            data.SetSaveStore(this);
            return data;
        }

        /// <summary>
        /// Saves the current data to the save file.
        /// </summary>
        public void Save()
        {
            Save(Data);
        }

        /// <summary>
        /// Saves the provided data to the save file.
        /// </summary>
        private void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);

            if (Obfuscate)
                json = EncryptionUtilities.Encrypt(json);

            File.WriteAllText(FilePath, json);

            Data = data;
        }

        /// <summary>
        /// Initializes a new empty save data instance and saves it to the store.
        /// </summary>
        private SaveData InitNewSave()
        {
            SaveData newData = new SaveData();
            Save(newData);
            newData.SetSaveStore(this);
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

        private bool TryParseJson(string json, out SaveData data)
        {
            try
            {
                data = JsonUtility.FromJson<SaveData>(json);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to parse save data from '{FilePath}'. Resetting save file due to potential corruption.\nError: {e.Message}");
                data = null;
                return false;
            }
        }
    }
}
