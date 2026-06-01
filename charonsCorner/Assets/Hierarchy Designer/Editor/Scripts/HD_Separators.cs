#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HierarchyDesigner
{
    internal static class HD_Separators
    {
        #region Properties
        [System.Serializable]
        public class HD_SeparatorData
        {
            public string Name = "Separator";
            public Color TextColor = HD_Settings.SeparatorDefaultTextColor;
            public bool IsGradientBackground = HD_Settings.SeparatorDefaultIsGradientBackground;
            public Color BackgroundColor = HD_Settings.SeparatorDefaultBackgroundColor;
            public Gradient BackgroundGradient;
            public int FontSize = HD_Settings.SeparatorDefaultFontSize;
            public FontStyle FontStyle = HD_Settings.SeparatorDefaultFontStyle;
            public TextAnchor TextAnchor = HD_Settings.SeparatorDefaultTextAnchor;
            public SeparatorImageType ImageType = HD_Settings.SeparatorDefaultImageType;

            public HD_SeparatorData()
            {
                BackgroundGradient = HD_Color.CopyGradient(HD_Settings.SeparatorDefaultBackgroundGradient);
            }
        }
        public enum SeparatorImageType
        {
            Default,
            DefaultFadedTop,
            DefaultFadedLeft,
            DefaultFadedRight,
            DefaultFadedBottom,
            DefaultFadedSideways,
            ClassicI,
            ClassicII,
            ModernI,
            ModernII,
            ModernIII,
            NeoI,
            NeoII,
            NextGenI,
            NextGenII,
            PostmodernI,
            PostmodernII,
        }
        private static Dictionary<string, HD_SeparatorData> separators = new();
        #endregion

        #region Initialization
        public static void Initialize()
        {
            LoadSettings();
            LoadHierarchyDesignerManagerGameObjectCaches();
        }

        private static void LoadHierarchyDesignerManagerGameObjectCaches()
        {
            Dictionary<int, (Color textColor, bool isGradientBackground, Color backgroundColor, Gradient backgroundGradient, int fontSize, FontStyle fontStyle, TextAnchor textAnchor, SeparatorImageType separatorImageType)> separatorCache = new();
            foreach (KeyValuePair<string, HD_SeparatorData> separator in separators)
            {
                int instanceID = separator.Key.GetHashCode();
                separatorCache[instanceID] = (separator.Value.TextColor, separator.Value.IsGradientBackground, separator.Value.BackgroundColor, separator.Value.BackgroundGradient, separator.Value.FontSize, separator.Value.FontStyle, separator.Value.TextAnchor, separator.Value.ImageType);
            }
            HD_Manager.SeparatorCache = separatorCache;
        }
        #endregion

        #region Methods
        public static void SetSeparatorData(string separatorName, Color textColor, bool isGradientBackground, Color backgroundColor, Gradient backgroundGradient, int fontSize, FontStyle fontStyle, TextAnchor textAnchor, SeparatorImageType imageType)
        {
            separatorName = HD_Operations.StripPrefix(separatorName);
            if (separators.TryGetValue(separatorName, out HD_SeparatorData separatorData))
            {
                separatorData.TextColor = textColor;
                separatorData.IsGradientBackground = isGradientBackground;
                separatorData.BackgroundColor = backgroundColor;
                separatorData.BackgroundGradient = HD_Color.CopyGradient(backgroundGradient);
                separatorData.FontSize = fontSize;
                separatorData.FontStyle = fontStyle;
                separatorData.TextAnchor = textAnchor;
                separatorData.ImageType = imageType;
            }
            else
            {
                separators[separatorName] = new()
                {
                    Name = separatorName,
                    TextColor = textColor,
                    IsGradientBackground = isGradientBackground,
                    BackgroundColor = backgroundColor,
                    BackgroundGradient = HD_Color.CopyGradient(backgroundGradient),
                    FontSize = fontSize,
                    FontStyle = fontStyle,
                    TextAnchor = textAnchor,
                    ImageType = imageType
                };
            }
            SaveSettings();
            HD_Manager.ClearSeparatorCache();
        }

        public static void ApplyChangesToSeparators(Dictionary<string, HD_SeparatorData> tempSeparators, List<string> separatorsOrder)
        {
            Dictionary<string, HD_SeparatorData> orderedSeparators = new();
            foreach (string key in separatorsOrder)
            {
                if (tempSeparators.TryGetValue(key, out HD_SeparatorData separatorData))
                {
                    orderedSeparators[key] = separatorData;
                }
            }
            separators = orderedSeparators;
        }

        public static HD_SeparatorData GetSeparatorData(string separatorName)
        {
            separatorName = HD_Operations.StripPrefix(separatorName);
            if (separators.TryGetValue(separatorName, out HD_SeparatorData separatorData))
            {
                return separatorData;
            }
            return null;
        }

        public static Dictionary<string, HD_SeparatorData> GetAllSeparatorsData(bool updateData)
        {
            if (updateData) LoadSettings();
            return new(separators);
        }

        public static bool RemoveSeparatorData(string separatorName)
        {
            separatorName = HD_Operations.StripPrefix(separatorName);
            if (separators.TryGetValue(separatorName, out _))
            {
                separators.Remove(separatorName);
                SaveSettings();
                HD_Manager.ClearSeparatorCache();
                return true;
            }
            return false;
        }

        public static Dictionary<string, List<string>> GetSeparatorImageTypesGrouped()
        {
            return new()
            {
                {
                    "Default", new()
                    {
                        "Default",
                        "Default Faded Top",
                        "Default Faded Left",
                        "Default Faded Right",
                        "Default Faded Bottom",
                        "Default Faded Sideways"
                    }
                },
                {
                    "Classic", new()
                    {
                        "Classic I",
                        "Classic II",
                    }
                },
                {
                    "Modern", new()
                    {
                        "Modern I",
                        "Modern II",
                        "Modern III"
                    }
                },
                {
                    "Neo", new()
                    {
                        "Neo I",
                        "Neo II"
                    } 
                },
                {
                    "Next-Gen", new()
                    {
                        "Next-Gen I",
                        "Next-Gen II"
                    }
                },
                {
                    "Postmodern",
                    new()
                    {
                        "Postmodern I",
                        "Postmodern II",
                    }
                }
            };
        }

        public static SeparatorImageType ParseSeparatorImageType(string displayName)
        {
            return displayName switch
            {
                "Default" => SeparatorImageType.Default,
                "Default Faded Top" => SeparatorImageType.DefaultFadedTop,
                "Default Faded Left" => SeparatorImageType.DefaultFadedLeft,
                "Default Faded Right" => SeparatorImageType.DefaultFadedRight,
                "Default Faded Bottom" => SeparatorImageType.DefaultFadedBottom,
                "Default Faded Sideways" => SeparatorImageType.DefaultFadedSideways,
                "Classic I" => SeparatorImageType.ClassicI,
                "Classic II" => SeparatorImageType.ClassicII,
                "Modern I" => SeparatorImageType.ModernI,
                "Modern II" => SeparatorImageType.ModernII,
                "Modern III" => SeparatorImageType.ModernIII,
                "Neo I" => SeparatorImageType.NeoI,
                "Neo II" => SeparatorImageType.NeoII,
                "Next-Gen I" => SeparatorImageType.NextGenI,
                "Next-Gen II" => SeparatorImageType.NextGenII,
                "Postmodern I" => SeparatorImageType.PostmodernI,
                "Postmodern II" => SeparatorImageType.PostmodernII,
                _ => SeparatorImageType.Default,
            };
        }

        public static string GetSeparatorImageTypeDisplayName(SeparatorImageType imageType)
        {
            return imageType switch
            {
                SeparatorImageType.Default => "Default",
                SeparatorImageType.DefaultFadedTop => "Default Faded Top",
                SeparatorImageType.DefaultFadedLeft => "Default Faded Left",
                SeparatorImageType.DefaultFadedRight => "Default Faded Right",
                SeparatorImageType.DefaultFadedBottom => "Default Faded Bottom",
                SeparatorImageType.DefaultFadedSideways => "Default Faded Sideways",
                SeparatorImageType.ClassicI => "Classic I",
                SeparatorImageType.ClassicII => "Classic II",
                SeparatorImageType.ModernI => "Modern I",
                SeparatorImageType.ModernII => "Modern II",
                SeparatorImageType.ModernIII => "Modern III",
                SeparatorImageType.NeoI => "Neo I",
                SeparatorImageType.NeoII => "Neo II",
                SeparatorImageType.NextGenI => "Next-Gen I",
                SeparatorImageType.NextGenII => "Next-Gen II",
                SeparatorImageType.PostmodernI => "Postmodern I",
                SeparatorImageType.PostmodernII => "Postmodern II",
                _ => imageType.ToString(),
            };
        }
        #endregion

        #region Save and Load
        public static void SaveSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.SeparatorSettingsTextFileName);
            HD_Serializable<HD_SeparatorData> serializableList = new(new(separators.Values));
            string json = JsonUtility.ToJson(serializableList, true);
            File.WriteAllText(dataFilePath, json);
            AssetDatabase.Refresh();
        }

        public static void LoadSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.SeparatorSettingsTextFileName);
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                HD_Serializable<HD_SeparatorData> loadedSeparators = JsonUtility.FromJson<HD_Serializable<HD_SeparatorData>>(json);
                separators.Clear();
                foreach (HD_SeparatorData separator in loadedSeparators.items)
                {
                    separator.ImageType = HD_Validation.ParseEnum(separator.ImageType.ToString(), SeparatorImageType.Default);
                    separator.BackgroundGradient = HD_Color.CopyGradient(separator.BackgroundGradient);
                    separators[separator.Name] = separator;
                }
            }
            else
            {
                SetDefaultSettings();
            }
        }

        private static void SetDefaultSettings()
        {
            separators = new();
        }
        #endregion
    }
}
#endif