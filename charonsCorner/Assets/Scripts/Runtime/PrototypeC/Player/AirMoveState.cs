using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirMoveState : State<PrototypePlayerController>
    {
        [field:SerializeField] public float MaxFlatSpeed {get; private set;} = 25f;
        [field:SerializeField] public float MaxVerticalSpeed {get; private set;} = 25f;
        [field:SerializeField] public float Acceleration {get; private set;} = 30f;
        [field:SerializeField] public float Deceleration {get; private set;} = 10f;

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
            
            // Perform deceleration if we aren't pressing an input direction
            if (input == Vector2.zero)
            {
                context.Rb.AddForce(-context.Rb.linearVelocity.normalized * Deceleration, ForceMode.Acceleration);
            }
            
            context.Rb.AddForce(context.Orientation.forward * (input.y * Acceleration) + context.Orientation.right * (input.x * Acceleration), ForceMode.Acceleration);

            // Clamp max velocity manually
            Vector3 flatVel = new Vector3(context.Rb.linearVelocity.x, 0, context.Rb.linearVelocity.z);
            
            if (flatVel.magnitude > MaxFlatSpeed)
            {
                context.Rb.linearVelocity = new Vector3(flatVel.normalized.x * MaxFlatSpeed, context.Rb.linearVelocity.y, flatVel.normalized.z * MaxFlatSpeed);
            }

            if (context.Rb.linearVelocity.y > MaxVerticalSpeed)
            {
                context.Rb.linearVelocity = new Vector3(context.Rb.linearVelocity.x, MaxVerticalSpeed, context.Rb.linearVelocity.z);
            }

        }

        private protected override State<PrototypePlayerController> GetTransition()
        {
            // if (context.Input.MoveDirection == Vector2.zero)
            //     return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}