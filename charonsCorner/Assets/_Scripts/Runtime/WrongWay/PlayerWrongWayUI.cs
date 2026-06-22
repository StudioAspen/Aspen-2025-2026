using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PlayerWrongWayUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Required] private PlayerWrongWayDetector _wrongWayDetector;
        [SerializeField, Required] private Transform _visiblePositionTransform;
        [SerializeField, Required] private Transform _notVisiblePositionTransform;
        
        [Header("UI Elements")]
        [SerializeField, Required] private RectTransform _indicatorTransform;
        
        [Header("Animation Settings")]
        [SerializeField] private float _animDuration = 0.5f;
        [SerializeField] private Ease _animEaseType = Ease.OutCubic;

        [Header("Wrong Way Sensitivity")]
        [SerializeField, Tooltip("Player must be going the wrong way for this many seconds before the UI appears.")]
        private float _wrongWayDelay = 1.5f;

        private Tweener _animTween;
        private float _wrongWayTimer;
        private bool _uiIsShowing;

        private void OnEnable()
        {
            _wrongWayDetector.OnWrongWayChanged.AddListener(OnWrongWayChanged);
        }

        private void OnDisable()
        {
            _wrongWayDetector.OnWrongWayChanged.RemoveListener(OnWrongWayChanged);
        }

        private void Update()
        {
            if (_wrongWayTimer <= 0f) return;

            _wrongWayTimer -= Time.deltaTime;

            if (_wrongWayTimer <= 0f)
            {
                _wrongWayTimer = 0f;
                ShowIndicator();
            }
        }

        private void OnWrongWayChanged(bool isWrongWay)
        {
            if (isWrongWay)
            {
                _wrongWayTimer = _wrongWayDelay;
            }
            else
            {
                // Cancel any pending show and immediately hide
                _wrongWayTimer = 0f;
                HideIndicator();
            }
        }

        private void ShowIndicator()
        {
            if (_uiIsShowing) return;
            _uiIsShowing = true;

            _animTween?.Kill();
            _animTween = _indicatorTransform
                .DOLocalMove(_visiblePositionTransform.localPosition, _animDuration)
                .SetEase(_animEaseType);
        }

        private void HideIndicator()
        {
            if (!_uiIsShowing) return;
            _uiIsShowing = false;

            _animTween?.Kill();
            _animTween = _indicatorTransform
                .DOLocalMove(_notVisiblePositionTransform.localPosition, _animDuration)
                .SetEase(_animEaseType);
        }
    }
}