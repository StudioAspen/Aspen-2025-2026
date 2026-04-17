using UnityEngine;
using UnityEngine.Tilemaps;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundMoveState : State<GameplayPlayerController>
    {
        [Header("Speed Settings")]
        [field:SerializeField] public float MaxSpeed { get; private set; } = 25f;
        [field:SerializeField] public float Acceleration { get; private set; } = 30f;
        [field:SerializeField] public float Deceleration { get; private set; } = 10f;
        [field:SerializeField] public float SlopeBoost { get; private set; } = 10f; // optional slope influence
        [field:SerializeField] public float GroundStickForce { get; private set; } = 10f;

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

            bool isOnSlope = _context.SlopeSensor.IsOnSlope;
            // Apply movement along slope if we are on a slope
            if (isOnSlope)
            {
                RaycastHit hit = _context.SlopeSensor.Hit;
                
                forwardOriented = Vector3.ProjectOnPlane(_context.Orientation.forward, hit.normal).normalized;
                rightOriented = Vector3.ProjectOnPlane(_context.Orientation.right, hit.normal).normalized;
                
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
                    // Do not apply slope boost if the player has just jumped
                    if (_context.JumpHandler == null || !_context.JumpHandler.HasJumped)
                    {
                        float slopeFactor = Mathf.Clamp01(
                            _context.SlopeSensor.CurrentSlopeAngle /
                            _context.SlopeSensor.MaxSlopeAngle
                        );
                        bonusForce += downhillDir * (SlopeBoost * slopeFactor * downhillDot);
                    }
                }
                else if (_context.JumpHandler == null || !_context.JumpHandler.HasJumped)
                {
                    // might need to cancel gravity?
                    _context.Rb.AddForce(Vector3.up * _context.Gravity, ForceMode.Acceleration);
                }
                
                if (_context.JumpHandler == null || !_context.JumpHandler.HasJumped)
                {
                    _context.Rb.AddForce(-hit.normal * GroundStickForce, ForceMode.Acceleration); // ground stick
                }
            }
            else
            {
                forwardOriented = _context.Orientation.forward;
                rightOriented = _context.Orientation.right;
            }
            
            Vector3 inputForce =
                forwardOriented * (input.y * Acceleration) +
                rightOriented * (input.x * Acceleration);

            Debug.DrawLine(_context.transform.position,
                _context.transform.position + 100f * inputForce,
                Color.red, 0.25f);

            if (_context.JumpHandler == null || !_context.JumpHandler.HasJumped)
            {
                _context.Rb.AddForce(inputForce + bonusForce, ForceMode.Acceleration);
            }
            else
            {
                // If jumping, we only apply the input force (horizontal movement) to avoid slope-related vertical boosts
                // The vertical jump force was already applied in JumpHandler.Jump()
                _context.Rb.AddForce(inputForce, ForceMode.Acceleration);
            }

            // Clamp max velocity along ground (slope plane) so uphill doesn't slow you
            Vector3 vel = _context.Rb.linearVelocity;

            // If we've just jumped, don't clamp the vertical component of the velocity using the ground speed limit
            if (_context.JumpHandler != null && _context.JumpHandler.HasJumped && vel.y > 0.01f)
            {
                // We let the vertical velocity pass through to AirMoveState or be handled by gravity/vertical clamping
            }
            else if (isOnSlope)
            {
                Vector3 n = _context.SlopeSensor.Hit.normal;

                Vector3 tangentVel = Vector3.ProjectOnPlane(vel, n);
                float tangentSpeed = tangentVel.magnitude;

                if (tangentSpeed > MaxSpeed)
                {
                    Vector3 normalVel = vel - tangentVel;
                    Vector3 clampedTangent = tangentVel.normalized * MaxSpeed;
                    _context.Rb.linearVelocity = clampedTangent + normalVel;
                }
            }
            else
            {
                float speed = vel.magnitude;
                if (speed > MaxSpeed)
                    _context.Rb.linearVelocity = vel.normalized * MaxSpeed;
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
