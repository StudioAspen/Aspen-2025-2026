using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundMoveState : State<PrototypePlayerController>
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

            Vector3 forwardOriented;
            Vector3 rightOriented;
            if (context.SlopeSensor.IsOnSlope)
            {
                RaycastHit hit = context.SlopeSensor.Hit;
                forwardOriented = Vector3.Cross(context.Orientation.right, hit.normal).normalized;
                rightOriented = Vector3.Cross(hit.normal, forwardOriented).normalized;
                
            }
            else
            {
                forwardOriented = context.Orientation.forward;
                rightOriented = context.Orientation.right;
            }
            context.Rb.AddForce(forwardOriented * (input.y * Speed) + rightOriented * (input.x * Speed), ForceMode.VelocityChange);
        }

        private protected override State<PrototypePlayerController> GetTransition()
        {
            // if (context.Input.MoveDirection == Vector2.zero)
            //     return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}
