using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class TeleportBumperState : State<GameplayPlayerController>
    {
        private TeleportBumper _currentTeleportBumper;
        private bool _isTeleporting = false;


        private protected override void OnEnter()
        {
            _isTeleporting = true;
            _context.Rb.isKinematic = true;
        }

        private protected override void OnExit()
        {
            _isTeleporting = false;
            _context.Rb.isKinematic = false;
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override void OnUpdate()
        {
            if(_isTeleporting) Teleport();
        }

        public void SetTeleportBumperReference(TeleportBumper teleportBumper )
        {
            if (_currentTeleportBumper != null) return;
            _currentTeleportBumper = teleportBumper;

            _context.Rb.linearVelocity = Vector3.zero;
        }
        private protected override State<GameplayPlayerController> GetTransition()
        {
            return null;
        }

        private void Teleport()
        {
            _context.SetPlayerPosition(_currentTeleportBumper.teleportDestination);
            _isTeleporting = false;
            _context.IsTeleporting = false;

            // Setting the player to max speed once they're done teleporting
            Vector3 forwardDirection = _context.Orientation.forward.normalized;
            _context.Rb.linearVelocity = forwardDirection * _context.GroundState.MoveState.MaxSpeed;
        }

    }
}
