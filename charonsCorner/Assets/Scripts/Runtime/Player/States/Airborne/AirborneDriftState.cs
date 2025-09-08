using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirborneDriftState : State<PlayerController>
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

        private protected override State<PlayerController> GetTransition()
        {
            if (!context.Input.InputActions.Player.Jump.IsPressed())
                return context.AirborneSuperState.DropDashState;

            return null;
        }
    }
}
