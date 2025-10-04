using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedMoveState : State<PlayerController>
    {
        [field: SerializeField] public float Acceleration { get; private set; } = 1f;
        [field: SerializeField] public float TurnResponsiveness { get; private set; } = 1f;
        [field: SerializeField] public float MaxSpeed { get; private set; } = 25f;

        // Temporary speed cap for boost
        public float TemporaryMaxSpeed { get; private set; } = 0f;
        private float temporaryMaxSpeedTimer = 0f;
        private const float TemporaryMaxSpeedDuration = 0.75f; // seconds, tweak as needed

        public void ActivateTemporaryMaxSpeed(float speed)
        {
            TemporaryMaxSpeed = speed;
            temporaryMaxSpeedTimer = TemporaryMaxSpeedDuration;
        }

        private protected override void OnEnter()
        {
            // Not resetting TemporaryMaxSpeed here to allow continuity
        }

        private protected override void OnExit()
        {
            
        }

        private protected override void OnUpdate()
        {
            // Handle timer for temporary max speed
            if (TemporaryMaxSpeed > 0f)
            {
                temporaryMaxSpeedTimer -= Time.deltaTime;
                if (temporaryMaxSpeedTimer <= 0f)
                {
                    TemporaryMaxSpeed = 0f;
                }
            }
        }

        private protected override void OnFixedUpdate()
        {
            // Prevent move logic if currently drifting
            if (context.GroundedSuperState.DriftState.IsDrifting)
                return;

            Vector3 desiredDirection = Utilities.GetCameraBasedMoveInput(
                CameraManager.Instance.CurrentCamera.transform,
                context.Input.MoveDirection
            );

            Vector3 currentDirection = context.RigidBody.linearVelocity;
            currentDirection.y = 0;
            currentDirection.Normalize();

            float directionChangeFactor = 1f;
            if (context.RigidBody.linearVelocity.sqrMagnitude > 0.01f)
            {
                float angle = Vector3.Angle(currentDirection, desiredDirection);
                directionChangeFactor = Mathf.InverseLerp(0f, 180f, angle);
            }

            Vector3 turnTorque = TurnResponsiveness * directionChangeFactor * Vector3.Cross(Vector3.up, desiredDirection);
            float speedFactor = Mathf.Clamp01(context.RigidBody.linearVelocity.magnitude / MaxSpeed);
            Vector3 propulsionTorque = Acceleration * Vector3.Cross(Vector3.up, desiredDirection) * (1f - speedFactor);

            Vector3 totalTorque = turnTorque + propulsionTorque;
            context.RigidBody.AddTorque(totalTorque, ForceMode.VelocityChange);

            context.RigidBody.angularVelocity = Vector3.ProjectOnPlane(context.RigidBody.angularVelocity, Vector3.up);

            // Use temporary max speed if active
            float speedCap = TemporaryMaxSpeed > 0f ? TemporaryMaxSpeed : MaxSpeed;
            if (context.RigidBody.linearVelocity.magnitude > speedCap)
                context.RigidBody.linearVelocity = Vector3.ClampMagnitude(context.RigidBody.linearVelocity, speedCap);
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.Input.MoveDirection == Vector2.zero)
                return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}