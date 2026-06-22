using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// A component that automatically displays the current interact button's icon or text.
    /// It updates whenever the control scheme changes.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class InteractButtonDisplayer : MonoBehaviour
    {
        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnControlSchemeChanged += OnControlSchemeChanged;
                UpdateDisplay();
            }
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnControlSchemeChanged -= OnControlSchemeChanged;
            }
        }

        private void OnControlSchemeChanged(InputManager.ControlScheme newScheme)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (InputManager.Instance == null || _text == null) return;

            string displayString = GetInteractDisplayString(InputManager.Instance.CurrentControlScheme);
            _text.text = displayString;
        }

        private string GetInteractDisplayString(InputManager.ControlScheme controlScheme)
        {
            if (InputManager.Instance == null || InputManager.Instance.InputActions == null) return string.Empty;
            
            var action = InputManager.Instance.InputActions.Player.Interact;
            return InputDisplayer.GetInputDisplayString(action, controlScheme);
        }
    }
}
