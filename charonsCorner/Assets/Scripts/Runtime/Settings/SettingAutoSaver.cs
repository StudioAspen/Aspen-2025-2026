using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Setting))]
    public class SettingAutoSaver : MonoBehaviour
    {
        private Setting setting;

        private void Awake()
        {
            setting = GetComponent<Setting>();
        }

        private void Start()
        {
            setting.Load();
        }

        private void OnDestroy()
        {
            setting.Save();
        }
    }
}
