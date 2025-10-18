using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharonsCorner.Editor
{
    /// <summary>
    /// This Unity Editor class is responsible for switching to the Bootstrap scene when you press the Play button in the Editor.
    /// Bootstrapping is important since it serves as our Single Entry Point to guarantee that core systems are initialized.
    /// When exiting out of Play mode, this script switches the current scene back to the one you started with.
    /// This script forces you to save your current scene, so playing from an unsaved scene is no longer possible.
    /// </summary>
    [InitializeOnLoad]
    public static class BootstrapOnPlayMode
    {
        /// <summary>
        /// The path of the "Bootstrap" scene. This scene houses all core systems that are required for the game.
        /// </summary>
        public static readonly string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        /// <summary>
        /// The path of the "Title" scene. This is a fallback scene in case you press play from the Bootstrap scene, bringing you to the Title.
        /// </summary>
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
                EditorUtilities.MarkAllScenesAddressable();
                EditorPrefs.SetString("Bootstrap_OriginalScene", currentScenePath);

                EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            }
            else if(newState == PlayModeStateChange.EnteredEditMode)
            {
                // Go back to the original scene after exiting play mode
                string originalScenePath = EditorPrefs.GetString("Bootstrap_OriginalScene", TitleScenePath);
                EditorUtilities.UnmarkAllScenesAddressable();
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }
        }
    }
}
