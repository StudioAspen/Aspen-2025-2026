using UnityEngine;

namespace CharonsCorner.Runtime
{
    public abstract class UIPanel : MonoBehaviour
    {
        /// <summary>
        /// Replacement for Unity's Awake method due to the panels being disabled on start.
        /// </summary>
        public abstract void Initialize();

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
