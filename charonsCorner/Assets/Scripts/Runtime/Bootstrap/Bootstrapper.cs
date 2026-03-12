using System;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
#endif

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Controls the Bootstrap scene, serving as the Single Entry Point manager.
    /// Handles the initial game state to a target scene inside the editor through the BootstrapConfigSO.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        private static Bootstrapper _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoSpawn()
        {
            if (_instance != null) return;

            var prefab = Resources.Load<GameObject>("Bootstrap");
            if (prefab == null)
            {
                Debug.LogError("Bootstrap prefab not found in Resources. Place a 'Bootstrap' prefab under a Resources folder.");
                return;
            }

            UnityEngine.Object.Instantiate(prefab);
        }
        
#if UNITY_EDITOR
        [SerializeField, Required] private BootstrapConfigSO _bootstrapConfig;
#endif

        private void Awake()
        {
#if UNITY_EDITOR
            _bootstrapConfig.Initialize();
#endif
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (_bootstrapConfig != null)
            {
                var currentScene = GameManager.Instance.GetCurrentScene();
                var initialState = _bootstrapConfig.GetSceneInitialState(currentScene);
                GameManager.Instance.ChangeGameState(initialState, true);
            }
            else
            {
                Debug.LogWarning("BootstrapConfigSO is not assigned on the Bootstrap prefab.", this);
            }
#endif
        }
    }
}
