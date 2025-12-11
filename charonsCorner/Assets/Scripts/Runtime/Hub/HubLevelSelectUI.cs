using System;
using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class HubLevelSelectUI : SceneUI
    {
        [SerializeField] private HubLevelSelectController _controller;
        [SerializeField] private GameObject _levelSelect1;
        [SerializeField] private GameObject _levelSelect2;
        [SerializeField] private TMP_Text _levelNameText;
    

        private protected override void OnAwake()
        {
            _controller.OnLevelSelectOpen += HandleLevelSelectOpen;
            _controller.OnLevelSelectClose += HandleLevelSelectClose;
        }
        private protected override void OnOnDestroy()
        {
            _controller.OnLevelSelectOpen -= HandleLevelSelectOpen;
            _controller.OnLevelSelectClose -= HandleLevelSelectClose;
        }


        private void HandleLevelSelectOpen(LevelDataSO levelData)
        {
            _levelNameText.text = levelData.LevelTitle;
            Debug.Log("Level Select Opened for level: " + levelData.LevelTitle);
            InputManager.Instance.EnableUIActions();
            _levelSelect1.SetActive(true);
            _levelSelect2.SetActive(false);
        }
        private void HandleLevelSelectClose()
        {
            InputManager.Instance.EnablePlayerActions();
            _levelSelect1.SetActive(false);
            _levelSelect2.SetActive(false);
        }
        public void CloseLevelSelect()
        {
            _controller.CloseLevelSelect();
        }
    }
}
