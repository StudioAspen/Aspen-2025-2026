using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class FramerateCapSetting : Setting
    {
        private protected override string saveKey => "FramerateCap";
        private static int defaultValue = 0; // Default to no cap (first index)
        public static readonly int[] FramerateCapValues = {-1, 30, 60, 75, 100, 120, 144, 165, 170, 180, 200, 240}; // 0 means no cap
        public static int CurrentIndex { get; private set; }

        [SerializeField] private TMP_Dropdown framerateCapDropdown;

        private void OnEnable()
        {
            framerateCapDropdown.value = CurrentIndex; 
        }

        public override void Load()
        {
            // Clear existing options and add new ones
            framerateCapDropdown.ClearOptions();
            foreach (int value in FramerateCapValues)
                framerateCapDropdown.options.Add(new TMP_Dropdown.OptionData(value == -1 ? "No Cap" : value.ToString()));

            CurrentIndex = SaveManager.SettingsStore.Data.GetInt(saveKey, defaultValue);
            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, FramerateCapValues.Length - 1); // Ensure index is within bounds

            framerateCapDropdown.value = CurrentIndex;
            Application.targetFrameRate = FramerateCapValues[CurrentIndex];
        }

        public override void Apply()
        {
            int framerateCapIndex = framerateCapDropdown.value;
            SaveManager.SettingsStore.Data.SetInt(saveKey, framerateCapIndex);
            CurrentIndex = framerateCapIndex;
            Application.targetFrameRate = FramerateCapValues[CurrentIndex];
        }

        public override void Discard()
        {
            framerateCapDropdown.value = CurrentIndex;
        }

        public override bool IsDirty() => framerateCapDropdown.value != CurrentIndex;
    }
}
