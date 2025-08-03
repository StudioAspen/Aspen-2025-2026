using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class VsyncSetting : Setting
    {
        private protected override string saveKey => "Vsync";
        private static readonly int defaultValue = 0;
        public static int CurrentValue { get; private set; }

        [SerializeField] private Toggle vsyncToggle;

        private void OnEnable()
        {
            vsyncToggle.isOn = CurrentValue == 1;
        }

        public override void Load()
        {
            CurrentValue = SaveManager.SettingsStore.Data.GetInt(saveKey, defaultValue);
            vsyncToggle.isOn = CurrentValue == 1;
            QualitySettings.vSyncCount = CurrentValue;
        }

        public override void Apply()
        {
            int vsyncValue = vsyncToggle.isOn ? 1 : 0;
            SaveManager.SettingsStore.Data.SetInt(saveKey, vsyncValue);
            CurrentValue = vsyncValue;
            QualitySettings.vSyncCount = CurrentValue;
        }

        public override void Discard()
        {
            vsyncToggle.isOn = CurrentValue == 1;
        }

        public override bool IsDirty() => vsyncToggle.isOn != (CurrentValue == 1);
    }
}
