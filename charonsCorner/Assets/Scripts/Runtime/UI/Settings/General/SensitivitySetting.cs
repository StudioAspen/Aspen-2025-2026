using System;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class SensitivitySetting : Setting
    {
        private protected override string SaveKey => "Sensitivity";
        private static readonly int DefaultValue = 50;
        public static float CurrentValue { get; private set; }

        [SerializeField] private Slider _sensitivitySlider;

        private void OnEnable()
        {
            _sensitivitySlider.value = CurrentValue;
            UpdateLabelColor();
        }

        public override void Load()
        {
            CurrentValue = SaveManager.SettingsStore.GetInt(SaveKey, DefaultValue);
            _sensitivitySlider.value = CurrentValue;
        }

        public override void Apply()
        {
            int sensitivityValue = (int)_sensitivitySlider.value;
            SaveManager.SettingsStore.SetInt(SaveKey, sensitivityValue);
            CurrentValue = sensitivityValue;
        }

        public override void Discard()
        {
            _sensitivitySlider.value = CurrentValue;
        }

        public override bool IsDirty() => _sensitivitySlider.value != CurrentValue;
    }
}
