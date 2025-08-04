using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class VsyncSetting : Setting
    {
        private protected override string saveKey => "Vsync";
        private static readonly bool defaultValue = false;
        public static bool CurrentValue { get; private set; }

        [SerializeField] private Toggle vsyncToggle;

        private void OnEnable()
        {
            vsyncToggle.isOn = CurrentValue;
        }

        public override void Load()
        {
            CurrentValue = SaveManager.SettingsStore.GetBool(saveKey, defaultValue);
            vsyncToggle.isOn = CurrentValue;
            QualitySettings.vSyncCount = CurrentValue ? 1 : 0;
        }

        public override void Apply()
        {
            bool vsyncValue = vsyncToggle.isOn;
            SaveManager.SettingsStore.SetBool(saveKey, vsyncValue);
            CurrentValue = vsyncValue;
            QualitySettings.vSyncCount = CurrentValue ? 1 : 0;
        }

        public override void Discard()
        {
            vsyncToggle.isOn = CurrentValue;
        }

        public override bool IsDirty() => vsyncToggle.isOn != CurrentValue;
    }
}
