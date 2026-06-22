using System.Collections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField]
        GameObject
            pointA,
            pointB;
        [SerializeField] float speed = 1f;
        [SerializeField] float delay = 10f;
        [SerializeField] GameObject platform;

        private Vector3 targetPosition;

        //Start is called before the first frame update
        void Start()
        {
            platform.transform.position = pointA.transform.position;
            targetPosition = pointB.transform.position;
            StartCoroutine(MovePlatform());
        }

        IEnumerator MovePlatform()
        {
            while (true)
            {
                while ((targetPosition - platform.transform.position).sqrMagnitude > 0.1f)
                {
                    platform.transform.position = Vector3.MoveTowards(platform.transform.position, targetPosition, speed * Time.deltaTime);
                    yield return null;
                }
                // Swap target position
                targetPosition = targetPosition == pointA.transform.position ? pointB.transform.position : pointA.transform.position;
                // Wait for the specified delay before moving again
                yield return new WaitForSeconds(delay);
            }
        }
    }
}
