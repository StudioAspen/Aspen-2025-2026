using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirMoveState : State<GameplayPlayerController>
    {
        [SerializeField] private float _maxFlatSpeed = 40f;
        [SerializeField] private float _maxVerticalSpeed = 20f;
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _deceleration = 10f;

        /// <summary>
        /// Sets the context's current substate to this state's type name when the state is entered.
        /// </summary>
        private protected override void OnEnter()
        {
            _context.CurrentSubState = GetType().Name;
        }

        /// <summary>
        /// Invoked when the state is exited; currently performs no teardown or side effects.
        /// </summary>
        private protected override void OnExit()
        {

        }

        private protected override void OnUpdate()
        {

        }

        private protected override void OnFixedUpdate()
        {
            Vector2 input = InputManager.Instance.MoveDirection;
            Vector3 inputDirection = _context.Orientation.right * input.x + _context.Orientation.forward * input.y;
            
            // Perform deceleration if we aren't pressing an input direction
            if (input == Vector2.zero)
            {
                _context.Rb.AddForce(-_context.Rb.linearVelocity.normalized * _deceleration, ForceMode.Acceleration);
            }
            
            _context.Rb.AddForce(_context.Orientation.forward * (input.y * _acceleration) + _context.Orientation.right * (input.x * _acceleration), ForceMode.Acceleration);
            
            Vector3 flatVel = new Vector3(_context.Rb.linearVelocity.x, 0, _context.Rb.linearVelocity.z);
            
            float dot = Vector3.Dot(inputDirection, flatVel.normalized);

            if (dot < 0f)
            {
                Vector3 decelerationForce = -flatVel.normalized * (_deceleration * Mathf.Abs(dot));
                _context.Rb.AddForce(decelerationForce, ForceMode.Acceleration);
            }
            
            // Clamp max velocity manually
            if (flatVel.magnitude > _maxFlatSpeed)
            {
                _context.Rb.linearVelocity = new Vector3(flatVel.normalized.x * _maxFlatSpeed, _context.Rb.linearVelocity.y, flatVel.normalized.z * _maxFlatSpeed);
            }

            if (_context.Rb.linearVelocity.y > _maxVerticalSpeed)
            {
                _context.Rb.linearVelocity = new Vector3(_context.Rb.linearVelocity.x, _maxVerticalSpeed, _context.Rb.linearVelocity.z);
            }
        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            return null;
        }
    }
}