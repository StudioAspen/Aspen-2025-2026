using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirborneSuperState : SuperState<PlayerController>
    {
        public override State<PlayerController> InitialSubState => IdleState;

        [field: SerializeField] public AirborneIdleState IdleState { get; private set; } = new();
        [field: SerializeField] public AirborneMoveState MoveState { get; private set; } = new();
        [field: SerializeField] public AirborneDriftState DriftState { get; private set; } = new();

        private protected override void InitializeSubStates()
        {
            IdleState.Init(SubStateMachine, context);
            MoveState.Init(SubStateMachine, context);
            DriftState.Init(SubStateMachine, context);
        }

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
            if (context.IsGrounded)
                return context.GroundedSuperState;

            return null;
        }

        private protected override State<PlayerController> GetSubStateTransition()
        {
            if (context.Input.InputActions.Player.Drift.IsPressed())
                return DriftState;

            return null;
        }
    }
}
