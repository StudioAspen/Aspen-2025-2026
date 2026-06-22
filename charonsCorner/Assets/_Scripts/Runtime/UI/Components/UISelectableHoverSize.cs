using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Selectable))]
    public class UISelectableHoverSize : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Selectable _selectable;

        [SerializeField] private float _scaleModifier = 0.1f;
        [SerializeField] private float _duration = 0.1f;
        [SerializeField] private Ease _easeType = Ease.OutCirc;
        private float _startScale;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
            _startScale = _selectable.transform.localScale.x;
        }

        private void OnEnable()
        {
            _selectable.DOKill();
            _selectable.transform.localScale = _startScale * Vector3.one;
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!_selectable.interactable)
                return;

            _selectable.DOKill();
            _selectable.transform.DOScale(_startScale * (1f +  _scaleModifier), _duration)
                .SetEase(_easeType)
                .SetUpdate(true);
        }
    
        public void OnDeselect(BaseEventData eventData)
        {
            _selectable.DOKill();
            _selectable.transform.DOScale(_startScale, _duration)            
                .SetEase(_easeType)
                .SetUpdate(true);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnSelect(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnDeselect(eventData);
        }
    }
}