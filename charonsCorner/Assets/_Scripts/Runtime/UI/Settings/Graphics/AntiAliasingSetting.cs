using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class AntiAliasingSetting : SettingsToggleIndex
    {
        public static readonly List<(string name, AntiAliasingMode mode)> AntiAliasingOptions = new List<(string, AntiAliasingMode)>
        {
            ("None", AntiAliasingMode.None),
            ("FXAA", AntiAliasingMode.FXAA),
            ("SMAA", AntiAliasingMode.SMAA),
            ("TAA", AntiAliasingMode.TAA)
        };

        private protected override string SaveKey => "AntiAliasing";
        private static int DefaultValue => 0; // Default to the first option (None)
        public static int CurrentIndex { get; private set; }

        [SerializeField] private List<TMP_Text> _antiAliasingOptionTexts;
        
        [SerializeField] private Camera _targetCamera;

        public override void Load()
        {
            PopulateTexts();

            // Restore saved index (visual only). Do not apply until explicit Apply().
            CurrentIndex = SaveManager.SettingsStore.GetInt(SaveKey, DefaultValue);
            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, AntiAliasingOptions.Count - 1);

            SetTogglesToIndex(CurrentIndex);

            // Ensure runtime reflects last applied AA mode (best-effort).
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

            for (int i = 0; i < _antiAliasingOptionTexts.Count; i++)
            {
                if (_antiAliasingOptionTexts[i] == null)
                    continue;

                bool hasOption = i < AntiAliasingOptions.Count;
                _antiAliasingOptionTexts[i].text = hasOption ? AntiAliasingOptions[i].name : string.Empty;

                if (i < _toggles.Count && _toggles[i] != null)
                    _toggles[i].gameObject.SetActive(hasOption);
            }
        }

        private void ApplyMode(AntiAliasingMode mode)
        {
            //if (_targetCamera == null)
            //    return;

            //var cameraData = _targetCamera.GetUniversalAdditionalCameraData();
            //if (cameraData == null)
            //    return;

            switch (mode)
            {
                case AntiAliasingMode.None:
                    //cameraData.antialiasing = AntialiasingMode.None;
                    //QualitySettings.antiAliasing = 0;
                    break;

                case AntiAliasingMode.FXAA:
                    //cameraData.antialiasing = Anti.FastApproximateAntialiasing;
                    //QualitySettings.antiAliasing = 0;
                    break;

                case AntiAliasingMode.SMAA:
                    //cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    //QualitySettings.antiAliasing = 0;
                    break;

                case AntiAliasingMode.TAA:
                    //cameraData.antialiasing = AntialiasingMode.TemporalAntiAliasing;
                    //QualitySettings.antiAliasing = 0;
                    break;
            }
        }

        public enum AntiAliasingMode
        {
            None,
            FXAA,
            SMAA,
            TAA
        }
    }
}
