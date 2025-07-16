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
        [SerializeField] private GameObject generalSettingsUI;
        [SerializeField] private GameObject graphicsSettingsUI;
        [SerializeField] private GameObject audioSettingsUI;
        [SerializeField] private GameObject controlsSettingsUI;

        private protected override void Initialize()
        {
            
        }

        private void OnEnable()
        {
            ChangeSettingsUI(generalSettingsUI);
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

        public void ChangeToGeneralSettings() => ChangeSettingsUI(generalSettingsUI);
        public void ChangeToGraphicsSettings() => ChangeSettingsUI(graphicsSettingsUI);
        public void ChangeToAudioSettings() => ChangeSettingsUI(audioSettingsUI);
        public void ChangeToControlsSettings() => ChangeSettingsUI(controlsSettingsUI);

        public override void CloseUI()
        {
            if (GameManager.Instance.CurrentGameState == GameState.Title)
                uiManager.HideAllPanels();
            else
                uiManager.ShowPanel(UIManager.PanelName.PauseMenu);
        }
    }
}
