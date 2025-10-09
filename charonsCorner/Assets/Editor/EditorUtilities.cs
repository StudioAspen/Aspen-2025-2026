using UnityEditor.AddressableAssets;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CharonsCorner.Editor
{
    /// <summary>
    /// This class houses Editor Utilities that are commonly used.
    /// </summary>
    public static class EditorUtilities
    {
        /// <summary>
        /// Goes through the AssetDatabase and tries to find any Scene files in the project.
        /// </summary>
        /// <returns>A list of all the scene paths.</returns>
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

        /// <summary>
        /// Marks all scenes found from EditorUtilities.GetAllScenePathsInAssets() as addressable.
        /// Used in BootstrapOnPlayMode to load into scenes that are not in the build scene list.
        /// </summary>
        public static void MarkAllScenesAddressable()
        {
            foreach (var scenePath in GetAllScenePathsInAssets())
                MarkSceneAddressable(scenePath);
        }

        /// <summary>
        /// Reverts all scenes found from EditorUtilities.GetAllScenePathsInAssets() back to non-addressable scenes.
        /// Used in BootstrapOnPlayMode to revert addressable scenes back to normal.
        /// </summary>
        public static void UnmarkAllScenesAddressable()
        {
            foreach (var scenePath in GetAllScenePathsInAssets())
                UnmarkSceneAddressable(scenePath);
        }

        /// <summary>
        /// Marks a specific scene addressable.
        /// </summary>
        /// <param name="scenePath">The path of the scene to mark addressable.</param>
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

        /// <summary>
        /// Reverts a specific scene back to a non-addressable one.
        /// </summary>
        /// <param name="scenePath">The path of the scene to unmark addressable.</param>
        public static void UnmarkSceneAddressable(string scenePath)
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            var guid = AssetDatabase.AssetPathToGUID(scenePath);
            settings.RemoveAssetEntry(guid);
        }

        /// <summary>
        /// Checks if a specific scene is in the build scene list.
        /// </summary>
        /// <param name="scenePath">The path of the scene to check.</param>
        /// <returns>True if the scene exists in the build scene list, false otherwise.</returns>
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
