using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SettingsCanvas : Singleton<SettingsCanvas>
    {
        [field: SerializeField, Required] public SettingsPanel Panel { get; private set; }

        private void Start()
        {
            Panel.Initialize();
        }

        /// <summary>
        /// Focuses the settings panel.
        /// </summary>
        public static void ShowSettings()
        {
            UIPanel.Focus(Instance.Panel);
        }
    }
}