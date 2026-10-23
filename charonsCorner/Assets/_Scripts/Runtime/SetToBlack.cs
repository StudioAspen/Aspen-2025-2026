using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace CharonsCorner.Runtime
{
    public class SetToBlack : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        [SerializeField] private bool _active = false;
        [SerializeField] private string _mmGameEvent = "SetToBlack";
        [SerializeField] private bool _altColor = false;
        [SerializeField] private Color _altColorValue = Color.white;
        [SerializeField] private bool _randomizedAltColor = false;
        [SerializeField] private List<Color> _randomizedAltColors = new List<Color>();
        [SerializeField] private BalloonBunch _balloonBunch;

        private Color? _externalAltColor = null;

        private struct RendererData
        {
            public Renderer Renderer;
            public Material Material0;
            public Material Material1;
            public Color OriginalGlowColor0;
            public Color OriginalBaseColor0;
            public Color OriginalOutlineColor1;
            public bool HasMaterial0;
            public bool HasMaterial1;
        }

        private List<RendererData> _renderersData = new List<RendererData>();
        
        private int _glowColorId;
        private int _baseColorId;
        private int _outlineColorId;

        private bool _wasActive = false;
        private bool _initialized = false;

        public bool Active
        {
            get => _active;
            set => _active = value;
        }

        public bool RandomizedAltColor => _randomizedAltColor;
        public List<Color> RandomizedAltColors => _randomizedAltColors;

        public void SetExternalAltColor(Color color)
        {
            _externalAltColor = color;
            if (_initialized)
            {
                ApplyState();
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
            if (gameEvent.EventName == _mmGameEvent)
            {
                Active = true;
            }
        }

        private void Awake()
        {
            _glowColorId = Shader.PropertyToID("_GlowColor");
            _baseColorId = Shader.PropertyToID("_BaseColor");
            _outlineColorId = Shader.PropertyToID("_OutlineColor");

            if (_balloonBunch == null)
            {
                _balloonBunch = GetComponent<BalloonBunch>();
            }
        }

        private void Initialize()
        {
            InitializeRenderers();
            
            _initialized = true;
            _wasActive = _active;
            ApplyState();
        }

        private void InitializeRenderers()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                RendererData data = new RendererData();
                data.Renderer = renderer;
                
                Material[] materials = renderer.materials;
                
                if (materials.Length > 0)
                {
                    data.Material0 = materials[0];
                    data.HasMaterial0 = true;
                    if (data.Material0.HasProperty(_glowColorId)) data.OriginalGlowColor0 = data.Material0.GetColor(_glowColorId);
                    if (data.Material0.HasProperty(_baseColorId)) data.OriginalBaseColor0 = data.Material0.GetColor(_baseColorId);
                }

                if (materials.Length > 1)
                {
                    data.Material1 = materials[1];
                    data.HasMaterial1 = true;
                    if (data.Material1.HasProperty(_outlineColorId)) data.OriginalOutlineColor1 = data.Material1.GetColor(_outlineColorId);
                }
                
                // Re-assigning materials array triggers the instantiation of copies if not already done by .materials
                renderer.materials = materials;
                _renderersData.Add(data);
            }
        }

        private void Update()
        {
            if (!_initialized)
            {
                if (_balloonBunch == null || _balloonBunch.IsInitialized)
                {
                    Initialize();
                }
                return;
            }

            if (_active != _wasActive)
            {
                ApplyState();
                _wasActive = _active;
            }
        }

        private void ApplyState()
        {
            foreach (var data in _renderersData)
            {
                if (_active)
                {
                    if (data.HasMaterial0)
                    {
                        if (data.Material0.HasProperty(_glowColorId)) data.Material0.SetColor(_glowColorId, Color.black);
                        if (data.Material0.HasProperty(_baseColorId)) data.Material0.SetColor(_baseColorId, Color.black);
                    }

                    if (data.HasMaterial1)
                    {
                        if (data.Material1.HasProperty(_outlineColorId))
                        {
                            Color targetColor = Color.white;
                            
                            if (_externalAltColor.HasValue)
                            {
                                targetColor = _externalAltColor.Value;
                            }
                            else if (_randomizedAltColor && _randomizedAltColors != null && _randomizedAltColors.Count > 0)
                            {
                                // If not set externally, pick one now (though it might vary between renderers if not careful)
                                // Actually, since we iterate through renderers here, we should pick it once per ApplyState call
                                // But usually it's better if it's set externally for consistency.
                                // Let's cache it if we pick it here.
                                if (!_externalAltColor.HasValue)
                                {
                                    _externalAltColor = _randomizedAltColors[Random.Range(0, _randomizedAltColors.Count)];
                                }
                                targetColor = _externalAltColor.Value;
                            }
                            else
                            {
                                targetColor = _altColor ? _altColorValue : Color.white;
                            }
                            
                            data.Material1.SetColor(_outlineColorId, targetColor);
                        }
                    }
                }
                else
                {
                    if (data.HasMaterial0)
                    {
                        if (data.Material0.HasProperty(_glowColorId)) data.Material0.SetColor(_glowColorId, data.OriginalGlowColor0);
                        if (data.Material0.HasProperty(_baseColorId)) data.Material0.SetColor(_baseColorId, data.OriginalBaseColor0);
                    }

                    if (data.HasMaterial1)
                    {
                        if (data.Material1.HasProperty(_outlineColorId)) data.Material1.SetColor(_outlineColorId, data.OriginalOutlineColor1);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var data in _renderersData)
            {
                if (data.Material0 != null) Destroy(data.Material0);
                if (data.Material1 != null) Destroy(data.Material1);
            }
            _renderersData.Clear();
        }
    }
}
