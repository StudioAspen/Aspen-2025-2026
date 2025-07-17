using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SettingsUI : UIPanel
    {
        [field: SerializeField, ReadOnly] public SubSettingsUI CurrentSettingsUI { get; private set; }
        public event Action<SubSettingsUI> OnSettingsUIChanged = delegate { };

        [Header("UI Elements")]
        [SerializeField] private SubSettingsUI generalSubSettingsUI;
        [SerializeField] private SubSettingsUI graphicsSubSettingsUI;
        [SerializeField] private SubSettingsUI audioSubSettingsUI;
        [SerializeField] private SubSettingsUI controlsSubSettingsUI;

        private protected override void Initialize()
        {
            // Initialize the sub-settings UIs
            List<SubSettingsUI> subSettingsUIs = new List<SubSettingsUI>
            {
                generalSubSettingsUI,
                graphicsSubSettingsUI,
                audioSubSettingsUI,
                controlsSubSettingsUI
            };
            foreach(SubSettingsUI subSettingsUI in subSettingsUIs)
                subSettingsUI.Initialize();
        }

        private void OnEnable()
        {
            ChangeSettingsUI(generalSubSettingsUI);
        }

        private void OnDisable()
        {
            
        }

        private void ChangeSettingsUI(SubSettingsUI subSettingsUI)
        {
            if(CurrentSettingsUI == subSettingsUI)
                return;

            if (CurrentSettingsUI != null)
                CurrentSettingsUI.Hide();

            CurrentSettingsUI = subSettingsUI;
            CurrentSettingsUI.Show();

            OnSettingsUIChanged.Invoke(subSettingsUI);
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
