using System;
using DG.Tweening;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PauseUI : UIPanel
    {
        [SerializeField] private float _animationDuration = 0.5f;
        [SerializeField] private Ease _animationEaseType = Ease.Linear;

        private protected override void Initialize()
        {

        }

        private void OnEnable()
        {
            InputManager.Instance.Unpause += InputManager_Unpause;

            RectTransform rt = GetComponent<RectTransform>();
            rt.position = rt.position.WithY(Screen.height);
            //rt.DOMoveY(Screen.height/2, _animationDuration).SetUpdate(true).SetEase(_animationEaseType).OnComplete(CloseUI).SetId(1);

            rt.DOMoveY(Screen.height / 2, _animationDuration).SetUpdate(true).SetEase(_animationEaseType);
        }

        private void OnDisable()
        {
            if(InputManager.Instance != null)
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
            Debug.Log("Closing Pause UI");
            GameManager.Instance.ChangeGameState(GameState.Gameplay);
        }

        // Called by button UI event
        public void QuitGame() => GameManager.QuitGame();

        private void InputManager_Unpause() => CloseUI();
    }
}
