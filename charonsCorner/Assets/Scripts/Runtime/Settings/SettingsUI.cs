using NaughtyAttributes;
using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SettingsUI : UIPanel
    {
        [field: SerializeField, ReadOnly] public GameObject CurrentSettingsUI { get; private set; }
        public event Action<GameObject> OnSettingsUIChanged = delegate { };

        [Header("UI Elements")]
        [SerializeField] private GameObject generalSubSettingsUI;
        [SerializeField] private GameObject graphicsSubSettingsUI;
        [SerializeField] private GameObject audioSubSettingsUI;
        [SerializeField] private GameObject controlsSubSettingsUI;

        private protected override void Initialize()
        {
            foreach(SettingAutoSaver settingAutoSaver in GetComponentsInChildren<SettingAutoSaver>(true))
            {
                if (settingAutoSaver == null)
                    continue;
                settingAutoSaver.Initialize();
            }
        }

        private void OnEnable()
        {
            ChangeSettingsUI(generalSubSettingsUI);
        }

        private void OnDisable()
        {
            
        }

        private void ChangeSettingsUI(GameObject settingsUI)
        {
            if(CurrentSettingsUI == settingsUI)
                return;

            if(CurrentSettingsUI != null)
                CurrentSettingsUI.SetActive(false);

            CurrentSettingsUI = settingsUI;
            CurrentSettingsUI.SetActive(true);

            OnSettingsUIChanged.Invoke(settingsUI);
        }

        public void ChangeToGeneralSettings() => ChangeSettingsUI(generalSubSettingsUI);
        public void ChangeToGraphicsSettings() => ChangeSettingsUI(graphicsSubSettingsUI);
        public void ChangeToAudioSettings() => ChangeSettingsUI(audioSubSettingsUI);
        public void ChangeToControlsSettings() => ChangeSettingsUI(controlsSubSettingsUI);

        public override void CloseUI()
        {
            if (GameManager.Instance.CurrentGameState == GameState.Title)
                uiManager.HideAllPanels();
            else
                uiManager.ShowPanel(UIManager.PanelName.PauseMenu);
        }
    }
}
