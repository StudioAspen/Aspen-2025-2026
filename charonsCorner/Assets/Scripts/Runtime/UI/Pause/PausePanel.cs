using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class PausePanel : UIPanel
    {
        private RectTransform _rectTransform;
        private Coroutine _deactivationCoroutine;
        private Action _pendingSceneTransition;

        [Header("References")]
        [SerializeField] private List<Button> _buttons;

        [Header("Animation")]
        [SerializeField] private MMF_Player _openPlayer;
        [SerializeField] private MMF_Player _closePlayer;
        [SerializeField] private float _deactivationDelay = 0.5f;

        private bool _isAnimating = false;    

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                Debug.LogError("PauseUI: RectTransform component not found.");
            }

            OnFocused.AddListener(HandleFocused);
        }

        private void HandleFocused()
        {
            // If we are already active and animating (likely closing), interrupt it.
            if (_isAnimating || _deactivationCoroutine != null)
            {
                InterruptDeactivation();
                PlayOpenAnimation();
            }
        }

        private void InterruptDeactivation()
        {
            if (_deactivationCoroutine != null)
            {
                StopCoroutine(_deactivationCoroutine);
                _deactivationCoroutine = null;
            }

            if (_closePlayer != null && _closePlayer.IsPlaying)
            {
                _closePlayer.StopFeedbacks();
            }

            _pendingSceneTransition = null;
            _isAnimating = false;
        }

        private void Start()
        {
            // Ensure the panel is disabled at the start of the scene if we are not in Paused state.
            if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameState.Paused)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Subscribes to the unpause event and plays the pause panel's opening animation while temporarily disabling button interaction.
        /// </summary>
        /// <remarks>
        /// If a closing animation is in progress, this method returns early and does not start an opening animation. Buttons are disabled before the animation begins and re-enabled when the animation completes.
        /// </remarks>
        private void OnEnable()
        {
            // Only play animation and setup if the game is actually paused.
            // This prevents the animation from playing if the panel is active on scene start.
            if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameState.Paused)
            {
                return;
            }

            // Ensure any pending deactivation is cleared if we are enabling.
            InterruptDeactivation();

            InputManager.Instance.Unpause += InputManager_Unpause;

            PlayOpenAnimation();
        }

        private void PlayOpenAnimation()
        {
            _isAnimating = true;

            //Disable all buttons (so player can't click while animation is playing)
            EnableButtons(false);

            if (_openPlayer != null)
            {
                _openPlayer.Events.OnComplete.RemoveListener(OnOpenAnimationComplete);
                _openPlayer.Events.OnComplete.AddListener(OnOpenAnimationComplete);
                _openPlayer.PlayFeedbacks();
            }
            else
            {
                OnOpenAnimationComplete();
            }
        }

        private void OnOpenAnimationComplete()
        {
            EnableButtons(true);
            _isAnimating = false;
            if (_openPlayer != null)
            {
                _openPlayer.Events.OnComplete.RemoveListener(OnOpenAnimationComplete);
            }
        }

        /// <summary>
        /// Unsubscribes this component's Unpause handler from the InputManager if an instance exists when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.Unpause -= InputManager_Unpause;
            
            _isAnimating = false;

            if (_openPlayer != null)
            {
                _openPlayer.Events.OnComplete.RemoveListener(OnOpenAnimationComplete);
                _openPlayer.StopFeedbacks();
            }

            if (_closePlayer != null)
            {
                _closePlayer.Events.OnComplete.RemoveListener(OnCloseAnimationComplete);
                _closePlayer.StopFeedbacks();
            }
        }

        // Called by button UI event
        public void Restart()
        {
            _pendingSceneTransition = () => GameManager.Instance.ReloadScene(GameState.Gameplay);
            CloseUI();
        }

        // Called by button UI event
        public void ShowSettingsPanel() => SettingsCanvas.ShowSettings();

        // Called by button UI event
        public void GoBackToMenu()
        {
            _pendingSceneTransition = () => GameManager.Instance.ReturnToMenu();
            CloseUI();
        }

        // Called by button UI event
        public void GoBackToHub()
        {
            _pendingSceneTransition = () => GameManager.Instance.ReturnToHub();
            CloseUI();
        }

        /// <summary>
        /// Closes the pause UI by animating the panel out of view and returning the game to gameplay.
        /// </summary>
        /// <remarks>
        /// If a close operation is already in progress, the method returns immediately. It disables UI interactions, plays the close animation independent of timescale, and changes the game state to GameState.Gameplay when the animation completes.
        /// </remarks>
        public void CloseUI()
        {
            if (_isAnimating) return; // Prevent multiple close calls
            _isAnimating = true;

            //Disabling all buttons
            EnableButtons(false);

            if (_closePlayer != null)
            {
                _closePlayer.Events.OnComplete.RemoveListener(OnCloseAnimationComplete);
                _closePlayer.Events.OnComplete.AddListener(OnCloseAnimationComplete);
                _closePlayer.PlayFeedbacks();
            }
            else
            {
                OnCloseAnimationComplete();
            }
        }

        private void OnCloseAnimationComplete()
        {
            // Only unpause if we're not about to transition to a different scene.
            if (_pendingSceneTransition == null)
            {
                GameManager.Instance.ChangeGameState(GameState.Gameplay);
            }
            
            _deactivationCoroutine = StartCoroutine(WaitAndDeactivate());
        }

        private IEnumerator WaitAndDeactivate()
        {
            if (_deactivationDelay > 0)
            {
                yield return new WaitForSecondsRealtime(_deactivationDelay);
            }

            _deactivationCoroutine = null;
            
            if (_pendingSceneTransition != null)
            {
                _pendingSceneTransition.Invoke();
                _pendingSceneTransition = null;
            }

            BackOrClose();
            _isAnimating = false;

            if (_closePlayer != null)
            {
                _closePlayer.Events.OnComplete.RemoveListener(OnCloseAnimationComplete);
            }
        }

        /// <summary>
        /// Initiates quitting the game via the GameManager.
        /// </summary>
        public void QuitGame() => GameManager.QuitGame();

        /// <summary>
        /// Handles the unpause input action by closing the pause UI.
        /// </summary>
        private void InputManager_Unpause() => CloseUI();

        /// <summary>
        /// Sets the interactable state of all configured pause panel buttons.
        /// </summary>
        /// <param name="enable">`true` to make buttons interactable, `false` to disable their interaction.</param>
        private void EnableButtons(bool enable)
        {
            if (_buttons == null) return;

            foreach (var button in _buttons)
            {
                if (button == null) continue;
                button.interactable = enable;
            }
        }
    }
}