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

            // Get ground normal via raycast
            Vector3 groundNormal = Vector3.up;
            RaycastHit hit;
            if (Physics.Raycast(context.transform.position, Vector3.down, out hit, 2f, context.GroundedSuperState.GroundLayerMask))
            {
                groundNormal = hit.normal;

                // Only apply ground stick if NOT in water
                if (context.IsGrounded && !context.IsInWater)
                {
                    Vector3 velocity = context.RigidBody.linearVelocity;
                    float velocityAway = Vector3.Dot(velocity, groundNormal);

                    if (velocityAway > 0f)
                    {
                        float stickStrength = stickToGroundForce * Mathf.Clamp01(velocityAway);
                        context.RigidBody.AddForce(-groundNormal * stickStrength, ForceMode.VelocityChange);
                    }
                }
            }

            // Project movement and velocity onto ground plane
            Vector3 projectedDesiredDirection = Vector3.ProjectOnPlane(desiredDirection, groundNormal).normalized;
            Vector3 velocityProj = context.RigidBody.linearVelocity;
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(velocityProj, groundNormal);

            float speed = projectedVelocity.magnitude;
            Vector3 currentDirection = speed > 0.01f ? projectedVelocity.normalized : projectedDesiredDirection;

            // Turn Responsiveness
            float angleToDesired = Vector3.SignedAngle(currentDirection, projectedDesiredDirection, groundNormal);
            float turnAmount = Mathf.Clamp(angleToDesired * TurnResponsiveness * 0.01f, -1f, 1f);
            Vector3 turnTorque = groundNormal * turnAmount * Acceleration;
            context.RigidBody.AddTorque(turnTorque, ForceMode.VelocityChange);

            // Movement logic (projected)
            float alignmentDirection = Vector3.Dot(currentDirection, projectedDesiredDirection);
            float speedFactor = Mathf.Clamp01(speed / MaxSpeed);
            Vector3 pushTorque = Acceleration * Vector3.Cross(groundNormal, projectedDesiredDirection) * (1f - speedFactor);
            Vector3 totalTorque = pushTorque; // turnTorque already applied above

            bool isReversing = alignmentDirection < -0.1f && speed > 0.1f;

            if (isReversing)
            {
                float brakeStrength = Acceleration * 0.5f;
                Vector3 brakeForce = -projectedVelocity * brakeStrength * Time.fixedDeltaTime;
                context.RigidBody.AddForce(brakeForce, ForceMode.VelocityChange);
                projectedVelocity = Vector3.Lerp(projectedVelocity, Vector3.zero, 0.04f);
            }

            float angle = Vector3.Angle(currentDirection, projectedDesiredDirection);
            float snapFactor = Mathf.Lerp(0.02f, 0.12f, Mathf.InverseLerp(0f, 60f, angle));
            snapFactor *= Mathf.Lerp(0.7f, 0.3f, speed / MaxSpeed);

            Vector3 targetVelocity = projectedDesiredDirection * speed;
            projectedVelocity = Vector3.Lerp(projectedVelocity, targetVelocity, snapFactor);

            context.RigidBody.AddTorque(totalTorque, ForceMode.VelocityChange);

            // Apply the projected velocity, but preserve any vertical velocity
            float verticalVelocity = Vector3.Dot(velocityProj, groundNormal);
            context.RigidBody.linearVelocity = projectedVelocity + groundNormal * verticalVelocity;

            context.RigidBody.angularVelocity = Vector3.ProjectOnPlane(context.RigidBody.angularVelocity, groundNormal);

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