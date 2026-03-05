using System;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PlayerSpeedFovChanger : MonoBehaviour
    {
        [SerializeField, Required] private GameplayPlayerController _playerController;
        [SerializeField] private FloatRange _fovRange = new FloatRange(75, 100);
        [SerializeField] private Ease _lerpCurveEase = Ease.InCubic;
        [SerializeField] private float _lerpSpeed = 15f;

        private void Update()
        {
            float playerSpeed = _playerController.Rb.linearVelocity.magnitude;
            float parameter = DOVirtual.EasedValue(0f, 1f, playerSpeed / _playerController.GroundSuperState.MoveState.MaxSpeed, _lerpCurveEase);
            float targetFov = _fovRange.Lerp(parameter);
            
            _playerController.PlayerCamera.Lens.FieldOfView = Mathf.Lerp(_playerController.PlayerCamera.Lens.FieldOfView, targetFov, _lerpSpeed * Time.deltaTime);
        }
    }
}