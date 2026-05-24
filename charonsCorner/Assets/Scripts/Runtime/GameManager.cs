using Eflatun.SceneReference;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;

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

    public class GameManager : Singleton<GameManager>
    {
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

        [Header("References")]
        [SerializeField] private SceneReference _titleScene;
        [SerializeField] private SceneReference _hubScene;
        [SerializeField] private SceneReference _tutorialScene;
        [SerializeField] private SceneReference _numkey1Scene;
        [SerializeField] private TMP_Text _gameStateDisplayText;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.minusKey.wasPressedThisFrame)
            {
                RestartGameFromScratch();
            }
            if (Keyboard.current != null && Keyboard.current.equalsKey.wasPressedThisFrame)
            {
                ReturnToMenu();
            }
            if (Keyboard.current != null && Keyboard.current.commaKey.wasPressedThisFrame)
            {
                SwitchScenes(_hubScene, GameState.Gameplay).Forget();
            }
            if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                SwitchScenes(_numkey1Scene, GameState.Gameplay).Forget();
            }
        }

        private protected override void Awake()
        {
            base.Awake();
            UpdateGameStateDisplay();
            UIPanel.OnPanelChanged += HandlePanelChanged;
        }

        private void OnDestroy()
        {
            UIPanel.OnPanelChanged -= HandlePanelChanged;
        }

        private void HandlePanelChanged(UIPanel panel)
        {
            UpdateGameStateDisplay();
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
            UpdateGameStateDisplay();
        }

        private void UpdateGameStateDisplay()
        {
            if (_gameStateDisplayText != null)
            {
                string panelName = UIPanel.ActivePanel != null ? UIPanel.ActivePanel.name : "None";
                _gameStateDisplayText.text = $"State: {CurrentGameState} | UI: {panelName}";
            }
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
                    InputManager.Instance.EnableUIActions();
                    break;
                case GameState.Loading:
                    Time.timeScale = 0f;
                    InputManager.Instance.DisableAllActions();
                    InputManager.Instance.LockCursor(false);
                    break;
                case GameState.Gameplay:
                    Time.timeScale = 1f;
                    InputManager.Instance.EnablePlayerActions();
                    // If the Steam overlay is open, pause the game
                    if(SteamOverlayListener.IsOverlayOpen)
                        TryPauseGame();
                    break;
                case GameState.Dialogue:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    PauseCanvas.Instance.ShowPause();
                    break;
                case GameState.Cutscene:
                    Time.timeScale = 1f;
                    InputManager.Instance.EnablePlayerActions();
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
            await LoadingCanvas.Instance.FadeIn();
            
            Debug.Log($"[GameManager] Switching to scene: {scene.Name}. Clearing UIPanel.ActivePanel (excluding Loading).");
            UIPanel.CloseAll(false);

            await SceneManager.LoadSceneAsync(scene.Name);
            
            // Re-ensure UI is closed after scene load if we are in gameplay
            if (afterState == GameState.Gameplay)
            {
                UIPanel.CloseAll(false);
            }

            ChangeGameState(afterState);

            await LoadingCanvas.Instance.FadeOut(null);
        }

        /// <summary>
        /// TODO in the future because of loading from saved point.
        /// </summary>
        public void StartGame()
        {
            SwitchScenes(_hubScene, GameState.Title).Forget();
        }

        /// <summary>
        /// Reloads the scene by switching to the current scene and changing the game state afterwards.
        /// </summary>
        /// <param name="afterState"></param>
        public void ReloadScene(GameState afterState) => SwitchScenes(GetCurrentScene(), afterState).Forget();

        /// <summary>
        /// Helper method to switch back to the title scene and set the game state to Title.
        /// </summary>
        public void ReturnToMenu() => SwitchScenes(_hubScene, GameState.Title).Forget();

        /// <summary>
        /// Helper method to switch back to the hub scene and set the game state to Gameplay.
        /// </summary>
        public void ReturnToHub() => SwitchScenes(_hubScene, GameState.Title).Forget();

        /// <summary>
        /// Resets all progression and returns the player to the hub scene.
        /// </summary>
        public void RestartGameFromScratch()
        {
            FlagManager.ResetAll();
            SwitchScenes(_hubScene, GameState.Title).Forget();
        }

        public SceneReference GetCurrentScene() => SceneReference.FromScenePath(SceneManager.GetActiveScene().path);

        public void TryPauseGame()
        {
            if (PauseCanvas.IsPauseBlocked)
            {
                Debug.LogWarning("Can't pause game because pausing is blocked.");
                return;
            }

            if (!LoadingCanvas.Instance.IsLoaded)
            {
                Debug.LogWarning("Can't pause game because game is not finished loading.");
                return;
            }
            
            if (CurrentGameState != GameState.Gameplay)
            {
                Debug.LogWarning("Can't pause game because game is not in gameplay state.");
                return;
            }
            
            ChangeGameState(GameState.Paused);
        }
        
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
