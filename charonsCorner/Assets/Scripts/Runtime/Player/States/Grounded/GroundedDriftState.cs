using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedDriftState : State<PlayerController>
    {
        [SerializeField] private float driftAdjustSpeed = 20f;
        [SerializeField] private float maxDriftOffsetAngle = 45f;
        [SerializeField] private float driftSpeed = 10f;

        private Vector3 startDirection;
        private float currentOffsetAngle;
        private Vector3 driftMoveDirection;

        private float driftTimer;

        public bool IsDrifting { get; private set; }

        private protected override void OnEnter()
        {
            IsDrifting = true;
            driftTimer = 0f;

            currentOffsetAngle = 0f;

            startDirection = context.RigidBody.linearVelocity.WithY(0).normalized;

            context.RigidBody.isKinematic = true;
        }

        private protected override void OnExit()
        {
            IsDrifting = false;

            context.RigidBody.isKinematic = false;
        }

        private protected override void OnUpdate()
        {
            driftTimer += Time.deltaTime;

            HandleDriftOffsetAngle();

            Vector3 currentOffsetDirection = Quaternion.Euler(0f, currentOffsetAngle, 0f) * startDirection;
            Debug.DrawLine(context.transform.position, context.transform.position + startDirection * 25f, Color.magenta, 0.1f);
            Debug.DrawLine(context.transform.position, context.transform.position + currentOffsetDirection * 25f, Color.yellow, 0.1f);

            // The actual drift direction is halfway between the start direction and the current offset direction
            driftMoveDirection = Quaternion.Euler(0f, currentOffsetAngle / 2f, 0f) * startDirection;
            Debug.DrawLine(context.transform.position, context.transform.position + driftMoveDirection * 25f, Color.green, 0.1f);
        }

        private protected override void OnFixedUpdate()
        {
            context.RigidBody.MovePosition(context.transform.position + driftSpeed * driftMoveDirection * Time.fixedDeltaTime);
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (!context.Input.InputActions.Player.Drift.IsPressed())
                return context.GroundedSuperState.IdleState;

            return null;
        }

        private void HandleDriftOffsetAngle()
        {
            float horizontalInput = context.Input.MoveDirection.x;
            currentOffsetAngle += horizontalInput * driftAdjustSpeed * Time.deltaTime;

            // Clamp the offset angle to the maximum allowed drift angle
            if (Mathf.Abs(currentOffsetAngle) > maxDriftOffsetAngle)
                currentOffsetAngle = Mathf.Sign(currentOffsetAngle) * maxDriftOffsetAngle;
        }
    }
}
