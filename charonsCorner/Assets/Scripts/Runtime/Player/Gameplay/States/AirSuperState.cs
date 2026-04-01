using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirSuperState : SuperState<GameplayPlayerController>
    {
        public override State<GameplayPlayerController> InitialSubState => MoveState;
        [field: SerializeField] public AirMoveState MoveState { get; private set; } = new();
        
        public float InAirTime { get; private set; }

        private protected override void InitializeSubStates()
        {
            MoveState.Init(SubStateMachine, _context);
        }

        private protected override void OnEnter()
        {
            _context.Rb.linearDamping = 0;
            InAirTime = 0f;
        }

        private protected override void OnExit()
        {
            InAirTime = 0f;
        }

        private protected override void OnUpdate()
        {
            InAirTime += Time.deltaTime;
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
    }
}