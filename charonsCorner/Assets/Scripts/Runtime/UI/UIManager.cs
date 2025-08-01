using AYellowpaper.SerializedCollections;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CharonsCorner.Runtime.InputManager;

namespace CharonsCorner.Runtime
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [field: SerializeField, ReadOnly] public GameObject DesiredSelectedObject { get; private set; }

        public enum PanelName
        {
            PauseMenu,
            Settings,
            Dialogue,
            Confirm, // When the player wants to confirm an action, like starting a level
        }

        [Header("Panels")]
        [SerializeField, SerializedDictionary("Panel Name", "Panel")]
        private SerializedDictionary<PanelName, UIPanel> panels = new SerializedDictionary<PanelName, UIPanel>();
        [field: SerializeField, ReadOnly] public UIPanel CurrentPanel { get; private set; }

        [field: Header("Loading")]
        [field: SerializeField] public LoadingPanel LoadingPanel { get; private set; }

        /// <summary>
        /// Action that is invoked when the current panel is changed.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description><c>UIPanel newPanel</c>: The new panel that was switched to.</description></item>
        /// </list>
        /// </remarks>
        public event Action<UIPanel> OnPanelChanged = delegate { };
        /// <summary>
        /// Action that is invoked when UI under the UIManager is closed.
        /// </summary>
        public event Action OnUIClose = delegate { };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            InitializePanels();
        }

        private void Start()
        {
            InputManager.Instance.OnControlSchemeChanged += InputManager_OnControlSchemeChanged;
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.OnControlSchemeChanged -= InputManager_OnControlSchemeChanged;
        }

        private void InitializePanels()
        {
            foreach (UIPanel panel in panels.Values)
                panel.Init(this);
        }

        /// <summary>
        /// Shows the specified panel, hiding the current one if it exists.
        /// </summary>
        /// <param name="panelName"></param>
        public void ShowPanel(PanelName panelName)
        {
            UIPanel panel = panels[panelName];

            if (CurrentPanel == panel)
                return;

            if (CurrentPanel != null && CurrentPanel != panel)
                CurrentPanel.Hide();

            CurrentPanel = panel;
            CurrentPanel.Show();
            ChangeCurrentSelectedObject(CurrentPanel.DefaultSelectedObject);

            OnPanelChanged.Invoke(CurrentPanel);

            InputManager.Instance.EnableUIActions();
        }

        /// <summary>
        /// Essentially closes the UI by hiding all panels and resetting the current panel.
        /// </summary>
        public void HideAllPanels()
        {
            foreach (UIPanel panel in panels.Values)
                panel.Hide();

            CurrentPanel = null;

            OnUIClose.Invoke();
        }

        /// <summary>
        /// Changes the currently selected object in the UI.
        /// KeyboardMouse control scheme will always set the selected object to null.
        /// </summary>
        /// <param name="selectedObject"></param>
        public void ChangeCurrentSelectedObject(GameObject selectedObject)
        {
            DesiredSelectedObject = selectedObject;
            SetCurrentSelectedObject();
        }

        /// <summary>
        /// Helper method to set the currently selected object in the EventSystem based on the current control scheme.
        /// </summary>
        private void SetCurrentSelectedObject()
        {
            if (InputManager.Instance.CurrentControlScheme == ControlScheme.KeyboardMouse)
                EventSystem.current.SetSelectedGameObject(null);
            else
                EventSystem.current.SetSelectedGameObject(DesiredSelectedObject);
        }

        private void InputManager_OnControlSchemeChanged(ControlScheme newScheme)
        {
            SetCurrentSelectedObject();
        }

        /// <summary>
        /// Helper method to change the contents of the confirm panel.
        /// </summary>
        public void ChangeConfirmPanelContents(string warningContent, string yesContent, UnityEngine.Events.UnityAction yesButtonAction)
        {
            GameplayConfirmPanel confirmPanel = panels[PanelName.Confirm] as GameplayConfirmPanel;
            if(confirmPanel == null)
            {
                Debug.LogError("Confirm panel is not of type GameplayConfirmPanel or is not set up correctly.");
                return;
            }
            confirmPanel.SetupContents(warningContent, yesContent, yesButtonAction);
        }

        /// <summary>
        /// Utility method to check if a UI object is interactable by raycasting against it to see if any other UI elements block it.
        /// Needed for gamepad interactions.
        /// </summary>
        /// <param name="eventSystem"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsUIObjectInteractable(EventSystem eventSystem, GameObject target)
        {
            if (target == null || !target.activeInHierarchy)
                return false;

            RectTransform rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null)
                return false;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
            var pointerData = new PointerEventData(eventSystem) { position = screenPoint };

            List<RaycastResult> results = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (result.gameObject == target || result.gameObject.transform.IsChildOf(target.transform))
                    return true;

                // Hit something else first
                return false;
            }

            return false;
        }
    }
}
