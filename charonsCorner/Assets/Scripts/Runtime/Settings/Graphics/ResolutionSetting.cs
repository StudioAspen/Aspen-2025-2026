using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ResolutionSetting : Setting
    {
        private protected override string saveKey => "Resolution";
        private static int defaultValue => 0; // Default to the first resolution in the list (highest available in this case)
        public static int CurrentIndex { get; private set; }

        [SerializeField] private TMP_Dropdown resolutionDropdown;

        private static List<Resolution> validResolutions;

        private void OnEnable()
        {
            resolutionDropdown.value = CurrentIndex;
        }

        public override void Load()
        {
            validResolutions = GetValidResolutions();

            // Clear existing options and add new ones
            resolutionDropdown.ClearOptions();
            foreach (Resolution resolution in validResolutions)
                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData($"{resolution.width}x{resolution.height}"));

            CurrentIndex = SaveManager.SettingsStore.Data.GetInt(saveKey, defaultValue);
            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, validResolutions.Count - 1); // Ensure index is within bounds
            resolutionDropdown.value = CurrentIndex;

            Resolution selectedResolution = validResolutions[CurrentIndex];
            SetResolution(selectedResolution);
        }

        public override void Apply()
        {
            int windowModeIndex = resolutionDropdown.value;
            SaveManager.SettingsStore.Data.SetInt(saveKey, windowModeIndex);
            CurrentIndex = windowModeIndex;

            Resolution selectedResolution = validResolutions[CurrentIndex];
            SetResolution(selectedResolution);
        }

        public override void Discard()
        {
            resolutionDropdown.value = CurrentIndex;
        }

        public override bool IsDirty() => resolutionDropdown.value != CurrentIndex;

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
