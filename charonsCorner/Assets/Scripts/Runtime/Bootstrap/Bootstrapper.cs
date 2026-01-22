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
            Instantiate(prefab);
        }
        
#if UNITY_EDITOR
        [SerializeField, Required] private BootstrapConfigSO _bootstrapConfig;
#endif

        private void Awake()
        {
#if UNITY_EDITOR
            _bootstrapConfig.Initialize();
#endif
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            GameManager.Instance.ChangeGameState(_bootstrapConfig.GetSceneInitialState(GameManager.Instance.GetCurrentScene()), true);
        }
    }
}
