using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class EffectVolumeSetting : Setting
    {
        private protected override string SaveKey => "SFXVolume";
        private const float DefaultValue = 0.5f;

        public static float CurrentValue { get; private set; } = DefaultValue;

        [SerializeField] private Slider _effectSlider;

        private bool EnsureSliderReference()
        {
            if (_effectSlider != null) return true;

            Debug.LogError($"{nameof(EffectVolumeSetting)} requires {nameof(_effectSlider)} to be assigned.", this);
            return false;
        }

        private void OnEnable()
        {
            if (!EnsureSliderReference()) return;
            _effectSlider.value = CurrentValue;
        }

        public override void Load()
        {
            if (!EnsureSliderReference()) return;
            CurrentValue = Mathf.Clamp01(SaveManager.SettingsStore.GetFloat(SaveKey, DefaultValue));
            _effectSlider.value = CurrentValue;

            AudioManager.SetMixerVolume(AudioManager.SfxVolumeParam, CurrentValue);
        }

        public override void Apply()
        {
            if (!EnsureSliderReference()) return;
            float effectVolume = Mathf.Clamp01(_effectSlider.value);
            SaveManager.SettingsStore.SetFloat(SaveKey, effectVolume);
            CurrentValue = effectVolume;

            AudioManager.SetMixerVolume(AudioManager.SfxVolumeParam, CurrentValue);
        }

        public override void Discard()
        {
            if (!EnsureSliderReference()) return;
            _effectSlider.value = CurrentValue;
        }

        public override bool IsDirty() => _effectSlider.value != CurrentValue;
    }
}
