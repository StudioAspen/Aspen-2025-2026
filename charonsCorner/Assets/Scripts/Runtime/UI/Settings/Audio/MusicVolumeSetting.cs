using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class MusicVolumeSetting : Setting
    {
        private protected override string SaveKey => "MusicVolume";
        private const float DefaultValue = 0.5f;

        public static float CurrentValue { get; private set; } = DefaultValue;

        [SerializeField] private Slider _musicSlider;

        private bool IsSliderBound()
        {
            if (_musicSlider != null) return true;
            Debug.LogError($"{nameof(MusicVolumeSetting)} requires {nameof(_musicSlider)} to be assigned.", this);
            return false;
        }

        private void OnEnable()
        {
            if (!IsSliderBound()) return;
            _musicSlider.value = CurrentValue;
        }

        public override void Load()
        {
            if (!IsSliderBound()) return;
            CurrentValue = SaveManager.SettingsStore.GetFloat(SaveKey, DefaultValue);
            _musicSlider.value = CurrentValue;

            AudioManager.SetMixerVolume(AudioManager.MusicVolumeParam, CurrentValue);
        }

        public override void Apply()
        {
            if (!IsSliderBound()) return;
            float musicVolume = _musicSlider.value;
            SaveManager.SettingsStore.SetFloat(SaveKey, musicVolume);
            CurrentValue = musicVolume;

            AudioManager.SetMixerVolume(AudioManager.MusicVolumeParam, CurrentValue);
        }

        public override void Discard()
        {
            if (!IsSliderBound()) return;
            _musicSlider.value = CurrentValue;
        }

        public override bool IsDirty() => IsSliderBound() && _musicSlider.value != CurrentValue;
    }
}
