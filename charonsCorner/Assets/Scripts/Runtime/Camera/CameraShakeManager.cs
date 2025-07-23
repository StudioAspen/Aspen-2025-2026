using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(CinemachineCamera))]
    [RequireComponent(typeof(CinemachineBasicMultiChannelPerlin))]
    public class CameraShakeManager : MonoBehaviour
    {
        public static CameraShakeManager Instance { get; private set; }

        private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;

        private float startingAmplitude;
        private float startingFrequency;
        private float shakeTimer;
        private float shakeDuration;

        private void Awake()
        {
            if (Instance != null)
                Destroy(Instance.gameObject);

            Instance = this;

            cinemachineBasicMultiChannelPerlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
        }

        /// <summary>
        /// Shakes the camera with the specified amplitude, frequency, and duration.
        /// </summary>
        /// <param name="amplitude">How large the shakes are.</param>
        /// <param name="frequency">How often are the shakes.</param>
        /// <param name="duration">How long will the camera be shaking.</param>
        public void ShakeCamera(float amplitude, float frequency, float duration)
        {
            cinemachineBasicMultiChannelPerlin.ReSeed();

            cinemachineBasicMultiChannelPerlin.AmplitudeGain = amplitude;
            cinemachineBasicMultiChannelPerlin.FrequencyGain = frequency;
            startingAmplitude = amplitude;
            startingFrequency = frequency;
            shakeDuration = duration;
            shakeTimer = duration;
        }

        private void Update()
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
