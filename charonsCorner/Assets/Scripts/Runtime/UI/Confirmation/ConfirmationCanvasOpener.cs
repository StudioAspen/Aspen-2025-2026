using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    public class ConfirmationCanvasOpener : MonoBehaviour
    {
        [SerializeField] private string _confirmationText = "Confirm?";
        [SerializeField] private string _yesText = "Yes";
        [SerializeField] private string _noText = "No";
        [SerializeField] private UnityEvent _yesAction = new UnityEvent();
        [SerializeField] private UnityEvent _noAction = new UnityEvent();

        public void ShowConfirmation()
        {
            ConfirmationCanvas.Instance.ShowConfirmation(_confirmationText, _yesText, _noText, _yesAction.Invoke, _noAction.Invoke);
        }
    }
}