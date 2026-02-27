using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class WindowModeSetting : Setting
    {
        public static readonly List<(string name, FullScreenMode mode)> WindowModes = new List<(string, FullScreenMode)>
        {
            ("Fullscreen", FullScreenMode.ExclusiveFullScreen),
            ("Windowed", FullScreenMode.Windowed),
            ("Borderless", FullScreenMode.FullScreenWindow),
        };

        private protected override string SaveKey => "WindowMode";
        private static int DefaultValue => 0; // Default to the first mode (Fullscreen)
        public static int CurrentIndex { get; private set; }

        [SerializeField] private List<Toggle> _windowModeToggles;
        [SerializeField] private List<TMP_Text> _windowModeOptionTexts;

        private void OnEnable()
        {
            AttachToggleListeners();
        }

        private void OnDisable()
        {
            DetachToggleListeners();
        }

        public override void Load()
        {
            // Clear all texts first
            if (_windowModeOptionTexts != null)
            {
                foreach (TMP_Text text in _windowModeOptionTexts)
                {
                    if (text != null)
                        text.text = string.Empty;
                }
            }

            // Populate option texts and show/hide toggles
            int optionCount = _windowModeOptionTexts != null ? _windowModeOptionTexts.Count : 0;
            for (int i = 0; i < optionCount; i++)
            {
                TMP_Text optionText = _windowModeOptionTexts[i];
                bool hasOption = i < WindowModes.Count;
                if (optionText != null)
                    optionText.text = hasOption ? WindowModes[i].name : string.Empty;

                if (_windowModeToggles != null && i < _windowModeToggles.Count)
                {
                    var toggle = _windowModeToggles[i];
                    if (toggle != null)
                    {
                        toggle.gameObject.SetActive(hasOption); // Show toggle only if there's a corresponding window mode
                        toggle.isOn = false;
                    }
                }
            }

            // Ensure any extra toggles beyond labels are hidden
            if (_windowModeToggles != null)
            {
                for (int i = optionCount; i < _windowModeToggles.Count; i++)
                {
                    var t = _windowModeToggles[i];
                    if (t != null)
                        t.gameObject.SetActive(false);
                }
            }

            // Restore saved index (visual only). Do not apply until explicit Apply().
            CurrentIndex = SaveManager.SettingsStore.GetInt(SaveKey, DefaultValue);
            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, WindowModes.Count - 1);

            SetTogglesToIndex(CurrentIndex);

            // Ensure runtime reflects last applied window mode
            Screen.fullScreenMode = WindowModes[CurrentIndex].mode;
        }

        public override void Apply()
        {
            int windowModeIndex = GetSelectedIndex();
            if (windowModeIndex < 0 || windowModeIndex >= WindowModes.Count)
                return;

            SaveManager.SettingsStore.SetInt(SaveKey, windowModeIndex);
            CurrentIndex = windowModeIndex;
            Screen.fullScreenMode = WindowModes[CurrentIndex].mode;
        }

        public override void Discard()
        {
            SetTogglesToIndex(CurrentIndex);
        }

        public override bool IsDirty() => GetSelectedIndex() != CurrentIndex;

        private void AttachToggleListeners()
        {
            if (_windowModeToggles == null)
                return;

            for (int i = 0; i < _windowModeToggles.Count; i++)
            {
                var toggle = _windowModeToggles[i];
                if (toggle == null)
                    continue;

                // Remove previous listeners to avoid duplicates and then add our listener
                toggle.onValueChanged.RemoveAllListeners();
                int capturedIndex = i;
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (!isOn)
                        return;

                    // Enforce single selection in UI (do not apply here; explicit Apply required)
                    for (int j = 0; j < _windowModeToggles.Count; j++)
                    {
                        if (j == capturedIndex) continue;
                        var other = _windowModeToggles[j];
                        if (other != null && other.isOn)
                            other.isOn = false;
                    }

                    // Notify changes (e.g., to enable Apply button) - hook up in UI controller if needed
                    OnToggleValueChanged();
                });
            }
        }

        private void DetachToggleListeners()
        {
            if (_windowModeToggles == null)
                return;

            foreach (var toggle in _windowModeToggles)
            {
                if (toggle != null)
                    toggle.onValueChanged.RemoveAllListeners();
            }
        }

        private void OnToggleValueChanged()
        {
            // Placeholder for UI hook (e.g., Apply button enabling).
            // Example usage: SettingsPanel.ApplyButton.interactable = IsDirty();
        }

        private int GetSelectedIndex()
        {
            if (_windowModeToggles == null)
                return DefaultValue;

            for (int i = 0; i < _windowModeToggles.Count; i++)
            {
                var t = _windowModeToggles[i];
                if (t != null && t.isOn)
                    return i;
            }

            // fallback
            return DefaultValue;
        }

        private void SetTogglesToIndex(int index)
        {
            if (_windowModeToggles == null)
                return;

            for (int i = 0; i < _windowModeToggles.Count; i++)
            {
                var t = _windowModeToggles[i];
                if (t == null)
                    continue;

                // Temporarily remove listeners to avoid triggering logic while setting programmatically
                t.onValueChanged.RemoveAllListeners();
                t.isOn = (i == index);
                int capturedIndex = i;
                t.onValueChanged.AddListener(isOn =>
                {
                    if (!isOn) return;
                    for (int j = 0; j < _windowModeToggles.Count; j++)
                    {
                        if (j == capturedIndex) continue;
                        var other = _windowModeToggles[j];
                        if (other != null && other.isOn)
                            other.isOn = false;
                    }
                    OnToggleValueChanged();
                });
            }
        }
    }
}
