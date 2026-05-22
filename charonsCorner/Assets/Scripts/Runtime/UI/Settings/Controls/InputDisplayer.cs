using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharonsCorner.Runtime
{
    public class InputDisplayer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputActionReference _inputAction;
        [SerializeField] private TMP_Text _inputDisplayText;

        [Header("Config")]
        [SerializeField] private bool _autoDetectControlScheme = false;
        private bool ShowManualControlScheme => !_autoDetectControlScheme;
        [SerializeField, ShowIf(nameof(ShowManualControlScheme))] private InputManager.ControlScheme _controlScheme;

        private void Start()
        {
            if (_autoDetectControlScheme)
            {
                _controlScheme = InputManager.Instance.CurrentControlScheme;
                InputManager.Instance.OnControlSchemeChanged += InputManager_OnControlSchemeChanged;
            }

            _inputDisplayText.text = GetInputDisplayString(_inputAction, _controlScheme);
        }

        private void OnDestroy()
        {
            if(InputManager.Instance != null)
                InputManager.Instance.OnControlSchemeChanged -= InputManager_OnControlSchemeChanged;
        }

        private void InputManager_OnControlSchemeChanged(InputManager.ControlScheme newScheme)
        {
            _controlScheme = newScheme;
            _inputDisplayText.text = GetInputDisplayString(_inputAction, _controlScheme);
        }

        private static readonly Dictionary<string, string> KeyboardIconMap = new Dictionary<string, string>
        {
            { "Space", "\uE0CB" },
            { "Enter", "\uE05E" },
            { "Escape", "\uE062" },
            { "Backspace", "\uE038" },
            { "Tab", "\uE0D1" },
            { "Shift", "\uE0C3" },
            { "Ctrl", "\uE054" },
            { "Alt", "\uE017" },
            { "E", "\uE05A" },
            { "F", "\uE066" },
            { "Q", "\uE0B3" },
            { "R", "\uE0B9" },
            { "W", "\uE0DF" },
            { "A", "\uE015" },
            { "S", "\uE0BD" },
            { "D", "\uE056" }
        };

        private static readonly Dictionary<string, string> XboxIconMap = new Dictionary<string, string>
        {
            { "A", "\uE00C" },  // xbox_button_color_a
            { "B", "\uE00E" },  // xbox_button_color_b
            { "X", "\uE010" },  // xbox_button_color_x
            { "Y", "\uE012" },  // xbox_button_color_y
            { "Left Stick", "\uE04F" },
            { "Right Stick", "\uE057" },
            { "Left Shoulder", "\uE043" },
            { "Right Shoulder", "\uE049" },
            { "Left Trigger", "\uE047" },
            { "Right Trigger", "\uE04D" },
            { "Start", "\uE018" },
            { "Select", "\uE008" },
            { "D-Pad Up", "\uE035" },
            { "D-Pad Down", "\uE024" },
            { "D-Pad Left", "\uE028" },
            { "D-Pad Right", "\uE02B" }
        };

        private static readonly Dictionary<string, string> PSIconMap = new Dictionary<string, string>
        {
            { "Cross",    "\uE043" },  // playstation_button_color_cross
            { "Circle",   "\uE041" },  // playstation_button_color_circle
            { "Square",   "\uE045" },  // playstation_button_color_square
            { "Triangle", "\uE047" },  // playstation_button_color_triangle
            { "L3", "\uE04B" },
            { "R3", "\uE04D" },
            { "L1", "\uE076" },
            { "R1", "\uE07E" },
            { "L2", "\uE07A" },
            { "R2", "\uE082" },
            { "Options", "\uE009" },
            { "Share", "\uE00B" },
            { "D-Pad Up", "\uE05E" },
            { "D-Pad Down", "\uE055" },
            { "D-Pad Left", "\uE059" },
            { "D-Pad Right", "\uE05C" }
        };

        /// <summary>
        /// Gets the display string for the given input action based on the specified control scheme.
        /// </summary>
        public static string GetInputDisplayString(InputAction action, InputManager.ControlScheme controlScheme, InputBinding.DisplayStringOptions displayOptions = InputBinding.DisplayStringOptions.DontIncludeInteractions)
        {
            int bindingIndex = action.bindings.IndexOf(binding => binding.groups.Contains($"{InputManager.ControlSchemeInternalNames[controlScheme]}"));
            string displayString = action.GetBindingDisplayString(bindingIndex, displayOptions);

            Dictionary<string, string> currentMap = controlScheme switch
            {
                InputManager.ControlScheme.KeyboardMouse => KeyboardIconMap,
                InputManager.ControlScheme.Xbox => XboxIconMap,
                InputManager.ControlScheme.PS => PSIconMap,
                InputManager.ControlScheme.Gamepad => XboxIconMap,
                _ => null
            };

            if (currentMap != null && currentMap.TryGetValue(displayString, out string iconHex))
            {
                return iconHex;
            }

            return displayString;
        }

        /// <summary>
        /// Gets the display string for the given input action based on the specified control scheme.
        /// </summary>
        public static string GetInputDisplayString(InputActionReference inputAction, InputManager.ControlScheme controlScheme, InputBinding.DisplayStringOptions displayOptions = InputBinding.DisplayStringOptions.DontIncludeInteractions)
        {
            return GetInputDisplayString(inputAction.action, controlScheme, displayOptions);
        }
    }
}
