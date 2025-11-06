using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    public class GameplayConfirmPromptOpener : MonoBehaviour
    {
        [SerializeField] private string warningPrompt = string.Empty;
        [SerializeField] private string yesButtonText = string.Empty;
        [SerializeField] private UnityEvent yesEvent;

        public void OpenGameplayConfirmPrompt()
        {
            GameManager.Instance.ChangeGameState(GameState.GameplayConfirm);
            UIManager.Instance.ChangeConfirmPanelContents(warningPrompt, yesButtonText, ()=> yesEvent.Invoke());
        }
    }
}
