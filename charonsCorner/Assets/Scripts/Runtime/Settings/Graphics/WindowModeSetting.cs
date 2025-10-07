using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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
        private protected override string saveKey => "WindowMode";
        private static int defaultValue => 0; // Default to the first mode (Fullscreen)
        public static int CurrentIndex { get; private set; }

        [SerializeField] private TMP_Dropdown windowModeDropdown;

        private void OnEnable()
        {
            windowModeDropdown.value = CurrentIndex;
        }

        public override void Load()
        {
            // Clear existing options and add new ones
            windowModeDropdown.ClearOptions();
            foreach (var mode in WindowModes)
                windowModeDropdown.options.Add(new TMP_Dropdown.OptionData($"{mode.name}"));

            CurrentIndex = SaveManager.SettingsStore.GetInt(saveKey, defaultValue);
            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, WindowModes.Count - 1); // Ensure index is within bounds

            windowModeDropdown.value = CurrentIndex;
            Screen.fullScreenMode = WindowModes[CurrentIndex].mode;
        }

        public override void Apply()
        {
            int windowModeIndex = windowModeDropdown.value;
            SaveManager.SettingsStore.SetInt(saveKey, windowModeIndex);
            CurrentIndex = windowModeIndex;
            Screen.fullScreenMode = WindowModes[CurrentIndex].mode;
        }

        public override void Discard()
        {
            windowModeDropdown.value = CurrentIndex;
        }

        public override bool IsDirty() => windowModeDropdown.value != CurrentIndex;
    }
}
