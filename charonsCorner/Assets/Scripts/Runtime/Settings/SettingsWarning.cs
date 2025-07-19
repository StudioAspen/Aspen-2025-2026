using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SettingsWarning : MonoBehaviour
    {
        [SerializeField] private SettingsUI settingsUI;
        [SerializeField] private GameObject warningDefaultSelectedObject;

        private void OnEnable()
        {
            UIManager.Instance.ChangeCurrentSelectedObject(warningDefaultSelectedObject);
        }

        private void OnDisable()
        {
            UIManager.Instance.ChangeCurrentSelectedObject(settingsUI.DefaultSelectedObject);
        }
    }
}
