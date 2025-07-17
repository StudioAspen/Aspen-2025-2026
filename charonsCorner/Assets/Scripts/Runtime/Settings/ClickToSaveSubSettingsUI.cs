using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ClickToSaveSubSettingsUI : MonoBehaviour
    {
        private List<Setting> settings = new List<Setting>();

        // Disabled objects dont get awake called, so we need to initialize manually
        public void Initialize()
        {
            // Find all Setting components in children and add them to the list
            settings.AddRange(GetComponentsInChildren<Setting>());

            Load();
        }

        public void Save()
        {
            foreach (Setting setting in settings)
            {
                if (setting == null)
                    continue;

                setting.Save();
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
    }
}
