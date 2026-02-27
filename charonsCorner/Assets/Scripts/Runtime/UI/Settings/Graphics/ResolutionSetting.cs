using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ResolutionSetting : Setting
    {
        private protected override string SaveKey => "Resolution";
        private static int DefaultValue => 0; // Default to the first resolution in the list (highest available in this case)
        public static int CurrentIndex { get; private set; }

        [SerializeField] private TMP_Dropdown _resolutionDropdown;

        private static List<Resolution> _validResolutions;

        private void OnEnable()
        {
            _resolutionDropdown.value = CurrentIndex;
        }

        public override void Load()
        {
            _validResolutions = GetValidResolutions();

            // Clear existing options and add new ones
            _resolutionDropdown.ClearOptions();
            foreach (Resolution resolution in _validResolutions)
                _resolutionDropdown.options.Add(new TMP_Dropdown.OptionData($"{resolution.width}x{resolution.height}"));

            CurrentIndex = SaveManager.SettingsStore.GetInt(SaveKey, DefaultValue);
            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, _validResolutions.Count - 1); // Ensure index is within bounds
            _resolutionDropdown.value = CurrentIndex;

            Resolution selectedResolution = _validResolutions[CurrentIndex];
            SetResolution(selectedResolution);
        }

        public override void Apply()
        {
            int windowModeIndex = _resolutionDropdown.value;
            SaveManager.SettingsStore.SetInt(SaveKey, windowModeIndex);
            CurrentIndex = windowModeIndex;

            Resolution selectedResolution = _validResolutions[CurrentIndex];
            SetResolution(selectedResolution);
        }

        public override void Discard()
        {
            _resolutionDropdown.value = CurrentIndex;
        }

        public override bool IsDirty() => _resolutionDropdown.value != CurrentIndex;

        /// <summary>
        /// Filter out duplicate resolutions based on width and height,
        /// only include 16:9 aspect ratios and minimum resolution of 1280x720
        /// </summary>
        private List<Resolution> GetValidResolutions()
        {
            Resolution[] allResolutions = Screen.resolutions.Reverse().ToArray(); // Reverse to prioritize higher resolutions first
            HashSet<(int, int)> validDimensions = new HashSet<(int, int)>();

            // Filter resolutions to only include those that are 16:9 and at least 1280x720
            foreach (var res in allResolutions)
            {
                if (res.width < 1280 || res.height < 720)
                    continue;

                int expectedHeight = Mathf.RoundToInt(res.width * 9f / 16f);
                if (Mathf.Abs(res.height - expectedHeight) > 1) // allow 1 pixel difference max
                    continue;

                validDimensions.Add((res.width, expectedHeight)); // normalize height to exact 16:9
            }

            List<Resolution> validResolutions = new List<Resolution>();
            foreach (var (width, height) in validDimensions)
                validResolutions.Add(new Resolution { width = width, height = height });

            return validResolutions;
        }

        /// <summary>
        /// Helper method to set the resolution, considering fullscreen mode.
        /// </summary>
        private void SetResolution(Resolution resolution) => Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
    }
}
