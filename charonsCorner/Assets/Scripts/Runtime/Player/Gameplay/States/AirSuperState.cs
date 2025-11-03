using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirSuperState : SuperState<GameplayPlayerController>
    {
        public override State<GameplayPlayerController> InitialSubState => MoveState;
        [field: SerializeField] public AirMoveState MoveState { get; private set; } = new();

        private protected override void InitializeSubStates()
        {
            MoveState.Init(SubStateMachine, _context);
        }

        private protected override void OnEnter()
        {
            _context.Rb.linearDamping = 0;
        }

        private protected override void OnExit()
        {
            
        }

        private protected override void OnUpdate()
        {
            _context.CurrentSubState = MoveState.GetType().Name;
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (_context.IsGrounded)
            {
                return _context.GroundState;
            }
            
            return null;
        }
    }
}