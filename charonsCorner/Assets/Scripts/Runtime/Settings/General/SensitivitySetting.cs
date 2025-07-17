using System;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class SensitivitySetting : Setting
    {
        private protected override string playerPrefsKey => "Settings_Sensitivity";
        private static readonly float defaultValue = 50;
        public static float CurrentValue { get; private set; }


        [SerializeField] private Slider sensitivitySlider;

        private void OnEnable()
        {
            sensitivitySlider.value = CurrentValue;
        }

        public override void Load()
        {
            CurrentValue = PlayerPrefs.GetFloat(playerPrefsKey, defaultValue);
            sensitivitySlider.value = CurrentValue;
        }

        public override void Apply()
        {
            Debug.Log($"Applying sensitivity setting: {sensitivitySlider.value}");
            PlayerPrefs.SetFloat(playerPrefsKey, sensitivitySlider.value);
            CurrentValue = sensitivitySlider.value;
        }

        public override void Discard()
        {
            sensitivitySlider.value = CurrentValue;
        }
    }
}
