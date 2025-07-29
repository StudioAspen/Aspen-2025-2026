using Eflatun.SceneReference;
using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        private static bool hasBootstrapped = false;

        [SerializeField, Required] private BootstrapConfigSO bootstrapConfig;
        [SerializeField, Required] private GameManager gameManager;

        private void Awake()
        {
            if (hasBootstrapped)
            {
                Destroy(gameObject);
                return;
            }

            hasBootstrapped = true;

            bootstrapConfig.Initialize();

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SceneReference currentScene = gameManager.GetCurrentScene();
            gameManager.ChangeGameState(bootstrapConfig.GetSceneInitialState(currentScene), true); // Required because different scenes may have different initial game states
        }
    }

    public static class BootstrapLoader
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadBootstrap()
        {
            if (GameObject.FindFirstObjectByType<Bootstrap>() == null)
            {
                GameObject prefab = Resources.Load<GameObject>("Bootstrap");
                GameObject go = GameObject.Instantiate(prefab);
            }
        }
    }
}
