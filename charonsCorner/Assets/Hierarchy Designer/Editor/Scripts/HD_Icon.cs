#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HierarchyDesigner
{
    internal static class HD_Icon
    {
        #region Properties
        [Serializable]
        private struct IconOverrideRecord
        {
            public string globalId;
            public string source;
            public string value;
        }

        [Serializable]
        private class IconOverrideList { public List<IconOverrideRecord> items = new(); }

        private static readonly Dictionary<string, IconOverrideRecord> map = new();
        private static readonly Dictionary<string, Texture2D> resolvedCache = new();
        private static readonly Dictionary<int, string> globalObjectIdCache = new();

        private const string FileName = "HierarchyDesigner_SavedData_IconOverrides.json";
        #endregion

        #region Initialization
        public static void Initialize()
        {
            LoadSettings();
        }
        #endregion

        #region Methods
        public static bool TryGetTexture(string globalId, out Texture2D tex)
        {
            tex = null;
            if (!map.TryGetValue(globalId, out var rec)) return false;
            if (resolvedCache.TryGetValue(globalId, out tex)) return tex != null;

            tex = Resolve(rec);
            resolvedCache[globalId] = tex;
            return tex != null;
        }

        public static bool TryGetTexture(GameObject gameObject, int instanceID, out Texture2D tex)
        {
            tex = null;

            if (!HasOverrides || gameObject == null)
            {
                return false;
            }

            string globalId = GetGlobalObjectId(gameObject, instanceID);
            return TryGetTexture(globalId, out tex);
        }

        private static string GetGlobalObjectId(GameObject gameObject, int instanceID)
        {
            if (globalObjectIdCache.TryGetValue(instanceID, out string globalId))
            {
                return globalId;
            }

            globalId = GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();
            globalObjectIdCache[instanceID] = globalId;

            return globalId;
        }

        public static void SetBuiltin(string globalId, Texture2D builtinTex)
        {
            if (builtinTex == null) return;
            map[globalId] = new IconOverrideRecord { globalId = globalId, source = "builtin", value = builtinTex.name };
            resolvedCache[globalId] = builtinTex;
            SaveSettings();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void SetAsset(string globalId, string textureGuid)
        {
            if (string.IsNullOrEmpty(textureGuid)) return;
            map[globalId] = new IconOverrideRecord { globalId = globalId, source = "asset", value = textureGuid };
            resolvedCache.Remove(globalId);
            SaveSettings();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static bool Clear(string globalId)
        {
            bool removed = map.Remove(globalId);
            if (removed) { resolvedCache.Remove(globalId); SaveSettings(); EditorApplication.RepaintHierarchyWindow(); }
            return removed;
        }

        public static bool Has(string globalId) => map.ContainsKey(globalId);

        public static bool HasOverrides => map.Count > 0;

        public static void ClearGlobalObjectIdCache()
        {
            globalObjectIdCache.Clear();
        }

        private static Texture2D Resolve(IconOverrideRecord rec)
        {
            if (rec.source == "builtin")
            {
                GUIContent c = EditorGUIUtility.IconContent(rec.value);
                Texture2D t1 = c != null ? c.image as Texture2D : null;
                if (t1 != null) return t1;

                Texture2D[] all = Resources.FindObjectsOfTypeAll<Texture2D>();
                for (int i = 0; i < all.Length; i++)
                {
                    Texture2D t2 = all[i];
                    if (t2 != null && t2.name == rec.value) return t2;
                }
                return null;
            }
            if (rec.source == "asset")
            {
                string path = AssetDatabase.GUIDToAssetPath(rec.value);
                return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return null;
        }
        #endregion

        #region Save and Load Settings
        private static void SaveSettings()
        {
            string path = HD_File.GetSavedDataFilePath(FileName);
            IconOverrideList list = new() { items = new List<IconOverrideRecord>(map.Values) };
            File.WriteAllText(path, JsonUtility.ToJson(list, true));
            AssetDatabase.Refresh();
        }

        private static void LoadSettings()
        {
            map.Clear();
            resolvedCache.Clear();
            globalObjectIdCache.Clear();

            string path = HD_File.GetSavedDataFilePath(FileName);
            if (!File.Exists(path)) return;

            IconOverrideList list = JsonUtility.FromJson<IconOverrideList>(File.ReadAllText(path));
            if (list?.items == null) return;

            foreach (IconOverrideRecord rec in list.items)
            {
                map[rec.globalId] = rec;
            }
        }
        #endregion
    }
}
#endif