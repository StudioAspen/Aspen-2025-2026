using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundIdleState : State<PrototypePlayerController>
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
        }

        private protected override State<PrototypePlayerController> GetTransition()
        {
            // if (context.Input.MoveDirection == Vector2.zero)
            //     return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}