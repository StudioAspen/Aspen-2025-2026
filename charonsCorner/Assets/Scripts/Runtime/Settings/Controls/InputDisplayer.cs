using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharonsCorner.Runtime
{
    public class InputDisplayer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputActionReference inputAction;
        [SerializeField] private TMP_Text inputDisplayText;

        [Header("Config")]
        [SerializeField] private bool autoDetectControlScheme = false;
        private bool ShowManualControlScheme => !autoDetectControlScheme;
        [SerializeField, ShowIf(nameof(ShowManualControlScheme))] private InputManager.ControlScheme controlScheme;

        private void Start()
        {
            if (autoDetectControlScheme)
            {
                controlScheme = InputManager.Instance.CurrentControlScheme;
                InputManager.Instance.OnControlSchemeChanged += InputManager_OnControlSchemeChanged;
            }

            inputDisplayText.text = GetInputDisplayString(inputAction, controlScheme);
        }

        private void OnDestroy()
        {
            if(InputManager.Instance != null)
                InputManager.Instance.OnControlSchemeChanged -= InputManager_OnControlSchemeChanged;
        }

        private void InputManager_OnControlSchemeChanged(InputManager.ControlScheme newScheme)
        {
            controlScheme = newScheme;
            inputDisplayText.text = GetInputDisplayString(inputAction, controlScheme);
        }

        /// <summary>
        /// Gets the display string for the given input action based on the specified control scheme.
        /// </summary>
        public static string GetInputDisplayString(InputActionReference inputAction, InputManager.ControlScheme controlScheme, InputBinding.DisplayStringOptions displayOptions = InputBinding.DisplayStringOptions.DontIncludeInteractions)
        {
            int bindingIndex = inputAction.action.bindings.IndexOf(binding => binding.groups.Contains($"{InputManager.ControlSchemeInternalNames[controlScheme]}"));
            return inputAction.action.GetBindingDisplayString(bindingIndex, displayOptions);
        }
    }
}
