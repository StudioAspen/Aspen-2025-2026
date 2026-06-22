using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class DriftingChargeState : State<GameplayPlayerController>
    {
        [Header("Camera Distance")]
        [SerializeField] private float _driftDesiredDistance = 7.5f;
        [SerializeField] private float _cameraDistanceChangeSpeed = 5;
        [SerializeField] private float _cameraTargetHeight = 1f;
        [SerializeField] private float _cameraRealignSpeed = 5f;
        
        [SerializeField] private float _slowPlayerStrength = 5f;
        [SerializeField] private float _groundStickForce = 10f;

        [SerializeField] private float _stateDuration = 0.5f;  
        private float _timer;
        private float _targetYaw;
        private bool _isLerpingCamera;

        private protected override void OnEnter()
        {
            _context.CurrentSubState = GetType().Name; // for debug
            
            _timer = 0;
            _isLerpingCamera = false;

            if (InputManager.Instance.CurrentControlScheme != InputManager.ControlScheme.KeyboardMouse && 
                _context.DriftSuperState.CameraOrbitalFollow != null && _context.DriftSuperState.InitialVelocity.sqrMagnitude > 0.01f)
            {
                _targetYaw = Quaternion.LookRotation(_context.DriftSuperState.InitialVelocity).eulerAngles.y;
                _isLerpingCamera = true;
            }
        }

        private protected override void OnExit()
        {
            
        }

        private protected override void OnUpdate()
        {
            _timer += Time.deltaTime;

            if (_isLerpingCamera && _context.DriftSuperState.CameraOrbitalFollow != null)
            {
                // Check if user is trying to manually move the camera
                float horizontalInput = InputManager.Instance.LookDirection.x;
                if (Mathf.Abs(horizontalInput) > 0.01f)
                {
                    _isLerpingCamera = false;
                }
            }

            if (_context.DriftSuperState.CameraOrbitalFollow != null)
            {
                _context.DriftSuperState.CameraOrbitalFollow.Radius = Mathf.Lerp(
                    _context.DriftSuperState.CameraOrbitalFollow.Radius, _driftDesiredDistance,
                    _cameraDistanceChangeSpeed * Time.deltaTime);

                if (_isLerpingCamera)
                {
                    float currentYaw = _context.DriftSuperState.CameraOrbitalFollow.HorizontalAxis.Value;
                    float newYaw = Mathf.LerpAngle(currentYaw, _targetYaw, _cameraRealignSpeed * Time.deltaTime);
                    _context.DriftSuperState.CameraOrbitalFollow.HorizontalAxis.Value = newYaw;

                    if (Mathf.Abs(Mathf.DeltaAngle(newYaw, _targetYaw)) < 0.1f)
                    {
                        _isLerpingCamera = false;
                    }
                }
            }

            _context.CameraTargetFollowTarget.SetPositionOffset(Vector3.zero.WithY(Mathf.Lerp(_context.CameraTargetFollowTarget.PositionOffset.y, _cameraTargetHeight, _cameraDistanceChangeSpeed * Time.deltaTime)));
        }

        private protected override void OnFixedUpdate()
        {
            Vector3 velocity = _context.Rb.linearVelocity;
            Vector3 groundNormal = Vector3.up;
            bool isOnSlope = false;
            
            if (_context.IsGrounded && _context.SlopeSensor != null && _context.SlopeSensor.Hit.normal != Vector3.zero)
            {
                groundNormal = _context.SlopeSensor.Hit.normal;
                isOnSlope = true;
            }

            // Calculate "planar" velocity relative to the ground normal
            Vector3 planarVelocity = Vector3.ProjectOnPlane(velocity, groundNormal);

            // Apply counterforce (braking) along the ground plane
            Vector3 counterforce = -planarVelocity * _slowPlayerStrength;
            _context.Rb.AddForce(counterforce, ForceMode.Acceleration);

            // Apply ground stick force if grounded to prevent floating during charge
            if (_context.IsGrounded)
            {
                _context.Rb.AddForce(-groundNormal * _groundStickForce, ForceMode.Acceleration);
            }
        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (_timer >= _stateDuration)
                return _context.DriftSuperState.DriftingBoostState;
            return null;
        }
    }
}
