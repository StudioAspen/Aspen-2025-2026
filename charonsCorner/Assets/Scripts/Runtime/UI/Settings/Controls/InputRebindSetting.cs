using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class InputRebindSetting : Setting
    {
        private protected override string SaveKey => $"{_actionToRebind.name}";

        [Header("Config")]
        [SerializeField] private InputActionReference _actionToRebind;
        private int _keyboardBindingIndex;
        private int _mouseBindingIndex;
        private int _gamepadBindingIndex;
        private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

        [Header("UI Elements")]
        [SerializeField] private Button _keyboardMouseRebindButton;
        [SerializeField] private Button _gamepadRebindButton;
        private TMP_Text _keyboardMouseRebindButtonText;
        private TMP_Text _gamepadRebindButtonText;

        private void Awake()
        {
            _keyboardBindingIndex = _actionToRebind.action.bindings.IndexOf(binding => binding.groups.Contains("Keyboard"));
            _mouseBindingIndex = _actionToRebind.action.bindings.IndexOf(binding => binding.groups.Contains("Mouse"));
            _gamepadBindingIndex = _actionToRebind.action.bindings.IndexOf(binding => binding.groups.Contains("Gamepad"));

            _keyboardMouseRebindButtonText = _keyboardMouseRebindButton.GetComponentInChildren<TMP_Text>();
            _gamepadRebindButtonText = _gamepadRebindButton.GetComponentInChildren<TMP_Text>();
        }

        public void StartKeyboardRebind()
        {
            _keyboardMouseRebindButtonText.text = "...";

            _actionToRebind.action.Disable();

            _rebindingOperation = _actionToRebind.action
                .PerformInteractiveRebinding()
                .WithControlsExcluding("Mouse")
                .WithControlsExcluding("Gamepad")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    _actionToRebind.action.Enable();
                    _keyboardMouseRebindButtonText.text = _actionToRebind.action.GetBindingDisplayString(_keyboardBindingIndex);
                    operation.Dispose();
                })
                .Start();
        }

        public void StartMouseRebind()
        {
            _keyboardMouseRebindButtonText.text = "...";

            _actionToRebind.action.Disable();

            _rebindingOperation = _actionToRebind.action
                .PerformInteractiveRebinding()
                .WithControlsExcluding("Keyboard")
                .WithControlsExcluding("Gamepad")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    _actionToRebind.action.Enable();
                    _keyboardMouseRebindButtonText.text = _actionToRebind.action.GetBindingDisplayString(_mouseBindingIndex);
                    operation.Dispose();
                })
                .Start();
        }

        public void StartGamepadRebind()
        {
            _gamepadRebindButtonText.text = "...";

            _actionToRebind.action.Disable();

            _rebindingOperation = _actionToRebind.action
                .PerformInteractiveRebinding()
                .WithControlsExcluding("Keyboard")
                .WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    _actionToRebind.action.Enable();
                    _gamepadRebindButtonText.text = _actionToRebind.action.GetBindingDisplayString(_gamepadBindingIndex);
                    operation.Dispose();
                })
                .Start();
        }

        public void CancelRebind()
        {
            _rebindingOperation?.Cancel();
            _rebindingOperation?.Dispose();
            _rebindingOperation = null;
            _keyboardMouseRebindButtonText.text = _actionToRebind.action.GetBindingDisplayString(_actionToRebind.action.bindings.IndexOf(binding => binding.groups.Contains("Keyboard")));
            _gamepadRebindButtonText.text = _actionToRebind.action.GetBindingDisplayString(_actionToRebind.action.bindings.IndexOf(binding => binding.groups.Contains("Gamepad")));
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
