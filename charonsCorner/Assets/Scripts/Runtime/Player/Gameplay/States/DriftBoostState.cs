using MoreMountains.Feedbacks;
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

        [Header("Camera Shake")]
        [SerializeField] private float _maxCameraShakeDuration = 1f;
        [SerializeField] private float _maxCameraShakeAmplitude = 10f;
        [SerializeField] private float _maxCameraShakeFrequency = 10f;
        [SerializeField] private MMChannelData _cameraShakeChannel;
        
        public bool IsComplete { get; private set; } // to break out of super state
        private float _timer;

        private protected override void OnEnter()
        {
            _context.CurrentSubState = GetType().Name; // for debug
            
            _timer = 0f;

            Vector3 driftDir = GetDriftDirection();
            
            // If grounded, project the drift direction onto the ground/slope plane
            if (_context.IsGrounded)
            {
                Vector3 groundNormal = Vector3.up;
                if (_context.SlopeSensor != null && _context.SlopeSensor.Hit.normal != Vector3.zero)
                {
                    groundNormal = _context.SlopeSensor.Hit.normal;
                }
                
                driftDir = Vector3.ProjectOnPlane(driftDir, groundNormal).normalized;
            }
            
            Vector3 initialVel = _context.DriftSuperState.InitialVelocity;
            Vector3 currentDir = driftDir; // if not moving, treat as aligned -> angle 0
            if (initialVel.sqrMagnitude > 0.0001f)
                currentDir = initialVel.normalized; // if moving use our initial vel

            float angle = Vector3.Angle(currentDir, driftDir);
            float boostAmountMultiplier = Mathf.Clamp01(angle / Mathf.Max(0.0001f, MaxAngle)); // Fraction of maxAngle (clamped). Do NOT use look direction.

            // Boost scales only with angle (plus optional initial dash). Caps at _maxBoostAmount.
            _boostAmount = Mathf.Min(_maxBoostAmount, (_maxBoostAmount * boostAmountMultiplier) + _initialDashSpeed);
            _context.Rb.AddForce(driftDir * _boostAmount, ForceMode.VelocityChange);

            // Juice
            _context.DriftFeedbacks.PlayFeedbacks();

            if (_context.DriftHandler != null)
            {
                _context.DriftHandler.PlayPersistentDriftParticles();
            }
            
            float boostAmountNormalized = _boostAmount / _maxBoostAmount;

            if (_context.PlayerSpeedFovChanger != null)
            {
                _context.PlayerSpeedFovChanger.TriggerDriftPulse(boostAmountMultiplier);
            }

            MMCameraShakeEvent.Trigger(
                _maxCameraShakeDuration * boostAmountNormalized,
                _maxCameraShakeAmplitude * boostAmountNormalized,
                _maxCameraShakeFrequency * boostAmountNormalized,
                0f, 0f, 0f, false, _cameraShakeChannel);
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
