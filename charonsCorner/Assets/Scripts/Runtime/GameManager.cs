using Eflatun.SceneReference;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharonsCorner.Runtime
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private SceneReference tutorialScene;

        [Header("Debug")]
        [SerializeField] private SceneReference debugScene;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        [Button("Switch Scenes")]
        public void SwitchScenes()
        {
            SceneManager.LoadScene(debugScene.Name);
        }

        public void SwitchScenes(SceneReference scene)
        {
            SceneManager.LoadScene(scene.Name);
        }

        public void StartGame()
        {
            SwitchScenes(tutorialScene);
        }
    }
}
