using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// A simple component to enable and disable player input. currently exists mainly for Level 0's intro
    /// </summary>
    public class IntroInputHandler : Singleton<IntroInputHandler>
    {
        private protected override void Awake()
        {
            base.Awake();
        }
        /// <summary>
        /// Disables all player and UI actions.
        /// </summary>
        public void DisableInput()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.DisableAllActions();
                Debug.Log("[IntroInputHandler] Player input disabled.");
            }
        }

        /// <summary>
        /// Enables gameplay player actions and locks the cursor.
        /// </summary>
        public void EnableInput()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.EnablePlayerActions();
                InputManager.Instance.LockCursor(true);
                Debug.Log("[IntroInputHandler] Player input enabled.");
            }
        }

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
