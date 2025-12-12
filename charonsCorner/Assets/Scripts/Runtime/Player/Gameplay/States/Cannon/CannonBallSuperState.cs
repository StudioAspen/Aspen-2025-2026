using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class CannonBallSuperState : SuperState<GameplayPlayerController>
    {
        // Substates
        [field: SerializeField] public CannonBallEntryState EntryState { get; private set; } = new();
        [field: SerializeField] public CannonPillarMoveState PillarMoveState { get; private set; } = new();
        [field: SerializeField] public CannonFiredState FiredState { get; private set; } = new();

        public override State<GameplayPlayerController> InitialSubState => EntryState;
        public bool LaunchCompleted { get; internal set; }
        public bool LaunchFailed { get; internal set; }


        private protected override void InitializeSubStates()
        {
            EntryState.Init(SubStateMachine, _context);
            PillarMoveState.Init(SubStateMachine, _context);
            FiredState.Init(SubStateMachine, _context);
        }

        private protected override void OnEnter()
        {
            _context.Rb.isKinematic = false;
            LaunchCompleted = false;
            LaunchFailed = false;
        }

        private protected override void OnExit()
        {
            LaunchFailed = false;
            LaunchCompleted = false;
            _context.Rb.isKinematic = false;
        }

        private protected override void OnUpdate()
        {
            //Update Current Substate Name in Context:
            _context.CurrentSubState = SubStateMachine.CurrentState?.GetType().Name ?? "None";
        }

        private protected override void OnFixedUpdate() { }

        public void SetCannonReference(CannonBall cannon)
        {
            //Set The Cannon Reference in the Player Context:
            _context.SetCurrentCannon(cannon);
            LaunchFailed = false;
            LaunchCompleted = false;

            //Reset Rigidbody State:
            _context.Rb.isKinematic = false;
            _context.Rb.linearVelocity = Vector3.zero;
            _context.Rb.isKinematic = true;

            //Reset to Entry State:
            SubStateMachine.ChangeState(EntryState);
        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            //If Launch is Completed, Transition to Ground State:
            if (LaunchCompleted)
            {
                _context.SetCannonAir(true);
                return _context.GroundSuperState;
            }
            else if (LaunchFailed)
            {
                return _context.GroundSuperState;
            }   

            return null;
        }
    }
}
