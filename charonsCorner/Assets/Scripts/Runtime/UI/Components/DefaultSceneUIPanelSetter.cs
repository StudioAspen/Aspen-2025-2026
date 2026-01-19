using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(UIPanel))]
    public class DefaultSceneUIPanelSetter : MonoBehaviour
    {
        private UIPanel _panel;

        private void Awake()
        {
            _panel = GetComponent<UIPanel>();
            _panel.SetPreviousPanel(null);
            if (UIPanel.ActivePanel == null)
            {
                _panel.Focus();
            }
            else
            {
                UIPanel.ActivePanel.SetPreviousPanel(_panel);
            }
        }
    }
}