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
            if (_player.CannonBallSuperState.LaunchCompleted == false && (_player.CannonBallSuperState.PillarMoveState.IsInCannon || _player.CannonBallSuperState.FiredState.IsLaunching)) return;

            if (_isActivated) return;
            ActivateCannon();
        }

        private void ActivateCannon()
        {
            _isActivated = true;

            if (_cannonBall.UseCamera && _cannonBall.CinemachineCamera != null) CameraManager.Instance.ChangeActiveCamera(_cannonBall.CinemachineCamera);
            _player.CannonBallSuperState.SetCannonReference(_cannonBall);
            _player.StateMachine.ChangeState(_player.CannonBallSuperState, true);
        }

        public void ResetActivation()
        {
            _isActivated = false;
        }
    }
}
