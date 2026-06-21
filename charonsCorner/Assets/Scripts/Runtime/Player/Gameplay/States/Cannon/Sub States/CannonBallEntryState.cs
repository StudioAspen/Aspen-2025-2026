using UnityEngine;
using System.Collections;


namespace CharonsCorner.Runtime
{
    public class CannonBallEntryState : State<GameplayPlayerController>
    {
        private Coroutine _lerpRoutine;

        private bool _shouldTransitionToPillarMove = false;
        private bool _shouldTransitionToFired = false;

        public bool IsInCannon { get; private set; }

        private protected override void OnEnter()
        {
            //Set Is In Cannon Flag:
            IsInCannon = true;

            //Set Current Substate Name:
            _context.CurrentSubState = GetType().Name;

            //Conditions For Transitioning To Next State:
            _shouldTransitionToPillarMove = false;
            _shouldTransitionToFired = false;

            //Get Current Cannon Being Used By Player:
            CannonBall cannon = _context.CurrentCannon;
            if (cannon == null)
            {
                //If No Cannon Found, Return To Ground State:
                _context.CannonBallSuperState.LaunchFailed = true;
                return;
            }

            //Disable Player Physics:
            _context.Rb.isKinematic = true;

            //Start Lerp To Cannon Base Position:
            _lerpRoutine = _context.StartCoroutine(LerpToCannonBase(cannon));
        }

        private protected override void OnExit()
        {
            //Stop Lerp Routine If Active:
            if (_lerpRoutine != null)
            {
                _context.StopCoroutine(_lerpRoutine);
                _lerpRoutine = null;
            }

            //Reset Is In Cannon Flag:
            IsInCannon = false;
        }

        private IEnumerator LerpToCannonBase(CannonBall cannonBall)
        {
            //Parameter Settings:
            float time = 0f;
            Vector3 startPos = _context.transform.position;
            Quaternion startRot = _context.transform.rotation;

            Transform targetTransform = cannonBall.LaunchObject != null ? cannonBall.LaunchObject : cannonBall.transform;
            Vector3 targetPos = targetTransform.position;
            Quaternion targetRot = Quaternion.LookRotation(targetTransform.forward, Vector3.up);
            float loadDuration = 1f; // Default load time

            //Loading Cannon Sequence:
            while (time < 1f)
            {
                //Lerp Position And Rotation:
                time += Time.deltaTime / Mathf.Max(0.0001f, loadDuration);
                _context.transform.position = Vector3.Lerp(startPos, targetPos, time);
                _context.transform.rotation = Quaternion.Slerp(startRot, targetRot, time);

                yield return null;
            }

            //Start Transition To Next State Based On Cannon Settings:
            IsInCannon = false;
            _shouldTransitionToPillarMove = true;
        }

        private protected override void OnUpdate() {}

        private protected override void OnFixedUpdate() {}

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (_shouldTransitionToPillarMove)
            {
                return _context.CannonBallSuperState.PillarMoveState;
            }
            else if (_shouldTransitionToFired)
            {
                return _context.CannonBallSuperState.FiredState;
            }

            return null;
        }

    }
}
