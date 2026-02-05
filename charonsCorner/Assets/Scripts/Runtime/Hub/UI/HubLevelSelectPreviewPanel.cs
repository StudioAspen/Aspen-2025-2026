using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class HubLevelSelectPreviewPanel : UIPanel
    {
        [SerializeField] private TMP_Text _previewPanelTitleText;
        [SerializeField] private Image _previewPanelImage;

        public void SetData(LevelDataSO levelData)
        {
            // Set preview panel data
            _previewPanelTitleText.text = levelData.LevelTitle;
            _previewPanelImage.sprite = levelData.PreviewImage;
        }
    }
}