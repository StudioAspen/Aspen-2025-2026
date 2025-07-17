using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class SensitivitySetting : Setting
    {
        public static readonly string PlayerPrefsKey = "Settings_Sensitivity";
        public static readonly float DefaultValue = 50;

        [SerializeField] private Slider sensitivitySlider;

        public override void Load()
        {
            sensitivitySlider.value = GetSensitivity();
        }

        public override void Save()
        {
            PlayerPrefs.SetFloat(PlayerPrefsKey, sensitivitySlider.value);
        }

        public static float GetSensitivity() => PlayerPrefs.GetFloat(PlayerPrefsKey, DefaultValue);
    }
}
