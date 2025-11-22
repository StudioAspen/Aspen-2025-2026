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

            SetupTrajectoryRenderer();

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

            //Clear Trajectory Renderer Positions:
            if (_trajectoryRenderer != null)
            {
                _trajectoryRenderer.positionCount = 0;
            }

            //Reset Is In Cannon Flag:
            IsInCannon = false;
        }

        private IEnumerator PillarAngleOscillation(CannonBall cannonBall)
        {
            //Parameters Settings:
            float elapsed = 0f;
            float speed = cannonBall.PillarSpeed;
            const float perpendicularAngle = 90f;

            //Oscillate Until Player Confirms Launch:
            while (!_isComplete)
            {
                elapsed += Time.deltaTime * speed;

                //Oscillate Back <-> Forth & Set The Current Angle Based On Updating Position:
                float angle = Mathf.Lerp(cannonBall.ShotAngleMin, cannonBall.ShotAngleMax, Mathf.PingPong(elapsed, 1f));
                cannonBall.currentShotAngle = angle;

                //Update Cannon Pillar Rotation Based On Current Angle:
                if (cannonBall.CannonPillar != null)
                {
                    Vector3 forward = cannonBall.LaunchDirection.forward;

                    //Fix Visual & Calculated Offsets:
                    float adjustedAngle = angle - perpendicularAngle;

                    //Calculate Target Rotation Based On Adjusted Angle:
                    Quaternion angleRotation = Quaternion.AngleAxis(adjustedAngle, cannonBall.CannonBase.right);
                    Vector3 direction = angleRotation * forward;
                    Quaternion targetRotation = Quaternion.LookRotation(direction, cannonBall.CannonBase.up);

                    //Apply Rotation:
                    cannonBall.CannonPillar.rotation = targetRotation;
                    cannonBall.ShotAngle = cannonBall.currentShotAngle;
                }

                //Draw Live Trajectory During Oscillation:
                DrawTrajectoryRuntime(cannonBall);

                //Check For Player Input To Confirm Launch:
                if (InputManager.Instance.InputActions.Player.Jump.triggered)
                {
                    //Reset Camera If Using Cannon Camera:
                    if (cannonBall.UseCamera) CameraManager.Instance.ResetActiveCamera();

                    cannonBall.ShotAngle = cannonBall.currentShotAngle;
                    _isComplete = true;
                }

                yield return null;
            }
        }

        #region Trajectory renderer
        private void SetupTrajectoryRenderer()
        {
            if (_trajectoryRenderer != null) return;

            //Create New GameObject With LineRenderer Component:
            GameObject go = new GameObject("CannonTrajectory_Renderer");
            _trajectoryRenderer = go.AddComponent<LineRenderer>();

            _trajectoryRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _trajectoryRenderer.startColor = Color.red;
            _trajectoryRenderer.endColor = Color.red;
            _trajectoryRenderer.startWidth = 0.05f;
            _trajectoryRenderer.endWidth = 0.05f;
            _trajectoryRenderer.positionCount = 0;
        }

        private void DrawTrajectoryRuntime(CannonBall cannonBall)
        {
            if (cannonBall == null || _trajectoryRenderer == null) return;

            //Initial Parameters:
            Vector3 startPosition = cannonBall.CannonBase.position;
            Vector3 forward = cannonBall.LaunchDirection ? cannonBall.LaunchDirection.forward : _context.transform.forward;

            //Calculate Initial Velocity Vector Based On Angle & Launch Velocity:
            Quaternion angleRotation = Quaternion.AngleAxis(cannonBall.ShotAngle, cannonBall.CannonBase.right);
            Vector3 direction = angleRotation * forward;
            Vector3 velocity = direction.normalized * -cannonBall.LaunchVelocity;

            //Get Trajectory Points Settings:
            float timeStep = cannonBall.TimeStep;
            int numPoints = Mathf.Max(2, cannonBall.NumPoints);
            Vector3[] points = new Vector3[numPoints];

            //Calculate Each Point In The Trajectory:
            for (int i = 0; i < numPoints; i++)
            {
                float t = i * timeStep;
                Vector3 calculatedPosition = startPosition + (velocity * t);
                calculatedPosition.y += (0.5f * cannonBall.Acceleration * (t * t));
                points[i] = calculatedPosition;
            }

            //Set Positions In Line Renderer:
            _trajectoryRenderer.positionCount = numPoints;
            _trajectoryRenderer.SetPositions(points);
        }
        #endregion

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
