using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace CharonsCorner.Runtime
{
    public class TextureQualitySetting : SettingsToggleIndex
    {
        public static readonly List<string> TextureQualityOptions = new List<string>
        {
            "Low",
            "Medium",
            "High"
        };
        private protected override string SaveKey => "TextureQuality";
        private static int DefaultValue => TextureQualityOptions.Count - 1; // Default to the highest quality (last index)
        public static int CurrentIndex { get; private set; }

        [SerializeField] private List<TMP_Text> _textureQualityOptionTexts; //Text components for each toggle option

        public override void Load()
        {
            PopulateTexts();

            CurrentIndex = SaveManager.SettingsStore.GetInt(SaveKey, DefaultValue);
            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, TextureQualityOptions.Count - 1);

            SetTogglesToIndex(CurrentIndex);
            QualitySettings.globalTextureMipmapLimit = (TextureQualityOptions.Count - 1) - CurrentIndex; // First load: 0 = Full Res (High)
        }

        public override void Apply()
        {
            int qualityLevelIndex = GetSelectedIndex();
            if (qualityLevelIndex < 0 || qualityLevelIndex >= TextureQualityOptions.Count)
            {
                Debug.LogWarning($"Selected texture quality index {qualityLevelIndex} is out of range. Defaulting to {DefaultValue}.");
                return;
            }

            SaveManager.SettingsStore.SetInt(SaveKey, qualityLevelIndex);
            CurrentIndex = qualityLevelIndex;
            // UI index 0 = Low, last index = High. Invert for Unity's mipmap limit so High => 0.
            QualitySettings.globalTextureMipmapLimit = (TextureQualityOptions.Count - 1) - qualityLevelIndex;
        }

        public override void Discard()
        {
            SetTogglesToIndex(CurrentIndex);
        }

        public override bool IsDirty() => GetSelectedIndex() != CurrentIndex;

        private void PopulateTexts()
        {
            if (_textureQualityOptionTexts == null) return;

            int toggleCount = _toggles?.Count ?? 0;
            for (int i = 0; i < _textureQualityOptionTexts.Count; i++)
            {
                if (_textureQualityOptionTexts[i] == null) continue;

                bool hasLevel = i < TextureQualityOptions.Count;
                _textureQualityOptionTexts[i].text = hasLevel ? TextureQualityOptions[i] : string.Empty;

                if (i < toggleCount && _toggles[i] != null)
                {
                    _toggles[i].gameObject.SetActive(hasLevel); // Show toggle only if there's a corresponding texture quality option
                    _toggles[i].isOn = false;
                }
            }
        }
    }
}
