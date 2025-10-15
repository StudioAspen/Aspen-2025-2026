using UnityEngine;
using UnityEngine.Tilemaps;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundMoveState : State<PrototypePlayerController>
    {
        [Header("Speed Settings")]
        [field:SerializeField] public float MaxSpeed {get; private set;} = 25f;
        [field:SerializeField] public float Acceleration {get; private set;} = 30f;
        [field:SerializeField] public float Deceleration {get; private set;} = 10f;
        [field:SerializeField] public float SlopeBoost {get; private set;} = 10f; // optional slope influence
        [field:SerializeField] public float GroundStickForce {get; private set;} = 10f;
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
            
            Vector3 forwardOriented;
            Vector3 rightOriented;
            Vector3 bonusForce = Vector3.zero;
            // Apply movement along slope if we are on a slope
            if (context.SlopeSensor.IsOnSlope)
            {
                RaycastHit hit = context.SlopeSensor.Hit;
                forwardOriented = Vector3.Cross(context.Orientation.right, hit.normal).normalized;
                rightOriented = Vector3.Cross(hit.normal, forwardOriented).normalized;
                
                // Add additional force down a slope.
                float slopeFactor = Mathf.Clamp01(context.SlopeSensor.CurrentSlopeAngle / context.SlopeSensor.MaxSlopeAngle);
                bonusForce += Vector3.ProjectOnPlane(Vector3.down, context.SlopeSensor.Hit.normal) * (SlopeBoost * slopeFactor);
                
                // Ensure that the player is stuck on the slope
                context.Rb.AddForce(-context.SlopeSensor.Hit.normal * GroundStickForce, ForceMode.Acceleration);
                
            }
            else
            {
                forwardOriented = context.Orientation.forward;
                rightOriented = context.Orientation.right;
            }

            Vector3 inputForce = forwardOriented * (input.y * Acceleration) + rightOriented * (input.x * Acceleration);
            context.Rb.AddForce(inputForce + bonusForce, ForceMode.Acceleration);
            

            
            // Clamp max velocity manually
            if (context.Rb.linearVelocity.magnitude > MaxSpeed)
            {
                context.Rb.linearVelocity = context.Rb.linearVelocity.normalized * MaxSpeed;
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
