using System;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class SensitivitySetting : Setting
    {
        private protected override string saveKey => "Sensitivity";
        private static readonly int defaultValue = 50;
        public static float CurrentValue { get; private set; }

        [SerializeField] private Slider sensitivitySlider;

        private void OnEnable()
        {
            sensitivitySlider.value = CurrentValue;
        }

        public override void Load()
        {
            CurrentValue = SaveManager.SettingsStore.Data.GetInt(saveKey, defaultValue);
            sensitivitySlider.value = CurrentValue;
        }

        public override void Apply()
        {
            int sensitivityValue = (int)sensitivitySlider.value;
            SaveManager.SettingsStore.Data.SetInt(saveKey, sensitivityValue);
            CurrentValue = sensitivityValue;
        }

        public override void Discard()
        {
            sensitivitySlider.value = CurrentValue;
        }

        public override bool IsDirty() => sensitivitySlider.value != CurrentValue;
    }
}
