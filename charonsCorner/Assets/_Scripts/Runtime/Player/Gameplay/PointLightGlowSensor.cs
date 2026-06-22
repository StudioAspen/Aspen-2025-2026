using UnityEngine;
using Sirenix.OdinInspector;

namespace CharonsCorner.Runtime
{
    public class PointLightGlowSensor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Required] private Renderer _targetRenderer;
        [SerializeField, Required] private Light _targetLight;
        
        [Header("Glow Range (Source)")]
        [SerializeField] private string _glowPropertyName = "_Glow";
        [SerializeField] private float _minGlow = 0f;
        [SerializeField] private float _maxGlow = 5f;

        [Header("Intensity Range (Target)")]
        [SerializeField] private float _minIntensity = 0f;
        [SerializeField] private float _maxIntensity = 10f;

        private MaterialPropertyBlock _propBlock;
        private int _glowPropertyId;

        private void Awake()
        {
            _propBlock = new MaterialPropertyBlock();
            _glowPropertyId = Shader.PropertyToID(_glowPropertyName);
        }

        private void Update()
        {
            if (_targetRenderer == null || _targetLight == null) return;

            // Read the current _Glow value from the renderer's property block
            _targetRenderer.GetPropertyBlock(_propBlock);
            float currentGlow = _propBlock.GetFloat(_glowPropertyId);

            // Remap currentGlow from [_minGlow, _maxGlow] to [_minIntensity, _maxIntensity]
            float t = Mathf.InverseLerp(_minGlow, _maxGlow, currentGlow);
            _targetLight.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, t);
        }
    }
}
