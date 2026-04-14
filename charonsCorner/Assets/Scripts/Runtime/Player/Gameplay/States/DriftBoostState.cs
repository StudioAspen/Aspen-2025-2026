using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class DriftBoostState : State<GameplayPlayerController>
    {
        [Header("Drift Settings")]
        [field: SerializeField] public float MaxAngle { get; private set; } = 90f;

        [SerializeField, ReadOnly] private float _boostAmount;
        [SerializeField] private float _maxBoostAmount = 50f;
        [SerializeField, Tooltip("Base initial dash speed added regardless of angle")] private float _initialDashSpeed = 10f;
        
        [Header("Camera Distance")]
        [SerializeField] private float _cameraChangeDistanceSpeed = 10f;
        
        [Header("Time Scale")]
        [SerializeField] private float _timeScaleSpeed = 20f;
        [SerializeField] private float _stateDuration = 0.4f;

        [Header("Drift Boost VFX")]
        [SerializeField, Required] private GameObject _boostVFX;
        
        [Header("Camera Shake")]
        [SerializeField] private float _maxCameraShakeDuration = 1f;
        [SerializeField] private float _maxCameraShakeAmplitude = 10f;
        [SerializeField] private float _maxCameraShakeFrequency = 10f;
        
        public bool IsComplete { get; private set; } // to break out of super state
        private float _timer;

        private protected override void OnEnter()
        {
            _context.CurrentSubState = GetType().Name; // for debug
            
            _timer = 0f;

            Vector3 driftDir = GetDriftDirection();
            
            Vector3 currentVel = _context.Rb.linearVelocity.WithY(0);
            Vector3 currentDir = driftDir; // if not moving, treat as aligned -> angle 0
            if (currentVel.sqrMagnitude > 0.0001f)
                currentDir = currentVel.normalized; // if moving use our curr vel

            float angle = Vector3.Angle(currentDir, driftDir);
            float boostAmountMultiplier = Mathf.Clamp01(angle / Mathf.Max(0.0001f, MaxAngle)); // Fraction of maxAngle (clamped). Do NOT use look direction.

            // Boost scales only with angle (plus optional initial dash). Caps at _maxBoostAmount.
            _boostAmount = Mathf.Min(_maxBoostAmount, (_maxBoostAmount * boostAmountMultiplier) + _initialDashSpeed);
            _context.Rb.AddForce(driftDir * _boostAmount, ForceMode.VelocityChange);

            // Juice
            _context.DriftFeedbacks.PlayFeedbacks();
            _boostVFX.SetActive(true);
            float boostAmountNormalized = _boostAmount / _maxBoostAmount;

            if (CameraManager.Instance != null && CameraManager.Instance.CameraShaker != null)
            {
                CameraManager.Instance.CameraShaker.ShakeCamera(
                    _maxCameraShakeAmplitude * boostAmountNormalized,
                    _maxCameraShakeFrequency * boostAmountNormalized,
                    _maxCameraShakeDuration * boostAmountNormalized
                );
            }
        }

        private protected override void OnExit()
        {
            IsComplete = false;
            
            Time.timeScale = 1;
            
            // Reset juice
            if (_context.DriftSuperState.CameraOrbitalFollow != null)
            {
                _context.DriftSuperState.CameraOrbitalFollow.Radius = _context.DriftSuperState.CameraBaseOrbitalDistance;
            }

            _context.CameraTargetFollowTarget.SetPositionOffset(Vector3.zero);
            _boostVFX.SetActive(false);
        }

        private protected override void OnUpdate()
        {
            if (_context.DriftSuperState.CameraOrbitalFollow != null)
            {
                _context.DriftSuperState.CameraOrbitalFollow.Radius = Mathf.Lerp(
                    _context.DriftSuperState.CameraOrbitalFollow.Radius,
                    _context.DriftSuperState.CameraBaseOrbitalDistance, _cameraChangeDistanceSpeed * Time.unscaledDeltaTime);
            }

            _context.CameraTargetFollowTarget.SetPositionOffset(
                Vector3.zero.WithY(Mathf.Lerp(_context.CameraTargetFollowTarget.PositionOffset.y, 0, _cameraChangeDistanceSpeed * Time.unscaledDeltaTime))
                );
            
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, _timeScaleSpeed * Time.unscaledDeltaTime);
            _timer += Time.unscaledDeltaTime;
            
            IsComplete = _timer >= _stateDuration; // will trigger super state transition to end drifting
        }

        private protected override void OnFixedUpdate()
        {

        }

        public Vector3 GetDriftDirection()
        {
            Transform camTransform = null;
            if (CameraManager.Instance != null && CameraManager.Instance.CurrentCamera != null)
            {
                camTransform = CameraManager.Instance.CurrentCamera.transform;
            }
            else if (_context.PlayerCamera != null)
            {
                camTransform = _context.PlayerCamera.transform;
            }
            else if (Camera.main != null)
            {
                camTransform = Camera.main.transform;
            }

            if (camTransform == null)
            {
                Debug.LogWarning("No camera found for drift direction. Falling back to player orientation.");
                return _context.Orientation.forward;
            }

            // Use camera if kb/mouse
            if (InputManager.Instance.CurrentControlScheme == InputManager.ControlScheme.KeyboardMouse)
                return camTransform.forward.WithY(0).normalized;
            
            // Use stick dir if gamepad
            return CustomUtils.GetCameraBasedMoveInput(camTransform,
                InputManager.Instance.MoveDirection);
        }
    }
}
