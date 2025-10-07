using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    public class GameplayConfirmPromptOpener : MonoBehaviour
    {
        [SerializeField] private string _warningPrompt = string.Empty;
        [SerializeField] private string _yesButtonText = string.Empty;
        [SerializeField] private UnityEvent _yesEvent;

        public void OpenGameplayConfirmPrompt()
        {
            GameManager.Instance.ChangeGameState(GameState.GameplayConfirm);
            UIManager.Instance.ChangeConfirmPanelContents(_warningPrompt, _yesButtonText, ()=> _yesEvent.Invoke());
        }
    }
}
