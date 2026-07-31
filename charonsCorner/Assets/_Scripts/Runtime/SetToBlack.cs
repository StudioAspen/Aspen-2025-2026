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

        public bool Active
        {
            get => _active;
            set => _active = value;
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

            InitializeRenderers();
            
            _wasActive = _active;
            ApplyState();
        }

        private void InitializeRenderers()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
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
                            Color targetColor = _altColor ? _altColorValue : Color.white;
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
