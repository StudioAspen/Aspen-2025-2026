using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PauseUI : UIPanel
    {
        private protected override void Initialize()
        {

        }

        public void Restart()
        {
            GameManager.Instance.ReloadScene();
            GameManager.Instance.ChangeGameState(GameState.Gameplay);
        }

        public void ShowSettingsPanel() => uiManager.ShowPanel(UIManager.PanelName.Settings);

        public void GoBackToMenu() => GameManager.Instance.ReturnToMenu();

        public override void CloseUI()
        {
            uiManager.HideAllPanels();
            GameManager.Instance.ChangeGameState(GameState.Gameplay);
        }
    }
}
