using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(InputManager))]
    public class UIInputBridge : MonoBehaviour
    {
        private InputManager _inputManager;
    
        private void Start()
        {
            _inputManager = GetComponent<InputManager>();

            _inputManager.OnControlSchemeChanged += InputManager_OnControlSchemeChanged;
        
            UIPanel.OnPanelChanged += UIPanel_OnPanelChanged;
            UIPanel_OnPanelChanged(UIPanel.ActivePanel);
        }

        private void OnDestroy()
        {
            _inputManager.OnControlSchemeChanged -= InputManager_OnControlSchemeChanged;
        
            UIPanel.OnPanelChanged -= UIPanel_OnPanelChanged;
        }

        private void UIPanel_OnPanelChanged(UIPanel newPanel)
        {
            if (newPanel == null || newPanel.EnablePlayerActions)
            {
                _inputManager.EnablePlayerActions();
            }
            else
            {
                _inputManager.EnableUIActions();
            }
        }
    
        private void InputManager_OnControlSchemeChanged(InputManager.ControlScheme newControlScheme)
        {
            UIPanel.SetCurrentSelectedObject();
        }
    }
}