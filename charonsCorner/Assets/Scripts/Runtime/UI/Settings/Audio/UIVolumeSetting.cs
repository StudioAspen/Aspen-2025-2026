using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class UIVolumeSetting : Setting
    {
        private protected override string SaveKey => "UIVolume";
        private static readonly float DefaultValue = 0.5f;

        public static float CurrentValue { get; private set; } = DefaultValue;
        [SerializeField] private Slider _dialogueSlider;

        private bool EnsureSliderReference()
        {
            if (_dialogueSlider != null) return true;
            Debug.LogError($"{nameof(UIVolumeSetting)} requires {nameof(_dialogueSlider)} to be assigned.", this);
            return false;
        }

        private void OnEnable()
        {
            if (!EnsureSliderReference()) return;

            _dialogueSlider.value = CurrentValue;
        }

        public override void Load()
        {
            if (!EnsureSliderReference()) return;

            CurrentValue = Mathf.Clamp01(SaveManager.SettingsStore.GetFloat(SaveKey, DefaultValue));
            _dialogueSlider.value = CurrentValue;

            AudioManager.SetMixerVolume(AudioManager.UIVolumeParam, CurrentValue);
        }

        public override void Apply()
        {
            if (!EnsureSliderReference()) return;

            float dialogueVolume = Mathf.Clamp01(_dialogueSlider.value);
            SaveManager.SettingsStore.SetFloat(SaveKey, dialogueVolume);
            CurrentValue = dialogueVolume;
            AudioManager.SetMixerVolume(AudioManager.UIVolumeParam, CurrentValue);
        }

        public override void Discard()
        {
            if (!EnsureSliderReference()) return;

            _dialogueSlider.value = CurrentValue;
        }

        public override bool IsDirty() => !Mathf.Approximately(_dialogueSlider.value, CurrentValue);
    }
}
