using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class DriftBoostState : State<GameplayPlayerController>
    {
        [Header("Drift Settings")]
        [SerializeField] private Transform _driftDirection;
        [SerializeField] private float _maxAngle = 90f;

        [SerializeField] private float _boostAmount;
        [SerializeField] private float _maxBoostAmount = 50f;
        [SerializeField, Tooltip("Base initial dash speed added regardless of angle")]
        private float _initialDashSpeed = 0f;
        
        [Header("Camera FOV")]
        [SerializeField] private float _baseFOV = 75f;
        [SerializeField] private float _fovSpeed = 10f;
        
        [Header("Time Scale")]
        [SerializeField] private float _timeScaleSpeed = 20f;
        
        [SerializeField] private float _stateDuration = 0.4f;
        public bool IsComplete { get; private set; }
        private float _timer;

        private protected override void OnEnter()
        {
            _timer = 0f;
            _context.CurrentSubState = GetType().Name;
            Vector3 currentVel = _context.Rb.linearVelocity;
            Vector3 currentDir;
            if (currentVel.sqrMagnitude > 0.0001f)
                currentDir = currentVel.normalized;
            else
                currentDir = _driftDirection.forward; // if not moving, treat as aligned -> angle 0

            float angle = Vector3.Angle(currentDir, _driftDirection.forward);

            // Fraction of maxAngle (clamped). Do NOT use look direction.
            float frac = Mathf.Clamp01(angle / Mathf.Max(0.0001f, _maxAngle));

            // Boost scales only with angle (plus optional initial dash). Caps at _maxBoostAmount.
            _boostAmount = Mathf.Min(_maxBoostAmount, (_maxBoostAmount * frac) + _initialDashSpeed);

            _context.Rb.AddForce(_driftDirection.forward * _boostAmount, ForceMode.VelocityChange);
        }

        private protected override void OnExit()
        {
            _driftDirection.localEulerAngles = Vector3.zero;
            _driftDirection.gameObject.SetActive(false);
            _context.PlayerCamera.Lens.FieldOfView = _baseFOV;
            Time.timeScale = 1;
            IsComplete = false;
        }

        private protected override void OnUpdate()
        {
            _context.PlayerCamera.Lens.FieldOfView = Mathf.Lerp(_context.PlayerCamera.Lens.FieldOfView, _baseFOV, _fovSpeed * Time.unscaledDeltaTime);
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, _timeScaleSpeed * Time.unscaledDeltaTime);
            _timer += Time.unscaledDeltaTime;
            IsComplete = _timer >= _stateDuration;
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            return null;
        }
    }
}
