using UnityEngine;

namespace CharonsCorner.Runtime
{
   [System.Serializable]
    public class DriftSuperState : SuperState<GameplayPlayerController>
    {
        // Substates
        [field: SerializeField] public DriftingChargeState DriftingChargeState {get; private set;} = new();
        [field: SerializeField] public DriftBoostState DriftingBoostState {get; private set;} = new();

        public override State<GameplayPlayerController> InitialSubState => DriftingChargeState;

        /// <summary>
        /// Initializes the drifting substates with the sub-state machine and shared context.
        /// </summary>
        private protected override void InitializeSubStates()
        {
            
            DriftingChargeState.Init(SubStateMachine, _context);
            DriftingBoostState.Init(SubStateMachine, _context);
        }

        /// <summary>
        /// Activates the drifting charge substate when the drift super state is entered.
        /// </summary>
        private protected override void OnEnter()
        {
            SubStateMachine.ChangeState(DriftingChargeState);
        }

        /// <summary>
        /// Cleans up when exiting the drift super state.
        /// </summary>
        /// <remarks>
        /// Restores any modified settings and removes drift-specific effects created while drifting.
        /// </remarks>
        private protected override void OnExit()
        {
            // Clean up drift effects or restore settings
        }

        /// <summary>
/// Called once per frame while this super state is active; intentionally has no per-frame logic at this level.
/// </summary>
/// <remarks>
/// Substates are expected to handle any per-frame behavior.
/// </remarks>
private protected override void OnUpdate() { }
        /// <summary>
/// Called on the fixed (physics) update tick while this super state is active.
/// </summary>
/// <remarks>
/// This super state performs no physics-step logic; substates are expected to handle fixed-update behavior.
/// </remarks>
private protected override void OnFixedUpdate() { }

        /// <summary>
        /// Determines the next top-level gameplay super state when drifting completes.
        /// </summary>
        /// <returns>
        /// <c>GroundSuperState</c> if the drift boost is complete and the player is grounded, <c>AirSuperState</c> if the boost is complete and the player is airborne, or <c>null</c> if no transition should occur.
        /// </returns>
        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (DriftingBoostState.IsComplete)
                return _context.IsGrounded ? _context.GroundSuperState : _context.AirSuperState;
            return null;
        }
    }
}