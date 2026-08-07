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
            public Color IconColor;
            public Color PrefabAlternativeIconColor;
            public Color RecursiveAlternativeIconColor;
            
            public HD_CustomMainIconData() { }

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
                ColorUtility.TryParseHtmlString("#A2A6A2", out Color defaultIconColor);
                ColorUtility.TryParseHtmlString("#7FD6FC", out Color defaultPrefabColor);
                IconColor = defaultIconColor;
                PrefabAlternativeIconColor = defaultPrefabColor;
                RecursiveAlternativeIconColor = defaultIconColor;
            }

            public HD_CustomMainIconData(string scriptName, string iconGuid, string builtinIconName, bool isBuiltin, Color iconColor, string prefabAlternativeIconGuid, string prefabAlternativeBuiltinIconName, bool prefabAlternativeIsBuiltin, Color prefabAlternativeIconColor, string recursiveAlternativeIconGuid, string recursiveAlternativeBuiltinIconName, bool recursiveAlternativeIsBuiltin, Color recursiveAlternativeIconColor)
            {
                ScriptName = scriptName;
                IconGuid = iconGuid;
                BuiltinIconName = builtinIconName;
                IsBuiltin = isBuiltin;
                IconColor = iconColor;
                PrefabAlternativeIconGuid = prefabAlternativeIconGuid;
                PrefabAlternativeBuiltinIconName = prefabAlternativeBuiltinIconName;
                PrefabAlternativeIsBuiltin = prefabAlternativeIsBuiltin;
                PrefabAlternativeIconColor = prefabAlternativeIconColor;
                RecursiveAlternativeIconGuid = recursiveAlternativeIconGuid;
                RecursiveAlternativeBuiltinIconName = recursiveAlternativeBuiltinIconName;
                RecursiveAlternativeIsBuiltin = recursiveAlternativeIsBuiltin;
                RecursiveAlternativeIconColor = recursiveAlternativeIconColor;
            }
        }

        private static Dictionary<string, HD_CustomMainIconData> customMainIcons = new();
        private static readonly Dictionary<string, Texture2D> cachedIcons = new();
        private static readonly Dictionary<string, Texture2D> cachedAlternativeIcons = new();
        private static readonly Dictionary<string, Texture2D> cachedRecursiveIcons = new();
        #endregion

        #region Initialization
        public static void Initialize()
        {
            LoadSettings();
        }
        #endregion

        #region Methods
        public static void SetCustomMainIconData(string scriptName, string iconGuid, string builtinIconName, bool isBuiltin, Color iconColor, string prefabAlternativeIconGuid, string prefabAlternativeBuiltinIconName, bool prefabAlternativeIsBuiltin, Color prefabAlternativeIconColor, string recursiveAlternativeIconGuid, string recursiveAlternativeBuiltinIconName, bool recursiveAlternativeIsBuiltin, Color recursiveAlternativeIconColor)
        {
            customMainIcons[scriptName] = new HD_CustomMainIconData(scriptName, iconGuid, builtinIconName, isBuiltin, iconColor, prefabAlternativeIconGuid, prefabAlternativeBuiltinIconName, prefabAlternativeIsBuiltin, prefabAlternativeIconColor, recursiveAlternativeIconGuid, recursiveAlternativeBuiltinIconName, recursiveAlternativeIsBuiltin, recursiveAlternativeIconColor);
            
            if (cachedIcons.TryGetValue(scriptName, out Texture2D icon)) { DestroyTexture(icon); cachedIcons.Remove(scriptName); }
            if (cachedAlternativeIcons.TryGetValue(scriptName, out Texture2D altIcon)) { DestroyTexture(altIcon); cachedAlternativeIcons.Remove(scriptName); }
            if (cachedRecursiveIcons.TryGetValue(scriptName, out Texture2D recIcon)) { DestroyTexture(recIcon); cachedRecursiveIcons.Remove(scriptName); }
            
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
            ClearIconCache();
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
                if (cachedIcons.TryGetValue(scriptName, out Texture2D icon)) { DestroyTexture(icon); cachedIcons.Remove(scriptName); }
                if (cachedAlternativeIcons.TryGetValue(scriptName, out Texture2D altIcon)) { DestroyTexture(altIcon); cachedAlternativeIcons.Remove(scriptName); }
                if (cachedRecursiveIcons.TryGetValue(scriptName, out Texture2D recIcon)) { DestroyTexture(recIcon); cachedRecursiveIcons.Remove(scriptName); }
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
                if (cachedAlternativeIcons.TryGetValue(scriptName, out Texture2D cachedAltIcon) && cachedAltIcon != null)
                {
                    return cachedAltIcon;
                }
            }
            else if (hasChildren)
            {
                if (cachedRecursiveIcons.TryGetValue(scriptName, out Texture2D cachedRecIcon) && cachedRecIcon != null)
                {
                    return cachedRecIcon;
                }
            }
            else
            {
                if (cachedIcons.TryGetValue(scriptName, out Texture2D cachedIcon) && cachedIcon != null)
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
                Color tintColor;
 
                if (isPrefabParent)
                {
                    isBuiltin = data.PrefabAlternativeIsBuiltin;
                    builtinName = data.PrefabAlternativeBuiltinIconName;
                    guid = data.PrefabAlternativeIconGuid;
                    tintColor = data.PrefabAlternativeIconColor;
                }
                else if (hasChildren)
                {
                    isBuiltin = data.RecursiveAlternativeIsBuiltin;
                    builtinName = data.RecursiveAlternativeBuiltinIconName;
                    guid = data.RecursiveAlternativeIconGuid;
                    tintColor = data.RecursiveAlternativeIconColor;
                }
                else
                {
                    isBuiltin = data.IsBuiltin;
                    builtinName = data.BuiltinIconName;
                    guid = data.IconGuid;
                    tintColor = data.IconColor;
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

                if (icon != null && tintColor != Color.white)
                {
                    icon = CreateTintedTexture(icon, tintColor);
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
            AssetDatabase.Refresh();
        }

        private static Texture2D CreateTintedTexture(Texture2D original, Color tint)
        {
            RenderTexture rt = RenderTexture.GetTemporary(original.width, original.height, 0, RenderTextureFormat.ARGB32);
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear);
            Graphics.Blit(original, rt);

            Texture2D result = new Texture2D(original.width, original.height, TextureFormat.ARGB32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            result.ReadPixels(new Rect(0, 0, original.width, original.height), 0, 0);

            Color[] pixels = result.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r *= tint.r;
                pixels[i].g *= tint.g;
                pixels[i].b *= tint.b;
                pixels[i].a *= tint.a;
            }
            result.SetPixels(pixels);
            result.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            return result;
        }

        public static void LoadSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.CustomMainIconSettingsTextFileName);
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                HD_Serializable<HD_CustomMainIconData> loadedIcons = JsonUtility.FromJson<HD_Serializable<HD_CustomMainIconData>>(json);
                customMainIcons.Clear();
                ClearIconCache();
                if (loadedIcons != null && loadedIcons.items != null)
                {
                    foreach (HD_CustomMainIconData data in loadedIcons.items)
                    {
                        customMainIcons[data.ScriptName] = data;
                    }
                }
                HD_Manager.ClearGameObjectDataCache();
            }
            else
            {
                customMainIcons = new();
            }
        }

        private static void ClearIconCache()
        {
            foreach (Texture2D tex in cachedIcons.Values) { DestroyTexture(tex); }
            foreach (Texture2D tex in cachedAlternativeIcons.Values) { DestroyTexture(tex); }
            foreach (Texture2D tex in cachedRecursiveIcons.Values) { DestroyTexture(tex); }
            cachedIcons.Clear();
            cachedAlternativeIcons.Clear();
            cachedRecursiveIcons.Clear();
        }

        private static void DestroyTexture(Texture2D tex)
        {
            if (tex != null)
            {
                if (Application.isPlaying) { Object.Destroy(tex); }
                else { Object.DestroyImmediate(tex); }
            }
        }
        #endregion
    }
}
#endif
