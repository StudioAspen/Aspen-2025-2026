using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class FramerateCapSetting : Setting
    {
        private protected override string playerPrefsKey => "Settings_FramerateCap";
        private static readonly int defaultValue = FramerateCapValues.Length - 1; // Default to no cap (last index)
        public static readonly int[] FramerateCapValues = { 30, 60, 120, 144, 240, 0 }; // 0 means no cap
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
                framerateCapDropdown.options.Add(new TMP_Dropdown.OptionData(value == 0 ? "No Cap" : value.ToString()));

            CurrentIndex = PlayerPrefs.GetInt(playerPrefsKey, defaultValue);
            framerateCapDropdown.value = CurrentIndex;
            Application.targetFrameRate = FramerateCapValues[CurrentIndex];
        }

        public override void Apply()
        {
            int framerateCapIndex = framerateCapDropdown.value;
            PlayerPrefs.SetInt(playerPrefsKey, framerateCapIndex);
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
