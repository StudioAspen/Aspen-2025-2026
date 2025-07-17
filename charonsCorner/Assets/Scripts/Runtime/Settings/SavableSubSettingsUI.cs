using System.Collections.Generic;
using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SavableSubSettingsUI : MonoBehaviour
    {
        private List<Setting> settings = new List<Setting>();

        private void Awake()
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
