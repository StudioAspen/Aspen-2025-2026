using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.Feedbacks;
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
        [SerializeField] private bool _useFadeTransition = false;
        [SerializeField] private MMF_Player _closeUIFeedback;
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

        private void OnEnable()
        {
            Debug.Log($"[UIPanel] {name} Enabled. ActivePanel: {(ActivePanel != null ? ActivePanel.name : "null")}");
        }

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
            Debug.Log($"[UIPanel] Static Focus called for {panel?.name}. ActivePanel: {(ActivePanel != null ? ActivePanel.name : "null")}");
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
        public static void CloseAll(bool includeLoading = false)
        {
            CloseAllAsync(includeLoading).Forget();
        }

        public static async UniTask CloseAllAsync(bool includeLoading = false)
        {
            // Debug.Log($"[UIPanel] CloseAll called. ActivePanel: {(ActivePanel != null ? ActivePanel.name : "null")}");
            if (ActivePanel == null)
            {
                OnPanelChanged?.Invoke(null);
                return;
            }
            
            // If we are starting with Loading and not including it, we might have panels behind it.
            // We clear those out to ensure a clean state for the next scene.
            if (!includeLoading && ActivePanel.name.Contains("Loading") && ActivePanel.PreviousPanel != null)
            {
                UIPanel current = ActivePanel.PreviousPanel;
                ActivePanel.SetPreviousPanel(null);
                while (current != null)
                {
                    UIPanel next = current.PreviousPanel;
                    await current.Unfocus();
                    current = next;
                }
            }

            int safety = 0;
            while(ActivePanel && safety < 100)
            {
                if (!includeLoading && ActivePanel.name.Contains("Loading"))
                {
                    // If the loading panel is the only one left or we shouldn't close it, we stop.
                    // But we should also make sure it's not masking other panels if we want a clean state.
                    // However, in our system, Loading is usually focused on top.
                    break;
                }

                UIPanel toClose = ActivePanel;
                await ActivePanel.BackOrCloseAsync();
                
                // If BackOrClose didn't change ActivePanel (e.g. it was already null or didn't move), 
                // and it was the loading panel we were skipping, we'd be stuck.
                // But the name check above handles it.
                
                if (ActivePanel == toClose)
                {
                    // Force break if it didn't change to avoid infinite loop
                    break;
                }
                
                safety++;
            }
        }
        
        private protected virtual void OnDestroy()
        {
            // Debug.Log($"[UIPanel] {name} OnDestroy. ActivePanel is {(ActivePanel != null ? ActivePanel.name : "null")}");
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
                
                // If the scene is changing, don't try to go back, just clear the reference
                if (!gameObject.scene.isLoaded)
                {
                    // Debug.Log($"[UIPanel] {name} OnDestroy while scene is NOT loaded. Clearing ActivePanel.");
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
            if(!panel.IsAdditive)
                Unfocus().Forget();
            else
                Group.interactable = false;

            panel.Focus();
        }

        public void Focus()
        {
            // Debug.Log($"[UIPanel] {name} Focus called. interactable before: {Group.interactable}, blocksRaycasts before: {Group.blocksRaycasts}");
            DOTween.Kill(Group);
            Group.interactable = true;
            Group.blocksRaycasts = true; // Ensure raycasts are enabled when focused
            gameObject.SetActive(true);
            ActivePanel = this;
            
            if (_useFadeTransition)
            {
                Group.alpha = 0f;
                Group.DOFade(1f, _fadeDuration);
                
            }
            else
            {
                Group.alpha = 1f; // Ensure alpha is 1 if not using fade
            }

            ChangeCurrentSelectedObject(DefaultSelected);

            OnFocused?.Invoke();
            OnPanelChanged?.Invoke(this);
        }

        public async UniTask Unfocus()
        {
            // Debug.Log($"[UIPanel] {name} Unfocus called. interactable before: {Group.interactable}");
            DOTween.Kill(Group);
            Group.interactable = false;
            
            if (ActivePanel == this)
            {
                ActivePanel = null;
            }

            if (_closeUIFeedback != null)
            {
                _closeUIFeedback.PlayFeedbacks();
                await UniTask.WaitUntil(() => !_closeUIFeedback.IsPlaying);
            }

            if (_useFadeTransition)
            {
                Group.blocksRaycasts = false;

                await Group.DOFade(0f, _fadeDuration).AsyncWaitForCompletion();
                
                Group.blocksRaycasts = true;
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }

            OnUnfocused?.Invoke();
        }

        /// <summary>
        /// Closes the current panel, but does not enable the previous screen.
        /// Useful for additive panels or other special cases.
        /// </summary>
        public void Close()
        {
            CloseAsync().Forget();
        }

        public async UniTask CloseAsync()
        {
            await Unfocus();
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
            BackAsync().Forget();
        }

        public async UniTask BackAsync()
        {
            // Debug.Log($"[UIPanel] {name} Back called. PreviousPanel: {(PreviousPanel != null ? PreviousPanel.name : "null")}");
            if (PreviousPanel)
            {
                UIPanel target = PreviousPanel;
                PreviousPanel = null;
                await Unfocus();
                target.Focus();
            }
        }

        /// <summary>
        /// Safe exit method for the current panel.
        /// </summary>
        public void BackOrClose()
        {
            BackOrCloseAsync().Forget();
        }

        public async UniTask BackOrCloseAsync()
        {
            // Debug.Log($"[UIPanel] {name} BackOrClose called. PreviousPanel: {(PreviousPanel != null ? PreviousPanel.name : "null")}");
            if (PreviousPanel)
            {
                await BackAsync();
            }
            else
            {
                await Unfocus();
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
        
        public void SetPreviousPanel(UIPanel previous)
        {
            Debug.Log($"[UIPanel] {name} SetPreviousPanel to {(previous != null ? previous.name : "null")}");
            PreviousPanel = previous;
        }
        
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
            if (EventSystem.current == null)
            {
                Debug.LogWarning("[UIPanel] SetCurrentSelectedObject: EventSystem.current is null.");
                return;
            }

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
