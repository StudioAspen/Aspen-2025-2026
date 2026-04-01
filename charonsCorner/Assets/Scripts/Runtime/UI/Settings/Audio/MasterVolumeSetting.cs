using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class MasterVolumeSetting : Setting
    {
        private protected override string SaveKey => "MasterVolume";
        private static readonly float DefaultValue = 0.5f;
        public static float CurrentValue { get; private set; }

        [SerializeField] private Slider _volumeSlider;

        private void OnEnable()
        {
            _volumeSlider.value = CurrentValue;
        }

        public override void Load()
        {
            CurrentValue = SaveManager.SettingsStore.GetFloat(SaveKey, DefaultValue);
            _volumeSlider.value = CurrentValue;

            AudioManager.SetMixerVolume(AudioManager.MasterVolumeParam, CurrentValue);
        }

        public override void Apply()
        {
            float volume = _volumeSlider.value;
            SaveManager.SettingsStore.SetFloat(SaveKey, volume);
            CurrentValue = volume;
        
            AudioManager.SetMixerVolume(AudioManager.MasterVolumeParam, CurrentValue);
        }

        public override void Discard()
        {
            _volumeSlider.value = CurrentValue;
        }

        public override bool IsDirty() => _volumeSlider.value != CurrentValue;
    }
}