using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using Sirenix.Serialization;
using JetBrains.Annotations;
using System;


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
            for (int i = 0; i < _textureQualityOptionTexts.Count; i++)
            {
                bool hasLevel = i < TextureQualityOptions.Count;
                _textureQualityOptionTexts[i].text = hasLevel ? TextureQualityOptions[i] : string.Empty;

                if (i < _toggles.Count && _toggles[i] != null)
                {
                    _toggles[i].gameObject.SetActive(hasLevel); // Show toggle only if there's a corresponding texture quality option
                    _toggles[i].isOn = false;
                }
            }
        }
    }
}
