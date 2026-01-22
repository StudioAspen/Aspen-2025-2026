using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class OnCollisionCameraShaker : MonoBehaviour
    {
        [SerializeField] private FloatRange _impactForceRange = new FloatRange(0f, 20f);

        [InfoBox("X-Axis is the impact force")]
        [SerializeField] private AnimationCurve _amplitudeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _durationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float _frequency = 10f;

        private void OnCollisionEnter(Collision collision)
        {
            float normalizedImpactForce = _impactForceRange.Clamp01(collision.impulse.magnitude);
            float amplitude = _amplitudeCurve.Evaluate(normalizedImpactForce);
            float duration = _durationCurve.Evaluate(normalizedImpactForce);

            CameraManager.Instance.CameraShaker.ShakeCamera(amplitude, _frequency, duration);
        }
    }
}
