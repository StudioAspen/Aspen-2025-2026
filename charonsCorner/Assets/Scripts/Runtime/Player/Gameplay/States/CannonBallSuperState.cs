using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class CannonBallSuperState : SuperState<GameplayPlayerController>
    {
        public override State<GameplayPlayerController> InitialSubState => CannonBallState;
        [field: SerializeField] public CannonBallState CannonBallState { get; private set; } = new();

        private protected override void InitializeSubStates()
        {
            CannonBallState.Init(SubStateMachine, _context);
        }

        private protected override void OnEnter()
        {
            _context.Rb.isKinematic = false;
        }

        private protected override void OnExit()
        {
            _context.Rb.isKinematic = false;
        }

        private protected override void OnUpdate()
        {
            _context.CurrentSubState = CannonBallState.GetType().Name;
        }

        private protected override void OnFixedUpdate() { }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (CannonBallState.LaunchCompleted)
            {
                _context.SetCannonAir(true);
                return _context.GroundState;
            }

            return null;
        }
    }
}
