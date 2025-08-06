using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static CharonsCorner.Runtime.InputActions;

namespace CharonsCorner.Runtime
{
    public class InputManager : MonoBehaviour, IPlayerActions, IUIActions
    {
        public static InputManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private PlayerInput playerInput; // We are using C# generated inputs, so this is just a dummy playerInput to detect controlScheme changes
        [SerializeField] private EventSystem eventSystem;

        public InputActions InputActions { get; private set; }

        // Player
        public event Action<Vector2> Move = delegate { };
        public event Action<Vector2> Look = delegate { };
        public event Action Jump = delegate { };
        /// <summary>
        /// Invoked when start/stop drifting. True for start, false for stop.
        /// </summary>
        public event Action<bool> Drift = delegate { };
        public event Action Interact = delegate { };

        public Vector2 LookDirection => InputActions.Player.Look.ReadValue<Vector2>();

        // UI
        public event Action Unpause = delegate { };

        #region Control Scheme
        public enum ControlScheme
        {
            KeyboardMouse,
            Gamepad
        }
        [field: Header("Control Scheme")]
        [field: SerializeField, ReadOnly] public ControlScheme CurrentControlScheme { get; private set; }
        /// <summary>
        /// Action that is invoked when the current control scheme is changed based on the input device used.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description><c>ControlScheme controlScheme</c>: The new controlScheme that was changed to.</description></item>
        /// </list>
        /// </remarks>
        public event Action<ControlScheme> OnControlSchemeChanged = delegate { };
        public static readonly Dictionary<ControlScheme, string> ControlSchemeInternalNames = new Dictionary<ControlScheme, string>
        {
            { ControlScheme.KeyboardMouse, "Keyboard&Mouse" },
            { ControlScheme.Gamepad, "Gamepad" }
        };
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

            playerInput.onControlsChanged += PlayerInput_OnControlsChanged;
        }

        private void OnDestroy()
        {
            playerInput.onControlsChanged -= PlayerInput_OnControlsChanged;
        }

        /// <summary>
        /// Creates a new InputActions instance and sets the callbacks for Player and UI actions.
        /// Needs to be done once in an entire game session.
        /// </summary>
        private void CreatePlayerActions()
        {
            InputActions = new InputActions();
            InputActions.Player.SetCallbacks(this);
            InputActions.UI.SetCallbacks(this);
        }

        /// <summary>
        /// Prevents all inputs from being processed.
        /// </summary>
        public void DisableAllActions()
        {
            if(InputActions == null)
                CreatePlayerActions();

            InputActions.Disable();
            eventSystem.sendNavigationEvents = false;
        }

        /// <summary>
        /// Enables the player actions and disables the UI actions.
        /// Locks the cursor accordingly.
        /// </summary>
        public void EnablePlayerActions()
        {
            if (InputActions == null)
                CreatePlayerActions();

            InputActions.Enable();
            InputActions.Player.Enable();
            InputActions.UI.Disable();
            eventSystem.sendNavigationEvents = false;

            LockCursor(true);
        }

        /// <summary>
        /// Enables the UI actions and disables the player actions.
        /// Unlocks the cursor accordingly.
        /// </summary>
        public void EnableUIActions()
        {
            if (InputActions == null)
                CreatePlayerActions();

            InputActions.Enable();
            InputActions.Player.Disable();
            InputActions.UI.Enable();
            eventSystem.sendNavigationEvents = true;

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
 
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
                Interact.Invoke();
        }

        public void OnDrift(InputAction.CallbackContext context)
        {
            if(context.started)
                Drift.Invoke(true); // Start drifting
            else if(context.canceled)
                Drift.Invoke(false); // Stop drifting
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed)
                GameManager.Instance.ChangeGameState(GameState.Paused);
        }

        // UI Actions
        public void OnUnpause(InputAction.CallbackContext context)
        {
            if (context.performed && GameManager.Instance.CurrentGameState == GameState.Paused)
                Unpause.Invoke();
        }

        public void OnNavigate(InputAction.CallbackContext context) { }
        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            GameObject selectedObject = eventSystem.currentSelectedGameObject;
            if (selectedObject == null)
                return;

            if (!UIManager.IsUIObjectInteractable(eventSystem, selectedObject))
                return;

            ExecuteEvents.Execute(selectedObject, new BaseEventData(eventSystem), ExecuteEvents.submitHandler);
        }
        public void OnCancel(InputAction.CallbackContext context) { }
        public void OnPoint(InputAction.CallbackContext context) { }
        public void OnClick(InputAction.CallbackContext context) { }
        public void OnRightClick(InputAction.CallbackContext context) { }
        public void OnMiddleClick(InputAction.CallbackContext context) { }
        public void OnScrollWheel(InputAction.CallbackContext context) { }
        public void OnTrackedDevicePosition(InputAction.CallbackContext context) { }
        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context) { }

        /// <summary>
        /// Detects whether the control scheme has changed based on the input device used.
        /// </summary>
        /// <param name="eventPtr"></param>
        /// <param name="device"></param>
        private void PlayerInput_OnControlsChanged(PlayerInput input)
        {
            ControlScheme newScheme = input.currentControlScheme == "Gamepad" ? ControlScheme.Gamepad : ControlScheme.KeyboardMouse;

            if (newScheme != CurrentControlScheme)
            {
                CurrentControlScheme = newScheme;
                OnControlSchemeChanged.Invoke(CurrentControlScheme);

                ApplyCursorLockState();
            }
        }

        /// <summary>
        /// Locks or unlocks the cursor based on the user's preference.
        /// Keeps track of the desired cursor lock state, which is used when switching between control schemes.
        /// Gamepad always locks the cursor.
        /// </summary>
        /// <param name="locked"></param>
        public void LockCursor(bool locked)
        {
            // Always remember the user's preference
            desiredCursorLockState = locked ? CursorLockMode.Locked : CursorLockMode.None;

            ApplyCursorLockState();
        }

        /// <summary>
        /// Helper method to apply the desired cursor lock state based on the current control scheme.
        /// </summary>
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
