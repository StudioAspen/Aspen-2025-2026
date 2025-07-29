using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class CameraShakeDialogueEffect : DialogueEffect
    {
        [field: SerializeField] public float Duration { get; private set; } = 1f;
        [field: SerializeField] public float Amplitude { get; private set; } = 1f;
        [field: SerializeField] public float Frequency { get; private set; } = 1f;

        public override void ApplyEffect()
        {
            CameraManager.Instance.CameraShaker.ShakeCamera(Amplitude, Frequency, Duration);
        }
    }
}
