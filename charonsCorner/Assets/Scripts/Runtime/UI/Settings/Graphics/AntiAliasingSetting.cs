using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CharonsCorner.Runtime
{
    public class AntiAliasingSetting : SettingsToggleIndex
    {
        public static readonly List<(string name, AntiAliasingMode mode)> AntiAliasingOptions = new List<(string, AntiAliasingMode)>
        {
            ("None", AntiAliasingMode.None),
            ("MSAA", AntiAliasingMode.MSAA),
            ("FXAA", AntiAliasingMode.FXAA),
            ("TAA", AntiAliasingMode.TAA)
        };

        private protected override string SaveKey => "AntiAliasing";
        private static int DefaultValue => 0; // Default to None
        public static int CurrentIndex { get; private set; }

        [SerializeField] private List<TMP_Text> _antiAliasingOptionTexts;

        [SerializeField] private Camera _targetCamera;

        public override void Load()
        {
            PopulateTexts();

            int maxIndex = AntiAliasingOptions.Count - 1;
            if (_toggles != null && _toggles.Count > 0)
                maxIndex = Mathf.Min(maxIndex, _toggles.Count - 1);
            if (_antiAliasingOptionTexts != null && _antiAliasingOptionTexts.Count > 0)
                maxIndex = Mathf.Min(maxIndex, _antiAliasingOptionTexts.Count - 1);

            CurrentIndex = Mathf.Clamp(SaveManager.SettingsStore.GetInt(SaveKey, DefaultValue), 0, maxIndex);

            SetTogglesToIndex(CurrentIndex);
            ApplyMode(AntiAliasingOptions[CurrentIndex].mode);
        }

        public override void Apply()
        {
            int aaIndex = GetSelectedIndex();
            if (aaIndex < 0 || aaIndex >= AntiAliasingOptions.Count)
                return;

            SaveManager.SettingsStore.SetInt(SaveKey, aaIndex);
            CurrentIndex = aaIndex;
            ApplyMode(AntiAliasingOptions[CurrentIndex].mode);
        }

        public override void Discard()
        {
            SetTogglesToIndex(CurrentIndex);
        }

        public override bool IsDirty() => GetSelectedIndex() != CurrentIndex;

        private void PopulateTexts()
        {
            if (_antiAliasingOptionTexts == null)
                return;

            int toggleCount = _toggles?.Count ?? 0;
            for (int i = 0; i < _antiAliasingOptionTexts.Count; i++)
            {
                if (_antiAliasingOptionTexts[i] == null)
                    continue;

                bool hasOption = i < AntiAliasingOptions.Count;
                _antiAliasingOptionTexts[i].text = hasOption ? AntiAliasingOptions[i].name : string.Empty;

                if (i < toggleCount && _toggles[i] != null)
                    _toggles[i].gameObject.SetActive(hasOption);
            }
        }

        private void ApplyMode(AntiAliasingMode mode)
        {
            if (_targetCamera == null)
            {
                Debug.LogWarning("Target camera not assigned for AntiAliasingSetting.");
                return;
            }
            
            var cameraData = _targetCamera.GetUniversalAdditionalCameraData();
            switch (mode)
            {
                case AntiAliasingMode.None:
                    cameraData.antialiasing = AntialiasingMode.None;
                    QualitySettings.antiAliasing = 0; // Disable MSAA in quality settings
                    break;

                case AntiAliasingMode.MSAA:
                    cameraData.antialiasing = AntialiasingMode.None; // URP handles MSAA via quality settings
                    QualitySettings.antiAliasing = 4;   // Set to 4x MSAA
                    break;

                case AntiAliasingMode.FXAA:
                    cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                    QualitySettings.antiAliasing = 0;
                    break;

                case AntiAliasingMode.TAA:
                    cameraData.antialiasing = AntialiasingMode.TemporalAntiAliasing;
                    QualitySettings.antiAliasing = 0;
                    break;
            }
        }

        public enum AntiAliasingMode
        {
            None,
            MSAA,
            FXAA,
            TAA
        }
    }
}
