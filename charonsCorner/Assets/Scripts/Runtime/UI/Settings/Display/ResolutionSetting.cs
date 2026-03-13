using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ResolutionSetting : SettingsToggleIndex
    {
        private protected override string SaveKey => "Resolution";
        private static int DefaultValue => 0; // Default to the first resolution in the list (highest available in this case)
        public static int CurrentIndex { get; private set; }

        private static List<Resolution> _validResolutions;
        [SerializeField] private List<TMP_Text> _resolutionOptionTexts; //Text components for each toggle option

        public override void Load()
        {
            _validResolutions = GetValidResolutions();
            if (_validResolutions == null || _validResolutions.Count == 0)
            {
                Debug.LogError("No valid resolutions found. Please ensure your system supports at least one 16:9 resolution of 1280x720 or higher.");
                return;
            }

            PopulateTexts();

            CurrentIndex = SaveManager.SettingsStore.GetInt(SaveKey, DefaultValue);
            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, _validResolutions.Count - 1); // Ensure index is within bounds
            SetTogglesToIndex(CurrentIndex);
        }

        public override void Apply()
        {
            if (_validResolutions == null || _validResolutions.Count == 0)
            {
                Debug.LogError("Cannot apply resolution setting because no valid resolutions are available.");
                return;
            }

            int selectedIndex = GetSelectedIndex(); // Get index of currently selected toggle
            if (selectedIndex < 0 || selectedIndex >= _validResolutions.Count)
            {
                Debug.LogError($"Selected index {selectedIndex} is out of bounds for valid resolutions.");
                return;
            }

            SaveManager.SettingsStore.SetInt(SaveKey, selectedIndex);
            CurrentIndex = selectedIndex;

            Resolution selectedResolution = _validResolutions[CurrentIndex];
            SetResolution(selectedResolution);
        }

        public override void Discard()
        {
            SetTogglesToIndex(CurrentIndex); // Revert toggles to the last applied index
        }

        public override bool IsDirty() => GetSelectedIndex() != CurrentIndex;

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

        private void PopulateTexts()
        {
            if (_resolutionOptionTexts == null)
            {
                Debug.LogError("Resolution option texts are not assigned in the inspector.");
                return;
            }

            int toggleCount = _toggles?.Count ?? 0;
            for (int i = 0; i < _resolutionOptionTexts.Count; i++)
            {
                if (_resolutionOptionTexts[i] == null)
                {
                    Debug.LogError($"Resolution option text at index {i} is not assigned.");
                    continue;
                }

                bool hasResolution = i < _validResolutions.Count;
                _resolutionOptionTexts[i].text = hasResolution
                    ? $"{_validResolutions[i].width} x {_validResolutions[i].height}"
                    : "N/A"; // Display "N/A" for options that exceed available resolutions

                if (i < toggleCount && _toggles[i] != null)
                {
                    _toggles[i].interactable = hasResolution; // Disable toggles that don't have a corresponding resolution
                }
            }
        }
    }
}
