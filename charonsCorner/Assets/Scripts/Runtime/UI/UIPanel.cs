using UnityEngine;

namespace CharonsCorner.Runtime
{
    public abstract class UIPanel : MonoBehaviour
    {
        private protected UIManager uiManager;

        [field: SerializeField] public GameObject DefaultSelectedObject { get; private set; }

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

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public abstract void CloseUI();
    }
}
