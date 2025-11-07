using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(GameplayPlayerController))]
    public class JumpHandler : MonoBehaviour
    {
        private GameplayPlayerController player;
        [SerializeField] private float _jumpForce = 10f;
        /// <summary>
        /// Caches the GameplayPlayerController component attached to the same GameObject.
        /// </summary>
        void Awake()
        {
            player = GetComponent<GameplayPlayerController>();
            
        }

        /// <summary>
        /// Subscribes the local OnJump handler to the global Jump event on the InputManager so jump input triggers this component.
        /// </summary>
        void OnEnable()
        {
            InputManager.Instance.Jump += OnJump;
        }
        /// <summary>
        /// Unsubscribes the <c>OnJump</c> handler from the input manager's Jump event when this component is disabled.
        /// </summary>
        void OnDisable()
        {
            InputManager.Instance.Jump -= OnJump;
        }

        /// <summary>
        /// Handles jump input and initiates a jump when the player's state machine is in the GroundSuperState.
        /// </summary>
        private void OnJump()
        {
            
            if (player.StateMachine.CurrentState == player.GroundSuperState)
            {
                
                Jump();
            }
        }

        /// <summary>
        /// Performs the jump action for the cached player: resets vertical velocity and applies an upward velocity change.
        /// </summary>
        /// <remarks>
        /// Preserves the player's horizontal velocity components, sets the vertical component to zero, then applies an upward force using the player's orientation multiplied by the configured jump force.
        /// </remarks>
        private void Jump()
        {
            Debug.Log("Jump");
            player.Rb.linearVelocity = new Vector3(player.Rb.linearVelocity.x, 0f, player.Rb.linearVelocity.z);
            player.Rb.AddForce(player.Orientation.up * _jumpForce, ForceMode.VelocityChange);
        }
        
    }
}