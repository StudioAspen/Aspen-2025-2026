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

            Vector3 targetPos = cannonBall.cannonBase.position;
            Quaternion targetRot = Quaternion.LookRotation(cannonBall.launchDirection.forward, Vector3.up);

            float loadDuration = cannonBall.shotLoadTime;

            //Lerp The Player -> CannonBall Base Object (Visually Storing them In):
            while (time < 1f)
            {
                time += Time.deltaTime / loadDuration;
                _context.transform.position = Vector3.Lerp(startPos, targetPos, time);
                _context.transform.rotation = Quaternion.Slerp(startRot, targetRot, time);

                //Set The Pillar Rotation To Match That Of The Shot Angle:
                if (cannonBall.cannonPillar != null)
                {
                    Vector3 forward = cannonBall.launchDirection.forward;
                    Vector3 launchDir = Quaternion.AngleAxis(-cannonBall.shotAngle, cannonBall.cannonBase.right) * forward;
                    Quaternion targetPillarRot = Quaternion.LookRotation(launchDir, cannonBall.cannonBase.up);
                }

                yield return null;
            }

            //Moving Pillar == Set Axis Aiming Cannon:
            if (cannonBall.movingPillar)
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
                if (cannonBall._useCamera) CameraManager.Instance.ResetActiveCamera();
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
            float speed = cannonBall.pillarSpeed;

            //Perpendicular Angle Used to Fix Any Offsets with Object & Arc:
            float perpendicularAngle = 90f;

            while (_isAdjustingPillar)
            {
                elapsed += Time.deltaTime * speed;

                //Oscillate Back <-> Forth & Set The Current Angle Based On Updating Position:
                float angle = Mathf.Lerp(cannonBall.shotAngleMin, cannonBall.shotAngleMax, Mathf.PingPong(elapsed, 1f));
                cannonBall.currentShotAngle = angle;

                if (cannonBall.cannonPillar != null)
                {
                    Vector3 forward = cannonBall.launchDirection.forward;

                    //Fix Visual & Calculated Offsets:
                    float adjustedAngle = angle - perpendicularAngle;

                    Quaternion angleRotation = Quaternion.AngleAxis(adjustedAngle, cannonBall.cannonBase.right);
                    Vector3 direction = angleRotation * forward;
                    Quaternion targetRotation = Quaternion.LookRotation(direction, cannonBall.cannonBase.up);

                    cannonBall.cannonPillar.rotation = targetRotation;
                    cannonBall.shotAngle = cannonBall.currentShotAngle;
                }

                //Temporary Key Press To Test Functionality For Launch:
                if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
                {
                    cannonBall.shotAngle = cannonBall.currentShotAngle;
                    _isAdjustingPillar = false;

                    //Makes ApplyCannonLaunch() Ready To Be Called:
                    if (cannonBall._useCamera) CameraManager.Instance.ResetActiveCamera();
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
            _launchTimer += deltaTime * _currentCannon.shotPower;
            float time = _launchTimer;

            Vector3 startPos = _currentCannon.cannonBase.position;

            Vector3 forward = _currentCannon.launchDirection.forward;
            Quaternion angleRot = Quaternion.AngleAxis(_currentCannon.shotAngle, _currentCannon.cannonBase.right);
            Vector3 launchDir = angleRot * forward;

            Vector3 initialVelocity = launchDir.normalized * -_currentCannon.launchVelocity;

            //Applying 1st Order [Velocity/Slope]:
            Vector3 displacement = (initialVelocity * time);

            //Applying 2nd Order [Acceleration/Curvature]:
            displacement.y += (0.5f * _currentCannon.acceleration * time * time) + _currentCannon.currentHeight;

            //Calculate a Time Step:
            Vector3 targetPosition = startPos + displacement;

            //Move To Time Step Position:
            _context.transform.position = targetPosition;

            //Player Forward Following the Arc:
            _context.transform.rotation = Quaternion.LookRotation(launchDir, Vector3.up);
     
            float verticalVelocity = initialVelocity.y + _currentCannon.acceleration * time;
            //If We Are Past The Peak of Ark:
            if (verticalVelocity <= 0f)
            {
                //Enable Rb Physics:
                if (_context.Rb.isKinematic) _context.Rb.isKinematic = false;

                //Variable to Change State:
                if (_context.IsGrounded) _launchCompleted = true;
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

            Vector3 startPosition = cannonBall.cannonBase.position;
            Vector3 forward = cannonBall.launchDirection ? cannonBall.launchDirection.forward : _context.transform.forward;

            Quaternion angleRotation = Quaternion.AngleAxis(cannonBall.shotAngle, cannonBall.cannonBase.right);
            Vector3 direction = angleRotation * forward;
            Vector3 velocity = direction.normalized * -cannonBall.launchVelocity;

            float timeStep = cannonBall.timeStep;
            int numPoints = cannonBall.numPoints;

            Vector3[] points = new Vector3[numPoints];
            Vector3 previousPosition = startPosition;

            for (int i = 0; i < numPoints; i++)
            {
                float t = i * timeStep;
                Vector3 calculatedPosition = startPosition + (velocity * t);
                calculatedPosition.y += (0.5f * cannonBall.acceleration * (t * t));
                points[i] = calculatedPosition;
            }

            _trajectoryRenderer.positionCount = numPoints;
            _trajectoryRenderer.SetPositions(points);
        }
    }
}
