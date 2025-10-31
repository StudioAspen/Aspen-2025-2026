using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(GameplayPlayerController))]
    public class JumpHandler : MonoBehaviour
    {
        private GameplayPlayerController player;
        [SerializeField] private float _jumpForce = 10f;
        void Awake()
        {
            player = GetComponent<GameplayPlayerController>();
            
        }

        void OnEnable()
        {
            InputManager.Instance.Jump += OnJump;
        }
        void OnDisable()
        {
            InputManager.Instance.Jump -= OnJump;
        }

        private void OnJump()
        {
            
            if (player.StateMachine.CurrentState == player.GroundSuperState)
            {
                
                Jump();
            }
        }

        private void Jump()
        {
            Debug.Log("Jump");
            player.Rb.linearVelocity = new Vector3(player.Rb.linearVelocity.x, 0f, player.Rb.linearVelocity.z);
            player.Rb.AddForce(player.Orientation.up * _jumpForce, ForceMode.VelocityChange);
        }
        
    }
}
