using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// UI screen navigation manager. State machine + stack.
    /// Use DefaultSceneUIPanelSetter component.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : MonoBehaviour
    {
        /// <summary>
        /// Whether this panel will block inputs on the previous screen. Doesn't replace the previous panel.
        /// Think of popup UI.
        /// </summary>
        [field: SerializeField] public bool IsAdditive { get; private set; } = false;

        [SerializeField] private float _fadeDuration = 1f;
        [ShowInInspector, ReadOnly] public static Selectable TargetSelectedObject { get; private set; }
        /// <summary>
        /// The first object to select when opening this panel.
        /// For controller navigation.
        /// </summary>
        [field: SerializeField] public Selectable DefaultSelected { get; private set; }

        [Header("Events")]
        public UnityEvent OnFocused = new();
        public UnityEvent OnUnfocused = new();

        private CanvasGroup _group = null;
        public CanvasGroup Group
        {
            get
            {
                if (_group == null)
                {
                    _group = GetComponent<CanvasGroup>();
                }

                return _group;
            }
        }
        
        [ShowInInspector] private UIPanel _currentActivePanel => ActivePanel;
        [field: SerializeField, ReadOnly] public UIPanel PreviousPanel = null;
        /// <summary>
        /// Globally accessible reference to the current active panel.
        /// </summary>
        public static UIPanel ActivePanel { get; private set; }
        public static event Action<UIPanel> OnPanelChanged = delegate { };

        /// <summary>
        /// Helper to hide the active panel.
        /// Good for temporarily hiding and showing again later.
        /// </summary>
        public static void HideActive()
        {
            ActivePanel.gameObject.SetActive(false);
        }

        /// <summary>
        /// Helper to show the active panel.
        /// Good for temporarily hiding and showing again later.
        /// </summary>
        public static void ShowActive()
        {
            ActivePanel.gameObject.SetActive(true);
        }

        public static void Focus(UIPanel panel)
        {
            if (ActivePanel != null)
                ActivePanel.FocusPanel(panel);
            else
                panel.Focus();
        }

        /// <summary>
        /// Repeatedly goes back until there is no previous panel.
        /// Returns back to the "root" UI.
        /// </summary>
        public static void GoBackToInitial()
        {
            while(ActivePanel?.PreviousPanel)
                ActivePanel.Back();
        }

        /// <summary>
        /// Closes everything and clears the stack.
        /// </summary>
        public static void CloseAll()
        {
            if (ActivePanel == null)
            {
                OnPanelChanged?.Invoke(null);
                return;
            }
            
            while(ActivePanel)
                ActivePanel.BackOrClose();
        }
        
        private protected virtual void OnDestroy()
        {
            // Safely cleans up the ActivePanel static variable
            if (ActivePanel == this)
            {
    #if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode == false) 
                    return;
    #endif
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isLoaded == false)
                {
                    ActivePanel = null;
                    OnPanelChanged?.Invoke(null);
                    return;
                }
                Debug.LogWarning($"Active UIScreen {this} is being destroyed");
                Back();
            }
        }

        private void OnApplicationQuit()
        {
            OnPanelChanged = delegate { };
        }

        public void FocusPanel(UIPanel panel)
        {
            if (panel == this)
            {
                Focus();
                return;
            }
            
            panel.SetPreviousPanel(this);
            panel.Focus();
            if(!panel.IsAdditive)
                Unfocus();
            else
                Group.interactable = false;
        }

        public void Focus()
        {
            DOTween.Kill(Group);
            Group.alpha = 0f;
            Group.interactable = true;
            gameObject.SetActive(true);
            ActivePanel = this;
            
            Group.DOFade(1f, _fadeDuration);

            ChangeCurrentSelectedObject(DefaultSelected);

            OnFocused?.Invoke();
            OnPanelChanged?.Invoke(this);
        }

        public void Unfocus()
        {
            DOTween.Kill(Group);
            Group.interactable = false;
            Group.blocksRaycasts = false;

            Group.DOFade(0f, _fadeDuration).OnComplete(() =>
            {
                Group.blocksRaycasts = true;
                gameObject.SetActive(false);
            });


            OnUnfocused?.Invoke();
        }

        /// <summary>
        /// Closes the current panel, but does not enable the previous screen.
        /// Useful for additive panels or other special cases.
        /// </summary>
        public void Close()
        {
            Unfocus();
            if (PreviousPanel)
            {
                ActivePanel = PreviousPanel;
                SetPreviousPanel(null);
            }
            else
            {
                ActivePanel = null;
            }
            OnPanelChanged?.Invoke(null);
        }

        /// <summary>
        /// Returns to the previous panel if it exists.
        /// </summary>
        public void Back()
        {
            if (PreviousPanel)
            {
                Unfocus();
                PreviousPanel.Focus();
                PreviousPanel = null;
            }
        }

        /// <summary>
        /// Safe exit method for the current panel.
        /// </summary>
        public void BackOrClose()
        {
            Unfocus();
            if (PreviousPanel)
            {
                Back();
            }
            else
            {
                ActivePanel = null;
                OnPanelChanged?.Invoke(null);
            }
        }

        /// <summary>
        /// Jumps directly to a panel, clearing navigation history.
        /// Useful for return to main menu type actions.
        /// </summary>
        public void GoBackTo(UIPanel panel)
        {
            FocusPanel(panel);
            SetPreviousPanel(null);
        }
        
        public void SetPreviousPanel(UIPanel previous) => PreviousPanel = previous;
        
        /// <summary>
        /// Changes the currently selected object in the UI.
        /// KeyboardMouse control scheme will always set the selected object to null.
        /// </summary>
        /// <param name="selectedObject"></param>
        public static void ChangeCurrentSelectedObject(Selectable selectedObject)
        {
            TargetSelectedObject = selectedObject;
            SetCurrentSelectedObject();
        }
        
        public static void ChangeCurrentSelectedObject(GameObject selectedGameObject)
        {
            TargetSelectedObject = selectedGameObject.GetComponent<Selectable>();
            SetCurrentSelectedObject();
        }

        /// <summary>
        /// Helper method to set the currently selected object in the EventSystem based on the current control scheme.
        /// </summary>
        public static void SetCurrentSelectedObject()
        {
            if (InputManager.Instance.CurrentControlScheme == InputManager.ControlScheme.KeyboardMouse)
                EventSystem.current.SetSelectedGameObject(null);
            else
            {
                if(TargetSelectedObject != null)
                    EventSystem.current.SetSelectedGameObject(TargetSelectedObject.gameObject);
                else
                    EventSystem.current.SetSelectedGameObject(null);
            }
        }
        
        /// <summary>
        /// Utility method to check if a UI object is interactable by raycasting against it to see if any other UI elements block it.
        /// Needed for gamepad interactions.
        /// </summary>
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
        
    #if UNITY_EDITOR
        // To show all UI buttons in the inspector
        [FoldoutGroup("Child Buttons")]
        [ShowInInspector]
        private Button[] ChildButtons => GetComponentsInChildren<Button>(true);

        [FoldoutGroup("Child Buttons")]
        [Button(ButtonSizes.Small)]
        private void SelectButton([ValueDropdown(nameof(ChildButtons))] Button button)
        {
            button.onClick.Invoke();
        }
    #endif
    }
}
