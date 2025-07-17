using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static CharonsCorner.Runtime.InputActions;

namespace CharonsCorner.Runtime
{
    public class InputManager : MonoBehaviour, IPlayerActions, IUIActions
    {
        public static InputManager Instance { get; private set; }

        public InputActions InputActions { get; private set; }

        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2> Look = delegate { };
        public event UnityAction<bool> Jump = delegate { };
        public event UnityAction<bool> Interact = delegate { };

        public Vector2 MoveDirection => InputActions.Player.Move.ReadValue<Vector2>();
        public Vector2 LookDirection => InputActions.Player.Look.ReadValue<Vector2>();
        public bool IsJumpKeyPressed => InputActions.Player.Jump.IsPressed();

        #region Control Scheme
        public enum ControlScheme
        {
            KeyboardMouse,
            Gamepad
        }
        [field: SerializeField, ReadOnly] public ControlScheme CurrentControlScheme { get; private set; }
        public event Action<ControlScheme> OnControlSchemeChanged = delegate { };
        #endregion

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CreatePlayerActions();
        }

        private void CreatePlayerActions()
        {
            InputActions = new InputActions();
            InputActions.Player.SetCallbacks(this);
            InputActions.UI.SetCallbacks(this);
        }

        public void DisableAllActions()
        {
            if(InputActions == null)
                CreatePlayerActions();

            InputActions.Disable();
        }

        public void EnablePlayerActions()
        {
            if (InputActions == null)
                CreatePlayerActions();

            InputActions.Enable();
            InputActions.UI.Disable();

            LockCursor(true);
        }

        public void EnableUIActions()
        {
            if (InputActions == null)
                CreatePlayerActions();

            InputActions.Disable();
            InputActions.UI.Enable();

            LockCursor(false);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Move.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            Look.Invoke(context.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Jump.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Jump.Invoke(false);
                    break;
            }
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                GameManager.Instance.ChangeGameState(GameState.Paused);
            }
        }
 
        public void OnInteract(InputAction.CallbackContext context) { }

        // UI Actions
        public void OnNavigate(InputAction.CallbackContext context) { }
        public void OnSubmit(InputAction.CallbackContext context) { }
        public void OnCancel(InputAction.CallbackContext context) { }
        public void OnPoint(InputAction.CallbackContext context) { }
        public void OnClick(InputAction.CallbackContext context) { }
        public void OnRightClick(InputAction.CallbackContext context) { }
        public void OnMiddleClick(InputAction.CallbackContext context) { }
        public void OnScrollWheel(InputAction.CallbackContext context) { }
        public void OnTrackedDevicePosition(InputAction.CallbackContext context) { }
        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context) { }

        // Control Scheme
        private void DetectControlScheme(InputAction.CallbackContext context)
        {
            var device = context.control.device;
            ControlScheme newScheme = device is Gamepad ? ControlScheme.Gamepad : ControlScheme.KeyboardMouse;

            if (newScheme != CurrentControlScheme)
            {
                CurrentControlScheme = newScheme;
                OnControlSchemeChanged.Invoke(CurrentControlScheme);
            }
        }

        // Cursor
        public void LockCursor(bool locked)
        {
            if (CurrentControlScheme == ControlScheme.Gamepad)
            {
                // If using gamepad, always lock the cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                return;
            }

            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
