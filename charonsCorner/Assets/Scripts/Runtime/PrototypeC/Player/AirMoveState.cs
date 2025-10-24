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
            Vector3 inputDirection = _context.Orientation.right * input.x + _context.Orientation.forward * input.y;
            
            // Perform deceleration if we aren't pressing an input direction
            if (input == Vector2.zero)
            {
                _context.Rb.AddForce(-_context.Rb.linearVelocity.normalized * Deceleration, ForceMode.Acceleration);
            }
            
            _context.Rb.AddForce(_context.Orientation.forward * (input.y * Acceleration) + _context.Orientation.right * (input.x * Acceleration), ForceMode.Acceleration);

            
            
            
            
            
            Vector3 flatVel = new Vector3(_context.Rb.linearVelocity.x, 0, _context.Rb.linearVelocity.z);
            
            float dot = Vector3.Dot(inputDirection, flatVel.normalized);

            if (dot < 0f)
            {
                Vector3 decelerationForce = -flatVel.normalized * (Deceleration * Mathf.Abs(dot));
                _context.Rb.AddForce(decelerationForce, ForceMode.Acceleration);
            }
            
            
            // Clamp max velocity manually
            if (flatVel.magnitude > MaxFlatSpeed)
            {
                _context.Rb.linearVelocity = new Vector3(flatVel.normalized.x * MaxFlatSpeed, _context.Rb.linearVelocity.y, flatVel.normalized.z * MaxFlatSpeed);
            }

            if (_context.Rb.linearVelocity.y > MaxVerticalSpeed)
            {
                _context.Rb.linearVelocity = new Vector3(_context.Rb.linearVelocity.x, MaxVerticalSpeed, _context.Rb.linearVelocity.z);
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