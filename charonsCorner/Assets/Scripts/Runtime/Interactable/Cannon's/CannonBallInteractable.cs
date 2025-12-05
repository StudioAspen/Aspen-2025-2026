using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CannonBallInteractable : InteractableObject
    {
        private CannonBall _cannonBall;
        private GameplayPlayerController _player;

        private void Awake()
        {
            _cannonBall = GetComponent<CannonBall>();
            _player = FindFirstObjectByType<GameplayPlayerController>();

            OnInteract.AddListener(HandleInteract);
        }

        public void HandleInteract()
        {
            if (_player == null || _cannonBall == null) return;

            var cannonSuper = _player.CannonBallSuperState;
            if (!cannonSuper.LaunchCompleted && (cannonSuper.EntryState.IsInCannon || cannonSuper.PillarMoveState.IsInCannon || cannonSuper.FiredState.IsLaunching)) return;

            ActivateCannon();
        }

        private void ActivateCannon()
        {
            if (_cannonBall.UseCamera && _cannonBall.CinemachineCamera != null) CameraManager.Instance.ChangeActiveCamera(_cannonBall.CinemachineCamera);
            _player.CannonBallSuperState.SetCannonReference(_cannonBall);
            _player.StateMachine.ChangeState(_player.CannonBallSuperState, true);
        }
    }
}
