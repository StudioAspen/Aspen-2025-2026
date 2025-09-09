using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedMoveState : State<PlayerController>
    {
        [SerializeField] private float acceleration = 1f;
        [SerializeField] private float turnResponsiveness = 1f;
        [SerializeField] private float maxSpeed = 25f;

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
            Vector3 desiredDirection = Utilities.GetCameraBasedMoveInput(
                CameraManager.Instance.CurrentCamera.transform, 
                _context.Input.MoveDirection
                );

            Vector3 currentDirection = _context.RigidBody.linearVelocity;
            currentDirection.y = 0;
            currentDirection.Normalize();

            float directionChangeFactor = 1f;
            if (_context.RigidBody.linearVelocity.sqrMagnitude > 0.01f)
            {
                float angle = Vector3.Angle(currentDirection, desiredDirection);
                directionChangeFactor = Mathf.InverseLerp(0f, 180f, angle); // Normalize angle (0 = no change, 180 = full change)
            }

            // Torque scales with how much you're turning
            Vector3 turnTorque = turnResponsiveness * directionChangeFactor * Vector3.Cross(Vector3.up, desiredDirection);

            // Propulsion torque based on speed for snappy direction changes at low speed
            float speedFactor = Mathf.Clamp01(_context.RigidBody.linearVelocity.magnitude / maxSpeed);
            Vector3 propulsionTorque = acceleration * Vector3.Cross(Vector3.up, desiredDirection) * (1f - speedFactor);

            Vector3 totalTorque = turnTorque + propulsionTorque;

            _context.RigidBody.AddTorque(totalTorque, ForceMode.VelocityChange);

            // To prevent y-axis spin
            _context.RigidBody.angularVelocity = Vector3.ProjectOnPlane(_context.RigidBody.angularVelocity, Vector3.up);

            // Cap final speed
            if (_context.RigidBody.linearVelocity.magnitude > maxSpeed)
                _context.RigidBody.linearVelocity = Vector3.ClampMagnitude(_context.RigidBody.linearVelocity, maxSpeed);

            _context.VisualObject.transform.rotation = _context.RigidBody.rotation;
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (_context.Input.MoveDirection == Vector2.zero)
                return _context.GroundedSuperState.IdleState;

            return null;
        }
    }
}
