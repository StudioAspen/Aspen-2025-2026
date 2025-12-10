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
            float dot;
            Vector3 moveDir;
            // Apply movement along slope if we are on a slope
            if (_context.SlopeSensor.IsOnSlope)
            {
                RaycastHit hit = _context.SlopeSensor.Hit;
                forwardOriented = Vector3.Cross(_context.Orientation.right, hit.normal).normalized;
                rightOriented = Vector3.Cross(hit.normal, forwardOriented).normalized;
                
                // Add additional force down a slope.
                float slopeFactor = Mathf.Clamp01(_context.SlopeSensor.CurrentSlopeAngle / _context.SlopeSensor.MaxSlopeAngle);
                bonusForce += Vector3.ProjectOnPlane(Vector3.down, _context.SlopeSensor.Hit.normal) * (SlopeBoost * slopeFactor);
                    
                // Ensure that the player is stuck on the slope
                _context.Rb.AddForce(-_context.SlopeSensor.Hit.normal * GroundStickForce, ForceMode.Acceleration);
                
                moveDir = Vector3.ProjectOnPlane(_context.Rb.linearVelocity, hit.normal).normalized;
                dot = Vector3.Dot(inputDirection, moveDir);
            }
            else
            {
                forwardOriented = _context.Orientation.forward;
                rightOriented = _context.Orientation.right;
                moveDir = _context.Rb.linearVelocity.normalized;
                dot = Vector3.Dot(inputDirection, moveDir);
            }
            
            // Deceleration force if the player is trying to move while on a slope
            if (dot < 0f)
            {
                Vector3 decelerationForce = -moveDir * (Deceleration * Mathf.Abs(dot));
                _context.Rb.AddForce(decelerationForce, ForceMode.Acceleration);
            }
            
            Vector3 inputForce = forwardOriented * (input.y * Acceleration) + rightOriented * (input.x * Acceleration);
            _context.Rb.AddForce(inputForce + bonusForce, ForceMode.Acceleration);
            
            // Clamp max velocity manually
            if (_context.Rb.linearVelocity.magnitude > MaxSpeed)
            {
                _context.Rb.linearVelocity = _context.Rb.linearVelocity.normalized * MaxSpeed;
            }
            
        }
        private void OnGameplayPlayerControllerColiderHit(ColliderHit hit)
        {
            // ColliderHit does not have a gameObject property, but it has a collider property.
            // Use collider.gameObject to access the GameObject
            switch (hit.collider.gameObject.tag)
            {
                case "SpeedBoost":
                    Debug.Log("SpeedBoost triggered"); // Fixed: replaced 'print()' with 'Debug.Log()'
                    float MaxSpeed = 25f;
                    break;
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
