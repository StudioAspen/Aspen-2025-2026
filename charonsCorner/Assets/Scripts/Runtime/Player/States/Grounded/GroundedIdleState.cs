using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedIdleState : State<PlayerController>
    {
        [SerializeField] private float damp = 0.1f;

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
            Vector3 torque = damp * -context.RigidBody.angularVelocity;
            context.RigidBody.AddTorque(torque, ForceMode.VelocityChange);

            context.VisualObject.transform.rotation = context.RigidBody.rotation;
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.Input.MoveDirection != Vector2.zero)
                return context.GroundedSuperState.MoveState;

            return null;
        }
    }
}
