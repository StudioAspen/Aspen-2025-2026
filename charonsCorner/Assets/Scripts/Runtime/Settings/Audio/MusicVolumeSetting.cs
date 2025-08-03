using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class MusicVolumeSetting : Setting
    {
        private protected override string saveKey => "MusicVolume";
        private static readonly int defaultValue = 50;
        public static int CurrentValue { get; private set; }

        [SerializeField] private Slider musicVolumeSlider;

        private void OnEnable()
        {
            musicVolumeSlider.value = CurrentValue;
        }

        public override void Load()
        {
            CurrentValue = SaveManager.SettingsStore.Data.GetInt(saveKey, defaultValue);
            musicVolumeSlider.value = CurrentValue;
        }

        public override void Apply()
        {
            int musicVolume = (int)musicVolumeSlider.value;
            SaveManager.SettingsStore.Data.SetInt(saveKey, musicVolume);
            CurrentValue = musicVolume;
        }

        public override void Discard()
        {
            musicVolumeSlider.value = CurrentValue;
        }

        public override bool IsDirty() => musicVolumeSlider.value != CurrentValue;
    }
}
