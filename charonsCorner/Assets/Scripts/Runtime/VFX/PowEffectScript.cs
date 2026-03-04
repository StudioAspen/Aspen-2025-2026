using System.Collections;
using UnityEngine;


namespace CharonsCorner.Runtime
{
    public class PowEffectScript : MonoBehaviour
    {
        [SerializeField] private float _growDuration = 0.2f;
        public float GrowTimer { get; private set; } = 0.5f;
        
        [SerializeField] private float _minScale = 1f;
        [SerializeField] private float _maxScale = 2f;
        [Tooltip("The maximum speed that will affect the pow effect size. Higher speeds will be clamped to this value.")]
        [SerializeField] private float _maxSpeed = 50f;
        
        private Coroutine _activateRoutine;
        
        private Camera _mainCamera;
        
        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void LateUpdate()
        {   
            // Make the pow effect always face the camera
            transform.forward = _mainCamera.transform.forward;
        }
        
        public void GrowPin(float speedScale, Vector3 position, Transform lookTarget)
        {
            transform.position = position;

            Vector3 direction = transform.position - lookTarget.position;
            transform.rotation = Quaternion.LookRotation(direction);

            float targetScaleValue = Mathf.Lerp(_minScale, _maxScale, speedScale);
            Vector3 targetScale = Vector3.one * targetScaleValue;

            StartCoroutine(GrowRoutine(targetScale));
        }

        private IEnumerator GrowRoutine(Vector3 targetScale)
        {
            transform.localScale = Vector3.one;

            float time = 0f;
            while (time < _growDuration)
            {
                time += Time.deltaTime;
                transform.localScale = Vector3.Lerp(Vector3.one, targetScale, (time / _growDuration));
                yield return null;
            }

            transform.localScale = targetScale;

            yield return new WaitForSeconds(GrowTimer - _growDuration);

            gameObject.SetActive(false);
        }
    }
}

