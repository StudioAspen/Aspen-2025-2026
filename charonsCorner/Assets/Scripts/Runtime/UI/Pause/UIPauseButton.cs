using Febucci.TextAnimatorForUnity.TextMeshPro;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Rive;
using Rive.Components;
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

        [Header("Rive Integration")]
        [SerializeField] private RiveWidget _riveWidget;
        [SerializeField] private RiveCanvasRenderer _riveCanvasRenderer;
        [SerializeField] private UnityEngine.Color _baseColor = UnityEngine.Color.white;
        [SerializeField] private UnityEngine.Color _hoverColor = UnityEngine.Color.white;

        private SMIBool _isHovering;
        private Material _matInstance;

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

            InitializeRive();
        }

        private void InitializeRive()
        {
            if (_riveWidget == null) return;

            // Setup material instance for tinting if renderer is available
            if (_riveCanvasRenderer != null)
            {
                Material sourceMat = _riveCanvasRenderer.CustomMaterial;
                if (sourceMat == null)
                {
                    // Fallback if no custom material is set
                    if (_riveCanvasRenderer.TryGetComponent<Graphic>(out var graphic))
                    {
                        sourceMat = graphic.material;
                    }
                }

                if (sourceMat != null)
                {
                    _matInstance = new Material(sourceMat);
                    _riveCanvasRenderer.CustomMaterial = _matInstance;
                    SetTint(_baseColor);
                }
            }
        }

        private void Update()
        {
            if (_riveWidget != null && _isHovering == null)
            {
                if (_riveWidget.StateMachine != null)
                {
                    _isHovering = _riveWidget.StateMachine.GetBool("isHovering");
                }
            }

            // Manually tick Rive with unscaled delta time if game is paused
            if (_riveWidget != null && Time.timeScale == 0)
            {
                _riveWidget.Tick(Time.unscaledDeltaTime);
            }
        }

        private void SetTint(UnityEngine.Color color)
        {
            if (_matInstance != null)
            {
                _matInstance.SetColor("_Color", color);
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

            // Rive Integration
            if (_isHovering != null)
            {
                _isHovering.Value = active;
            }

            if (_riveCanvasRenderer != null)
            {
                SetTint(active ? _hoverColor : _baseColor);
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