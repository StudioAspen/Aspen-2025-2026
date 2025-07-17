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
        [SerializeField] private GameObject warningPanelObject;

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
            {
                subSettingsUI.Hide();
                subSettingsUI.Initialize();
            }
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

            if (CurrentSettingsUI != null && CurrentSettingsUI.IsDirty)
            {
                warningPanelObject.SetActive(true);
                return;
            }

            if(CurrentSettingsUI != null)
                CurrentSettingsUI.Hide();

            CurrentSettingsUI = subSettingsUI;
            CurrentSettingsUI.Show();

            OnSettingsUIChanged.Invoke(subSettingsUI);
        }

        public void ChangeToGeneralSettings() => ChangeSettingsUI(generalSubSettingsUI);
        public void ChangeToGraphicsSettings() => ChangeSettingsUI(graphicsSubSettingsUI);
        public void ChangeToAudioSettings() => ChangeSettingsUI(audioSubSettingsUI);
        public void ChangeToControlsSettings() => ChangeSettingsUI(controlsSubSettingsUI);

        public void ApplyCurrentSubSettings() => CurrentSettingsUI?.Apply();
        public void DiscardCurrentSubSettings() => CurrentSettingsUI?.Discard();

        public override void CloseUI()
        {
            if (CurrentSettingsUI != null && CurrentSettingsUI.IsDirty)
            {
                warningPanelObject.SetActive(true);
                return;
            }

            if (GameManager.Instance.CurrentGameState == GameState.Title)
                uiManager.HideAllPanels();
            else
                uiManager.ShowPanel(UIManager.PanelName.PauseMenu);
        }
    }
}
