using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class HubLevelSelectUIController : MonoBehaviour
    {
        /*[Header("Controller")]
        [SerializeField] private HubLevelSelectController _controller;
        
        [Header("UI References")]
        [SerializeField] private HubLevelSelectPreviewPanel _previewPanel; 
        [SerializeField] private HubLevelSelectDetailsPanel _detailsPanel; 
        
        private void Awake()
        {
            //Subscribe to controller events
            _controller.OnLevelSelectOpen += HandleLevelSelectOpen;
            _controller.OnLevelSelectClose += HandleLevelSelectClose;
        }
        
        private void OnDestroy()
        {
            //Unsubscribe from controller events
            _controller.OnLevelSelectOpen -= HandleLevelSelectOpen;
            _controller.OnLevelSelectClose -= HandleLevelSelectClose;
        }
        
        private void HandleLevelSelectOpen(LevelDataSO levelData)
        {
            _previewPanel.SetData(levelData);
            _detailsPanel.SetData(levelData);
            
            UIPanel.Focus(_previewPanel);
        }

        private void HandleLevelSelectClose()
        {
            if(UIPanel.ActivePanel == _previewPanel)
                _previewPanel.Close();
            else if (UIPanel.ActivePanel == _detailsPanel)
                _detailsPanel.Close();
        }*/
    }
}