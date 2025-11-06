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
            if (_player.CannonState.CannonBallState.LaunchCompleted == false && (_player.CannonState.CannonBallState.IsInCannon || _player.CannonState.CannonBallState.IsLaunching)) return;

            ActivateCannon();
        }

        private void ActivateCannon()
        {
            _player.StateMachine.ChangeState(_player.CannonState, true);
            _player.CannonState.CannonBallState.SetCannonReference(_cannonBall);
        }
    }
}
