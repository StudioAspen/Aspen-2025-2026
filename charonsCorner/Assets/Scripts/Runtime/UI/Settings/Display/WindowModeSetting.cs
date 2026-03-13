using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class WindowModeSetting : SettingsToggleIndex
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

        [SerializeField] private List<TMP_Text> _windowModeOptionTexts;

        public override void Load()
        {
            PopulateTexts();

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

        private void PopulateTexts()
        {
            if (_windowModeOptionTexts == null) return;

            int toggleCount = _toggles?.Count ?? 0;
            for (int i = 0; i < _windowModeOptionTexts.Count; i++)
            {
                if (_windowModeOptionTexts[i] == null)
                    continue;

                bool hasOption = i < WindowModes.Count;
                _windowModeOptionTexts[i].text =
                    hasOption ? WindowModes[i].name : string.Empty;

                if (i < toggleCount && _toggles[i] != null)
                    _toggles[i].gameObject.SetActive(hasOption);
            }
        }
    }
}
