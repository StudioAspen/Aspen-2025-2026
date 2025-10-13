using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedDriftState : State<PlayerController>
    {
        [field: SerializeField] public float AngleOffset { get; private set; } = 1f;
        [field: SerializeField] public float TurnResponsiveness { get; private set; } = 1f;
        [field: SerializeField] public float Acceleration { get; private set; } = 1f;
        [field: SerializeField] public float Friction { get; private set; } = 0.95f;

        [field: SerializeField] public float MaxDriftSpeed { get; private set; } = 10f;
        [field: SerializeField] public float DriftDrag { get; private set; } = 0.995f;
        [field: SerializeField] public float DriftDeaccelerationMultiplier { get; private set; } = 0.25f;
        [field: SerializeField] public float DriftExitAcceleration { get; private set; } = 15f;


        private Vector3 driftDirection;
        private Vector3 lockedDriftDirection;


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
            //lockedDriftDirection = context.RigidBody.linearVelocity.normalized;
            /*Debug.DrawRay(context.transform.position, 100f * context.RigidBody.linearVelocity.normalized, Color.green, 10f);
            Debug.DrawRay(context.transform.position, 100f * driftDirection, Color.red, 10f);
            Debug.Break();*/
        }

        private protected override void OnExit()
        {
            IsDrifting = false;

            driftTimer = 0f;

             context.RigidBody.AddForce(driftDirection * DriftExitAcceleration, ForceMode.VelocityChange); //apply boost
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

            // ensure vectors are horizontal
            Vector3 flatVel = context.RigidBody.linearVelocity;
            flatVel.y = 0f;
            float speed = flatVel.magnitude;
            Vector3 currentVelDir = speed > 0.001f ? flatVel.normalized : desiredDirection.normalized;

           
            driftDirection = Vector3.Slerp(driftDirection, desiredDirection.normalized, TurnResponsiveness * Time.fixedDeltaTime);

           
            Vector3 desiredVelDir = Vector3.Slerp(currentVelDir, driftDirection.normalized, TurnResponsiveness * Time.fixedDeltaTime);
            Vector3 targetVelocity = desiredVelDir * speed; // keep current speed, only change direction for the arc

           
            Vector3 steering = (targetVelocity - flatVel) * DriftDeaccelerationMultiplier;
            context.RigidBody.AddForce(steering, ForceMode.VelocityChange);

            
            context.RigidBody.AddForce(driftDirection.normalized * Acceleration * 0.5f, ForceMode.Acceleration);

            
            context.RigidBody.linearVelocity = new Vector3(
                context.RigidBody.linearVelocity.x * DriftDrag,
                context.RigidBody.linearVelocity.y,
                context.RigidBody.linearVelocity.z * DriftDrag
            ); 

            // Debugging visual
            Debug.DrawRay(context.transform.position, driftDirection.normalized * 2f, Color.cyan);
            Debug.DrawRay(context.transform.position, context.RigidBody.linearVelocity.normalized * 2f, Color.green);
        
        } 

        private protected override State<PlayerController> GetTransition()
        {
            if (!context.Input.InputActions.Player.Drift.IsPressed() || context.Input.MoveDirection == Vector2.zero)
                return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}
