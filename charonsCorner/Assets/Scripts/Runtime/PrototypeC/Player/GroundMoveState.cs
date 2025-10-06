using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundMoveState : State<PrototypePlayerController>
    {
        [field: SerializeField] public float Acceleration { get; private set; } = 1f;
        [field: SerializeField] public float TurnResponsiveness { get; private set; } = 1f;
        [field: SerializeField] public float MaxSpeed { get; private set; } = 25f;

        private protected override void OnEnter()
        {
            
        }

        private protected override void OnExit()
        {

        }

        private protected override void OnUpdate()
        {

        }

        private protected override void OnFixedUpdate()
        {
                        
            context.Rb.linearDamping = context.IsGrounded() ? context.GroundDrag : 0;
            
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            
            context.Rb.AddForce(context.Orientation.forward * (input.y * context.Speed) + context.Orientation.right * (input.x * context.Speed), ForceMode.VelocityChange);
        }

        private protected override State<PrototypePlayerController> GetTransition()
        {
            // if (context.Input.MoveDirection == Vector2.zero)
            //     return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}
