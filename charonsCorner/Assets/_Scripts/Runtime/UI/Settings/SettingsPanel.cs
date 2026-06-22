using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SettingsPanel : UIPanel
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
        [SerializeField] private SubSettingsUI _graphicsSubSettingsUI;
        [SerializeField] private SubSettingsUI _audioSubSettingsUI;
        [SerializeField] private SubSettingsUI _controlsSubSettingsUI;
        [SerializeField] private Button _graphicsButton;
        [SerializeField] private Button _audioButton;
        [SerializeField] private Button _controlsButton;
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _closeButton;

        [Header("Apply Button Feedback")]
        [SerializeField] private TMP_Text _applyButtonText;
        [SerializeField] private Color _dirtyColor = Color.yellow;
        [SerializeField] private Color _cleanColor = Color.white;

        [Header("Button Animations")]
        [SerializeField] private MoreMountains.Feedbacks.MMSpringScale _graphicsButtonScale;
        [SerializeField] private MoreMountains.Feedbacks.MMSpringScale _audioButtonScale;
        [SerializeField] private MoreMountains.Feedbacks.MMSpringScale _controlsButtonScale;
        [SerializeField] private Vector3 _selectedScale = new Vector3(1.1f, 1.1f, 1.1f);
        [SerializeField] private Vector3 _unselectedScale = Vector3.one;

        public void Initialize()
        {
            // Initialize the sub-settings UIs
            List<SubSettingsUI> subSettingsUIs = new List<SubSettingsUI>
            {
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
            ChangeSettingsUI(_controlsSubSettingsUI);
            InputManager.Instance.Unpause += InputManager_Unpause;
        }

        protected virtual void Update()
        {
            UpdateApplyButtonColor();
        }

        private void UpdateApplyButtonColor()
        {
            if (_applyButtonText == null) return;
            bool isDirty = CurrentSettingsUI != null && CurrentSettingsUI.IsDirty;
            _applyButtonText.color = isDirty ? _dirtyColor : _cleanColor;
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.Unpause -= InputManager_Unpause;
        }

        private void InputManager_Unpause()
        {
            CloseUI();
        }

        /// <summary>
        /// Changes the current settings UI to the specified sub-settings UI. Closes the previous one if it exists.
        /// </summary>
        /// <param name="subSettingsUI"></param>
        private void ChangeSettingsUI(SubSettingsUI subSettingsUI)
        {
            if(CurrentSettingsUI == subSettingsUI)
                return;

            if(CurrentSettingsUI != null)
                CurrentSettingsUI.Hide();

            CurrentSettingsUI = subSettingsUI;
            CurrentSettingsUI.Show();

            UpdateButtonsScale();
            UpdateTabNavigation();

            if (CurrentSettingsUI.FirstSelected != null && InputManager.Instance.CurrentControlScheme != InputManager.ControlScheme.KeyboardMouse)
            {
                ChangeCurrentSelectedObject(CurrentSettingsUI.FirstSelected);
            }

            OnSettingsUIChanged.Invoke(subSettingsUI);
        }

        private void UpdateButtonsScale()
        {
            if (_graphicsButtonScale != null)
            {
                Vector3 target = CurrentSettingsUI == _graphicsSubSettingsUI ? _selectedScale : _unselectedScale;
                _graphicsButtonScale.MoveTo(target);
            }
            
            if (_audioButtonScale != null)
            {
                Vector3 target = CurrentSettingsUI == _audioSubSettingsUI ? _selectedScale : _unselectedScale;
                _audioButtonScale.MoveTo(target);
            }
            
            if (_controlsButtonScale != null)
            {
                Vector3 target = CurrentSettingsUI == _controlsSubSettingsUI ? _selectedScale : _unselectedScale;
                _controlsButtonScale.MoveTo(target);
            }
        }

        private void UpdateTabNavigation()
        {
            if (CurrentSettingsUI == null || CurrentSettingsUI.FirstSelected == null) return;

            SetButtonDownNavigation(_graphicsButton, CurrentSettingsUI.FirstSelected);
            SetButtonDownNavigation(_audioButton, CurrentSettingsUI.FirstSelected);
            SetButtonDownNavigation(_controlsButton, CurrentSettingsUI.FirstSelected);

            if (CurrentSettingsUI.LastSelected != null)
            {
                SetButtonUpNavigation(_applyButton, CurrentSettingsUI.LastSelected);
                SetButtonUpNavigation(_closeButton, CurrentSettingsUI.LastSelected);
            }
        }

        private void SetButtonDownNavigation(Button button, Selectable downTarget)
        {
            if (button == null) return;
            Navigation nav = button.navigation;
            nav.selectOnDown = downTarget;
            button.navigation = nav;
        }

        private void SetButtonUpNavigation(Button button, Selectable upTarget)
        {
            if (button == null) return;
            Navigation nav = button.navigation;
            nav.selectOnUp = upTarget;
            button.navigation = nav;
        }

        // Called by button UI events
        public void ChangeToGraphicsSettings() => ChangeSettingsUI(_graphicsSubSettingsUI);
        public void ChangeToAudioSettings() => ChangeSettingsUI(_audioSubSettingsUI);
        public void ChangeToControlsSettings() => ChangeSettingsUI(_controlsSubSettingsUI);
        public void ApplyCurrentSubSettings() => CurrentSettingsUI.IfNotNullThenCall(subSettingsUI => subSettingsUI.Apply());
        public void DiscardCurrentSubSettings() => CurrentSettingsUI.IfNotNullThenCall(subSettingsUI => subSettingsUI.Discard());

        public void CloseUI()
        {
            BackOrClose();
        }

    }
}
