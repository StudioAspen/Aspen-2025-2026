using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirborneDriftState : State<PlayerController>
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

        }

        private protected override State<PlayerController> GetTransition()
        {
            if (!context.Input.InputActions.Player.Drift.IsPressed() || context.Input.MoveDirection == Vector2.zero)
                return context.Config.AirborneSuperState.IdleState;

            return null;
        }
    }
}
