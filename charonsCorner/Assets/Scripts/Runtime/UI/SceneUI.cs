using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Base class for scene specific UI that aren't persistent.
    /// Will handle controller support automatically as long as you inherit.
    /// </summary>
    public abstract class SceneUI : MonoBehaviour
    {
        [field: SerializeField] public GameObject DefaultSelectedObject { get; private set; }
        private GameObject _previousSelectedObject;
        
        private void Awake()
        {
            UIManager.Instance.OnPanelChanged += UIManager_OnPanelChanged;
            UIManager.Instance.OnUIClose += UIManager_OnUIClose;
            
            OnAwake();
        }

        /// <summary>
        /// The replacement for Awake() method to not override base class default behavior.
        /// </summary>
        private protected abstract void OnAwake();

        private void OnDestroy()
        {
            UIManager.Instance.OnPanelChanged -= UIManager_OnPanelChanged;
            UIManager.Instance.OnUIClose -= UIManager_OnUIClose;
            
            OnOnDestroy();
        }

        /// <summary>
        /// The replacement for OnDestroy() method to not override base class default behavior.
        /// </summary>
        private protected abstract void OnOnDestroy();

        private void UIManager_OnPanelChanged(UIPanel newPanel)
        {
            _previousSelectedObject = EventSystem.current.currentSelectedGameObject;
        }
        
        private void UIManager_OnUIClose()
        {
            Focus();
        }

        /// <summary>
        /// Selects this UI as the scene's focus, reselecting the primary UI elements
        /// </summary>
        public void Focus()
        {
            GameObject targetSelectedObject = _previousSelectedObject;
            if(targetSelectedObject == null)
                targetSelectedObject = DefaultSelectedObject;
            UIManager.Instance.ChangeCurrentSelectedObject(targetSelectedObject);
        }

        /// <summary>
        /// Easy method to enable this UI.
        /// Use OnEnable for additional logic.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Easy method to hide this UI.
        /// Use OnDisable for additional logic.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}