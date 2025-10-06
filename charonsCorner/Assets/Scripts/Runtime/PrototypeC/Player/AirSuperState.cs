using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirSuperState : SuperState<PrototypePlayerController>
    {
        public override State<PrototypePlayerController> InitialSubState => MoveState;
        [field: SerializeField] public AirMoveState MoveState { get; private set; } = new();

        private protected override void InitializeSubStates()
        {
            MoveState.Init(SubStateMachine, context);
        }

        private protected override void OnEnter()
        {
            context.Rb.linearDamping = 0;
        }

        private protected override void OnExit()
        {
            
        }

        private protected override void OnUpdate()
        {
            context.CurrentSubState = MoveState.GetType().Name;
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