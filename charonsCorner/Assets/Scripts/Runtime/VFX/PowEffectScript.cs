using System.Collections;
using UnityEngine;


namespace CharonsCorner.Runtime
{
    public class PowEffectScript : MonoBehaviour
    {

        private float growDuration = 0.2f;
        public float growTimer = 0.5f;


        private float minScale = 1f;
        private float maxScale = 2f;
        private float maxSpeed = 50f;
        
        private Coroutine activateRoutine;
        
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

