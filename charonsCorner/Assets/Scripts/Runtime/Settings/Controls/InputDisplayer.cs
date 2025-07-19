using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharonsCorner.Runtime
{
    public class InputDisplayer : MonoBehaviour
    {
        [SerializeField] private InputActionReference inputAction;
        private int keyboardMouseBindingIndex;
        private int gamepadBindingIndex;

        public enum KeyboardMouseInputType
        {
            Keyboard,
            Mouse
        }

        [Header("Keyboard Mouse")]
        [SerializeField] private KeyboardMouseInputType keyboardMouseInputType;
        [SerializeField] private TMP_Text keyboardMouseInputText;

        [Header("Gamepad")]
        [SerializeField] private TMP_Text gamepadInputText;

        private void Awake()
        {
            keyboardMouseBindingIndex = inputAction.action.bindings.IndexOf(binding => binding.groups.Contains($"{keyboardMouseInputType}"));
            gamepadBindingIndex = inputAction.action.bindings.IndexOf(binding => binding.groups.Contains("Gamepad"));

            keyboardMouseInputText.text = inputAction.action.GetBindingDisplayString(keyboardMouseBindingIndex, InputBinding.DisplayStringOptions.DontIncludeInteractions);
            gamepadInputText.text = inputAction.action.GetBindingDisplayString(gamepadBindingIndex, InputBinding.DisplayStringOptions.DontIncludeInteractions);
        }
    }
}
