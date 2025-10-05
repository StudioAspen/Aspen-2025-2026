using UnityEditor.AddressableAssets;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CharonsCorner.Editor
{
    public static class EditorUtilities
    {
        public static List<string> GetAllScenePathsInAssets()
        {
            List<string> scenePaths = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                scenePaths.Add(path);
            }
            return scenePaths;
        }

        public static void MarkAllScenesAddressable()
        {
            foreach (var scenePath in GetAllScenePathsInAssets())
                MarkSceneAddressable(scenePath);
        }

        public static void UnmarkAllScenesAddressable()
        {
            foreach (var scenePath in GetAllScenePathsInAssets())
                UnmarkSceneAddressable(scenePath);
        }

        public static void MarkSceneAddressable(string scenePath)
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            var guid = AssetDatabase.AssetPathToGUID(scenePath);
            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                var group = settings.DefaultGroup;
                settings.CreateOrMoveEntry(guid, group);
            }
        }

        public static void UnmarkSceneAddressable(string scenePath)
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            var guid = AssetDatabase.AssetPathToGUID(scenePath);
            settings.RemoveAssetEntry(guid);
        }

        public static bool IsSceneInBuildSettings(string scenePath)
        {
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.path == scenePath)
                    return true;
            }
            return false;
        }
    }
}
