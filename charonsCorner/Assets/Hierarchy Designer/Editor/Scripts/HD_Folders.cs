#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HierarchyDesigner
{
    internal static class HD_Folders
    {
        #region Properties
        [System.Serializable]
        public class HD_FolderData
        {
            public string Name = "Folder";
            public Color TextColor = HD_Settings.FolderDefaultTextColor;
            public int FontSize = HD_Settings.FolderDefaultFontSize;
            public FontStyle FontStyle = HD_Settings.FolderDefaultFontStyle;
            public Color ImageColor = HD_Settings.FolderDefaultImageColor;
            public FolderImageType ImageType = HD_Settings.FolderDefaultImageType;
        }
        public enum FolderImageType
        { 
            Default, 
            DefaultOutline,
            ClassicI,
            ClassicII,
            ClassicOutline,
            ModernI,
            ModernII,
            ModernIII,
            ModernOutline,
            NeoI,
            NeoII,
            NeoOutline,
        }
        private static Dictionary<string, HD_FolderData> folders = new();
        #endregion

        #region Initialization
        public static void Initialize()
        {
            LoadSettings();
            LoadHierarchyDesignerManagerGameObjectCaches();
        }

        private static void LoadHierarchyDesignerManagerGameObjectCaches()
        {
            Dictionary<int, (Color textColor, int fontSize, FontStyle fontStyle, Color folderColor, FolderImageType folderImageType)> folderCache = new();
            foreach (KeyValuePair<string, HD_FolderData> folder in folders)
            {
                int instanceID = folder.Key.GetHashCode();
                folderCache[instanceID] = (folder.Value.TextColor, folder.Value.FontSize, folder.Value.FontStyle, folder.Value.ImageColor, folder.Value.ImageType);
            }
            HD_Manager.FolderCache = folderCache;
        }
        #endregion

        #region Methods
        public static void SetFolderData(string folderName, Color textColor, int fontSize, FontStyle fontStyle, Color imageColor, FolderImageType imageType)
        {
            if (folders.TryGetValue(folderName, out HD_FolderData folderData))
            {
                folderData.TextColor = textColor;
                folderData.FontSize = fontSize;
                folderData.FontStyle = fontStyle;
                folderData.ImageColor = imageColor;
                folderData.ImageType = imageType;
            }
            else
            {
                folders[folderName] = new()
                {
                    Name = folderName,
                    TextColor = textColor,
                    FontSize = fontSize,
                    FontStyle = fontStyle,
                    ImageColor = imageColor,
                    ImageType = imageType
                };
            }
            SaveSettings();
            HD_Manager.ClearFolderCache();
        }

        public static void ApplyChangesToFolders(Dictionary<string, HD_FolderData> tempFolders, List<string> foldersOrder)
        {
            Dictionary<string, HD_FolderData> orderedFolders = new();
            foreach (string key in foldersOrder)
            {
                if (tempFolders.TryGetValue(key, out HD_FolderData folderData))
                {
                    orderedFolders[key] = folderData;
                }
            }
            folders = orderedFolders;
        }

        public static HD_FolderData GetFolderData(string folderName)
        {
            if (folders.TryGetValue(folderName, out HD_FolderData folderData))
            {
                return folderData;
            }
            return null;
        }

        public static Dictionary<string, HD_FolderData> GetAllFoldersData(bool updateData)
        {
            if (updateData) LoadSettings();
            return new(folders);
        }

        public static bool RemoveFolderData(string folderName)
        {
            if (folders.TryGetValue(folderName, out _))
            {
                folders.Remove(folderName);
                SaveSettings();
                HD_Manager.ClearFolderCache();
                return true;
            }
            return false;
        }

        public static Dictionary<string, List<string>> GetFolderImageTypesGrouped()
        {
            return new()
            {
                {
                    "Default", new()
                    {
                        "Default",
                        "Default Outline"
                    }
                },
                {
                    "Classic",
                    new()
                    {
                        "Classic I",
                        "Classic II",
                        "Classic Outline"
                    }
                },
                {
                    "Modern", new()
                    {
                        "Modern I",
                        "Modern II",
                        "Modern III",
                        "Modern Outline"
                    }
                },
                {
                    "Neo",
                    new()
                    {
                        "Neo I",
                        "Neo II",
                        "Neo Outline",
                    }
                }
            };
        }

        public static FolderImageType ParseFolderImageType(string displayName)
        {
            return displayName switch
            {
                "Default" => FolderImageType.Default,
                "Default Outline" => FolderImageType.DefaultOutline,
                "Classic I" => FolderImageType.ClassicI,
                "Classic II" => FolderImageType.ClassicII,
                "Classic Outline" => FolderImageType.ClassicOutline,
                "Modern I" => FolderImageType.ModernI,
                "Modern II" => FolderImageType.ModernII,
                "Modern III" => FolderImageType.ModernIII,
                "Modern Outline" => FolderImageType.ModernOutline,
                "Neo I" => FolderImageType.NeoI,
                "Neo II" => FolderImageType.NeoII,
                "Neo Outline" => FolderImageType.NeoOutline,
                _ => FolderImageType.Default,
            };
        }

        public static string GetFolderImageTypeDisplayName(FolderImageType imageType)
        {
            return imageType switch
            {
                FolderImageType.Default => "Default",
                FolderImageType.DefaultOutline => "Default Outline",
                FolderImageType.ClassicI => "Classic I",
                FolderImageType.ClassicII => "Classic II",
                FolderImageType.ClassicOutline => "Classic Outline",
                FolderImageType.ModernI => "Modern I",
                FolderImageType.ModernII => "Modern II",
                FolderImageType.ModernIII => "Modern III",
                FolderImageType.ModernOutline => "Modern Outline",
                FolderImageType.NeoI => "Neo I",
                FolderImageType.NeoII => "Neo II",
                FolderImageType.NeoOutline => "Neo Outline",
                _ => imageType.ToString(),
            };
        }
        #endregion

        #region Save and Load
        public static void SaveSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.FolderSettingsTextFileName);
            HD_Serializable<HD_FolderData> serializableList = new(new(folders.Values));
            string json = JsonUtility.ToJson(serializableList, true);
            File.WriteAllText(dataFilePath, json);
            AssetDatabase.Refresh();
        }

        public static void LoadSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.FolderSettingsTextFileName);
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                HD_Serializable<HD_FolderData> loadedFolders = JsonUtility.FromJson<HD_Serializable<HD_FolderData>>(json);
                folders.Clear();
                foreach (HD_FolderData folder in loadedFolders.items)
                {
                    folder.ImageType = HD_Validation.ParseEnum(folder.ImageType.ToString(), FolderImageType.Default);
                    folders[folder.Name] = folder;
                }
            }
            else
            {
                SetDefaultSettings();
            }
        }

        private static void SetDefaultSettings()
        {
            folders = new();
        }
        #endregion
    }
}
#endif