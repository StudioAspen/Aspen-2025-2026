using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class VsyncSetting : Setting
    {
        private protected override string SaveKey => "Vsync";
        private static readonly bool DefaultValue = false;
        public static bool CurrentValue { get; private set; }

        [SerializeField] private Toggle _vsyncToggle;

        private void OnEnable()
        {
            _vsyncToggle.isOn = CurrentValue;
        }

        public override void Load()
        {
            CurrentValue = SaveManager.SettingsStore.GetBool(SaveKey, DefaultValue);
            _vsyncToggle.isOn = CurrentValue;
            QualitySettings.vSyncCount = CurrentValue ? 1 : 0;
        }

        public override void Apply()
        {
            bool vsyncValue = _vsyncToggle.isOn;
            SaveManager.SettingsStore.SetBool(SaveKey, vsyncValue);
            CurrentValue = vsyncValue;
            QualitySettings.vSyncCount = CurrentValue ? 1 : 0;
        }

        public override void Discard()
        {
            _vsyncToggle.isOn = CurrentValue;
        }

        public override bool IsDirty() => _vsyncToggle.isOn != CurrentValue;
    }
}
