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
            _context.Rb.isKinematic = true;
        }

        private protected override void OnExit()
        {
            _context.SetIsTeleporting(false);
            _context.Rb.isKinematic = false;
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override void OnUpdate()
        {
            _context.CurrentSubState = _context.TeleportState.GetType().Name;
            if (_context.IsTeleporting) Teleport();
        }

        public void SetTeleportBumperReference(TeleportBumper teleportBumper) => _currentTeleportBumper = teleportBumper;
        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (!_context.IsTeleporting) return _context.GroundState;
            return null;
        }

        private void Teleport()
        {
            _context.SetPlayerPosition(_currentTeleportBumper.teleportDestination);
            _context.SetIsTeleporting(false);

            // Setting the player to max speed once they're done teleporting
            Vector3 forwardDirection = _context.Orientation.forward.normalized;
            Vector3 accelerationForce = forwardDirection * _context.GetAcceleration();

            _context.Rb.AddForce(accelerationForce, ForceMode.Impulse);
        }

    }
}
