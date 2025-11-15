using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent (typeof(TeleportBumper))] 
    public class TeleportBumperTouchable : TouchInteractable
    {
        private TeleportBumper _teleportBumper;
        private GameplayPlayerController _player;
        private bool _isActivated = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _teleportBumper = GetComponent<TeleportBumper>();
            _player = FindAnyObjectByType<GameplayPlayerController>();
        }

        public void HandleTouch()
        {
            if (_player == null || _teleportBumper == null) return;
            if (_isActivated) return;

            _isActivated = true;
            ActivateTeleport();
        }

        private void ActivateTeleport()
        {
            _player.TeleportState.SetTeleportBumperReference(_teleportBumper);
            _player.StateMachine.ChangeState(_player.TeleportState, true);
        }

        private void OnTriggerExit(Collider other)
        {
            _isActivated = false;
        }
    }
}
