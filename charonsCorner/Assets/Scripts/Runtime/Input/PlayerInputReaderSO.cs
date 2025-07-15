using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static CharonsCorner.Runtime.InputActions;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "CharonsCorner/Input/InputReader", order = 0)]
    public class PlayerInputReaderSO : ScriptableObject, IPlayerActions, IInputReader
    {
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<bool> Jump = delegate { };
        public event UnityAction<bool> Interact = delegate { };

        public InputActions InputActions { get; private set; }

        public Vector2 Direction => InputActions.Player.Move.ReadValue<Vector2>();
        public bool IsJumpKeyPressed => InputActions.Player.Jump.IsPressed();

        public void EnableActions()
        {
            if(InputActions == null)
            {
                InputActions = new InputActions();
                InputActions.Player.SetCallbacks(this);
            }
            InputActions.Enable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Move.Invoke(context.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            switch(context.phase)
            {
                case InputActionPhase.Started:
                    Jump.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Jump.Invoke(false);
                    break;
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
           
        }
    }
}
