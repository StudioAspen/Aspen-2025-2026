using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class Pin : MonoBehaviour
    {
        private Rigidbody rigidBody;
        private BoxCollider boxCollider;

        [SerializeField] private float destroyDelay = 1f;
        [SerializeField] private float impulseForceMultiplier = 0.4f;

        private bool isHit;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            boxCollider = GetComponent<BoxCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isHit)
                return;

            if (other.gameObject.TryGetComponent(out PinCollector collector))
            {
                isHit = true;

                collector.CollectPin(1);

                Vector3 launchDirection = transform.position - other.transform.position + Vector3.down;
                float collectorSpeed = other.GetComponent<Rigidbody>().linearVelocity.magnitude;
                rigidBody.AddForce(impulseForceMultiplier * collectorSpeed * launchDirection, ForceMode.Impulse);

                Destroy(boxCollider);

                Destroy(gameObject, destroyDelay);
            }
        }
    }
}
