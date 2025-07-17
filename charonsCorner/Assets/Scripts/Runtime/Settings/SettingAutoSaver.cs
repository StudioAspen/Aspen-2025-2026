using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Setting))]
    public class SettingAutoSaver : MonoBehaviour
    {
        private Setting setting;

        // Disabled objects dont get awake called, so we need to initialize manually
        public void Initialize()
        {
            setting = GetComponent<Setting>();
            setting.Load();
        }

        private void OnDestroy()
        {
            setting.Save();
        }
    }
}
