using UnityEngine;
using UnityEngine.Tilemaps;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundMoveState : State<GameplayPlayerController>
    {
        [Header("Speed Settings")]
        [field:SerializeField] public float MaxSpeed {get; private set;} = 25f;
        [field:SerializeField] public float Acceleration {get; private set;} = 30f;
        [field:SerializeField] public float Deceleration {get; private set;} = 10f;
        [field:SerializeField] public float SlopeBoost {get; private set;} = 10f; // optional slope influence
        [field:SerializeField] public float GroundStickForce {get; private set;} = 10f;

        private protected override void OnEnter()
        {
            if (_context.CannonAir)
            {
                Vector3 forwardDirection = _context.Orientation.forward.normalized;
                _context.Rb.linearVelocity = forwardDirection * MaxSpeed;

                _context.SetCannonAir(false);
            }
            _context.CurrentSubState = GetType().Name;
        }

        private protected override void OnExit()
        {
            
        }

        private protected override void OnUpdate()
        {
            
        }

        private protected override void OnFixedUpdate()
        {
            _context.ApplyGravity();

            Vector2 input = InputManager.Instance.MoveDirection;
            Vector3 inputDirection = _context.Orientation.right * input.x + _context.Orientation.forward * input.y;
            
            // Perform deceleration if we aren't pressing an input direction
            if (input == Vector2.zero)
            {
                _context.Rb.AddForce(-_context.Rb.linearVelocity.normalized * Deceleration, ForceMode.Acceleration);
            }
            
            Vector3 forwardOriented;
            Vector3 rightOriented;
            Vector3 bonusForce = Vector3.zero;
            // Apply movement along slope if we are on a slope
            if (_context.SlopeSensor.IsOnSlope)
            {
                RaycastHit hit = _context.SlopeSensor.Hit;
                forwardOriented = Vector3.Cross(_context.Orientation.right, hit.normal).normalized;
                rightOriented = Vector3.Cross(hit.normal, forwardOriented).normalized;
                
                // Downhill direction along the slope plane
                Vector3 downhillDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
                
                // Decide if we should get boosted by the slope.
                // Use inputDirection when there is input, otherwise use current planar velocity direction.
                Vector3 planarVel = Vector3.ProjectOnPlane(_context.Rb.linearVelocity, hit.normal);
                Vector3 intentDir = (input != Vector2.zero)
                    ? Vector3.ProjectOnPlane(inputDirection, hit.normal).normalized
                    : (planarVel.sqrMagnitude > 0.0001f ? planarVel.normalized : Vector3.zero);

                // +1 = going downhill, -1 = going uphill
                float downhillDot = (intentDir == Vector3.zero) ? 0f : Vector3.Dot(intentDir, downhillDir);

                // Only add slope boost when moving/intent is downhill (or at least not uphill)
                if (downhillDot > 0f)
                {
                    float slopeFactor = Mathf.Clamp01(
                        _context.SlopeSensor.CurrentSlopeAngle /
                        _context.SlopeSensor.MaxSlopeAngle
                    );
                    bonusForce += downhillDir * (SlopeBoost * slopeFactor * downhillDot);
                }
                
                _context.Rb.AddForce(-hit.normal * GroundStickForce, ForceMode.Acceleration); // ground stick
            }
            else
            {
                forwardOriented = _context.Orientation.forward;
                rightOriented = _context.Orientation.right;
            }
            
            Vector3 inputForce = forwardOriented * (input.y * Acceleration) + rightOriented * (input.x * Acceleration);
            _context.Rb.AddForce(inputForce + bonusForce, ForceMode.Acceleration);
            
            // Clamp max velocity manually
            if (_context.Rb.linearVelocity.magnitude > MaxSpeed)
            {
                _context.Rb.linearVelocity = _context.Rb.linearVelocity.normalized * MaxSpeed;
            }
        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            // if (context.Input.MoveDirection == Vector2.zero)
            //     return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}
