using NaughtyAttributes;
using UnityEngine;
using DG.Tweening;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedDriftState : State<PlayerController>
    {
        [field: SerializeField] public float AngleOffset { get; private set; } = 20f;
        [field: SerializeField] public float TurnResponsiveness { get; private set; } = 1f;
        [field: SerializeField] public float Friction { get; private set; } = 0.9f;
        [field: SerializeField] public float DriftThreshold { get; private set; } = 2f;
        [field: SerializeField] public Color NotReadyColor { get; private set; } = Color.red;
        [field: SerializeField] public Color ReadyColor { get; private set; } = Color.green;
        [field: SerializeField] public Renderer DriftIndicatorRenderer { get; private set; }
        [field: SerializeField] public float MinDriftBoost { get; private set; } = 5f;
        [field: SerializeField] public float MaxDriftBoost { get; private set; } = 15f;

        private Vector3 driftDirection;
        private float driftTimer;
        private bool boostReady;
        private Tween colorTween;
        private float externalBoost = 0f;

        public bool IsDrifting { get; private set; }
        public bool IsBoostReady => boostReady;
        public float DriftCharge => Mathf.Clamp01(driftTimer / DriftThreshold);
        public float ExternalBoost => externalBoost;

        public float ConsumeExternalBoost()
        {
            float boost = externalBoost;
            externalBoost = 0f;
            return boost;
        }

        public void AddExternalBoost(float boost)
        {
            externalBoost += boost;
        }

        private protected override void OnEnter()
        {
            IsDrifting = true;
            boostReady = false;
            driftTimer = 0f;
            externalBoost = 0f;

            driftDirection = Utilities.GetCameraBasedMoveInput(
                CameraManager.Instance.CurrentCamera.transform,
                context.Input.MoveDirection
            );
            float angle = AngleOffset * (context.Input.MoveDirection.x > 0 ? 1 : -1);
            driftDirection = Quaternion.AngleAxis(angle, Vector3.up) * driftDirection;

            // Clamp velocity to MoveState's max speed
            float moveMaxSpeed = context.GroundedSuperState.MoveState.MaxSpeed;
            if (context.RigidBody.linearVelocity.magnitude > moveMaxSpeed)
                context.RigidBody.linearVelocity = context.RigidBody.linearVelocity.normalized * moveMaxSpeed;

            // Set indicator to red instantly
            if (DriftIndicatorRenderer != null)
            {
                colorTween?.Kill();
                DriftIndicatorRenderer.material.color = NotReadyColor;
            }
        }

        private protected override void OnExit()
        {
            IsDrifting = false;
            boostReady = false;
            driftTimer = 0f;
            colorTween?.Kill();
            externalBoost = 0f;

            // Reset indicator to red
            if (DriftIndicatorRenderer != null)
                DriftIndicatorRenderer.material.color = NotReadyColor;
        }

        private protected override void OnUpdate()
        {
            driftTimer += Time.deltaTime;

            if (!boostReady && driftTimer >= DriftThreshold)
            {
                boostReady = true;
                if (DriftIndicatorRenderer != null)
                {
                    colorTween?.Kill();
                    colorTween = DriftIndicatorRenderer.material.DOColor(ReadyColor, 0.3f);
                }
            }
            else if (!boostReady && DriftIndicatorRenderer != null)
            {
                // Interpolate color based on progress
                float t = Mathf.Clamp01(driftTimer / DriftThreshold);
                DriftIndicatorRenderer.material.color = Color.Lerp(NotReadyColor, ReadyColor, t);
            }
        }

        private protected override void OnFixedUpdate()
        {
            Vector3 desiredDirection = Utilities.GetCameraBasedMoveInput(
                CameraManager.Instance.CurrentCamera.transform,
                context.Input.MoveDirection
            );
            driftDirection = Vector3.Slerp(driftDirection, desiredDirection, TurnResponsiveness * 0.3f * Time.fixedDeltaTime);

            Vector3 turnTorque = Vector3.Cross(Vector3.up, driftDirection) * TurnResponsiveness;
            context.RigidBody.AddTorque(turnTorque, ForceMode.VelocityChange);

            Vector3 velocity = context.RigidBody.linearVelocity;
            Vector3 lateral = Vector3.ProjectOnPlane(velocity, driftDirection);
            context.RigidBody.linearVelocity = (velocity - lateral) + lateral * Friction;

            // Use MoveState's max speed for drift cap
            float moveMaxSpeed = context.GroundedSuperState.MoveState.MaxSpeed;
            if (context.RigidBody.linearVelocity.magnitude > moveMaxSpeed)
                context.RigidBody.linearVelocity = context.RigidBody.linearVelocity.normalized * moveMaxSpeed;
        }

        private protected override State<PlayerController> GetTransition()
        {
            // Only transition out on input release, boost logic handled by GroundedSuperState
            if (!context.Input.InputActions.Player.Drift.IsPressed())
                return context.GroundedSuperState.MoveState;

            if (context.Input.MoveDirection == Vector2.zero)
                return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}