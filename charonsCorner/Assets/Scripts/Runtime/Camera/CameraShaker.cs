using System;
using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Handles all camera shaking for Cinemachine cameras.
    /// Shake still persists even when switching cameras.
    /// </summary>
    public class CameraShaker
    {
        private CameraManager _cameraManager;

        private CinemachineBasicMultiChannelPerlin _cinemachineBasicMultiChannelPerlin;

        private float _startingAmplitude;
        private float _startingFrequency;
        private float _shakeTimer;
        private float _shakeDuration;

        public CameraShaker(CameraManager manager)
        {
            _cameraManager = manager;

            _cameraManager.OnActiveCameraChanged += CameraManager_OnActiveCameraChanged;
        }

        /// <summary>
        /// Shakes the camera with the specified amplitude, frequency, and duration.
        /// </summary>
        /// <param name="amplitude">How large the shakes are.</param>
        /// <param name="frequency">How often are the shakes.</param>
        /// <param name="duration">How long will the camera be shaking.</param>
        public void ShakeCamera(float amplitude, float frequency, float duration)
        {
            if(_cameraManager.CurrentCamera == null)
            {
                Debug.LogWarning("Current camera is not set. Cannot shake camera.");
                return;
            }

            _cinemachineBasicMultiChannelPerlin = _cameraManager.CurrentCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if(_cinemachineBasicMultiChannelPerlin == null )
            {
                Debug.LogWarning("CinemachineBasicMultiChannelPerlin component not found on the current camera. Cannot shake camera.");
                return;
            }

            _cinemachineBasicMultiChannelPerlin.ReSeed();

            _cinemachineBasicMultiChannelPerlin.AmplitudeGain = amplitude;
            _cinemachineBasicMultiChannelPerlin.FrequencyGain = frequency;
            _startingAmplitude = amplitude;
            _startingFrequency = frequency;
            _shakeDuration = duration;
            _shakeTimer = duration;
        }

        private void StopShake()
        {
            _cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0f;
            _cinemachineBasicMultiChannelPerlin.FrequencyGain = 0f;
            _shakeTimer = 0f;
        }

        public void Update()
        {
            if (_cinemachineBasicMultiChannelPerlin == null)
                return;

            if (_shakeTimer > 0)
            {
                _shakeTimer -= Time.deltaTime;

                _cinemachineBasicMultiChannelPerlin.AmplitudeGain =
                    Mathf.Lerp(_startingAmplitude, 0, 1 - (_shakeTimer / _shakeDuration));
                _cinemachineBasicMultiChannelPerlin.FrequencyGain =
                    Mathf.Lerp(_startingFrequency, 0, 1 - (_shakeTimer / _shakeDuration));
            }
            else
            {
                _cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0f;
                _cinemachineBasicMultiChannelPerlin.FrequencyGain = 0f;
            }
        }

        private void CameraManager_OnActiveCameraChanged(CinemachineCamera newCamera)
        {
            if (newCamera == null)
            {
                StopShake();
                return;
            }

            _cinemachineBasicMultiChannelPerlin = newCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (_cinemachineBasicMultiChannelPerlin == null)
            {
                StopShake();
                return;
            }
        }

        /// <summary>
        /// Properly handles the destroy lifecycle of this class.
        /// Unsub listeners from any actions here.
        /// </summary>
        public void Dispose()
        {
            _cameraManager.OnActiveCameraChanged -= CameraManager_OnActiveCameraChanged;
        }
    }
}
