using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class MusicVolumeSetting : Setting
    {
        private protected override string playerPrefsKey => "Settings_MusicVolume";
        private static readonly int defaultValue = 50;
        public static int CurrentValue { get; private set; }

        [SerializeField] private Slider musicVolumeSlider;

        private void OnEnable()
        {
            musicVolumeSlider.value = CurrentValue;
        }

        public override void Load()
        {
            CurrentValue = PlayerPrefs.GetInt(playerPrefsKey, defaultValue);
            musicVolumeSlider.value = CurrentValue;
        }

        public override void Apply()
        {
            int musicVolume = (int)musicVolumeSlider.value;
            PlayerPrefs.SetInt(playerPrefsKey, musicVolume);
            CurrentValue = musicVolume;
        }

        public override void Discard()
        {
            musicVolumeSlider.value = CurrentValue;
        }

        public override bool IsDirty() => musicVolumeSlider.value != CurrentValue;
    }
}
