using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class DialogueVolumeSetting : Setting
    {
        private protected override string SaveKey => "UIVolume";
        private static readonly float DefaultValue = 0.5f;

        public static float CurrentValue { get; private set; } = DefaultValue;
        [SerializeField] private Slider _dialogueSlider;

        private void OnEnable()
        {
            _dialogueSlider.value = CurrentValue;
        }

        public override void Load()
        {
            CurrentValue = SaveManager.SettingsStore.GetFloat(SaveKey, DefaultValue);
            _dialogueSlider.value = CurrentValue;

            AudioManager.SetMixerVolume(AudioManager.UIVolumeParam, CurrentValue);
        }

        public override void Apply()
        {
            float dialogueVolume = _dialogueSlider.value;
            SaveManager.SettingsStore.SetFloat(SaveKey, dialogueVolume);
            CurrentValue = dialogueVolume;
            AudioManager.SetMixerVolume(AudioManager.UIVolumeParam, CurrentValue);
        }

        public override void Discard()
        {
            _dialogueSlider.value = CurrentValue;
        }

        public override bool IsDirty() => _dialogueSlider.value != CurrentValue;
    }
}
