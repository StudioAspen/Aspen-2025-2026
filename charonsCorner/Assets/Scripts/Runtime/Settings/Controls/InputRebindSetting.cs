using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class InputRebindSetting : Setting
    {
        private protected override string saveKey => $"{actionToRebind.name}";

        [Header("Config")]
        [SerializeField] private InputActionReference actionToRebind;
        private int keyboardBindingIndex;
        private int mouseBindingIndex;
        private int gamepadBindingIndex;
        private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

        [Header("UI Elements")]
        [SerializeField] private Button keyboardMouseRebindButton;
        [SerializeField] private Button gamepadRebindButton;
        private TMP_Text keyboardMouseRebindButtonText;
        private TMP_Text gamepadRebindButtonText;

        private void Awake()
        {
            keyboardBindingIndex = actionToRebind.action.bindings.IndexOf(binding => binding.groups.Contains("Keyboard"));
            mouseBindingIndex = actionToRebind.action.bindings.IndexOf(binding => binding.groups.Contains("Mouse"));
            gamepadBindingIndex = actionToRebind.action.bindings.IndexOf(binding => binding.groups.Contains("Gamepad"));

            keyboardMouseRebindButtonText = keyboardMouseRebindButton.GetComponentInChildren<TMP_Text>();
            gamepadRebindButtonText = gamepadRebindButton.GetComponentInChildren<TMP_Text>();
        }

        public void StartKeyboardRebind()
        {
            keyboardMouseRebindButtonText.text = "...";

            actionToRebind.action.Disable();

            rebindingOperation = actionToRebind.action
                .PerformInteractiveRebinding()
                .WithControlsExcluding("Mouse")
                .WithControlsExcluding("Gamepad")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    actionToRebind.action.Enable();
                    keyboardMouseRebindButtonText.text = actionToRebind.action.GetBindingDisplayString(keyboardBindingIndex);
                    operation.Dispose();
                })
                .Start();
        }

        public void StartMouseRebind()
        {
            keyboardMouseRebindButtonText.text = "...";

            actionToRebind.action.Disable();

            rebindingOperation = actionToRebind.action
                .PerformInteractiveRebinding()
                .WithControlsExcluding("Keyboard")
                .WithControlsExcluding("Gamepad")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    actionToRebind.action.Enable();
                    keyboardMouseRebindButtonText.text = actionToRebind.action.GetBindingDisplayString(mouseBindingIndex);
                    operation.Dispose();
                })
                .Start();
        }

        public void StartGamepadRebind()
        {
            gamepadRebindButtonText.text = "...";

            actionToRebind.action.Disable();

            rebindingOperation = actionToRebind.action
                .PerformInteractiveRebinding()
                .WithControlsExcluding("Keyboard")
                .WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    actionToRebind.action.Enable();
                    gamepadRebindButtonText.text = actionToRebind.action.GetBindingDisplayString(gamepadBindingIndex);
                    operation.Dispose();
                })
                .Start();
        }

        public void CancelRebind()
        {
            rebindingOperation?.Cancel();
            rebindingOperation?.Dispose();
            rebindingOperation = null;
            keyboardMouseRebindButtonText.text = actionToRebind.action.GetBindingDisplayString(actionToRebind.action.bindings.IndexOf(binding => binding.groups.Contains("Keyboard")));
            gamepadRebindButtonText.text = actionToRebind.action.GetBindingDisplayString(actionToRebind.action.bindings.IndexOf(binding => binding.groups.Contains("Gamepad")));
        }

        public override void Apply()
        {
            
        }

        public override void Load()
        {
            
        }

        public override void Discard()
        {
            
        }

        public override bool IsDirty()
        {
            return false;
        }
    }
}
