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
        [SerializeField] private float _maxSpeed = 10f;
        [SerializeField] private float _acceleration = 2f;
        [SerializeField] private float _dampening = 0.25f;

        [Header("Lock Points")]
        [SerializeField] private float _minX = -10f;
        [SerializeField] private float _maxX = 10f;
        
        private void Awake()
        {
            _input = InputManager.Instance;
        }

        private void FixedUpdate()
        {
            float horizontalInput = _input.MoveDirection.x;

            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                // Move only along X axis
                Vector3 desiredDirection = horizontalInput > 0 ? Vector3.right : Vector3.left;

                // Propulsion torque based on speed
                float speedFactor = Mathf.Clamp01(_rigidBody.linearVelocity.magnitude / _maxSpeed);
                Vector3 propulsionTorque = _acceleration * Vector3.Cross(Vector3.up, desiredDirection) * (1f - speedFactor);

                _rigidBody.AddTorque(propulsionTorque, ForceMode.VelocityChange);

                // To prevent y-axis spin and any movement not on X
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

            // Enforce lock points
            Vector3 position = transform.position;
            if (position.x < _minX)
            {
                position.x = _minX;
                _rigidBody.linearVelocity = new Vector3(Mathf.Max(0, _rigidBody.linearVelocity.x), _rigidBody.linearVelocity.y, _rigidBody.linearVelocity.z);
            }
            else if (position.x > _maxX)
            {
                position.x = _maxX;
                _rigidBody.linearVelocity = new Vector3(Mathf.Min(0, _rigidBody.linearVelocity.x), _rigidBody.linearVelocity.y, _rigidBody.linearVelocity.z);
            }
            transform.position = position;

            // Constrain Z movement as well since we only want X
            Vector3 velocity = _rigidBody.linearVelocity;
            velocity.z = 0;
            _rigidBody.linearVelocity = velocity;
        }

        
    }
}
