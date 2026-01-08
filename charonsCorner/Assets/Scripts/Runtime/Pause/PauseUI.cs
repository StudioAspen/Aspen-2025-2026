using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class PauseUI : UIPanel
    {
        private RectTransform _rectTransform;

        [Header("References")]
        [SerializeField] private List<Button> _buttons;

        [Header("Animation")]
        [SerializeField] private float _openingAnimationDuration = 1f;
        [SerializeField] private float _closingAnimationDuration = 1f;
        [SerializeField] private Ease _openingAnimationEaseType = Ease.OutBack;
        [SerializeField] private Ease _closingAnimationEaseType = Ease.Linear;

        private bool _isAnimating = false;    

        private protected override void Initialize()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                Debug.LogError("PauseUI: RectTransform component not found.");
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
            _rectTransform.DOKill(); // Kill any existing animations from this RectTransform

            _isAnimating = true;

            InputManager.Instance.Unpause += InputManager_Unpause;

            //Disable all buttons (so player can't click while animation is playing)
            EnableButtons(false);

            //Set the starting position off screen
            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, Screen.height);

            //Animate the UI Menu into view
            _rectTransform
                .DOAnchorPosY(0, _openingAnimationDuration)
                .SetUpdate(true)
                .SetEase(_openingAnimationEaseType)
                .OnComplete(() =>
                {
                    EnableButtons(true);
                    _isAnimating = false;
                });
        }

        /// <summary>
        /// Unsubscribes this component's Unpause handler from the InputManager if an instance exists when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.Unpause -= InputManager_Unpause;
            
            _isAnimating = false;
            _rectTransform.DOKill(); // Cancel any in-progress animation
        }

        // Called by button UI event
        public void Restart() => GameManager.Instance.ReloadScene(GameState.Gameplay);

        // Called by button UI event
        public void ShowSettingsPanel() => _uiManager.ShowPanel(UIManager.PanelName.Settings);

        // Called by button UI event
        public void GoBackToMenu() => GameManager.Instance.ReturnToMenu();

        // Called by button UI event
        public void GoBackToHub() => GameManager.Instance.ReturnToHub();

        /// <summary>
        /// Closes the pause UI by animating the panel out of view and returning the game to gameplay.
        /// </summary>
        /// <remarks>
        /// If a close operation is already in progress, the method returns immediately. It disables UI interactions, plays the close animation independent of timescale, and changes the game state to GameState.Gameplay when the animation completes.
        /// </remarks>
        public override void CloseUI()
        {
            if (_isAnimating) return; // Prevent multiple close calls
            _isAnimating = true;

            //Disabling all buttons
            EnableButtons(false);

            //Kill any existing animations from this RectTransform
            _rectTransform.DOKill();

            //Animate the UI Menu out of view
            _rectTransform
                .DOAnchorPosY(Screen.height, _closingAnimationDuration)
                .SetUpdate(true)
                .SetEase(_closingAnimationEaseType)
                .OnComplete(() =>
                {
                    GameManager.Instance.ChangeGameState(GameState.Gameplay);
                    _isAnimating = false;
                });
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