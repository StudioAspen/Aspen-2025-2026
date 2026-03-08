using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

/// <summary>
/// Makes the camera zoom out by applying a simple camera position offset. 
/// This implementation is intended in order to not conflict with the existing camera shake implementation, which also modifies the camera's position.
/// 
/// Script is to be put on the same GameObject as the CinemachineOrbitalFollow component. 
/// It will lerp the camera's radius to create a zoom out effect, and then lerp it back to the original radius when the effect is removed.
/// </summary>
namespace CharonsCorner.Runtime
{
    public class CameraRadiusZoomOut : MonoBehaviour
    {
        private float _originalFOV;

        [SerializeField] private float _zoomOutFOVRadius = 30f;
        private float _currentZoomOutFOVRadius;

        private CinemachineOrbitalFollow _orbitalFollow;

        private Coroutine _currentLerpRoutine;

        private void Start()
        {
            _orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
            _originalFOV = _orbitalFollow.Radius;
            _currentZoomOutFOVRadius = _originalFOV + _zoomOutFOVRadius;
        }

        [ContextMenu("ApplyOffset")]
        public void ApplyOffset()
        {
            if (_currentLerpRoutine != null)
                StopCoroutine(_currentLerpRoutine);

            _currentLerpRoutine = StartCoroutine(LerpRadius(_currentZoomOutFOVRadius));
            _currentZoomOutFOVRadius += _zoomOutFOVRadius;
        }

        [ContextMenu("RemoveOffset")]
        public void RemoveOffset()
        {
            if (_currentLerpRoutine != null)
                StopCoroutine(_currentLerpRoutine);

            _currentLerpRoutine = StartCoroutine(LerpRadius(_originalFOV));
            _currentZoomOutFOVRadius = _originalFOV + _zoomOutFOVRadius;
        }

        private IEnumerator LerpRadius(float target)
        {
            float start = _orbitalFollow.Radius;
            float time = 0;
            float duration = 0.5f;

            while (time < duration)
            {
                time += Time.deltaTime;
                _orbitalFollow.Radius = Mathf.Lerp(start, target, time / duration);
                yield return null;
            }

            _orbitalFollow.Radius = target;
        }
    }
}

