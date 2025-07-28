using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PauseUI : UIPanel
    {
        private protected override void Initialize()
        {

        }

        private void OnEnable()
        {
            InputManager.Instance.Unpause += InputManager_Unpause;
        }

        private void OnDisable()
        {
            if(InputManager.Instance != null)
                InputManager.Instance.Unpause -= InputManager_Unpause;
        }

        // Called by button UI event
        public void Restart() => GameManager.Instance.ReloadScene(GameState.Gameplay);

        // Called by button UI event
        public void ShowSettingsPanel() => uiManager.ShowPanel(UIManager.PanelName.Settings);

        // Called by button UI event
        public void GoBackToMenu() => GameManager.Instance.ReturnToMenu();

        public override void CloseUI()
        {
            GameManager.Instance.ChangeGameState(GameState.Gameplay);
        }

        // Called by button UI event
        public void QuitGame() => GameManager.QuitGame();

        private void InputManager_Unpause() => CloseUI();
    }
}
