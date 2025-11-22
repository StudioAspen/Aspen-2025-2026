using System.Collections;
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
            _context.CurrentSubState = _context.TeleportState.GetType().Name;
        }

        private protected override void OnExit()
        {
            _context.Rb.isKinematic = false;
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override void OnUpdate()
        {
            if (_context.IsTeleporting)
            {
                Teleport();
            }

            if (_context.Rb.transform.position == _currentTeleportBumper.GetTeleportDestination().position)
            {
                _context.SetIsTeleporting(false);
            }


        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (!_context.IsTeleporting) return _context.AirState;

            return null;
        }

        public void SetTeleportBumperReference(TeleportBumper teleportBumper) => _currentTeleportBumper = teleportBumper;

        private void Teleport()
        {
            _context.Rb.isKinematic = true;
            _context.SetPlayerPosition(_currentTeleportBumper.GetTeleportDestination());
            _context.StartCoroutine(TempAccelerationIncrease(_currentTeleportBumper.GetBoostSpeedMultiplier(), _currentTeleportBumper.GetBoostSpeedDuration()));
        }

        private IEnumerator TempAccelerationIncrease(float boostMultiplier, float boostDuration)
        {
            _context.Rb.linearVelocity *= boostMultiplier;

            yield return new WaitForSeconds(boostDuration);

            _context.Rb.linearVelocity /= boostMultiplier;
        }

    }
}
