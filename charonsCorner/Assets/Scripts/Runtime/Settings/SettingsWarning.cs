using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SettingsWarning : MonoBehaviour
    {
        [SerializeField] private SettingsUI _settingsUI;
        [SerializeField] private GameObject _warningDefaultSelectedObject;

        private void OnEnable()
        {
            UIManager.Instance.ChangeCurrentSelectedObject(_warningDefaultSelectedObject);
        }

        private void OnDisable()
        {
            UIManager.Instance.ChangeCurrentSelectedObject(_settingsUI.DefaultSelectedObject);
        }
    }
}
