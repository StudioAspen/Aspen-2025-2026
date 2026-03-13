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

        [SerializeField] private int _msaaSampleCount = 4;

        public override void Load()
        {
            PopulateTexts();


            CurrentIndex = SaveManager.SettingsStore.GetInt(SaveKey, DefaultValue);
            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, AntiAliasingOptions.Count - 1);

            int maxIndex = AntiAliasingOptions.Count - 1;
            if (_toggles != null && _toggles.Count > 0) 
                maxIndex = Mathf.Min(maxIndex, _toggles.Count - 1);
            if (_antiAliasingOptionTexts != null && _antiAliasingOptionTexts.Count > 0)
                maxIndex = Mathf.Min(maxIndex, _antiAliasingOptionTexts.Count - 1);
            
            CurrentIndex = Mathf.Clamp(
            SaveManager.SettingsStore.GetInt(SaveKey, DefaultValue),
            0,
            maxIndex);
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
                return;

            var cameraData = _targetCamera.GetUniversalAdditionalCameraData();
            if (cameraData == null)
                return;

            switch (mode)
            {
                case AntiAliasingMode.None:
                    cameraData.antialiasing = AntialiasingMode.None;
                    QualitySettings.antiAliasing = 0;
                    break;

                case AntiAliasingMode.MSAA:
                    // MSAA is not a post-process in URP; enable via QualitySettings sample count.
                    cameraData.antialiasing = AntialiasingMode.None;
                    int samples = _msaaSampleCount;
                    // Ensure a supported value (0, 2, 4, 8). Default to 4 if invalid.
                    if (samples != 0 && samples != 2 && samples != 4 && samples != 8)
                        samples = 4;
                    QualitySettings.antiAliasing = samples;
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
