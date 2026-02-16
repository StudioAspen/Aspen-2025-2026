using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class HubLevelSelectDetailsPanel : UIPanel
    {
        [SerializeField] private TMP_Text _detailsPanelTitleText;
        [SerializeField] private TMP_Text _detailsPanelBestText;
        [SerializeField] private TMP_Text _detailsPanelPinsText;
        [SerializeField] private Image _detailsPanelImage;
        
        public void SetData(LevelDataSO levelData)
        {
            // Set details panel data
            _detailsPanelTitleText.text = levelData.LevelTitle;
            _detailsPanelImage.sprite = levelData.PreviewImage;
            _detailsPanelBestText.text = $"Best Time: WIP";
            _detailsPanelPinsText.text = $"Pins: WIP";
        }
    }
}