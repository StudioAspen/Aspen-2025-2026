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
            InputManager.Instance.Drift += Drift;
        }

        private protected override void OnExit()
        {
            InputManager.Instance.Drift -= Drift;
        }

        private protected override void OnUpdate()
        {
            
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (_context.IsGrounded)
            {
                return _context.GroundSuperState;
            }
            
            return null;
        }

        private void Drift(bool drift)
        {
            if (drift)
                _context.StateMachine.ChangeState(_context.DriftSuperState);
        }
    }
}