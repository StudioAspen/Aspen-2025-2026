using JetBrains.Annotations;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class TeleportBumperSuperState : SuperState<GameplayPlayerController>
    {
        public override State<GameplayPlayerController> InitialSubState => TeleportBumperState;

        [field: SerializeField] public TeleportBumperState TeleportBumperState { get; private set; } = new();

        private protected override void InitializeSubStates()
        {
            TeleportBumperState.Init(SubStateMachine, _context);
        }

        private protected override void OnEnter()
        {
            _context.IsTeleporting = true;
        }

        private protected override void OnExit()
        {
            _context.IsTeleporting = false;
        }

        private protected override void OnUpdate()
        {
            _context.CurrentSubState = TeleportBumperState.GetType().Name;
        }

        private protected override void OnFixedUpdate() { }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if(!_context.IsTeleporting)
            {
                Debug.LogError("Exiting teleport to ground state");
                //_context.IsTeleporting = false;
                return _context.GroundState;
            }

            return null;
        }
        
    }
}
