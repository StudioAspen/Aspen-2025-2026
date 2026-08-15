using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using CharonsCorner.Runtime.VFX;

namespace CharonsCorner.Runtime
{
    public class GlowController : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        [SerializeField] private GameObject _parentObject;
        [SerializeField] private string _turnOnEventName;
        [SerializeField] private string _flickerOffEventName;
        [SerializeField] private bool _darkenOnAwake = true;

        [Header("Flicker Settings")]
        [SerializeField] private bool _flickerOn = false;
        [SerializeField, ShowIf("_flickerOn")] private FlickerSettingsSO _flickerSettings;

        [Header("Shadow Settings")]
        [SerializeField] private bool _editShadowColorAsWell = false;
        [SerializeField, ShowIf("_editShadowColorAsWell")] private Color _targetShadowColor = Color.black;
        
        private bool _isGlowing = true;
        private readonly List<RendererData> _renderers = new();
        private const string GlowPropertyName = "_Glow";
        private static readonly int GlowPropertyId = Shader.PropertyToID(GlowPropertyName);
        private const string ShadowColorPropertyName = "_ShadowColor";
        private static readonly int ShadowColorPropertyId = Shader.PropertyToID(ShadowColorPropertyName);

        private struct RendererData
        {
            public Renderer Renderer;
            public List<float> InitialGlowValues;
            public List<Color> InitialShadowColors;
        }

        private void Awake()
        {
            if (_parentObject == null)
            {
                _parentObject = gameObject;
            }
            
            InitializeGlowData();

            if (_darkenOnAwake)
            {
                TurnOff();
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
                TurnOff();
            }
            else if (!string.IsNullOrEmpty(_turnOnEventName) && gameEvent.EventName == _turnOnEventName)
            {
                if (!_isGlowing)
                {
                    TurnOn();
                }
            }
            else if (!string.IsNullOrEmpty(_flickerOffEventName) && gameEvent.EventName == _flickerOffEventName)
            {
                if (_isGlowing)
                {
                    FlickerOff();
                }
            }
        }

        private void InitializeGlowData()
        {
            _renderers.Clear();
            var allRenderers = _parentObject.GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in allRenderers)
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                
                var initialGlowValues = new List<float>();
                var initialShadowColors = new List<Color>();
                bool hasTargetProperty = false;

                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat == null)
                    {
                        initialGlowValues.Add(0f);
                        initialShadowColors.Add(Color.black);
                        continue;
                    }

                    if (mat.HasProperty(GlowPropertyId))
                    {
                        initialGlowValues.Add(mat.GetFloat(GlowPropertyId));
                        hasTargetProperty = true;
                    }
                    else
                    {
                        initialGlowValues.Add(0f);
                    }

                    if (mat.HasProperty(ShadowColorPropertyId))
                    {
                        initialShadowColors.Add(mat.GetColor(ShadowColorPropertyId));
                        hasTargetProperty = true;
                    }
                    else
                    {
                        initialShadowColors.Add(Color.black);
                    }
                }

                if (hasTargetProperty)
                {
                    _renderers.Add(new RendererData
                    {
                        Renderer = renderer,
                        InitialGlowValues = initialGlowValues,
                        InitialShadowColors = initialShadowColors
                    });
                }
            }
        }

        public void TurnOn()
        {
            if (_isGlowing) return;
            
            if (_flickerOn)
            {
                FlickerOnAsync().Forget();
            }
            else
            {
                ApplyTurnOn();
            }
        }

        public void FlickerOff()
        {
            if (!_isGlowing) return;

            if (_flickerOn)
            {
                FlickerOffAsync().Forget();
            }
            else
            {
                ApplyTurnOff();
            }
        }

        private async UniTaskVoid FlickerOnAsync()
        {
            if (_flickerSettings == null)
            {
                ApplyTurnOn();
                return;
            }

            float elapsed = 0f;
            bool isOn = false;
            float duration = _flickerSettings.FlickerLength;
            float rate = _flickerSettings.FlickerRate;

            while (elapsed < duration)
            {
                isOn = !isOn;
                if (isOn)
                {
                    ApplyTurnOn();
                }
                else
                {
                    ApplyTurnOff();
                }

                await UniTask.Delay((int)(rate * 1000));
                elapsed += rate;
            }

            ApplyTurnOn();
        }

        private void ApplyTurnOff()
        {
            _isGlowing = false;
            foreach (var data in _renderers)
            {
                if (data.Renderer == null) continue;
                
                var materials = data.Renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].HasProperty(GlowPropertyId))
                    {
                        materials[i].SetFloat(GlowPropertyId, 0f);
                    }

                    if (_editShadowColorAsWell && i < data.InitialShadowColors.Count && materials[i].HasProperty(ShadowColorPropertyId))
                    {
                        materials[i].SetColor(ShadowColorPropertyId, _targetShadowColor);
                    }
                }
                data.Renderer.materials = materials;
            }
        }

        private void ApplyTurnOn()
        {
            _isGlowing = true;
            foreach (var data in _renderers)
            {
                if (data.Renderer == null) continue;

                var materials = data.Renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (i < data.InitialGlowValues.Count && materials[i].HasProperty(GlowPropertyId))
                    {
                        materials[i].SetFloat(GlowPropertyId, data.InitialGlowValues[i]);
                    }

                    if (_editShadowColorAsWell && i < data.InitialShadowColors.Count && materials[i].HasProperty(ShadowColorPropertyId))
                    {
                        materials[i].SetColor(ShadowColorPropertyId, data.InitialShadowColors[i]);
                    }
                }
                data.Renderer.materials = materials;
            }
        }

        public void TurnOff()
        {
            ApplyTurnOff();
        }

        private async UniTaskVoid FlickerOffAsync()
        {
            if (_flickerSettings == null)
            {
                ApplyTurnOff();
                return;
            }

            float elapsed = 0f;
            bool isOn = true;
            float duration = _flickerSettings.FlickerLength;
            float rate = _flickerSettings.FlickerRate;

            while (elapsed < duration)
            {
                isOn = !isOn;
                if (isOn)
                {
                    ApplyTurnOn();
                }
                else
                {
                    ApplyTurnOff();
                }

                await UniTask.Delay((int)(rate * 1000));
                elapsed += rate;
            }

            ApplyTurnOff();
        }
    }
}
