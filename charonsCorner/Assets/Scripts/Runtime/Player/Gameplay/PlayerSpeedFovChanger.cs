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

        private float _pulseTimer;
        private float _pulseIntensity;
        private float _startPulseZ;

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
        }
    }
}