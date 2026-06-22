using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// A simple component to enable and disable player input. currently exists mainly for Level 0's intro
    /// </summary>
    public class Level0IntroStateManager : MonoBehaviour
    {
        /// <summary>
        /// Sets the game state to Cutscene, which typically disables normal gameplay input.
        /// </summary>
        public void SetCutsceneState()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeGameState(GameState.Cutscene);
            }
        }

        /// <summary>
        /// Sets the game state to Gameplay, which enables normal player input.
        /// </summary>
        public void SetGameplayState()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeGameState(GameState.Gameplay);
            }
        }
    }
}
