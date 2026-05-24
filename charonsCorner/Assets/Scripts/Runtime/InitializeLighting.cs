using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class InitializeLighting : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Light _light;
        [SerializeField] private float _intensity = 1f;

        private void Start()
        {
            if (_light == null)
            {
                _light = GetComponent<Light>();
            }

            if (_light != null)
            {
                _light.intensity = _intensity;
            }
            else
            {
                Debug.LogWarning($"[InitializeLighting] No Light component assigned or found on {gameObject.name}.");
            }
        }
    }
}
