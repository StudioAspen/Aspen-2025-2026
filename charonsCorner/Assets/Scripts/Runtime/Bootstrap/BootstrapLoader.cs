using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CharonsCorner.Runtime
{
    public class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private SceneReference titleScene;

#if UNITY_EDITOR
        [SerializeField, Required] private BootstrapConfigSO bootstrapConfig;

        private void Awake()
        {
            bootstrapConfig.Initialize();
        }
#endif

        private async void Start()
        {
#if UNITY_EDITOR
            string targetScenePath = EditorPrefs.GetString("Bootstrap_OriginalScene", null);
            SceneReference targetScene = string.IsNullOrEmpty(targetScenePath) ? titleScene : SceneReference.FromScenePath(targetScenePath);
            
            await SceneManager.LoadSceneAsync(targetScene.Name);
            GameManager.Instance.ChangeGameState(bootstrapConfig.GetSceneInitialState(targetScene), true);
#else
            SceneManager.LoadScene(titleScene.Name);
            GameManager.Instance.ChangeGameState(GameState.Title, true);
#endif
        }
    }
}
