#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HierarchyDesigner
{
    internal static class HD_CustomMainIcon
    {
        #region Properties
        [System.Serializable]
        public class HD_CustomMainIconData
        {
            public string ScriptName;
            public string IconGuid;
            public string BuiltinIconName;
            public bool IsBuiltin;

            public HD_CustomMainIconData(string scriptName, string iconGuid, string builtinIconName, bool isBuiltin)
            {
                ScriptName = scriptName;
                IconGuid = iconGuid;
                BuiltinIconName = builtinIconName;
                IsBuiltin = isBuiltin;
            }
        }

        private static Dictionary<string, HD_CustomMainIconData> customMainIcons = new();
        private static Dictionary<string, Texture2D> cachedIcons = new();
        #endregion

        #region Initialization
        public static void Initialize()
        {
            LoadSettings();
        }
        #endregion

        #region Methods
        public static void SetCustomMainIconData(string scriptName, string iconGuid, string builtinIconName, bool isBuiltin)
        {
            customMainIcons[scriptName] = new HD_CustomMainIconData(scriptName, iconGuid, builtinIconName, isBuiltin);
            cachedIcons.Remove(scriptName);
            SaveSettings();
            HD_Manager.ClearGameObjectDataCache();
        }

        public static void ApplyChangesToCustomMainIcons(Dictionary<string, HD_CustomMainIconData> tempCustomMainIcons, List<string> customMainIconsOrder)
        {
            Dictionary<string, HD_CustomMainIconData> orderedIcons = new();
            foreach (string key in customMainIconsOrder)
            {
                if (tempCustomMainIcons.TryGetValue(key, out HD_CustomMainIconData data))
                {
                    orderedIcons[key] = data;
                }
            }
            customMainIcons = orderedIcons;
            cachedIcons.Clear();
            SaveSettings();
            HD_Manager.ClearGameObjectDataCache();
        }

        public static HD_CustomMainIconData GetCustomMainIconData(string scriptName)
        {
            if (customMainIcons.TryGetValue(scriptName, out HD_CustomMainIconData data))
            {
                return data;
            }
            return null;
        }

        public static Dictionary<string, HD_CustomMainIconData> GetAllCustomMainIconsData(bool updateData)
        {
            if (updateData) LoadSettings();
            return new(customMainIcons);
        }

        public static bool RemoveCustomMainIconData(string scriptName)
        {
            if (customMainIcons.Remove(scriptName))
            {
                cachedIcons.Remove(scriptName);
                SaveSettings();
                HD_Manager.ClearGameObjectDataCache();
                return true;
            }
            return false;
        }

        public static Texture2D GetIconForScript(string scriptName)
        {
            if (cachedIcons.TryGetValue(scriptName, out Texture2D cachedIcon))
            {
                return cachedIcon;
            }

            if (customMainIcons.TryGetValue(scriptName, out HD_CustomMainIconData data))
            {
                Texture2D icon = null;
                if (data.IsBuiltin)
                {
                    GUIContent content = EditorGUIUtility.IconContent(data.BuiltinIconName);
                    icon = content?.image as Texture2D;
                }
                else
                {
                    string path = AssetDatabase.GUIDToAssetPath(data.IconGuid);
                    if (!string.IsNullOrEmpty(path))
                    {
                        icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    }
                }
                cachedIcons[scriptName] = icon;
                return icon;
            }
            return null;
        }
        #endregion

        #region Save and Load
        public static void SaveSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.CustomMainIconSettingsTextFileName);
            HD_Serializable<HD_CustomMainIconData> serializableList = new(new(customMainIcons.Values));
            string json = JsonUtility.ToJson(serializableList, true);
            File.WriteAllText(dataFilePath, json);
        }

        public static void LoadSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.CustomMainIconSettingsTextFileName);
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                HD_Serializable<HD_CustomMainIconData> loadedIcons = JsonUtility.FromJson<HD_Serializable<HD_CustomMainIconData>>(json);
                customMainIcons.Clear();
                cachedIcons.Clear();
                foreach (HD_CustomMainIconData data in loadedIcons.items)
                {
                    customMainIcons[data.ScriptName] = data;
                }
            }
        }
        #endregion
    }
}
#endif
