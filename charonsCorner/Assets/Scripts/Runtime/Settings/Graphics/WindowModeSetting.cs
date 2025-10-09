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
        private protected override string SaveKey => "WindowMode";
        private static int DefaultValue => 0; // Default to the first mode (Fullscreen)
        public static int CurrentIndex { get; private set; }

        [SerializeField] private TMP_Dropdown _windowModeDropdown;

        private void OnEnable()
        {
            _windowModeDropdown.value = CurrentIndex;
        }

        public override void Load()
        {
            // Clear existing options and add new ones
            _windowModeDropdown.ClearOptions();
            foreach (var mode in WindowModes)
                _windowModeDropdown.options.Add(new TMP_Dropdown.OptionData($"{mode.name}"));

            CurrentIndex = SaveManager.SettingsStore.GetInt(SaveKey, DefaultValue);
            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, WindowModes.Count - 1); // Ensure index is within bounds

            _windowModeDropdown.value = CurrentIndex;
            Screen.fullScreenMode = WindowModes[CurrentIndex].mode;
        }

        public override void Apply()
        {
            int windowModeIndex = _windowModeDropdown.value;
            SaveManager.SettingsStore.SetInt(SaveKey, windowModeIndex);
            CurrentIndex = windowModeIndex;
            Screen.fullScreenMode = WindowModes[CurrentIndex].mode;
        }

        public override void Discard()
        {
            _windowModeDropdown.value = CurrentIndex;
        }

        public override bool IsDirty() => _windowModeDropdown.value != CurrentIndex;
    }
}
