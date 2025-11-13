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
        }

        private protected override void OnExit()
        {
            //_isTeleporting = false;
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
            _context.Rb.isKinematic = true;
        }
        private protected override State<GameplayPlayerController> GetTransition()
        {
            return null;
        }

        private void Teleport()
        {
            Debug.LogError("Teleporting");
            _context.SetPlayerPosition(_currentTeleportBumper.teleportDestination);
            _isTeleporting = false;
            _context.IsTeleporting = false;
        }

    }
}
