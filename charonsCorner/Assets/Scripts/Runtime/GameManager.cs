using Eflatun.SceneReference;
using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace CharonsCorner.Runtime
{
    public enum GameState
    {
        Title,
        Loading,
        Gameplay,
        Dialogue,
        Paused,
        Cutscene
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [field: SerializeField, ReadOnly] public GameState CurrentGameState { get; private set; }
        public event Action<GameState> OnGameStateChanged = delegate { };
        private GameState initialGameState;

        public SceneReference CurrentScene { get; private set; }
        public event Action<SceneReference> OnSceneChanged = delegate { };

        [Header("References")]
        [SerializeField] private SceneReference titleScene;
        [SerializeField] private SceneReference tutorialScene;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // Initialize the game state to Title at the start
            ChangeGameState(initialGameState);
        }

        public void ChangeInitialGameState(GameState newInitialState) => initialGameState = newInitialState;

        public void ChangeGameState(GameState newState)
        {
            if(CurrentGameState == newState)
                return;

            CurrentGameState = newState;
            OnGameStateEnter(newState);

            OnGameStateChanged.Invoke(newState);
        }

        private void OnGameStateEnter(GameState newState)
        {
            switch (newState)
            {
                case GameState.Title:
                    Time.timeScale = 1f;
                    UIManager.Instance.HideAllPanels();
                    InputManager.Instance.LockCursor(false);
                    break;
                case GameState.Loading:
                    Time.timeScale = 1f;
                    UIManager.Instance.HideAllPanels();
                    UIManager.Instance.ShowLoadingPanel(true);
                    InputManager.Instance.DisableAllActions();
                    InputManager.Instance.LockCursor(false);
                    break;
                case GameState.Gameplay:
                    Time.timeScale = 1f;
                    UIManager.Instance.HideAllPanels();
                    InputManager.Instance.EnablePlayerActions();
                    InputManager.Instance.LockCursor(true);
                    break;
                case GameState.Dialogue:
                    Time.timeScale = 0f;
                    UIManager.Instance.ShowPanel(UIManager.PanelName.Dialogue);
                    InputManager.Instance.LockCursor(false);
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    UIManager.Instance.ShowPanel(UIManager.PanelName.PauseMenu);
                    InputManager.Instance.LockCursor(false);
                    break;
                case GameState.Cutscene:
                    Time.timeScale = 1f;
                    InputManager.Instance.LockCursor(false);
                    break;
            }
        }

        public async UniTask SwitchScenes(SceneReference scene, GameState afterState)
        {
            ChangeGameState(GameState.Loading);

            await SceneManager.LoadSceneAsync(scene.Name);
            CurrentScene = scene;
            OnSceneChanged.Invoke(CurrentScene);

            UIManager.Instance.ShowLoadingPanel(false);

            ChangeGameState(afterState);
        }

        public void StartGame()
        {
            SwitchScenes(tutorialScene, GameState.Gameplay).Forget();
        }

        public void ReloadScene(GameState afterState)
        {
            SceneReference currentSceneReference = SceneReference.FromScenePath(SceneManager.GetActiveScene().path);
            SwitchScenes(currentSceneReference, afterState).Forget();
        }

        public void ReturnToMenu()
        {
            SwitchScenes(titleScene, GameState.Title).Forget();
        }

        public static void QuitGame()
        {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }
    }
}
