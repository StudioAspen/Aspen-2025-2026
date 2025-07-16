using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Windows;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        private InputManager input;

        private Rigidbody rigidBody;
        private Transform mainCamera;

        private Vector3 moveInput;
        private Vector3 cameraBasedMoveInput;

        [Header("Roll Config")]
        [SerializeField] private float rollAcceleration = 1f;
        [SerializeField] private float turnResponsiveness = 1f;
        [SerializeField] private float airRollDamp = 0.02f;
        [SerializeField] private float maxSpeed = 25f;
        [SerializeField, ReadOnly] private float currentSpeed;
        [SerializeField, ReadOnly] private float currentRotationalSpeed;

        [Header("Jump Config")]
        [SerializeField] private float jumpHeight = 2f;

        [Header("Ground Check")]
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private float groundCheckRadius = 0.9f;
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField, ReadOnly] private bool isGrounded;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            mainCamera = Camera.main.transform;

            input = InputManager.Instance;
            input.Move += direction => moveInput = new Vector3(direction.x, 0, direction.y);
            input.Jump += isJumpKeyPressed =>
            {
                if (isJumpKeyPressed)
                    Jump();
            };

            input.EnablePlayerActions();
        }

        private void OnDrawGizmos()
        {
            // Draws the ground check sphere in the editor for visualization
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + groundCheckDistance * Vector3.down, groundCheckRadius);
        }

        private void Update()
        {
            CheckGrounded();

            currentSpeed = rigidBody.linearVelocity.magnitude;
            currentRotationalSpeed = rigidBody.angularVelocity.magnitude;
        }

        private void FixedUpdate()
        {
            HandleRoll();
        }

        private void CheckGrounded()
        {
            isGrounded = Physics.CheckSphere(transform.position + groundCheckDistance * Vector3.down, groundCheckRadius, groundLayerMask);
        }

        private void Jump()
        {
            if (!isGrounded)
                return;

            float jumpForce = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics.gravity.y));
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }

        private void HandleRoll()
        {
            cameraBasedMoveInput = GetCameraBasedMoveInput();

            Vector3 desiredDirection = cameraBasedMoveInput.normalized;

            Vector3 currentDirection = rigidBody.linearVelocity;
            currentDirection.y = 0;
            currentDirection.Normalize();

            float directionChangeFactor = 1f;
            if (rigidBody.linearVelocity.sqrMagnitude > 0.01f)
            {
                float angle = Vector3.Angle(currentDirection, desiredDirection);
                directionChangeFactor = Mathf.InverseLerp(0f, 180f, angle); // Normalize angle (0° = no change, 180° = full change)
            }

            // Torque scales with how much you’re turning
            Vector3 turnTorque = turnResponsiveness * directionChangeFactor * Vector3.Cross(Vector3.up, desiredDirection);

            // Propulsion torque based on speed for snappy direction changes at low speed
            float speedFactor = Mathf.Clamp01(rigidBody.linearVelocity.magnitude / maxSpeed);
            Vector3 propulsionTorque = rollAcceleration * Vector3.Cross(Vector3.up, desiredDirection) * (1f - speedFactor);

            Vector3 totalTorque = turnTorque + propulsionTorque;

            if (moveInput == Vector3.zero)
                totalTorque = Vector3.zero;

            if (!isGrounded)
                totalTorque = airRollDamp * -rigidBody.angularVelocity;

            rigidBody.AddTorque(totalTorque, ForceMode.VelocityChange);

            // To prevent y-axis spin
            rigidBody.angularVelocity = Vector3.ProjectOnPlane(rigidBody.angularVelocity, Vector3.up);

            // Cap final speed
            if (rigidBody.linearVelocity.magnitude > maxSpeed)
                rigidBody.linearVelocity = Vector3.ClampMagnitude(rigidBody.linearVelocity, maxSpeed);
        }

        private Vector3 GetCameraBasedMoveInput()
        {
            Vector3 forward = mainCamera.forward;
            Vector3 right = mainCamera.right;

            forward.y = 0; // Ignore vertical component
            right.y = 0; // Ignore vertical component

            forward.Normalize();
            right.Normalize();

            return (forward * moveInput.z + right * moveInput.x).normalized;
        }

        /// <summary>
        /// Multiplies the player's roll speed by a given multiplier, maintaining direction.
        /// </summary>
        /// <param name="multiplier"></param>
        public void MultiplyRollSpeed(float multiplier)
        {
            // Project current angular velocity onto XZ plane to ensure no Y spin
            Vector3 angularVel = Vector3.ProjectOnPlane(rigidBody.angularVelocity, Vector3.up);
            rigidBody.angularVelocity = multiplier * angularVel.magnitude * angularVel.normalized;
        }

        private void OnCollisionEnter(Collision collision)
        {
            float impactForce = collision.impulse.magnitude;
            float shakeAmplitude = Mathf.Lerp(0f, 4f, Mathf.Pow(impactForce / 20f, 2));
            float shakeDuration = Mathf.Lerp(0f, 0.4f, impactForce / 5f);

            CameraShakeManager.Instance.ShakeCamera(shakeAmplitude, 10f, shakeDuration);
        }
    }
}
