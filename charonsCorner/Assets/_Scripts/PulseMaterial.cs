using UnityEngine;
using MoreMountains.Tools;
using System.Collections;

namespace CharonsCorner.Runtime
{
    public class PulseMaterial : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        [Header("Event")]
        [SerializeField] private string _pulseEventName = "Pulse";

        [Header("Pulse Settings")]
        private float _pulseDuration = .5f;
        private float _pulseAmount = 3f;
        [SerializeField] private AnimationCurve _pulseCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        
        [Header("Material")]
        [SerializeField] private Renderer _targetRenderer;
        [SerializeField] private int _materialIndex = 0;
        [SerializeField] private string _glowPropertyName = "_Glow";

        private Material _material;
        private float _originalGlow;
        private Coroutine _pulseCoroutine;
        private int _glowPropertyId;

        private void Reset()
        {
            if (_targetRenderer == null)
            {
                _targetRenderer = GetComponent<Renderer>();
            }
        }

        private void Awake()
        {
            if (_targetRenderer == null)
            {
                _targetRenderer = GetComponent<Renderer>();
            }

            if (_targetRenderer != null && _targetRenderer.materials.Length > _materialIndex)
            {
                // Accessing .material creates a copy, which is what we want for pulsing individual objects
                _material = _targetRenderer.materials[_materialIndex];
                _glowPropertyId = Shader.PropertyToID(_glowPropertyName);
                if (_material.HasProperty(_glowPropertyId))
                {
                    _originalGlow = _material.GetFloat(_glowPropertyId);
                }
            }
        }

        private void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
        }

        private void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
                ResetGlow();
            }
        }

        public void OnMMEvent(MMGameEvent gameEvent)
        {
            if (gameEvent.EventName == _pulseEventName)
            {
                Pulse();
            }
        }

        public void Pulse()
        {
            if (_material == null) return;
            
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
            }
            _pulseCoroutine = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            float elapsed = 0f;
            while (elapsed < _pulseDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / _pulseDuration);
                float curveValue = _pulseCurve.Evaluate(progress);
                
                float currentGlow = _originalGlow + (curveValue * _pulseAmount);
                _material.SetFloat(_glowPropertyId, currentGlow);
                
                yield return null;
            }

            ResetGlow();
            _pulseCoroutine = null;
        }

        private void ResetGlow()
        {
            if (_material != null)
            {
                _material.SetFloat(_glowPropertyId, _originalGlow);
            }
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                // Clean up the instantiated material
                Destroy(_material);
            }
        }
    }
}
