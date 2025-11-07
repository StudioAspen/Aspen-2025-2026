using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundSuperState : SuperState<GameplayPlayerController>
    {
        public override State<GameplayPlayerController> InitialSubState => MoveState;
        [field: SerializeField] public GroundMoveState MoveState { get; private set; } = new();

        private protected override void InitializeSubStates()
        {
            MoveState.Init(SubStateMachine, _context);
        }

        /// <summary>
        /// Subscribes the state's drift input handler when this super state becomes active.
        /// </summary>
        /// <remarks>
        /// Registers the <c>Drift</c> handler with <c>InputManager.Instance.Drift</c> so drift input is processed while the state is entered.
        /// </remarks>
        private protected override void OnEnter()
        {
            InputManager.Instance.Drift += Drift;
        }

        /// <summary>
        /// Unsubscribes this state's Drift handler from the input manager when exiting the ground super state.
        /// </summary>
        private protected override void OnExit()
        {
            InputManager.Instance.Drift -= Drift;
        }

        /// <summary>
        /// Per-frame update hook for the grounded superstate; intentionally performs no per-frame logic.
        /// </summary>
        private protected override void OnUpdate()
        {
        }

        private protected override void OnFixedUpdate()
        {

        }

        /// <summary>
        /// Determines whether the super-state should transition based on the player's grounded status.
        /// </summary>
        /// <returns>The air super-state when the player is not grounded; otherwise <c>null</c>.</returns>
        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (!_context.IsGrounded)
            {
                return _context.AirSuperState;
            }
            
            return null;
        }

        /// <summary>
        /// Handles drift input by transitioning to the drift super-state when drift is requested.
        /// </summary>
        /// <param name="drift">If true, transition to the drift super-state; if false, do nothing.</param>
        private void Drift(bool drift)
        {
            if(drift)
                _context.StateMachine.ChangeState(_context.DriftSuperState);
        }
    }
}