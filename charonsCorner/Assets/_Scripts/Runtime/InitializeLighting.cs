using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class InitializeLighting : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Light _light;
        [SerializeField] private float _intensity = 1f;
        [SerializeField] private float _specialIntensity = 1f;

        private void Start()
        {
            if (_light == null)
            {
                _light = GetComponent<Light>();
            }

            if (_light != null)
            {
                float targetIntensity = _intensity;

                if (EnterGameplaySequence.IsSpecialSequenceQueued)
                {
                    targetIntensity = _specialIntensity;
                }

                _light.intensity = targetIntensity;
            }
            else
            {
                Debug.LogWarning($"[InitializeLighting] No Light component assigned or found on {gameObject.name}.");
            }
        }
    }
}
