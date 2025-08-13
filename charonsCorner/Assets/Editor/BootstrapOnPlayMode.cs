using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharonsCorner.Editor
{
    [InitializeOnLoad]
    public static class BootstrapOnPlayMode
    {
        public static readonly string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        public static readonly string TitleScenePath = "Assets/Scenes/Title.unity";

        static BootstrapOnPlayMode()
        {
            EditorApplication.playModeStateChanged += EditorApplication_PlayModeStateChanged;
        }

        private static void EditorApplication_PlayModeStateChanged(PlayModeStateChange newState)
        {
            if(newState == PlayModeStateChange.ExitingEditMode)
            {
                // Prompt user to save unsaved changes
                bool continuePlayMode = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                if (!continuePlayMode)
                {
                    EditorApplication.isPlaying = false;
                    return;
                }

                string currentScenePath = SceneManager.GetActiveScene().path;
                if(currentScenePath == BootstrapScenePath)
                    currentScenePath = TitleScenePath;
                EditorUtilities.MarkSceneAddressable(currentScenePath);
                EditorPrefs.SetString("Bootstrap_OriginalScene", currentScenePath);

                EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            }
            else if(newState == PlayModeStateChange.EnteredEditMode)
            {
                // Go back to the original scene after exiting play mode
                string originalScenePath = EditorPrefs.GetString("Bootstrap_OriginalScene", TitleScenePath);
                EditorUtilities.UnmarkSceneAddressable(originalScenePath);
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }
        }
    }
}
