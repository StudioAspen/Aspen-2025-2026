using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class HubLevelSelectUI : SceneUI
    {
        [Header("Controller")]
        [SerializeField] private HubLevelSelectController _controller;

        [Header("Preview Panel References")]
        [SerializeField] private GameObject _previewPanelObject;
        [SerializeField] private TMP_Text _previewPanelTitleText;
        [SerializeField] private Image _previewPanelImage;

        [Header("Details Panel References")]
        [SerializeField] private GameObject _detailsPanelObject;
        [SerializeField] private TMP_Text _detailsPanelTitleText;
        [SerializeField] private TMP_Text _detailsPanelBestText;
        [SerializeField] private TMP_Text _detailsPanelPinsText;
        [SerializeField] private Image _detailsPanelImage;

        public enum LevelSelectPanel
        {
            Preview,
            Details,
        }

        private protected override void OnAwake()
        {
            //Subscribe to controller events
            _controller.OnLevelSelectOpen += HandleLevelSelectOpen;
            _controller.OnLevelSelectClose += HandleLevelSelectClose;
        }
        private protected override void OnOnDestroy()
        {
            //Unsubscribe from controller events
            _controller.OnLevelSelectOpen -= HandleLevelSelectOpen;
            _controller.OnLevelSelectClose -= HandleLevelSelectClose;
        }

        private void ShowPanel(LevelSelectPanel panel)
        {
            if (panel == LevelSelectPanel.Preview)
            {
                _previewPanelObject.SetActive(true);
                _detailsPanelObject.SetActive(false);
            }
            else if (panel == LevelSelectPanel.Details)
            {
                _previewPanelObject.SetActive(false);
                _detailsPanelObject.SetActive(true);
            }
        }

        private void HandleLevelSelectOpen(LevelDataSO levelData)
        {
            InputManager.Instance.EnableUIActions();

            ShowPanel(LevelSelectPanel.Preview);
            // Set preview panel data
            _previewPanelTitleText.text = levelData.LevelTitle;
            _previewPanelImage.sprite = levelData.PreviewImage;
            // Set details panel data
            _detailsPanelTitleText.text = levelData.LevelTitle;
            _detailsPanelImage.sprite = levelData.PreviewImage;
            _detailsPanelBestText.text = $"Best Time: WIP";
            _detailsPanelPinsText.text = $"Pins: WIP";
        }

        private void HandleLevelSelectClose()
        {
            //Allow player controls again
            InputManager.Instance.EnablePlayerActions();
            _previewPanelObject.SetActive(false);
            _detailsPanelObject.SetActive(false);
        }

        /// <summary>
        /// Called by the preview panel's back button.
        /// </summary>
        public void CloseLevelSelect()
        {
            _controller.CloseLevelSelect();
        }

        public void StartLevel()
        {
            _controller.StartLevel();
        }

        public void OnPreviewPlayButtonPressed()
        {
            ShowPanel(LevelSelectPanel.Details);
        }

        public void OnDetailsBackButtonPressed()
        {
            ShowPanel(LevelSelectPanel.Preview);
        }
    }
}
