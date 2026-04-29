using Febucci.TextAnimatorForUnity.TextMeshPro;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class UIPauseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextAnimator_TMP _textAnimator;
        [SerializeField] private string _startTags;
        [Header("Feel")]
        [SerializeField] private MMSpringScale _springScale;
        [SerializeField] private Vector3 _hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
        [SerializeField] private Vector3 _normalScale = Vector3.one;
        [SerializeField] private MMRotationShaker _rotationShaker;

        private bool _isHovered;
        private bool _isSelected;
        private bool _wasActive;

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_textAnimator == null)
                _textAnimator = GetComponentInChildren<TextAnimator_TMP>();

            if (_rotationShaker != null)
            {
                _rotationShaker.TimescaleMode = TimescaleModes.Unscaled;
            }
        }

        private void Start()
        {
            if (_textAnimator != null && !string.IsNullOrEmpty(_startTags))
            {
                _textAnimator.TMProComponent.text = _startTags + _textAnimator.TMProComponent.text;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            UpdateVisualState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            UpdateVisualState();
        }

        public void OnSelect(BaseEventData eventData)
        {
            _isSelected = true;
            UpdateVisualState();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            bool active = _isHovered || _isSelected;

            if (active && !_wasActive)
            {
                MMGameEvent.Trigger("PauseButtonHover");
            }
            _wasActive = active;

            if (_textAnimator != null)
            {
                _textAnimator.SetBehaviorsActive(active);
            }

            if (_springScale != null)
            {
                _springScale.MoveTo(active ? _hoverScale : _normalScale);
            }

            if (_rotationShaker != null)
            {
                if (active)
                {
                    _rotationShaker.Play();
                }
                else
                {
                    _rotationShaker.Stop();
                }
            }
        }

        public void UpdateText(string newText)
        {
            if (_textAnimator != null)
            {
                _textAnimator.TMProComponent.text = newText;
            }
        }
    }
}