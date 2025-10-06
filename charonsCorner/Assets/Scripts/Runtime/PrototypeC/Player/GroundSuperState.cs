using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundSuperState : SuperState<PrototypePlayerController>
    {
        public override State<PrototypePlayerController> InitialSubState => MoveState;

        [field: SerializeField] public GroundIdleState IdleState { get; private set; } = new();
        [field: SerializeField] public GroundMoveState MoveState { get; private set; } = new();

        private protected override void InitializeSubStates()
        {
            IdleState.Init(SubStateMachine, context);
            MoveState.Init(SubStateMachine, context);
        }

        private protected override void OnEnter()
        {
            
        }

        private protected override void OnExit()
        {
            
        }

        private protected override void OnUpdate()
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input.magnitude > 0.01f)
            {
                SubStateMachine.ChangeState(MoveState);
                context.CurrentSubState = MoveState.ToString();
            }
            else
            {
                SubStateMachine.ChangeState(IdleState);
                context.CurrentSubState = IdleState.ToString();
            }
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<PrototypePlayerController> GetTransition()
        {
            return null;
        }
    }
}
