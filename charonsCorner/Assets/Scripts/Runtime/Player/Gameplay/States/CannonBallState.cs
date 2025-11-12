using UnityEngine;
using System.Collections;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class CannonBallState : State<GameplayPlayerController>
    {
        //Refrence to CannonBall Object In Use:
        private CannonBall _currentCannon;

        //Used as the Variable to hold deltaTime When traveling Across Arc:
        private float _launchTimer;

        //Holds Coroutine that Lerps to CannonBase:
        private Coroutine _lerpRoutine;

        private bool _inCannon;
        private bool _isLaunching;
        //Public Method's Used to Stop Trigger Loops In "CannonBallTouchable.cs":
        public bool IsInCannon => _inCannon;
        public bool IsLaunching => _isLaunching;

        //Used to Call Cannon Axis Aiming:
        private bool _isAdjustingPillar = false;
        public bool IsAdjustingPillar => _isAdjustingPillar;

        //Used to Call State Transition:
        private bool _launchCompleted;
        public bool LaunchCompleted => _launchCompleted;

        private Vector3 _targetVelocity;

        private LineRenderer _trajectoryRenderer;


        private protected override void OnEnter()
        {
            _launchTimer = 0f;

            _inCannon = false;
            _isLaunching = false;

            _launchCompleted = false;
        }

        private protected override void OnExit()
        {
            if (_lerpRoutine != null) _context.StopCoroutine(_lerpRoutine);

            _inCannon = false;
            _isLaunching = false;

            _currentCannon = null;

            _context.Rb.isKinematic = false;
        }

        private protected override void OnUpdate()
        {
            if (_isLaunching && _currentCannon != null)
            {
                ApplyCannonLaunch(Time.deltaTime);
            }


            if (_isAdjustingPillar && _currentCannon != null)
            {
                DrawTrajectoryRuntime(_currentCannon);
            }
            else if (_trajectoryRenderer != null)
            {
                _trajectoryRenderer.positionCount = 0;
            }
        }

        private protected override void OnFixedUpdate()
        {
            if (_isLaunching)
            {
                _context.Rb.linearVelocity = _targetVelocity;
            }
        }

        //Public Because It Is Called Within "CannonBallTouchable":
        public void SetCannonReference(CannonBall cannon)
        {
            //Sets Cannon Refrence:
            _currentCannon = cannon;
            if (_currentCannon == null) return;

            //Disable Movement, Physics, & Begin Lerping Coroutine:
            _context.Rb.linearVelocity = Vector3.zero;
            _context.Rb.isKinematic = true;
            _lerpRoutine = _context.StartCoroutine(LerpToCannonBase(_currentCannon));
        }

        //Lerping Coroutine:
        private IEnumerator LerpToCannonBase(CannonBall cannonBall)
        {
            Debug.Log("Moving Towards CannoN!!");

            _inCannon = true;

            float time = 0f;

            Vector3 startPos = _context.transform.position;
            Quaternion startRot = _context.transform.rotation;

            Vector3 targetPos = cannonBall.CannonBase.position;
            Quaternion targetRot = Quaternion.LookRotation(cannonBall.LaunchDirection.forward, Vector3.up);

            float loadDuration = cannonBall.ShotLoadTime;

            //Lerp The Player -> CannonBall Base Object (Visually Storing them In):
            while (time < 1f)
            {
                time += Time.deltaTime / loadDuration;
                _context.transform.position = Vector3.Lerp(startPos, targetPos, time);
                _context.transform.rotation = Quaternion.Slerp(startRot, targetRot, time);

                //Set The Pillar Rotation To Match That Of The Shot Angle:
                if (cannonBall.CannonPillar != null)
                {
                    Vector3 forward = cannonBall.LaunchDirection.forward;
                    Vector3 launchDir = Quaternion.AngleAxis(-cannonBall.ShotAngle, cannonBall.CannonBase.right) * forward;
                    Quaternion targetPillarRot = Quaternion.LookRotation(launchDir, cannonBall.CannonBase.up);
                }

                yield return null;
            }

            //Moving Pillar == Set Axis Aiming Cannon:
            if (cannonBall.MovingPillar)
            {
                _inCannon = false;

                _isAdjustingPillar = true;

                //Begin Cannon Axis Aiming Coroutine:
                _context.StartCoroutine(PillarAngleOscillation(cannonBall));
            }
            //Normal Cannon Launch:
            else
            {
                _inCannon = false;

                //Makes ApplyCannonLaunch() Ready To Be Called:
                if (cannonBall.UseCamera) CameraManager.Instance.ResetActiveCamera();
                _isLaunching = true;

                _launchTimer = 0f;

                //Enable Rb Physics:
                _context.Rb.isKinematic = false;
            }
        }

        //Cannon Axis Aiming Coroutine:
        private IEnumerator PillarAngleOscillation(CannonBall cannonBall)
        {
            float elapsed = 0f;
            float speed = cannonBall.PillarSpeed;

            //Perpendicular Angle Used to Fix Any Offsets with Object & Arc:
            float perpendicularAngle = 90f;

            while (_isAdjustingPillar)
            {
                elapsed += Time.deltaTime * speed;

                //Oscillate Back <-> Forth & Set The Current Angle Based On Updating Position:
                float angle = Mathf.Lerp(cannonBall.ShotAngleMin, cannonBall.ShotAngleMax, Mathf.PingPong(elapsed, 1f));
                cannonBall.currentShotAngle = angle;

                if (cannonBall.CannonPillar != null)
                {
                    Vector3 forward = cannonBall.LaunchDirection.forward;

                    //Fix Visual & Calculated Offsets:
                    float adjustedAngle = angle - perpendicularAngle;

                    Quaternion angleRotation = Quaternion.AngleAxis(adjustedAngle, cannonBall.CannonBase.right);
                    Vector3 direction = angleRotation * forward;
                    Quaternion targetRotation = Quaternion.LookRotation(direction, cannonBall.CannonBase.up);

                    cannonBall.CannonPillar.rotation = targetRotation;
                    cannonBall.ShotAngle= cannonBall.currentShotAngle;
                }

                //Temporary Key Press To Test Functionality For Launch:
                if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
                {
                    cannonBall.ShotAngle= cannonBall.currentShotAngle;
                    _isAdjustingPillar = false;

                    //Makes ApplyCannonLaunch() Ready To Be Called:
                    if (cannonBall.UseCamera) CameraManager.Instance.ResetActiveCamera();
                    _isLaunching = true;

                    _launchTimer = 0f;

                    //Enable Rb Physics:
                    _context.Rb.isKinematic = false;

                    yield break;
                }

                yield return null;
            }
        }

        //Calculates Projectile Motion Arc:
        private void ApplyCannonLaunch(float deltaTime)
        {
            //Enable Rb Physics:
            if (_context.Rb.isKinematic) _context.Rb.isKinematic = false;
            
            _launchTimer += deltaTime * _currentCannon.ShotPower;
            float time = _launchTimer;

            Vector3 startPos = _currentCannon.CannonBase.position;

            Vector3 forward = _currentCannon.LaunchDirection.forward;
            Quaternion angleRot = Quaternion.AngleAxis(_currentCannon.ShotAngle, _currentCannon.CannonBase.right);
            Vector3 launchDir = angleRot * forward;

            Vector3 initialVelocity = launchDir.normalized * -_currentCannon.LaunchVelocity;

            //Applying 1st Order [Velocity/Slope]:
            Vector3 displacement = (initialVelocity * time);

            //Applying 2nd Order [Acceleration/Curvature]:
            displacement.y += (0.5f * _currentCannon.Acceleration * time * time) + _currentCannon.CurrentHeight;

            //Calculate a Time Step:
            Vector3 targetPosition = startPos + displacement;

            //Move To Time Step Position:
            _context.transform.position = targetPosition;

            //Player Forward Following the Arc:
            _context.transform.rotation = Quaternion.LookRotation(launchDir, Vector3.up);
     
            float verticalVelocity = initialVelocity.y + _currentCannon.Acceleration * time;

            // Set the velocity for physics to continue the arc
            _targetVelocity = initialVelocity;
            _targetVelocity.y = verticalVelocity;
            
            //If We Are Past The Peak of Ark:
            if (verticalVelocity <= 0f)
            { 
                //Variable to Change State:
                if (_context.IsGrounded) _launchCompleted = true;
                return; // Stop manually setting position
            }
        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            return null;
        }

        //Setting Up Line Render Component For Visibility While In Cannon Axis Aiming:
        private void SetupTrajectoryRenderer()
        {
            if (_trajectoryRenderer != null) return;

            GameObject lineObj = new GameObject("TrajectoryRenderer");
            _trajectoryRenderer = lineObj.AddComponent<LineRenderer>();
            _trajectoryRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _trajectoryRenderer.startColor = Color.red;
            _trajectoryRenderer.endColor = Color.red;
            _trajectoryRenderer.startWidth = 0.05f;
            _trajectoryRenderer.endWidth = 0.05f;
            _trajectoryRenderer.positionCount = 0;
        }

        //Calculate the Arc Similar to "CannonBall.cs" and ApplyCannonLaunch() Versions:
        private void DrawTrajectoryRuntime(CannonBall cannonBall)
        {
            if (cannonBall == null || !IsAdjustingPillar) return;
            SetupTrajectoryRenderer();

            Vector3 startPosition = cannonBall.CannonBase.position;
            Vector3 forward = cannonBall.LaunchDirection ? cannonBall.LaunchDirection.forward : _context.transform.forward;

            Quaternion angleRotation = Quaternion.AngleAxis(cannonBall.ShotAngle, cannonBall.CannonBase.right);
            Vector3 direction = angleRotation * forward;
            Vector3 velocity = direction.normalized * -cannonBall.LaunchVelocity;

            float timeStep = cannonBall.TimeStep;
            int numPoints = cannonBall.NumPoints;

            Vector3[] points = new Vector3[numPoints];
            Vector3 previousPosition = startPosition;

            for (int i = 0; i < numPoints; i++)
            {
                float t = i * timeStep;
                Vector3 calculatedPosition = startPosition + (velocity * t);
                calculatedPosition.y += (0.5f * cannonBall.Acceleration* (t * t));
                points[i] = calculatedPosition;
            }

            _trajectoryRenderer.positionCount = numPoints;
            _trajectoryRenderer.SetPositions(points);
        }
    }
}
