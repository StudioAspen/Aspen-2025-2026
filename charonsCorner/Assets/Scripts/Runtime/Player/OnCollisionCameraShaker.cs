using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class OnCollisionCameraShaker : MonoBehaviour
    {
        [SerializeField] private FloatRange impactForceRange = new FloatRange(0f, 20f);

        [InfoBox("X-Axis is the impact force")]
        [SerializeField] private AnimationCurve amplitudeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve durationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float frequency = 10f;

        private void OnCollisionEnter(Collision collision)
        {
            float normalizedImpactForce = impactForceRange.Clamp01(collision.impulse.magnitude);
            float amplitude = amplitudeCurve.Evaluate(normalizedImpactForce);
            float duration = durationCurve.Evaluate(normalizedImpactForce);

            CameraManager.Instance.CameraShaker.ShakeCamera(amplitude, frequency, duration);
        }
    }
}
