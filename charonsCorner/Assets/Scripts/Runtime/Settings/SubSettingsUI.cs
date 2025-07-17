using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SubSettingsUI : MonoBehaviour
    {
        private List<Setting> settings = new List<Setting>();

        public bool IsDirty => settings.Exists(setting => setting != null && setting.IsDirty());

        // Disabled objects dont get awake called, so we need to initialize manually
        public void Initialize()
        {
            // Find all Setting components in children and add them to the list
            settings.AddRange(GetComponentsInChildren<Setting>());

            Load();
        }

        public void Apply()
        {
            foreach (Setting setting in settings)
            {
                if (setting == null)
                    continue;
                setting.Apply();
            }
        }

        private void Load()
        {
            foreach (Setting setting in settings)
            {
                if (setting == null)
                    continue;
                setting.Load();
            }
        }

        public void Discard()
        {
            foreach (Setting setting in settings)
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
