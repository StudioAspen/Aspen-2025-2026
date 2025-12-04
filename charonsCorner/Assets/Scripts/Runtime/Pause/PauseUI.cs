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

        private bool _isClosing = false;    // To prevent multiple close calls

        private protected override void Initialize()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            InputManager.Instance.Unpause += InputManager_Unpause;
            if (_isClosing) return; // Prevent opening animation if we are in the process of closing
            _isClosing = true;

            //Disable all buttons (so player click while animation is playing)
            EnableButtons(false);

            //Kill any existing animations from this RectTransform
            _rectTransform.DOKill();

            //Set the starting position of the UI Menu
            _rectTransform.localPosition = new Vector3(0, Screen.height, 0);

            //Animate the UI Menu into view
            _rectTransform
                .DOMoveY(Screen.height / 2f, _openingAnimationDuration)
                .SetUpdate(true)
                .SetEase(_openingAnimationEaseType)
                .OnComplete(() =>
                {
                    EnableButtons(true);
                    _isClosing = false;
                });
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.Unpause -= InputManager_Unpause;
        }

        // Called by button UI event
        public void Restart() => GameManager.Instance.ReloadScene(GameState.Gameplay);

        // Called by button UI event
        public void ShowSettingsPanel() => _uiManager.ShowPanel(UIManager.PanelName.Settings);

        // Called by button UI event
        public void GoBackToMenu() => GameManager.Instance.ReturnToMenu();

        // Called by button UI event
        public void GoBackToHub() => GameManager.Instance.ReturnToHub();

        public override void CloseUI()
        {
            if (_isClosing) return; // Prevent multiple close calls
            _isClosing = true;

            //Disabling all buttons
            EnableButtons(false);

            //Kill any existing animations from this RectTransform
            _rectTransform.DOKill();

            //Animate the UI Menu out of view
            _rectTransform
                .DOLocalMoveY(Screen.height, _closingAnimationDuration)
                .SetUpdate(true)
                .SetEase(_closingAnimationEaseType)
                .OnComplete(() =>
                {
                    GameManager.Instance.ChangeGameState(GameState.Gameplay);
                    _isClosing = false;
                });
        }

        // Called by button UI event
        public void QuitGame() => GameManager.QuitGame();

        private void InputManager_Unpause() => CloseUI();

        //Helper Methods
        private void EnableButtons(bool enable)
        {
            foreach (var button in _buttons)
            {
                button.interactable = enable;
            }
        }
    }
}
