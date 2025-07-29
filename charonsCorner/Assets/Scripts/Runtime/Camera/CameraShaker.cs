using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CameraShaker
    {
        private CameraManager cameraManager;

        private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;

        private float startingAmplitude;
        private float startingFrequency;
        private float shakeTimer;
        private float shakeDuration;

        public CameraShaker(CameraManager manager)
        {
            cameraManager = manager;
        }

        /// <summary>
        /// Shakes the camera with the specified amplitude, frequency, and duration.
        /// </summary>
        /// <param name="amplitude">How large the shakes are.</param>
        /// <param name="frequency">How often are the shakes.</param>
        /// <param name="duration">How long will the camera be shaking.</param>
        public void ShakeCamera(float amplitude, float frequency, float duration)
        {
            if(cameraManager.CurrentCamera == null)
            {
                Debug.LogWarning("Current camera is not set. Cannot shake camera.");
                return;
            }

            cinemachineBasicMultiChannelPerlin = cameraManager.CurrentCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if(cinemachineBasicMultiChannelPerlin == null )
            {
                Debug.LogWarning("CinemachineBasicMultiChannelPerlin component not found on the current camera. Cannot shake camera.");
                return;
            }

            cinemachineBasicMultiChannelPerlin.ReSeed();

            cinemachineBasicMultiChannelPerlin.AmplitudeGain = amplitude;
            cinemachineBasicMultiChannelPerlin.FrequencyGain = frequency;
            startingAmplitude = amplitude;
            startingFrequency = frequency;
            shakeDuration = duration;
            shakeTimer = duration;
        }

        public void Update()
        {
            if (shakeTimer > 0)
            {
                shakeTimer -= Time.deltaTime;

                cinemachineBasicMultiChannelPerlin.AmplitudeGain =
                    Mathf.Lerp(startingAmplitude, 0, 1 - (shakeTimer / shakeDuration));
                cinemachineBasicMultiChannelPerlin.FrequencyGain =
                    Mathf.Lerp(startingFrequency, 0, 1 - (shakeTimer / shakeDuration));
            }
        }
    }
}
