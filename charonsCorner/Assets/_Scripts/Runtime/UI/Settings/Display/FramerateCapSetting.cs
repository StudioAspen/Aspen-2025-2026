using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class FramerateCapSetting : Setting
    {
        private protected override string SaveKey => "FramerateCap";
        private static int _defaultValue = 0; // Default to no cap (first index)
        public static readonly int[] FramerateCapValues = {-1, 30, 60, 75, 100, 120, 144, 165, 170, 180, 200, 240}; // 0 means no cap
        public static int CurrentIndex { get; private set; }

        [SerializeField] private TMP_Dropdown _framerateCapDropdown;

        private void OnEnable()
        {
            _framerateCapDropdown.value = CurrentIndex;
            UpdateLabelColor();
        }

        public override void Load()
        {
            // Clear existing options and add new ones
            _framerateCapDropdown.ClearOptions();
            foreach (int value in FramerateCapValues)
                _framerateCapDropdown.options.Add(new TMP_Dropdown.OptionData(value == -1 ? "No Cap" : value.ToString()));

            CurrentIndex = SaveManager.SettingsStore.GetInt(SaveKey, _defaultValue);
            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, FramerateCapValues.Length - 1); // Ensure index is within bounds

            _framerateCapDropdown.value = CurrentIndex;
            Application.targetFrameRate = FramerateCapValues[CurrentIndex];
        }

        public override void Apply()
        {
            int framerateCapIndex = _framerateCapDropdown.value;
            SaveManager.SettingsStore.SetInt(SaveKey, framerateCapIndex);
            CurrentIndex = framerateCapIndex;
            Application.targetFrameRate = FramerateCapValues[CurrentIndex];
        }

        public override void Discard()
        {
            _framerateCapDropdown.value = CurrentIndex;
        }

        public override bool IsDirty() => _framerateCapDropdown.value != CurrentIndex;
    }
}
