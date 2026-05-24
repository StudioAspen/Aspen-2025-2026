using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Checks the current game state on start and activates the appropriate game object.
    /// </summary>
    public class CutsceneChecker : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Object to activate if the scene is entered in GameplayState.")]
        [SerializeField] private GameObject _gameplayGameObject;

        [Tooltip("Object to activate if the scene is entered in CutsceneState.")]
        [SerializeField] private GameObject _cutsceneGameObject;

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("[CutsceneChecker] GameManager instance not found. Cannot determine game state.");
                return;
            }

            GameState currentState = GameManager.Instance.CurrentGameState;

            if (currentState == GameState.Gameplay)
            {
                ActivateGameplay();
            }
            else if (currentState == GameState.Cutscene)
            {
                ActivateCutscene();
            }
            else
            {
                Debug.Log($"[CutsceneChecker] Current state is {currentState}. No action taken.");
            }
        }

        private void ActivateGameplay()
        {
            if (_gameplayGameObject != null)
                _gameplayGameObject.SetActive(true);
            
            if (_cutsceneGameObject != null)
                _cutsceneGameObject.SetActive(false);
        }

        private void ActivateCutscene()
        {
            if (_cutsceneGameObject != null)
                _cutsceneGameObject.SetActive(true);
            
            if (_gameplayGameObject != null)
                _gameplayGameObject.SetActive(false);
        }
    }
}
