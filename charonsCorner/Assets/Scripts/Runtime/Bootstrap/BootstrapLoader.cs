using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

namespace CharonsCorner.Runtime
{
    public class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private SceneReference titleScene;

#if UNITY_EDITOR
        [SerializeField, Required] private BootstrapConfigSO bootstrapConfig;
#endif

        private void Awake()
        {
#if UNITY_EDITOR
            bootstrapConfig.Initialize();
#endif
            DontDestroyOnLoad(gameObject);
        }


        private async void Start()
        {
#if UNITY_EDITOR
            string targetScenePath = EditorPrefs.GetString("Bootstrap_OriginalScene", null);
            SceneReference targetScene = string.IsNullOrEmpty(targetScenePath) ? titleScene : SceneReference.FromScenePath(targetScenePath);

            Debug.Log($"Bootstrapping into target scene: {targetScene.Name} (Path: {targetScene.Path})");

            // Use addressables to load the scene instead of SceneManager because the scene may not be in the build settings
            await Addressables.LoadSceneAsync(targetScene.Path, LoadSceneMode.Single).ToUniTask(this);
            Debug.Log($"Bootstrapped scene, {targetScene.Name}, successfully.");
            GameManager.Instance.ChangeGameState(bootstrapConfig.GetSceneInitialState(targetScene), true);
#else
            await SceneManager.LoadSceneAsync(titleScene.Name);
            GameManager.Instance.ChangeGameState(GameState.Title, true);
#endif
            Destroy(gameObject);
        }
    }
}
