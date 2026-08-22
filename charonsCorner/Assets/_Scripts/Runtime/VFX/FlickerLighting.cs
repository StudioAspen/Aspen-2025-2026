using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using CharonsCorner.Runtime.VFX;

namespace CharonsCorner.Runtime
{
    public class FlickerLighting : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        [Header("References")]
        [SerializeField] private List<Light> _lights = new();

        [Header("Events")]
        [SerializeField] private string _flickerOnEventName;
        [SerializeField] private string _flickerOffEventName;
        [SerializeField] private string _lightingGrowEventName;
        [SerializeField] private string _lightingShrinkEventName;
        [SerializeField] private bool _turnOffOnAwake = false;

        [Header("Flicker Settings")]
        [SerializeField] private bool _useFlicker = true;
        [SerializeField, ShowIf("_useFlicker")] private FlickerSettingsSO _flickerSettings;

        [Header("Transition Settings")]
        [SerializeField] private float _transitionDuration = 1f;
        [SerializeField] private AnimationCurve _transitionCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private Dictionary<Light, float> _initialIntensities = new();
        private bool _isOn = true;
        private bool _isTransitioning = false;
        private System.Threading.CancellationTokenSource _transitionCts;

        private void Awake()
        {
            CaptureInitialIntensities();
            if (_turnOffOnAwake)
            {
                SetOffInstant();
                _isOn = false;
            }
        }

        private void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
        }

        private void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
        }

        public void OnMMEvent(MMGameEvent gameEvent)
        {
            if (!string.IsNullOrEmpty(_flickerOnEventName) && gameEvent.EventName == _flickerOnEventName)
            {
                TurnOn();
            }
            else if (!string.IsNullOrEmpty(_flickerOffEventName) && gameEvent.EventName == _flickerOffEventName)
            {
                TurnOff();
            }
            else if (!string.IsNullOrEmpty(_lightingGrowEventName) && gameEvent.EventName == _lightingGrowEventName)
            {
                Grow();
            }
            else if (!string.IsNullOrEmpty(_lightingShrinkEventName) && gameEvent.EventName == _lightingShrinkEventName)
            {
                Shrink();
            }
        }

        private void CaptureInitialIntensities()
        {
            _initialIntensities.Clear();
            foreach (var light in _lights)
            {
                if (light != null && !_initialIntensities.ContainsKey(light))
                {
                    _initialIntensities.Add(light, light.intensity);
                }
            }
        }

        [Button]
        public void TurnOn()
        {
            CancelTransition();
            if (_isTransitioning) return;
            _isOn = true;
            _isTransitioning = true;
            
            if (_useFlicker)
            {
                FlickerOnAsync().Forget();
            }
            else
            {
                ApplyTurnOn();
                _isTransitioning = false;
            }
        }

        [Button]
        public void TurnOff()
        {
            CancelTransition();
            if (_isTransitioning) return;
            _isOn = false;
            _isTransitioning = true;

            if (_useFlicker)
            {
                FlickerOffAsync().Forget();
            }
            else
            {
                ApplyTurnOff();
                _isTransitioning = false;
            }
        }

        [Button]
        public void Grow()
        {
            CancelTransition();
            _isOn = true;
            _isTransitioning = true;
            _transitionCts = new System.Threading.CancellationTokenSource();
            LerpIntensityAsync(1f, _transitionCts.Token).Forget();
        }

        [Button]
        public void Shrink()
        {
            CancelTransition();
            _isOn = false;
            _isTransitioning = true;
            _transitionCts = new System.Threading.CancellationTokenSource();
            LerpIntensityAsync(0f, _transitionCts.Token).Forget();
        }

        private void CancelTransition()
        {
            if (_transitionCts != null)
            {
                _transitionCts.Cancel();
                _transitionCts.Dispose();
                _transitionCts = null;
            }
            _isTransitioning = false;
        }

        [Button]
        public void SetOffInstant()
        {
            _isOn = false;
            ApplyTurnOff();
        }

        private async UniTaskVoid FlickerOnAsync()
        {
            if (_flickerSettings == null)
            {
                ApplyTurnOn();
                _isTransitioning = false;
                return;
            }

            float elapsed = 0f;
            bool toggle = false;
            float duration = _flickerSettings.FlickerLength;
            float rate = _flickerSettings.FlickerRate;

            while (elapsed < duration)
            {
                toggle = !toggle;
                if (toggle) ApplyTurnOn();
                else ApplyTurnOff();

                await UniTask.Delay((int)(rate * 1000));
                elapsed += rate;
            }

            ApplyTurnOn();
            _isTransitioning = false;
        }

        private async UniTaskVoid FlickerOffAsync()
        {
            if (_flickerSettings == null)
            {
                ApplyTurnOff();
                _isTransitioning = false;
                return;
            }

            float elapsed = 0f;
            bool toggle = true;
            float duration = _flickerSettings.FlickerLength;
            float rate = _flickerSettings.FlickerRate;

            while (elapsed < duration)
            {
                toggle = !toggle;
                if (toggle) ApplyTurnOn();
                else ApplyTurnOff();

                await UniTask.Delay((int)(rate * 1000));
                elapsed += rate;
            }

            ApplyTurnOff();
            _isTransitioning = false;
        }

        private async UniTaskVoid LerpIntensityAsync(float targetMultiplier, System.Threading.CancellationToken ct)
        {
            float elapsed = 0f;
            Dictionary<Light, float> startIntensities = new();
            foreach (var light in _lights)
            {
                if (light != null)
                {
                    startIntensities[light] = light.intensity;
                    light.enabled = true;
                }
            }

            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _transitionDuration);
                float curveT = _transitionCurve.Evaluate(t);

                foreach (var light in _lights)
                {
                    if (light != null && _initialIntensities.TryGetValue(light, out float initialIntensity))
                    {
                        float targetIntensity = initialIntensity * targetMultiplier;
                        light.intensity = Mathf.Lerp(startIntensities[light], targetIntensity, curveT);
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (targetMultiplier <= 0)
            {
                ApplyTurnOff();
            }
            else
            {
                ApplyTurnOn();
            }

            _isTransitioning = false;
        }

        private void ApplyTurnOn()
        {
            _isOn = true;
            foreach (var light in _lights)
            {
                if (light != null && _initialIntensities.TryGetValue(light, out float intensity))
                {
                    light.intensity = intensity;
                    light.enabled = true;
                }
            }
        }

        private void ApplyTurnOff()
        {
            _isOn = false;
            foreach (var light in _lights)
            {
                if (light != null)
                {
                    light.intensity = 0f;
                    light.enabled = false;
                }
            }
        }
    }
}
