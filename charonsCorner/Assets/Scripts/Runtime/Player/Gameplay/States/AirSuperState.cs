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

        /// <summary>
        /// Per-frame update hook for this superstate; invoked each frame while the state is active.
        /// </summary>
        /// <remarks>
        /// This implementation performs no per-frame actions.
        /// </remarks>
        private protected override void OnUpdate()
        {
            
        }

        /// <summary>
        /// Called on the fixed-timestep update while this superstate is active; no behavior is performed.
        /// </summary>
        private protected override void OnFixedUpdate()
        {

        }

        /// <summary>
        /// Determine a state transition when the player is grounded.
        /// </summary>
        /// <returns>`GroundSuperState` when the context's IsGrounded is true, `null` otherwise.</returns>
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