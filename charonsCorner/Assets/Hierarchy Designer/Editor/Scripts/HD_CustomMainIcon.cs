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
            public string PrefabAlternativeIconGuid;
            public string PrefabAlternativeBuiltinIconName;
            public bool PrefabAlternativeIsBuiltin;
            public string RecursiveAlternativeIconGuid;
            public string RecursiveAlternativeBuiltinIconName;
            public bool RecursiveAlternativeIsBuiltin;

            public HD_CustomMainIconData(string scriptName, string iconGuid, string builtinIconName, bool isBuiltin, string prefabAlternativeIconGuid = "", string prefabAlternativeBuiltinIconName = "", bool prefabAlternativeIsBuiltin = false, string recursiveAlternativeIconGuid = "", string recursiveAlternativeBuiltinIconName = "", bool recursiveAlternativeIsBuiltin = false)
            {
                ScriptName = scriptName;
                IconGuid = iconGuid;
                BuiltinIconName = builtinIconName;
                IsBuiltin = isBuiltin;
                PrefabAlternativeIconGuid = prefabAlternativeIconGuid;
                PrefabAlternativeBuiltinIconName = prefabAlternativeBuiltinIconName;
                PrefabAlternativeIsBuiltin = prefabAlternativeIsBuiltin;
                RecursiveAlternativeIconGuid = recursiveAlternativeIconGuid;
                RecursiveAlternativeBuiltinIconName = recursiveAlternativeBuiltinIconName;
                RecursiveAlternativeIsBuiltin = recursiveAlternativeIsBuiltin;
            }
        }

        private static Dictionary<string, HD_CustomMainIconData> customMainIcons = new();
        private static Dictionary<string, Texture2D> cachedIcons = new();
        private static Dictionary<string, Texture2D> cachedAlternativeIcons = new();
        private static Dictionary<string, Texture2D> cachedRecursiveIcons = new();
        #endregion

        #region Initialization
        public static void Initialize()
        {
            LoadSettings();
        }
        #endregion

        #region Methods
        public static void SetCustomMainIconData(string scriptName, string iconGuid, string builtinIconName, bool isBuiltin, string prefabAlternativeIconGuid, string prefabAlternativeBuiltinIconName, bool prefabAlternativeIsBuiltin, string recursiveAlternativeIconGuid, string recursiveAlternativeBuiltinIconName, bool recursiveAlternativeIsBuiltin)
        {
            customMainIcons[scriptName] = new HD_CustomMainIconData(scriptName, iconGuid, builtinIconName, isBuiltin, prefabAlternativeIconGuid, prefabAlternativeBuiltinIconName, prefabAlternativeIsBuiltin, recursiveAlternativeIconGuid, recursiveAlternativeBuiltinIconName, recursiveAlternativeIsBuiltin);
            cachedIcons.Remove(scriptName);
            cachedAlternativeIcons.Remove(scriptName);
            cachedRecursiveIcons.Remove(scriptName);
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
            cachedAlternativeIcons.Clear();
            cachedRecursiveIcons.Clear();
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
                cachedAlternativeIcons.Remove(scriptName);
                cachedRecursiveIcons.Remove(scriptName);
                SaveSettings();
                HD_Manager.ClearGameObjectDataCache();
                return true;
            }
            return false;
        }

        public static Texture2D GetIconForScript(string scriptName, bool isPrefabParent, bool hasChildren)
        {
            if (isPrefabParent)
            {
                if (cachedAlternativeIcons.TryGetValue(scriptName, out Texture2D cachedAltIcon))
                {
                    return cachedAltIcon;
                }
            }
            else if (hasChildren)
            {
                if (cachedRecursiveIcons.TryGetValue(scriptName, out Texture2D cachedRecIcon))
                {
                    return cachedRecIcon;
                }
            }
            else
            {
                if (cachedIcons.TryGetValue(scriptName, out Texture2D cachedIcon))
                {
                    return cachedIcon;
                }
            }

            if (customMainIcons.TryGetValue(scriptName, out HD_CustomMainIconData data))
            {
                Texture2D icon = null;
                bool isBuiltin;
                string builtinName;
                string guid;

                if (isPrefabParent)
                {
                    isBuiltin = data.PrefabAlternativeIsBuiltin;
                    builtinName = data.PrefabAlternativeBuiltinIconName;
                    guid = data.PrefabAlternativeIconGuid;
                }
                else if (hasChildren)
                {
                    isBuiltin = data.RecursiveAlternativeIsBuiltin;
                    builtinName = data.RecursiveAlternativeBuiltinIconName;
                    guid = data.RecursiveAlternativeIconGuid;
                }
                else
                {
                    isBuiltin = data.IsBuiltin;
                    builtinName = data.BuiltinIconName;
                    guid = data.IconGuid;
                }

                if (isBuiltin)
                {
                    if (!string.IsNullOrEmpty(builtinName))
                    {
                        GUIContent content = EditorGUIUtility.IconContent(builtinName);
                        icon = content?.image as Texture2D;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(guid))
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (!string.IsNullOrEmpty(path))
                        {
                            icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                        }
                    }
                }

                if (icon == null)
                {
                    if (isPrefabParent || hasChildren) return GetIconForScript(scriptName, false, false);
                }

                if (isPrefabParent)
                {
                    cachedAlternativeIcons[scriptName] = icon;
                }
                else if (hasChildren)
                {
                    cachedRecursiveIcons[scriptName] = icon;
                }
                else
                {
                    cachedIcons[scriptName] = icon;
                }
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
                cachedAlternativeIcons.Clear();
                cachedRecursiveIcons.Clear();
                if (loadedIcons != null && loadedIcons.items != null)
                {
                    foreach (HD_CustomMainIconData data in loadedIcons.items)
                    {
                        customMainIcons[data.ScriptName] = data;
                    }
                }
            }
        }
        #endregion
    }
}
#endif
