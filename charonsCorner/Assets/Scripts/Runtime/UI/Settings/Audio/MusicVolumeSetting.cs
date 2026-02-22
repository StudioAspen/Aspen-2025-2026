using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class MusicVolumeSetting : Setting
    {
        private protected override string SaveKey => "MusicVolume";
        private static readonly float DefaultValue = 0.5f;

        public static float CurrentValue { get; private set; }

        [SerializeField] private Slider _musicSlider;

        private void OnEnable()
        {
            _musicSlider.value = CurrentValue;
        }

        public override void Load()
        {
            CurrentValue = SaveManager.SettingsStore.GetFloat(SaveKey, DefaultValue);
            _musicSlider.value = CurrentValue;

            AudioManager.SetMixerVolume(AudioManager.MusicVolumeParam, CurrentValue);
        }

        public override void Apply()
        {
            float musicVolume = _musicSlider.value;
            SaveManager.SettingsStore.SetFloat(SaveKey, musicVolume);
            CurrentValue = musicVolume;
        }

        public override void Discard()
        {
            _musicSlider.value = CurrentValue;
        }

        public override bool IsDirty() => _musicSlider.value != CurrentValue;
    }
}
