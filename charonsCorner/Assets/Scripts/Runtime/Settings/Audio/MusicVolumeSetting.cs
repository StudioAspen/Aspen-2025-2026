using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class MusicVolumeSetting : Setting
    {
        private protected override string SaveKey => "MusicVolume";
        private static readonly int DefaultValue = 50;
        public static int CurrentValue { get; private set; }

        [SerializeField] private Slider _musicVolumeSlider;

        private void OnEnable()
        {
            _musicVolumeSlider.value = CurrentValue;
        }

        public override void Load()
        {
            CurrentValue = SaveManager.SettingsStore.GetInt(SaveKey, DefaultValue);
            _musicVolumeSlider.value = CurrentValue;
        }

        public override void Apply()
        {
            int musicVolume = (int)_musicVolumeSlider.value;
            SaveManager.SettingsStore.SetInt(SaveKey, musicVolume);
            CurrentValue = musicVolume;
        }

        public override void Discard()
        {
            _musicVolumeSlider.value = CurrentValue;
        }

        public override bool IsDirty() => _musicVolumeSlider.value != CurrentValue;
    }
}
