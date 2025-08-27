using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedDriftState : State<PlayerController>
    {
        [field: SerializeField] public float AngleOffset { get; private set; } = 20f;
        [field: SerializeField] public float TurnResponsiveness { get; private set; } = 1f;
        [field: SerializeField] public float Acceleration { get; private set; } = 1f;
        [field: SerializeField] public float Friction { get; private set; } = 0.9f;

        private Vector3 driftDirection;
        [SerializeField, ReadOnly, AllowNesting] private float driftTimer;

        public bool IsDrifting { get; private set; }

        private protected override void OnEnter()
        {
            IsDrifting = true;

            driftTimer = 0f;

            driftDirection = Utilities.GetCameraBasedMoveInput(
                CameraManager.Instance.CurrentCamera.transform, 
                context.Input.MoveDirection
            );

            float angle = AngleOffset * (context.Input.MoveDirection.x > 0 ? 1 : -1);
            driftDirection = Quaternion.AngleAxis(angle, Vector3.up) * driftDirection;

/*            Debug.DrawRay(context.transform.position, 100f * context.RigidBody.linearVelocity.normalized, Color.green, 10f);
            Debug.DrawRay(context.transform.position, 100f * driftDirection, Color.red, 10f);
            Debug.Break();*/
        }

        private protected override void OnExit()
        {
            IsDrifting = false;

            driftTimer = 0f;
        }

        private protected override void OnUpdate()
        {
            driftTimer += Time.deltaTime;
        }

        private protected override void OnFixedUpdate()
        {
            Vector3 desiredDirection = Utilities.GetCameraBasedMoveInput(
                CameraManager.Instance.CurrentCamera.transform, 
                context.Input.MoveDirection
            );

            // Adjust drift direction slowly toward input
            driftDirection = Vector3.Slerp(driftDirection, desiredDirection, TurnResponsiveness * Time.fixedDeltaTime);

            Vector3 turnTorque = Vector3.Cross(Vector3.up, driftDirection) * TurnResponsiveness;
            context.RigidBody.AddTorque(turnTorque, ForceMode.VelocityChange);

            context.RigidBody.AddForce(driftDirection * Acceleration, ForceMode.Acceleration);

            Vector3 velocity = context.RigidBody.linearVelocity;
            Vector3 lateral = Vector3.ProjectOnPlane(velocity, driftDirection);
            context.RigidBody.linearVelocity = (velocity - lateral) + lateral * Friction;
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (!context.Input.InputActions.Player.Drift.IsPressed() || context.Input.MoveDirection == Vector2.zero)
                return context.Config.GroundedSuperState.IdleState;

            return null;
        }
    }
}
