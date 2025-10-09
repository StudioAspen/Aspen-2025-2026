using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedMoveState : State<PlayerController>
    {
        [field: SerializeField] public float Acceleration { get; private set; } = 15f;
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

            Vector3 velocity = context.RigidBody.linearVelocity; 
            Vector3 flatVelocity = velocity;
            flatVelocity.y = 0f;

            float speed = flatVelocity.magnitude;

       
            Vector3 currentDirection = speed > 0.01f ? flatVelocity.normalized : desiredDirection; //establish if you are moving forward
            float alignmentDirection = Vector3.Dot(currentDirection, desiredDirection); //is object's direction in alignment with player input?

   
            float directionChangeFactor = Mathf.InverseLerp(0f, 180f, Vector3.Angle(currentDirection, desiredDirection)); //torque responsiveness factor

     
            Vector3 turnTorque = TurnResponsiveness * directionChangeFactor * Vector3.Cross(Vector3.up, desiredDirection); //torque responsiveness


            float speedFactor = Mathf.Clamp01(speed / MaxSpeed);
            Vector3 pushTorque = Acceleration * Vector3.Cross(Vector3.up, desiredDirection) * (1f - speedFactor); //not effective on high speeds

            Vector3 totalTorque = turnTorque + pushTorque;

            
            if (alignmentDirection < -0.1f && speed > 0.1f) //Are you breaking/reversing?
            {
                //apply deceleration
                float brakeStrength = Acceleration * 1.0f;
                Vector3 brakeForce = -flatVelocity.normalized * brakeStrength;
                context.RigidBody.AddForce(brakeForce, ForceMode.Acceleration);

                //reduce snapFactor
                flatVelocity = Vector3.Lerp(flatVelocity, Vector3.zero, 0.20f);
            }
            else
            {
                
                float snapFactor = alignmentDirection < 0f ? 0.1f : 0.5f; //slow snap when turning around
                Vector3 targetVelocity = desiredDirection.normalized * speed;
                flatVelocity = Vector3.Lerp(flatVelocity, targetVelocity, snapFactor);

                context.RigidBody.AddTorque(totalTorque, ForceMode.VelocityChange);
            }

            //reapply velocity
            context.RigidBody.linearVelocity = new Vector3(flatVelocity.x, velocity.y, flatVelocity.z);

            // Prevent vertical spin
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
