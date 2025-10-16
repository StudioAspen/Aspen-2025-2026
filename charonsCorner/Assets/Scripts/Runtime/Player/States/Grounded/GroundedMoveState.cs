using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedMoveState : State<PlayerController>
    {
        [field: SerializeField] public float Acceleration { get; private set; } = 15f;
        [field: SerializeField] public float TurnResponsiveness { get; private set; } = 1f;
        [field: SerializeField] public float MaxSpeed { get; private set; } = 25f;
        [field: SerializeField] public float stickToGroundForce { get; private set; } = 20f; // Added for slope stickiness

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
                context.Input.MoveDirection
            );

            Vector3 velocity = context.RigidBody.linearVelocity;
            Vector3 flatVelocity = velocity;
            flatVelocity.y = 0f;
            float speed = flatVelocity.magnitude;

            Vector3 currentDirection = speed > 0.01f ? flatVelocity.normalized : desiredDirection;
            float alignmentDirection = Vector3.Dot(currentDirection, desiredDirection);

            float directionChangeFactor = Mathf.InverseLerp(0f, 180f, Vector3.Angle(currentDirection, desiredDirection));

            Vector3 slopeProjection = Vector3.ProjectOnPlane(Physics.gravity, Vector3.up).normalized;
            float slopeDirection = Vector3.Dot(desiredDirection.normalized, -slopeProjection);

            Vector3 turnTorque = TurnResponsiveness * directionChangeFactor * Vector3.Cross(Vector3.up, desiredDirection);
            float speedFactor = Mathf.Clamp01(speed / MaxSpeed);
            Vector3 pushTorque = Acceleration * Vector3.Cross(Vector3.up, desiredDirection) * (1f - speedFactor);
            Vector3 totalTorque = turnTorque + pushTorque;

            bool isReversing = alignmentDirection < -0.1f && speed > 0.1f;
            bool onSlope = slopeDirection > 0.1f;
            bool hillBoost = speed < 0.5f && onSlope;

            if (hillBoost)
            {
                Vector3 assistTorque = Vector3.Cross(Vector3.up, desiredDirection.normalized) * Acceleration;
                totalTorque += assistTorque;
                context.RigidBody.AddForce(desiredDirection.normalized * Acceleration * 0.3f, ForceMode.Acceleration);
            }

            if (isReversing)
            {
                float brakeStrength = Acceleration * 1.0f;
                Vector3 brakeForce = -flatVelocity.normalized * brakeStrength;
                context.RigidBody.AddForce(brakeForce, ForceMode.Acceleration);
                flatVelocity = Vector3.Lerp(flatVelocity, Vector3.zero, 0.1f);
            }

            float angle = Vector3.Angle(currentDirection, desiredDirection);
            float snapFactor = Mathf.Lerp(0.02f, 0.12f, Mathf.InverseLerp(0f, 60f, angle));
            snapFactor *= Mathf.Lerp(0.7f, 0.3f, speed / MaxSpeed);

            Vector3 targetVelocity = desiredDirection.normalized * speed;
            flatVelocity = Vector3.Lerp(flatVelocity, targetVelocity, snapFactor);

            context.RigidBody.AddTorque(totalTorque, ForceMode.VelocityChange);

            // Stick to ground on slopes (only if grounded)
            if (context.IsGrounded)
            {
                context.RigidBody.AddForce(Vector3.down * stickToGroundForce, ForceMode.Acceleration);
            }

            context.RigidBody.linearVelocity = new Vector3(flatVelocity.x, velocity.y, flatVelocity.z);
            context.RigidBody.angularVelocity = Vector3.ProjectOnPlane(context.RigidBody.angularVelocity, Vector3.up);

            if (context.RigidBody.linearVelocity.magnitude > MaxSpeed)
                context.RigidBody.linearVelocity = Vector3.ClampMagnitude(context.RigidBody.linearVelocity, MaxSpeed);
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.Input.MoveDirection == Vector2.zero)
                return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}