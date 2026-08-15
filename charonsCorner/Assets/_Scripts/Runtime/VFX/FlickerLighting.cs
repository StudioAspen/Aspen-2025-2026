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
        [SerializeField] private bool _turnOffOnAwake = false;

        [Header("Flicker Settings")]
        [SerializeField] private bool _useFlicker = true;
        [SerializeField, ShowIf("_useFlicker")] private FlickerSettingsSO _flickerSettings;

        private Dictionary<Light, float> _initialIntensities = new();
        private bool _isOn = true;
        private bool _isTransitioning = false;

        private void Awake()
        {
            CaptureInitialIntensities();
            if (_turnOffOnAwake)
            {
                ApplyTurnOff();
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
            if (gameEvent.EventName == "Darken")
            {
                SetOffInstant();
            }
            else if (!string.IsNullOrEmpty(_flickerOnEventName) && gameEvent.EventName == _flickerOnEventName)
            {
                TurnOn();
            }
            else if (!string.IsNullOrEmpty(_flickerOffEventName) && gameEvent.EventName == _flickerOffEventName)
            {
                TurnOff();
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
