using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class HubTitlePanel : UIPanel
    {
        private protected override void Initialize()
        {
            // This acts as your 'Awake' method. 
            // Use it to cache components or setup initial logic.
        }

        public override void CloseUI()
        {
            // Define what happens when this UI is closed (e.g., return to Gameplay)
            GameManager.Instance.ChangeGameState(GameState.Gameplay);
        }
    }
}
