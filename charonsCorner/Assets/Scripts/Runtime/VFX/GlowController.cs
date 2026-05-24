using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace CharonsCorner.Runtime
{
    public class GlowController : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        [SerializeField] private GameObject _parentObject;
        
        private readonly List<RendererData> _renderers = new();
        private const string GlowPropertyName = "_Glow";
        private static readonly int GlowPropertyId = Shader.PropertyToID(GlowPropertyName);

        private struct RendererData
        {
            public Renderer Renderer;
            public List<float> InitialGlowValues;
        }

        private void Awake()
        {
            if (_parentObject == null)
            {
                _parentObject = gameObject;
            }
            
            InitializeGlowData();
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
        }

        private void InitializeGlowData()
        {
            _renderers.Clear();
            var allRenderers = _parentObject.GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in allRenderers)
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                
                var initialValues = new List<float>();
                bool hasGlow = false;

                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat != null && mat.HasProperty(GlowPropertyId))
                    {
                        initialValues.Add(mat.GetFloat(GlowPropertyId));
                        hasGlow = true;
                    }
                    else
                    {
                        initialValues.Add(0f);
                    }
                }

                if (hasGlow)
                {
                    _renderers.Add(new RendererData
                    {
                        Renderer = renderer,
                        InitialGlowValues = initialValues
                    });
                }
            }
        }

        public void TurnOff()
        {
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
                }
                data.Renderer.materials = materials;
            }
        }

        public void TurnOn()
        {
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
                }
                data.Renderer.materials = materials;
            }
        }
    }
}
