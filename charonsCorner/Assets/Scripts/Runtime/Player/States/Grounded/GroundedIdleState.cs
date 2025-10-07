using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedIdleState : State<PlayerController>
    {
        //[field: SerializeField] public float Damp { get; private set; } = 0.1f;
        private float Damp = 0.1f;

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
            //Vector3 torque = Damp * -context.RigidBody.angularVelocity;
            //context.RigidBody.AddTorque(torque, ForceMode.VelocityChange);
            float ycheck = context.RigidBody.linearVelocity.y;
            if (ycheck >= 0) // checks if we are rolling downhill, removes damping if we are as gravity wins out. We still have it if we are rolling uphill.
            {
                Vector3 torque = Damp * -context.RigidBody.angularVelocity;
                context.RigidBody.AddTorque(torque, ForceMode.VelocityChange);
                Debug.Log("Damping is active");
                Debug.Log("Current liner y velocity: " + ycheck);
            }
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.Input.MoveDirection != Vector2.zero)
                return context.GroundedSuperState.MoveState;

            return null;
        }
       
        //private protected override State<PlayerController> GetTransition()
    }
}
