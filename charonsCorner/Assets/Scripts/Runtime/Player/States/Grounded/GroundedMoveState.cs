using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedMoveState : State<PlayerController>
    {
        [SerializeField] private float acceleration = 1f;
        [SerializeField] private float turnResponsiveness = 1f;
        [SerializeField] private float maxSpeed = 25f;

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

            Vector3 currentDirection = context.RigidBody.linearVelocity;
            currentDirection.y = 0;
            currentDirection.Normalize();

            float directionChangeFactor = 1f;
            if (context.RigidBody.linearVelocity.sqrMagnitude > 0.01f)
            {
                float angle = Vector3.Angle(currentDirection, desiredDirection);
                directionChangeFactor = Mathf.InverseLerp(0f, 180f, angle); // Normalize angle (0 = no change, 180 = full change)
            }

            // Torque scales with how much you're turning
            Vector3 turnTorque = turnResponsiveness * directionChangeFactor * Vector3.Cross(Vector3.up, desiredDirection);

            // Propulsion torque based on speed for snappy direction changes at low speed
            float speedFactor = Mathf.Clamp01(context.RigidBody.linearVelocity.magnitude / maxSpeed);
            Vector3 propulsionTorque = acceleration * Vector3.Cross(Vector3.up, desiredDirection) * (1f - speedFactor);

            Vector3 totalTorque = turnTorque + propulsionTorque;

            context.RigidBody.AddTorque(totalTorque, ForceMode.VelocityChange);

            // To prevent y-axis spin
            context.RigidBody.angularVelocity = Vector3.ProjectOnPlane(context.RigidBody.angularVelocity, Vector3.up);

            // Cap final speed
            if (context.RigidBody.linearVelocity.magnitude > maxSpeed)
                context.RigidBody.linearVelocity = Vector3.ClampMagnitude(context.RigidBody.linearVelocity, maxSpeed);

            context.VisualObject.transform.rotation = context.RigidBody.rotation;
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.Input.MoveDirection == Vector2.zero)
                return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}
