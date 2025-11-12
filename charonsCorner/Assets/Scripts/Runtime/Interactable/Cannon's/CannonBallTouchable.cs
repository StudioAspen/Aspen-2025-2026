using UnityEngine;
using System.Collections;

namespace CharonsCorner.Runtime
{
    public class CannonBallTouchable : TouchInteractable
    {
        private CannonBall _cannonBall;
        private GameplayPlayerController _player;

        private bool _isActivated = false;


        private void Awake()
        {
            _cannonBall = GetComponent<CannonBall>(); 
            _player = FindFirstObjectByType<GameplayPlayerController>();
        }

        public void HandleTouch()
        {
            if (_player == null || _cannonBall == null) return;
            if (_isActivated) return;
            if (_player.CannonState.CannonBallState.LaunchCompleted == false && (_player.CannonState.CannonBallState.IsInCannon || _player.CannonState.CannonBallState.IsLaunching)) return;

            if (_cannonBall.UseCamera) CameraManager.Instance.ChangeActiveCamera(_cannonBall.CinemachineCamera);

            _isActivated = true;
            ActivateCannon();
        }

        private void ActivateCannon()
        {
            _player.StateMachine.ChangeState(_player.CannonState, true);
            _player.CannonState.CannonBallState.SetCannonReference(_cannonBall);
        }

        private void OnTriggerExit(Collider other)
        {
            _isActivated = false;
        }
    }
}
