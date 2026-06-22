using System.Collections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SkyboxRotator : MonoBehaviour
    {
        public enum RotationAxis { X, Y, Z }

        [SerializeField] private RotationAxis _axis = RotationAxis.Y;
        [SerializeField] private float _rotationDuration = 0.5f;
        [SerializeField] private float _rotationAmount = 30f;
        [SerializeField] private AnimationCurve _rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Vector3 _currentRotation = Vector3.zero;
        private Coroutine _rotationCoroutine;
        private static readonly int RotationProperty = Shader.PropertyToID("_Rotation");

        public void Rotate(float direction)
        {
            if (_rotationCoroutine != null)
            {
                StopCoroutine(_rotationCoroutine);
            }

            Vector3 axisVector = Vector3.zero;
            switch (_axis)
            {
                case RotationAxis.X: axisVector = Vector3.right; break;
                case RotationAxis.Y: axisVector = Vector3.up; break;
                case RotationAxis.Z: axisVector = Vector3.forward; break;
            }

            Vector3 targetRotation = _currentRotation + (axisVector * direction * _rotationAmount);
            _rotationCoroutine = StartCoroutine(RotateRoutine(_currentRotation, targetRotation));
        }

        private IEnumerator RotateRoutine(Vector3 startRotation, Vector3 endRotation)
        {
            float elapsed = 0f;
            while (elapsed < _rotationDuration)
            {
                elapsed += Time.deltaTime;
                float t = _rotationCurve.Evaluate(elapsed / _rotationDuration);
                _currentRotation = Vector3.Lerp(startRotation, endRotation, t);
                UpdateSkyboxRotation();
                yield return null;
            }

            _currentRotation = endRotation;
            UpdateSkyboxRotation();
            _rotationCoroutine = null;
        }

        private void UpdateSkyboxRotation()
        {
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetVector(RotationProperty, new Vector4(_currentRotation.x, _currentRotation.y, _currentRotation.z, 0));
            }
        }

        private void OnDestroy()
        {
            // Optional: Reset skybox rotation when destroyed, though it might be shared
            // RenderSettings.skybox.SetFloat(RotationProperty, 0f);
        }
    }
}
