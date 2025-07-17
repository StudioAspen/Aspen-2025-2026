using System;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class SensitivitySetting : Setting
    {
        private protected override string playerPrefsKey => "Settings_Sensitivity";
        private static readonly int defaultValue = 50;
        public static int CurrentValue { get; private set; }

        [SerializeField] private Slider sensitivitySlider;

        private void OnEnable()
        {
            sensitivitySlider.value = CurrentValue;
        }

        public override void Load()
        {
            CurrentValue = PlayerPrefs.GetInt(playerPrefsKey, defaultValue);
            sensitivitySlider.value = CurrentValue;
        }

        public override void Apply()
        {
            int sensitivityValue = (int)sensitivitySlider.value;
            PlayerPrefs.SetInt(playerPrefsKey, sensitivityValue);
            CurrentValue = sensitivityValue;
        }

        public override void Discard()
        {
            sensitivitySlider.value = CurrentValue;
        }

        public override bool IsDirty() => sensitivitySlider.value != CurrentValue;
    }
}
