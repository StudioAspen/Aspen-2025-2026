using UnityEngine;

namespace CharonsCorner.Runtime
{
    public abstract class UIPanel : MonoBehaviour
    {
        private protected UIManager uiManager;

        [field: SerializeField] public GameObject DefaultSelectedObject { get; private set; }

        /// <summary>
        /// Since UIPanels are disabled on start, this method is used to initialize (fake Awake()) the panel with the UIManager reference.
        /// </summary>
        /// <param name="manager"></param>
        public void Init(UIManager manager)
        {
            uiManager = manager;
            Initialize();
        }

        /// <summary>
        /// Replacement for Unity's Awake method due to the panels being disabled on start.
        /// </summary>
        private protected abstract void Initialize();

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Forces the panel to hide without any additional logic.
        /// </summary>
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// For when the player wants to close the UI panel.
        /// This is different from Hide() because it also handles any additional cleanup or state changes needed when closing the UI.
        /// </summary>
        public abstract void CloseUI();
    }
}
