using UnityEngine;
using System.Collections;


namespace CharonsCorner.Runtime
{
    public class CannonPillarMoveState : State<GameplayPlayerController>
    {
        private Coroutine _oscillationRoutine;
        private LineRenderer _trajectoryRenderer;

        private bool _isComplete;
        public bool IsInCannon { get; private set; } = false;


        private protected override void OnEnter()
        {
            //Set Is In Cannon Flag:
            IsInCannon = true;

            //Set Current Sub-State Name:
            _context.CurrentSubState = GetType().Name;

            //Condition For Transitioning To Next State:
            _isComplete = false;

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

            //Start Oscillation Coroutine To Move Pillar Angle Back & Forth:
            _oscillationRoutine = _context.StartCoroutine(PillarAngleOscillation(cannon));
        }

        private protected override void OnExit()
        {
            //Stop Oscillation Routine If Active:
            if (_oscillationRoutine != null)
            {
                _context.StopCoroutine(_oscillationRoutine);
                _oscillationRoutine = null;
            }

            //Clear Line Renderer Positions:
            CannonBall cannon = _context.CurrentCannon;
            if (cannon != null && cannon.LineRenderer != null)
            {
                cannon.LineRenderer.positionCount = 0;
                cannon.LineRenderer.enabled = false;
            }

            //Reset Is In Cannon Flag:
            IsInCannon = false;
        }

        private IEnumerator PillarAngleOscillation(CannonBall cannonBall)
        {
            //Parameters Settings:
            float elapsed = 0f;
            Transform launchObject = cannonBall.LaunchObject != null ? cannonBall.LaunchObject : cannonBall.transform;

            //Oscillate Until Player Confirms Launch:
            while (!_isComplete)
            {
                elapsed += Time.deltaTime * cannonBall.LerpRate;

                //Oscillate Back <-> Forth between Angle A and Angle B:
                float t = Mathf.PingPong(elapsed, 1f);
                float currentAngle = Mathf.Lerp(cannonBall.AngleA, cannonBall.AngleB, t);

                //Update Cannon Pillar Rotation Based On Current Angle (Rotation around local right axis):
                if (cannonBall.CannonPillar != null)
                {
                    cannonBall.CannonPillar.localRotation = Quaternion.Euler(currentAngle, 0, 0);
                }

                //Update Line Renderer:
                if (cannonBall.LineRenderer != null)
                {
                    cannonBall.LineRenderer.enabled = true;
                    cannonBall.LineRenderer.positionCount = 2;
                    cannonBall.LineRenderer.SetPosition(0, launchObject.position);
                    cannonBall.LineRenderer.SetPosition(1, launchObject.position + launchObject.forward * 10f);
                }

                //Check For Player Input To Confirm Launch:
                if (InputManager.Instance.InputActions.Player.Jump.triggered)
                {
                    _isComplete = true;
                }

                yield return null;
            }
        }

        private protected override void OnUpdate() {}

        private protected override void OnFixedUpdate() {}

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (_isComplete)
            {
                return _context.CannonBallSuperState.FiredState;
            }
            return null;
        }
    }
}
