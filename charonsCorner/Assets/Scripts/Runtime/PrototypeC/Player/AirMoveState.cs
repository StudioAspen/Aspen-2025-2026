using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirMoveState : State<PrototypePlayerController>
    {
        [field: SerializeField] public float Speed {get; private set;}

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
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            
            context.Rb.AddForce(context.Orientation.forward * (input.y * Speed) + context.Orientation.right * (input.x * Speed), ForceMode.VelocityChange);
        }

        private protected override State<PrototypePlayerController> GetTransition()
        {
            // if (context.Input.MoveDirection == Vector2.zero)
            //     return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}