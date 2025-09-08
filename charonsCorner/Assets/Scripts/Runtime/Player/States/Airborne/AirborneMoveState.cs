using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirborneMoveState : State<PlayerController>
    {
        [field: SerializeField] public float Damp { get; private set; } = 0.1f;

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
            Vector3 moveDirection =
                Utilities.GetCameraBasedMoveInput(CameraManager.Instance.CurrentCamera.transform, context.Input.MoveDirection);

            if(moveDirection != Vector3.zero)
                context.RigidBody.AddForce(context.AirborneSuperState.MaxSpeed * moveDirection - context.RigidBody.linearVelocity.WithY(0), ForceMode.VelocityChange);
            
            context.VisualObject.transform.rotation = context.RigidBody.rotation;
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.Input.MoveDirection == Vector2.zero)
                return context.AirborneSuperState.IdleState;

            return null;
        }
    }
}
