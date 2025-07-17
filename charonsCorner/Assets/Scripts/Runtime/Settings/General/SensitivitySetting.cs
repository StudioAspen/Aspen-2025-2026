using System;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class SensitivitySetting : Setting
    {
        public static readonly string PlayerPrefsKey = "Settings_Sensitivity";
        public static readonly float DefaultValue = 50;
        public static float CurrentValue { get; private set; }

        [SerializeField] private Slider sensitivitySlider;

        private void OnEnable()
        {
            sensitivitySlider.onValueChanged.AddListener(SensitivitySlider_OnValueChanged);
        }

        private void OnDisable()
        {
            sensitivitySlider.onValueChanged.RemoveListener(SensitivitySlider_OnValueChanged);
        }

        private void SensitivitySlider_OnValueChanged(float newValue)
        {
            CurrentValue = newValue;
        }

        public override void Load()
        {
            CurrentValue = PlayerPrefs.GetFloat(PlayerPrefsKey, DefaultValue);
            sensitivitySlider.value = CurrentValue;
        }

        public override void Save()
        {
            PlayerPrefs.SetFloat(PlayerPrefsKey, sensitivitySlider.value);
        }
    }
}
