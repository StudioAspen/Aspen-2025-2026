using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace CharonsCorner.Runtime
{
    public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Scaling Settings")]
        private float scaleFactor = 1.1f;
        private Vector2 originalScale;
        private RectTransform rectTransform;

        [Header("Color Settings")]
        private Color originalColor = Color.white;
        private Color hoverColor = Color.yellow;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            rectTransform.localScale = originalScale * scaleFactor;
            
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            rectTransform.localScale = originalScale;
        }
    }
}
