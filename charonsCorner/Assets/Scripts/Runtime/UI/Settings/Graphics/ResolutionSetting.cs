using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class ResolutionSetting : Setting
    {
        private protected override string SaveKey => "Resolution";
        private static int DefaultValue => 0; // Default to the first resolution in the list (highest available in this case)
        public static int CurrentIndex { get; private set; }

        [SerializeField] private List<Toggle> _resolutionToggles;   //Toggle components for each resolution option
        [SerializeField] private List<TMP_Text> _resolutionOptionTexts; //Text components for each toggle option

        private static List<Resolution> _validResolutions;

        private void OnEnable()
        {
            AttachToggleListener();
        }

        private void OnDisable()
        {
            DetachToggleListener();
        }

        public override void Load()
        {
            _validResolutions = GetValidResolutions();

            //Clear all Texts
            foreach (TMP_Text text in _resolutionOptionTexts)
                text.text = string.Empty;

            //Initialize TMP_texts with valid resolutions
            int optionIndex = _resolutionOptionTexts != null ? _resolutionOptionTexts.Count : 0;
            for (int i = 0; i < optionIndex; i++)
            {
                // Assigning resolution text to each option, if available
                TMP_Text optionText = _resolutionOptionTexts[i]; 
                bool hasOption = i < _validResolutions.Count;
                if (optionText != null)
                {
                    optionText.text = hasOption ? $"{_validResolutions[i].width}x{_validResolutions[i].height}" : string.Empty;
                }

                // Show or hide toggles based on whether there's a corresponding resolution
                if (_resolutionToggles != null && i < _resolutionToggles.Count)
                {
                    var toggle = _resolutionToggles[i];
                    if (toggle != null)
                    {
                        toggle.gameObject.SetActive(hasOption); // Show toggle only if there's a corresponding resolution
                        toggle.isOn = false;
                    }
                }
            }

            CurrentIndex = SaveManager.SettingsStore.GetInt(SaveKey, DefaultValue);
            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, _validResolutions.Count - 1); // Ensure index is within bounds
            SetTogglesToIndex(CurrentIndex);
        }

        public override void Apply()
        {
            int selectedIndex = GetSelectedIndex(); // Get index of currently selected toggle
            
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
        

        private void AttachToggleListener()
        {
            if (_resolutionToggles == null)
                return;

            for (int i = 0; i < _resolutionToggles.Count; i++)
            {
                var toggle = _resolutionToggles[i];
                if (toggle == null)
                    continue;

                // Remove previous listeners to avoid duplicates, then add the listener.
                toggle.onValueChanged.RemoveAllListeners();
                int capturedIndex = i;
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        // Enforce single selection in UI
                        for (int j = 0; j < _resolutionToggles.Count; j++)
                        {
                            if (j == capturedIndex) continue;
                            var other = _resolutionToggles[j];
                            if (other != null && other.isOn)
                                other.isOn = false;
                        }
                    }

                    // Notify that selection changed (enable Apply button externally)
                    OnToggleValueChanged();
                });
            }
        }

        private void DetachToggleListener()
        {
            if (_resolutionToggles == null)
                return;

            foreach (var toggle in _resolutionToggles)
            {
                if (toggle != null)
                    toggle.onValueChanged.RemoveAllListeners();
            }
        }

        private void OnToggleValueChanged()
        {
            // Example: enable Apply button via some external manager or event
            // ApplyButton.interactable = IsDirty();
        }
        private int GetSelectedIndex()  
        {
            for (int i = 0; i < _resolutionToggles.Count; i++)
            {
                if (_resolutionToggles[i].isOn)
                    return i;
            }
            return DefaultValue; // Fallback to default if no toggle is selected
        }

        private void SetTogglesToIndex(int index)
        {
            for (int i = 0; i < _resolutionToggles.Count; i++)
            {
                if (_resolutionToggles[i] != null)
                    _resolutionToggles[i].isOn = (i == index);
            }
        }
    }
}
