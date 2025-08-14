using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirborneSuperState : HierarchicalState<PlayerController>
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

        public override void Enter()
        {
            
        }

        public override void Exit()
        {

        }

        public override void Update()
        {
            if (InputManager.Instance.InputActions.Player.Drift.IsPressed())
            {
                SubStateMachine.ChangeState(DriftState);
                return;
            }
        }

        public override void FixedUpdate()
        {

        }
    }
}
