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
        [SerializeField] private float _maxAngle = 90f;

        [SerializeField, ReadOnly] private float _boostAmount;
        [SerializeField] private float _maxBoostAmount = 50f;
        [SerializeField, Tooltip("Base initial dash speed added regardless of angle")] private float _initialDashSpeed = 10f;
        
        [Header("Camera FOV")]
        [SerializeField] private float _baseFOV = 75f;
        [SerializeField] private float _fovSpeed = 10f;
        
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
            
            Vector3 currentVel = _context.Rb.linearVelocity;
            Vector3 currentDir = driftDir; // if not moving, treat as aligned -> angle 0
            if (currentVel.sqrMagnitude > 0.0001f)
                currentDir = currentVel.normalized; // if moving use our curr vel

            float angle = Vector3.Angle(currentDir, driftDir);
            float boostAmountMultiplier = Mathf.Clamp01(angle / Mathf.Max(0.0001f, _maxAngle)); // Fraction of maxAngle (clamped). Do NOT use look direction.

            // Boost scales only with angle (plus optional initial dash). Caps at _maxBoostAmount.
            _boostAmount = Mathf.Min(_maxBoostAmount, (_maxBoostAmount * boostAmountMultiplier) + _initialDashSpeed);
            _context.Rb.AddForce(driftDir * _boostAmount, ForceMode.VelocityChange);

            // Juice
            _context.DriftFeedbacks.PlayFeedbacks();
            _boostVFX.SetActive(true);
            float boostAmountNormalized = _boostAmount / _maxBoostAmount;
            CameraManager.Instance.CameraShaker.ShakeCamera(
                _maxCameraShakeAmplitude * boostAmountNormalized,
                _maxCameraShakeFrequency * boostAmountNormalized, 
                _maxCameraShakeDuration * boostAmountNormalized
                );
        }

        private protected override void OnExit()
        {
            IsComplete = false;
            
            Time.timeScale = 1;
            
            // Reset juice
            _context.PlayerCamera.Lens.FieldOfView = _baseFOV;
            _boostVFX.SetActive(false);
        }

        private protected override void OnUpdate()
        {
            _context.PlayerCamera.Lens.FieldOfView = Mathf.Lerp(_context.PlayerCamera.Lens.FieldOfView, _baseFOV, _fovSpeed * Time.unscaledDeltaTime);
            _context.CameraTargetFollowTarget.SetPositionOffset(
                Vector3.zero.WithY(Mathf.Lerp(_context.CameraTargetFollowTarget.PositionOffset.y, 0, _fovSpeed * Time.unscaledDeltaTime))
                );
            
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, _timeScaleSpeed * Time.unscaledDeltaTime);
            _timer += Time.unscaledDeltaTime;
            
            IsComplete = _timer >= _stateDuration; // will trigger super state transition to end drifting
        }

        private protected override void OnFixedUpdate()
        {

        }

        private Vector3 GetDriftDirection()
        {
            // Use camera if kb/mouse
            if (InputManager.Instance.CurrentControlScheme == InputManager.ControlScheme.KeyboardMouse)
                return CameraManager.Instance.CurrentCamera.transform.forward.WithY(0).normalized;
            
            // Use stick dir if gamepad
            return CustomUtils.GetCameraBasedMoveInput(CameraManager.Instance.CurrentCamera.transform,
                InputManager.Instance.MoveDirection);
        }
    }
}
