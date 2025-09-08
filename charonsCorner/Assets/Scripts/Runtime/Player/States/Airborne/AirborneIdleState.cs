using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirborneIdleState : State<PlayerController>
    {
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
            context.RigidBody.AddForce(context.RigidBody.linearVelocity.WithY(0).normalized * context.AirborneSuperState.MaxSpeed - context.RigidBody.linearVelocity.WithY(0), ForceMode.VelocityChange);
            
            context.VisualObject.transform.rotation = context.RigidBody.rotation;
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.Input.MoveDirection != Vector2.zero)
                return context.AirborneSuperState.MoveState;

            return null;
        }
    }
}
