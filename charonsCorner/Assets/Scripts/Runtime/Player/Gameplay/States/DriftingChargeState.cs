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
        
        [SerializeField] private float _slowPlayerStrength = 5f;
        [SerializeField] private float _groundStickForce = 10f;

        [SerializeField] private float _stateDuration = 0.5f;  
        private float _timer;

        private protected override void OnEnter()
        {
            _context.CurrentSubState = GetType().Name; // for debug
            
            _timer = 0;
        }

        private protected override void OnExit()
        {
            
        }

        private protected override void OnUpdate()
        {
            _timer += Time.deltaTime;

            if (_context.DriftSuperState.CameraOrbitalFollow != null)
            {
                _context.DriftSuperState.CameraOrbitalFollow.Radius = Mathf.Lerp(
                    _context.DriftSuperState.CameraOrbitalFollow.Radius, _driftDesiredDistance,
                    _cameraDistanceChangeSpeed * Time.deltaTime);
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
