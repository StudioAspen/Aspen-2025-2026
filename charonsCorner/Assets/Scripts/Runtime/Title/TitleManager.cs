using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] private GameObject _defaultSelectedObject;

        private void Start()
        {
            UIManager.Instance.OnUIClose += UIManager_OnUIClose;
            UIManager_OnUIClose();
        }

        private void OnDestroy()
        {
            if(UIManager.Instance != null)
                UIManager.Instance.OnUIClose -= UIManager_OnUIClose;
        }

        private void UIManager_OnUIClose() => UIManager.Instance.ChangeCurrentSelectedObject(_defaultSelectedObject);

        // Called by button UI events
        public void StartGame() => GameManager.Instance.StartGame();
        public void ShowSettings() => UIManager.Instance.ShowPanel(UIManager.PanelName.Settings);
        public static void QuitGame() => GameManager.QuitGame();
    }
}
