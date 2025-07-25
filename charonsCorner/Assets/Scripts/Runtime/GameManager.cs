using Eflatun.SceneReference;
using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Steamworks;

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
        /// <summary>
        /// Action that is invoked when the game state is changed.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description><c>GameState newState</c>: The state that was changed to.</description></item>
        /// </list>
        /// </remarks>
        public event Action<GameState> OnGameStateChanged = delegate { };
        private GameState initialGameState;

        /// <summary>
        /// Action that is invoked the current scene is changed.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description><c>SceneReference newScene</c>: The scene that was changed to.</description></item>
        /// </list>
        /// </remarks>
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

        /// <summary>
        /// Changes the current game state to the specified new state.
        /// Will not change if the new state is the same as the current state unless 'force' is true.
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="force">Whether to rechange to the same state.</param>
        public void ChangeGameState(GameState newState, bool force = false)
        {
            if(CurrentGameState == newState && !force)
                return;

            CurrentGameState = newState;
            OnGameStateEnter(newState);

            OnGameStateChanged.Invoke(newState);
        }

        /// <summary>
        /// Called when after the game state has changed.
        /// Handle any UI updates or game logic that should occur when entering a new game state.
        /// </summary>
        /// <param name="newState"></param>
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
                    Time.timeScale = 0f;
                    InputManager.Instance.DisableAllActions();
                    InputManager.Instance.LockCursor(false);
                    break;
                case GameState.Gameplay:
                    Time.timeScale = 1f;
                    UIManager.Instance.HideAllPanels();
                    InputManager.Instance.EnablePlayerActions();
                    InputManager.Instance.LockCursor(true);
                    // If the Steam overlay is open, pause the game
                    if(SteamOverlayListener.IsOverlayOpen)
                        ChangeGameState(GameState.Paused);
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

        /// <summary>
        /// Asynchronously switches to the specified scene and changes the game state afterwards.
        /// While waiting for the scene to load, the game state is set to Loading.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="afterState"></param>
        /// <returns></returns>
        public async UniTask SwitchScenes(SceneReference scene, GameState afterState)
        {
            ChangeGameState(GameState.Loading);
            await UIManager.Instance.LoadingHandler.FadeIn();

            await SceneManager.LoadSceneAsync(scene.Name);
            OnSceneChanged.Invoke(scene);

            ChangeGameState(afterState);

            await UIManager.Instance.LoadingHandler.FadeOut();
        }

        /// <summary>
        /// Starts the game by switching to the tutorial scene and setting the game state to Gameplay.
        /// TODO in the future because of loading from saved point.
        /// </summary>
        public void StartGame()
        {
            SwitchScenes(tutorialScene, GameState.Gameplay).Forget();
        }

        /// <summary>
        /// Reloads the scene by switching to the current scene and changing the game state afterwards.
        /// </summary>
        /// <param name="afterState"></param>
        public void ReloadScene(GameState afterState) => SwitchScenes(GetCurrentScene(), afterState).Forget();

        /// <summary>
        /// Helper method to switch back to the title scene and set the game state to Title.
        /// </summary>
        public void ReturnToMenu() => SwitchScenes(titleScene, GameState.Title).Forget();

        public SceneReference GetCurrentScene() => SceneReference.FromScenePath(SceneManager.GetActiveScene().path);

        /// <summary>
        /// Quits the game properly based on the platform.
        /// </summary>
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
