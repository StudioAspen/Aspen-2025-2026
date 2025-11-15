using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class TeleportState : State<GameplayPlayerController>
    {
        private TeleportBumper _currentTeleportBumper = null;

        private protected override void OnEnter()
        {
            _context.SetIsTeleporting(true);
        }

        private protected override void OnExit()
        {
            _context.SetIsTeleporting(false);
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override void OnUpdate()
        {
            _context.CurrentSubState = _context.TeleportState.GetType().Name;
            if (_context.IsTeleporting) Teleport();
        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (!_context.IsTeleporting) return _context.GroundState;
            return null;
        }

        public void SetTeleportBumperReference(TeleportBumper teleportBumper) => _currentTeleportBumper = teleportBumper;

        private void Teleport()
        {
            _context.SetPlayerPosition(_currentTeleportBumper.TeleportDestination);
            _context.SetIsTeleporting(false);
        }

    }
}
