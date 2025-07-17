using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using static CharonsCorner.Runtime.InputActions;

namespace CharonsCorner.Runtime
{
    public class InputManager : MonoBehaviour, IPlayerActions, IUIActions
    {
        public static InputManager Instance { get; private set; }

        public InputActions InputActions { get; private set; }

        // Player
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2> Look = delegate { };
        public event UnityAction Jump = delegate { };

        public Vector2 MoveDirection => InputActions.Player.Move.ReadValue<Vector2>();
        public Vector2 LookDirection => InputActions.Player.Look.ReadValue<Vector2>();
        public bool IsJumpKeyPressed => InputActions.Player.Jump.IsPressed();

        // UI
        public event UnityAction Unpause = delegate { };

        #region Control Scheme
        public enum ControlScheme
        {
            KeyboardMouse,
            Gamepad
        }
        [field: SerializeField, ReadOnly] public ControlScheme CurrentControlScheme { get; private set; }
        public event Action<ControlScheme> OnControlSchemeChanged = delegate { };
        #endregion

        private CursorLockMode desiredCursorLockState;

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CreatePlayerActions();

            InputSystem.onEvent += InputSystem_OnEvent;
        }

        private void OnDestroy()
        {
            InputSystem.onEvent -= InputSystem_OnEvent;
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
            InputActions.Player.Enable();
            InputActions.UI.Disable();

            LockCursor(true);
        }

        public void EnableUIActions()
        {
            if (InputActions == null)
                CreatePlayerActions();

            InputActions.Enable();
            InputActions.Player.Disable();
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
            if (context.performed)
                Jump.Invoke();
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed)
                GameManager.Instance.ChangeGameState(GameState.Paused);
        }
 
        public void OnInteract(InputAction.CallbackContext context) { }

        // UI Actions
        public void OnUnpause(InputAction.CallbackContext context)
        {
            if (context.performed && GameManager.Instance.CurrentGameState == GameState.Paused)
                Unpause.Invoke();
        }

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
        private void InputSystem_OnEvent(InputEventPtr eventPtr, InputDevice device)
        {
            ControlScheme newScheme = device is Gamepad ? ControlScheme.Gamepad : ControlScheme.KeyboardMouse;

            if (newScheme != CurrentControlScheme)
            {
                Debug.Log(device.name + " detected, switching to " + newScheme.ToString() + " control scheme.");
                CurrentControlScheme = newScheme;
                OnControlSchemeChanged.Invoke(CurrentControlScheme);

                ApplyCursorLockState();
            }
        }

        // Cursor
        public void LockCursor(bool locked)
        {
            // Always remember the user's preference
            desiredCursorLockState = locked ? CursorLockMode.Locked : CursorLockMode.None;

            ApplyCursorLockState();
        }

        private void ApplyCursorLockState()
        {
            if (CurrentControlScheme == ControlScheme.Gamepad)
            {
                // Gamepad always locks
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                return;
            }

            Cursor.lockState = desiredCursorLockState;
            Cursor.visible = desiredCursorLockState != CursorLockMode.Locked;
        }
    }
}
