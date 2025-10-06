using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundSuperState : SuperState<PrototypePlayerController>
    {
        public override State<PrototypePlayerController> InitialSubState => MoveState;

        [field: SerializeField] public float GroundDrag {get; private set;}
        [field: SerializeField] public GroundIdleState IdleState { get; private set; } = new();
        [field: SerializeField] public GroundMoveState MoveState { get; private set; } = new();

        private protected override void InitializeSubStates()
        {
            IdleState.Init(SubStateMachine, context);
            MoveState.Init(SubStateMachine, context);
        }

        private protected override void OnEnter()
        {
            context.Rb.linearDamping = GroundDrag;
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
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input.magnitude > 0.01f)
            {
                context.CurrentSubState = MoveState.GetType().Name;
                return MoveState;
            }
            else
            {
                context.CurrentSubState = IdleState.GetType().Name;
                return IdleState;
                
            }
            return null;
        }
    }
}
