using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SettingsUI : UIPanel
    {
        [field: SerializeField, ReadOnly] public SubSettingsUI CurrentSettingsUI { get; private set; }
        /// <summary>
        /// Action that is invoked when the current subsettings UI is changed.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description><c>SubSettingsUI newSubSettingsUI</c>: The new subSettings UI that was changed to.</description></item>
        /// </list>
        /// </remarks>
        public event Action<SubSettingsUI> OnSettingsUIChanged = delegate { };

        [Header("UI Elements")]
        [SerializeField] private SubSettingsUI _generalSubSettingsUI;
        [SerializeField] private SubSettingsUI _graphicsSubSettingsUI;
        [SerializeField] private SubSettingsUI _audioSubSettingsUI;
        [SerializeField] private SubSettingsUI _controlsSubSettingsUI;
        [SerializeField] private GameObject _warningPanelObject;

        private protected override void Initialize()
        {
            // Initialize the sub-settings UIs
            List<SubSettingsUI> subSettingsUIs = new List<SubSettingsUI>
            {
                _generalSubSettingsUI,
                _graphicsSubSettingsUI,
                _audioSubSettingsUI,
                _controlsSubSettingsUI
            };
            foreach(SubSettingsUI subSettingsUI in subSettingsUIs)
            {
                subSettingsUI.Hide();
                subSettingsUI.Initialize();
            }
        }

        private void OnEnable()
        {
            InputManager.Instance.Unpause += InputManager_Unpause;

            ChangeSettingsUI(_generalSubSettingsUI);
        }

        private void OnDisable()
        {
            if(InputManager.Instance != null)
                InputManager.Instance.Unpause -= InputManager_Unpause;
        }

        /// <summary>
        /// Changes the current settings UI to the specified sub-settings UI. Closes the previous one if it exists.
        /// </summary>
        /// <param name="subSettingsUI"></param>
        private void ChangeSettingsUI(SubSettingsUI subSettingsUI)
        {
            if(CurrentSettingsUI == subSettingsUI)
                return;

            if (CurrentSettingsUI != null && CurrentSettingsUI.IsDirty)
            {
                _warningPanelObject.SetActive(true);
                return;
            }

            if(CurrentSettingsUI != null)
                CurrentSettingsUI.Hide();

            CurrentSettingsUI = subSettingsUI;
            CurrentSettingsUI.Show();

            OnSettingsUIChanged.Invoke(subSettingsUI);
        }

        // Called by button UI events
        public void ChangeToGeneralSettings() => ChangeSettingsUI(_generalSubSettingsUI);
        public void ChangeToGraphicsSettings() => ChangeSettingsUI(_graphicsSubSettingsUI);
        public void ChangeToAudioSettings() => ChangeSettingsUI(_audioSubSettingsUI);
        public void ChangeToControlsSettings() => ChangeSettingsUI(_controlsSubSettingsUI);
        public void ApplyCurrentSubSettings() => CurrentSettingsUI.IfNotNullThenCall(subSettingsUI => subSettingsUI.Apply());
        public void DiscardCurrentSubSettings() => CurrentSettingsUI.IfNotNullThenCall(subSettingsUI => subSettingsUI.Discard());

        public override void CloseUI()
        {
            if (CurrentSettingsUI != null && CurrentSettingsUI.IsDirty)
            {
                _warningPanelObject.SetActive(true);
                return;
            }

            if (GameManager.Instance.CurrentGameState == GameState.Title)
                _uiManager.HideAllPanels();
            else
                _uiManager.ShowPanel(UIManager.PanelName.PauseMenu);
        }

        private void InputManager_Unpause() => CloseUI();
    }
}
