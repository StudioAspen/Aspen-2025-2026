using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Simplified enemy that lerps its local Z position between two values over time using an AnimationCurve.
    /// </summary>
    public class ShifterEnemy : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _startZ = 0f;
        [SerializeField] private float _endZ = 10f;
        [SerializeField] private float _moveDuration = 2f;
        [SerializeField] private AnimationCurve _movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private float _elapsedTime;
        private bool _movingToEnd = true;

        private void Update()
        {
            if (_moveDuration <= 0) return;

            _elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(_elapsedTime / _moveDuration);
            float curveValue = _movementCurve.Evaluate(normalizedTime);

            float currentZ = _movingToEnd 
                ? Mathf.Lerp(_startZ, _endZ, curveValue) 
                : Mathf.Lerp(_endZ, _startZ, curveValue);

            Vector3 pos = transform.localPosition;
            pos.z = currentZ;
            transform.localPosition = pos;

            if (normalizedTime >= 1f)
            {
                _elapsedTime = 0f;
                _movingToEnd = !_movingToEnd;
            }
        }

        [ContextMenu("Reset to Start Z")]
        private void ResetToStart()
        {
            Vector3 pos = transform.localPosition;
            pos.z = _startZ;
            transform.localPosition = pos;
            _elapsedTime = 0f;
            _movingToEnd = true;
        }
    }
}
