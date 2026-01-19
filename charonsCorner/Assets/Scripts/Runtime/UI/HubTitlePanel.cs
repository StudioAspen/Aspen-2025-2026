using MoreMountains.Feedbacks;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class HubTitlePanel : UIPanel
    {
        /// <summary>
        /// Transition the game from the Title state to the Gameplay state.
        /// Can be attached to a "Start" button in the inspector.
        /// </summary>
        public void StartGame()
        {
            GameManager.Instance.ChangeGameState(GameState.Gameplay);
        }

        public void OpenSettings()
        {
            SettingsCanvas.ShowSettings();
        }
    }
}
