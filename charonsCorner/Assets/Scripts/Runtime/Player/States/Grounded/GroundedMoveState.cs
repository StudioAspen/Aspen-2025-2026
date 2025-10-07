using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedMoveState : State<PlayerController>
    {
        [field: SerializeField] public float Acceleration { get; private set; } = 1f;
        [field: SerializeField] public float TurnResponsiveness { get; private set; } = 1f;
        [field: SerializeField] public float MaxSpeed { get; private set; } = 25f;

    

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
            Vector3 turnTorque = 10f * TurnResponsiveness * directionChangeFactor * Vector3.Cross(Vector3.up, desiredDirection);

            // Propulsion torque based on speed for snappy direction changes at low speed
            float speedFactor = Mathf.Clamp01(context.RigidBody.linearVelocity.magnitude / MaxSpeed);
            Vector3 propulsionTorque = Acceleration * Vector3.Cross(Vector3.up, desiredDirection) * (1f - speedFactor);

            Vector3 totalTorque = turnTorque + propulsionTorque;

            Vector3 flatVelocity = context.RigidBody.linearVelocity;
            flatVelocity.y = 0f;

            float speed = flatVelocity.magnitude;
            if (speed > 0.01f)
            {    
                float snapFactor = 0.5f; // How quickly to realign velocity direction (0 = smooth, 1 = instant)

                Vector3 targetVelocity = desiredDirection.normalized * speed;
                flatVelocity = Vector3.Lerp(flatVelocity, targetVelocity, snapFactor);

                context.RigidBody.linearVelocity = new Vector3(flatVelocity.x, context.RigidBody.linearVelocity.y, flatVelocity.z);  
            }

            context.RigidBody.AddTorque(totalTorque, ForceMode.VelocityChange);

            // To prevent y-axis spin
            context.RigidBody.angularVelocity = Vector3.ProjectOnPlane(context.RigidBody.angularVelocity, Vector3.up);

            // Cap final speed
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
