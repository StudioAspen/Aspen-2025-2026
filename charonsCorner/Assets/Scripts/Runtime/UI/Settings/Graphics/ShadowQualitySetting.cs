using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace CharonsCorner.Runtime
{
    public class ShadowQualitySetting : SettingsToggleIndex
    {
        public static readonly List<(string name, ShadowQualityLevel quality)> ShadowQualityOptions = new List<(string, ShadowQualityLevel)>
        {
            ("Low", ShadowQualityLevel.Low),
            ("Medium", ShadowQualityLevel.Medium),
            ("High", ShadowQualityLevel.High)
        };

        private protected override string SaveKey => "ShadowQuality";
        private static int DefaultValue => 1; // Default to Medium
        public static int CurrentIndex { get; private set; }

        [SerializeField] private List<TMP_Text> _shadowQualityOptionTexts;

        public override void Load()
        {
            PopulateTexts();

            
            CurrentIndex = SaveManager.SettingsStore.GetInt(SaveKey, DefaultValue);
            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, ShadowQualityOptions.Count - 1);

            SetTogglesToIndex(CurrentIndex);

            ApplyQuality(ShadowQualityOptions[CurrentIndex].quality);
        }

        public override void Apply()
        {
            int qualityIndex = GetSelectedIndex();
            if (qualityIndex < 0 || qualityIndex >= ShadowQualityOptions.Count)
                return;
            SaveManager.SettingsStore.SetInt(SaveKey, qualityIndex);
            CurrentIndex = qualityIndex;
            ApplyQuality(ShadowQualityOptions[CurrentIndex].quality);
        }

        public override void Discard()
        {
            SetTogglesToIndex(CurrentIndex);
        }
        public override bool IsDirty() => GetSelectedIndex() != CurrentIndex;

        private void PopulateTexts()
        {
            if (_shadowQualityOptionTexts == null) return;

            for (int i = 0; i < _shadowQualityOptionTexts.Count && i < ShadowQualityOptions.Count; i++)
            {
                if (_shadowQualityOptionTexts[i] != null)
                    _shadowQualityOptionTexts[i].text = ShadowQualityOptions[i].name;
            }
        }

        private void ApplyQuality(ShadowQualityLevel quality)
        {
            UniversalRenderPipelineAsset urpAsset = 
                QualitySettings.renderPipeline as UniversalRenderPipelineAsset ??
                GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;

            if (urpAsset == null)
            {
                Debug.LogWarning("Universal Render Pipeline Asset not found. Shadow quality settings will not be applied.");
                return;
            }

            switch (quality)
            {
                case ShadowQualityLevel.Low:
                    urpAsset.shadowDistance = 50f;
                    urpAsset.shadowCascadeCount = 1;
                    break;
                case ShadowQualityLevel.Medium:
                    urpAsset.shadowDistance = 100f;
                    urpAsset.shadowCascadeCount = 4;
                    break;
                case ShadowQualityLevel.High:
                    urpAsset.shadowDistance = 200f;
                    urpAsset.shadowCascadeCount = 4;
                    break;
            }
        }
    }
}

public enum ShadowQualityLevel
{
    Low = 0,
    Medium = 1,
    High = 2
}
