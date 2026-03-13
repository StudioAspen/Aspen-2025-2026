using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class AmbientOcclusionSetting : Setting
    {
        private protected override string SaveKey => "AmbientOcclusion";
        private static readonly bool DefaultValue = false;
        public static bool CurrentValue { get; private set; }

        [SerializeField] private Toggle _ambientOcclusionToggle;

        private void OnEnable()
        {
            if (_ambientOcclusionToggle != null)
            {
                _ambientOcclusionToggle.isOn = CurrentValue;
            }
        }

        public override void Load()
        {
            CurrentValue = SaveManager.SettingsStore.GetBool(SaveKey, DefaultValue);
            if (_ambientOcclusionToggle != null)
            {
                _ambientOcclusionToggle.isOn = CurrentValue;
            }
            ApplyAmbientOcclusion(CurrentValue);
        }

        public override void Apply()
        {
            if (_ambientOcclusionToggle == null)
            {
                return;
            }

            bool aoValue = _ambientOcclusionToggle.isOn;
            SaveManager.SettingsStore.SetBool(SaveKey, aoValue);
            CurrentValue = aoValue;
            ApplyAmbientOcclusion(CurrentValue);
        }

        public override void Discard()
        {
            if (_ambientOcclusionToggle != null)
            {
                _ambientOcclusionToggle.isOn = CurrentValue;
            }
        }

        public override bool IsDirty() => _ambientOcclusionToggle != null && _ambientOcclusionToggle.isOn != CurrentValue;

        private void ApplyAmbientOcclusion(bool enabled)
        {
            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (!urpAsset) return;

            var dataListField = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
            var rendererDataList = dataListField?.GetValue(urpAsset) as System.Collections.IEnumerable;

            if (rendererDataList == null) return;

            foreach (var rendererData in rendererDataList)
            {
                if (rendererData is ScriptableRendererData srd)
                {
                    var ssaoFeature = srd.rendererFeatures.FirstOrDefault(f => f != null && f.name == "ScreenSpaceAmbientOcclusion");
                    if (ssaoFeature != null)
                    {
                        ssaoFeature.SetActive(enabled);
                    }
                }
            }
        }
    }
}
