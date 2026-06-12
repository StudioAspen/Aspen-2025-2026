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

        private void OnEnable()
        {
            _effectSlider.value = CurrentValue;
            UpdateLabelColor();
        }

        public override void Load()
        {
            CurrentValue = SaveManager.SettingsStore.GetFloat(SaveKey, DefaultValue);
            _effectSlider.value = CurrentValue;

            AudioManager.SetMixerVolume(AudioManager.SfxVolumeParam, CurrentValue);
        }

        public override void Apply()
        {
            float effectVolume = _effectSlider.value;
            SaveManager.SettingsStore.SetFloat(SaveKey, effectVolume);
            CurrentValue = effectVolume;

            AudioManager.SetMixerVolume(AudioManager.SfxVolumeParam, CurrentValue);
        }

        public override void Discard()
        {
            _effectSlider.value = CurrentValue;
        }

        public override bool IsDirty() => _effectSlider.value != CurrentValue;
    }
}
