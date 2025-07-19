using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class VsyncSetting : Setting
    {
        private protected override string playerPrefsKey => "Settings_Vsync";
        private static readonly int defaultValue = 0;
        public static int CurrentValue { get; private set; }

        [SerializeField] private Toggle vsyncToggle;

        private void OnEnable()
        {
            vsyncToggle.isOn = CurrentValue == 1;
        }

        public override void Load()
        {
            CurrentValue = PlayerPrefs.GetInt(playerPrefsKey, defaultValue);
            vsyncToggle.isOn = CurrentValue == 1;
            QualitySettings.vSyncCount = CurrentValue;
        }

        public override void Apply()
        {
            int vsyncValue = vsyncToggle.isOn ? 1 : 0;
            PlayerPrefs.SetInt(playerPrefsKey, vsyncValue);
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
