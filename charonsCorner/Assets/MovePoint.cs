using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class MovePoint : MonoBehaviour
    {
        [Header("Target Point")]
        [SerializeField] private Transform targetPoint;

        [Header("Move Settings")]
        [SerializeField] private bool smoothMove = false;
        [SerializeField] private float moveDuration = 1f;

        private void Start()
        {
            if (targetPoint == null)
                return;

            if (smoothMove && moveDuration > 0f)
            {
                StartCoroutine(SmoothMoveToPoint());
            }
            else
            {
                transform.position = targetPoint.position;
            }
        }

        private System.Collections.IEnumerator SmoothMoveToPoint()
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = targetPoint.position;
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                transform.position = Vector3.Lerp(startPos, endPos, elapsed / moveDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = endPos;
        }
    }
}