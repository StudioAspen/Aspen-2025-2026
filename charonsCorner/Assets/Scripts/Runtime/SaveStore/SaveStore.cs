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
        private const string fileExtension = ".dat";
        public string FilePath => Path.Combine(Application.persistentDataPath, $"{FileName}{fileExtension}");

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
            SaveData data;

            if (!File.Exists(FilePath))
                data = new SaveData();
            else
            {
                string json = File.ReadAllText(FilePath);
                if (Obfuscate)
                    json = EncryptionUtilities.Decrypt(json);

                data = JsonUtility.FromJson<SaveData>(json);
            }

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
        public void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);

            if(Obfuscate)
                json = EncryptionUtilities.Encrypt(json);

            File.WriteAllText(FilePath, json);

            Data = data;
        }

        /// <summary>
        /// Wipes the save file and resets the data.
        /// </summary>
        public void Clear()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
            Data = Load();
        }
    }
}
