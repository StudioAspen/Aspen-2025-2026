using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class SubSettingsUI : MonoBehaviour
    {
        [SerializeField] private Selectable _firstSelected;
        public Selectable FirstSelected => _firstSelected;
        [SerializeField] private Selectable _lastSelected;
        public Selectable LastSelected => _lastSelected;

        private List<Setting> _settings = new List<Setting>();

        public bool IsDirty => _settings.Exists(setting => setting != null && setting.IsDirty());

        /// <summary>
        /// Disabled objects dont get awake called, so we need to initialize manually
        /// </summary>
        public void Initialize()
        {
            // Find all Setting components in children and add them to the list
            _settings.AddRange(GetComponentsInChildren<Setting>());

            Load();
        }

        /// <summary>
        /// Applies the current values of all settings under this sub-settings UI.
        /// </summary>
        public void Apply()
        {
            foreach (Setting setting in _settings)
            {
                if (setting == null)
                    continue;
                setting.Apply();
            }
        }

        /// <summary>
        /// Loads the current values of all settings under this sub-settings UI.
        /// </summary>
        private void Load()
        {
            foreach (Setting setting in _settings)
            {
                if (setting == null)
                    continue;
                setting.Load();
            }
        }

        /// <summary>
        /// Discards any changes made to the settings under this sub-settings UI.
        /// </summary>
        public void Discard()
        {
            foreach (Setting setting in _settings)
            {
                if (setting == null)
                    continue;
                setting.Discard();
            }
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
