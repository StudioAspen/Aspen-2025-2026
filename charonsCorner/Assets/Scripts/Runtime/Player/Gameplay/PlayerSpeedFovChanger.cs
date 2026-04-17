using System;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PlayerSpeedFovChanger : MonoBehaviour
    {
        [SerializeField, Required] private GameplayPlayerController _playerController;
        
        [Header("FOV")]
        [SerializeField] private FloatRange _fovRange = new FloatRange(75, 100);
        [SerializeField] private Ease _lerpCurveEase = Ease.InCubic;
        [SerializeField] private float _lerpSpeed = 15f;

        [Header("Speed Lines")]
        [SerializeField] private Transform _speedLinesObject;
        [SerializeField] private FloatRange _speedLinesZRange = new FloatRange(0f, 10f);
        [SerializeField] private FloatRange _brakeDashZRange = new FloatRange(10f, 20f);
        [SerializeField] private float _pulseDuration = 0.5f;

        [Header("Trail")]
        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private FloatRange _trailWidthRange = new FloatRange(0.5f, 1.5f);
        [SerializeField] private AnimationCurve _trailWidthCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private FloatRange _trailLengthRange = new FloatRange(0.1f, 0.5f);
        [SerializeField] private AnimationCurve _minWidthCurve = AnimationCurve.Linear(0, 1, 1, 0);
        [SerializeField] private AnimationCurve _maxWidthCurve = AnimationCurve.Linear(0, 1, 1, 0);

        private float _pulseTimer;
        private float _pulseIntensity;
        private float _startPulseZ;
        private AnimationCurve _workingWidthCurve = new AnimationCurve();

        private void Start()
        {
            if (_playerController != null)
            {
                _playerController.PlayerSpeedFovChanger = this;
            }
        }

        public void TriggerDriftPulse(float chargeRatio)
        {
            if (_speedLinesObject == null) return;
            
            _pulseIntensity = chargeRatio;
            _pulseTimer = _pulseDuration;
            _startPulseZ = _speedLinesObject.localPosition.z;
        }

        private void Update()
        {
            float playerSpeed = _playerController.Rb.linearVelocity.magnitude;
            float parameter = DOVirtual.EasedValue(0f, 1f, playerSpeed / _playerController.GroundSuperState.MoveState.MaxSpeed, _lerpCurveEase);
            float targetFov = _fovRange.Lerp(parameter);
            
            _playerController.PlayerCamera.Lens.FieldOfView = Mathf.Lerp(_playerController.PlayerCamera.Lens.FieldOfView, targetFov, _lerpSpeed * Time.deltaTime);

            if (_speedLinesObject != null)
            {
                float targetZ = _speedLinesZRange.Lerp(parameter);
                
                if (_pulseTimer > 0)
                {
                    _pulseTimer -= Time.deltaTime;
                    float pulseTargetZ = _brakeDashZRange.Lerp(_pulseIntensity);
                    float pulseWeight = _pulseTimer / _pulseDuration;
                    // We want to pulse TO the target and then lerp back.
                    // The request says: "pulse the Z-Axis ... and then lerp back to normal Z-Axis Speed based adjustment over the course of X seconds"
                    targetZ = Mathf.Lerp(targetZ, pulseTargetZ, pulseWeight);
                }

                Vector3 localPos = _speedLinesObject.localPosition;
                localPos.z = Mathf.Lerp(localPos.z, targetZ, _lerpSpeed * Time.deltaTime);
                _speedLinesObject.localPosition = localPos;
            }

            if (_trailRenderer != null)
            {
                float trailWidth = _trailWidthRange.Lerp(_trailWidthCurve.Evaluate(parameter));
                _trailRenderer.widthMultiplier = Mathf.Lerp(_trailRenderer.widthMultiplier, trailWidth, _lerpSpeed * Time.deltaTime);

                float trailLength = _trailLengthRange.Lerp(parameter);
                _trailRenderer.time = Mathf.Lerp(_trailRenderer.time, trailLength, _lerpSpeed * Time.deltaTime);

                LerpAnimationCurve(_minWidthCurve, _maxWidthCurve, parameter, _workingWidthCurve);
                _trailRenderer.widthCurve = _workingWidthCurve;
            }
        }

        private void LerpAnimationCurve(AnimationCurve a, AnimationCurve b, float t, AnimationCurve result)
        {
            if (a.length != b.length)
            {
                // If they have different number of keys, we can't easily lerp keys.
                // Fallback: just use one or the other based on threshold, or don't lerp.
                // But typically users will provide identical structures.
                // For now, let's just copy one if they differ.
                result.keys = t < 0.5f ? a.keys : b.keys;
                return;
            }

            Keyframe[] keys = a.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].time = Mathf.Lerp(a.keys[i].time, b.keys[i].time, t);
                keys[i].value = Mathf.Lerp(a.keys[i].value, b.keys[i].value, t);
                keys[i].inTangent = Mathf.Lerp(a.keys[i].inTangent, b.keys[i].inTangent, t);
                keys[i].outTangent = Mathf.Lerp(a.keys[i].outTangent, b.keys[i].outTangent, t);
                keys[i].inWeight = Mathf.Lerp(a.keys[i].inWeight, b.keys[i].inWeight, t);
                keys[i].outWeight = Mathf.Lerp(a.keys[i].outWeight, b.keys[i].outWeight, t);
            }
            result.keys = keys;
        }
    }
}