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
        private Tweener _animTween;

        private void Awake()
        {
            if(_wrongWayDetector == null)
                Debug.LogError("PlayerWrongWayUI needs a reference to PlayerWrongWayDetector on the player controller.");
        }

        private void OnEnable()
        {
            _wrongWayDetector.OnWrongWayChanged.AddListener(OnWrongWayChanged);
        }

        private void OnDisable()
        {
            _wrongWayDetector.OnWrongWayChanged.RemoveListener(OnWrongWayChanged);
        }

        private void OnWrongWayChanged(bool isWrongWay)
        {
            if(_animTween != null)
                _animTween.Kill();

            if (isWrongWay)
            {
                _animTween = _indicatorTransform.DOLocalMove(_visiblePositionTransform.localPosition, _animDuration).SetEase(_animEaseType);
            }
            else
            {
                _animTween = _indicatorTransform.DOLocalMove(_notVisiblePositionTransform.localPosition, _animDuration).SetEase(_animEaseType);
            }
        }
    }
}