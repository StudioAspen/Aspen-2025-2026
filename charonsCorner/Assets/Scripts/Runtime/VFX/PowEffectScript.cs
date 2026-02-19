using System.Collections;
using UnityEngine;


namespace CharonsCorner.Runtime
{
    public class PowEffectScript : MonoBehaviour
    {

        [SerializeField]private float growDuration = 0.2f;
        public float growTimer = 0.5f;


        [SerializeField]private float minScale = 1f;
        [SerializeField]private float maxScale = 2f;
        [Tooltip("The maximum speed that will affect the pow effect size. Higher speeds will be clamped to this value.")]
        [SerializeField]private float maxSpeed = 50f;
        
        private Coroutine activateRoutine;


        private Camera _mainCamera;
        void Awake()
        {
            _mainCamera = Camera.main;
        }

        void LateUpdate()
        {   
            // Make the pow effect always face the camera
            transform.forward = _mainCamera.transform.forward;
        }

        
        public void growPin(float speedscale, Vector3 position, Transform lookTarget)
        {
            transform.position = position;

            Vector3 direction = transform.position - lookTarget.position;
            transform.rotation = Quaternion.LookRotation(direction);

            float targetScaleValue = Mathf.Lerp(minScale, maxScale, speedscale);
            Vector3 targetScale = Vector3.one * targetScaleValue;

            StartCoroutine(growRoutine(targetScale));
        }

        IEnumerator growRoutine(Vector3 targetScale)
        {
            transform.localScale = Vector3.one;

            float time = 0f;

            while (time < growDuration)
            {
                time += Time.deltaTime;
                transform.localScale = Vector3.Lerp(Vector3.one, targetScale, (time / growDuration));
                yield return null;
            }

            transform.localScale = targetScale;

            yield return new WaitForSeconds(growTimer - growDuration);

            gameObject.SetActive(false);
        }

    }
}

