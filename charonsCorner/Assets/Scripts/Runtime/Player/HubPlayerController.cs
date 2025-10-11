using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Hub specific player controller with simple movement implementation and removed extra behavior.
    /// </summary>
    public class HubPlayerController : MonoBehaviour
    {
        private InputManager _input;

        [Header("References")]
        [SerializeField] private Rigidbody _rigidBody;
        [SerializeField] private SphereCollider _sphereCollider;

        [Header("Movement Attributes")]
        [SerializeField] private float _turnResponsiveness = 1f;
        [SerializeField] private float _maxSpeed = 25f;
        [SerializeField] private float _acceleration = 1f;
        [SerializeField] private float _dampening = 1f;

        private void Awake()
        {
            _input = InputManager.Instance;
        }

        private void FixedUpdate()
        {
            if (_input.MoveDirection != Vector2.zero)
            {
                Vector3 desiredDirection = Utilities.GetCameraBasedMoveInput(
                CameraManager.Instance.CurrentCamera.transform,
                _input.MoveDirection
                );

                Vector3 currentDirection = _rigidBody.linearVelocity;
                currentDirection.y = 0;
                currentDirection.Normalize();

                float directionChangeFactor = 1f;
                if (_rigidBody.linearVelocity.sqrMagnitude > 0.01f)
                {
                    float angle = Vector3.Angle(currentDirection, desiredDirection);
                    directionChangeFactor = Mathf.InverseLerp(0f, 180f, angle); // Normalize angle (0 = no change, 180 = full change)
                }

                // Torque scales with how much you're turning
                Vector3 turnTorque = _turnResponsiveness * directionChangeFactor * Vector3.Cross(Vector3.up, desiredDirection);

                // Propulsion torque based on speed for snappy direction changes at low speed
                float speedFactor = Mathf.Clamp01(_rigidBody.linearVelocity.magnitude / _maxSpeed);
                Vector3 propulsionTorque = _acceleration * Vector3.Cross(Vector3.up, desiredDirection) * (1f - speedFactor);

                Vector3 totalTorque = turnTorque + propulsionTorque;

                _rigidBody.AddTorque(totalTorque, ForceMode.VelocityChange);

                // To prevent y-axis spin
                _rigidBody.angularVelocity = Vector3.ProjectOnPlane(_rigidBody.angularVelocity, Vector3.up);

                // Cap final speed
                if (_rigidBody.linearVelocity.magnitude > _maxSpeed)
                    _rigidBody.linearVelocity = Vector3.ClampMagnitude(_rigidBody.linearVelocity, _maxSpeed);
            }
            else
            {
                Vector3 torque = _dampening * -_rigidBody.angularVelocity;
                _rigidBody.AddTorque(torque, ForceMode.VelocityChange);
            }
        }

        
    }
}
