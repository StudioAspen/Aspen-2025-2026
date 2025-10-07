using UnityEditor.AddressableAssets;
using UnityEditor;
using UnityEngine;

namespace CharonsCorner.Editor
{
    public static class EditorUtilities
    {
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
