using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class Pin : MonoBehaviour
    {
        private Rigidbody _rigidBody;
        private BoxCollider _boxCollider;

        [SerializeField] private float _destroyDelay = 1f;
        [SerializeField] private float _impulseForceMultiplier = 0.4f;

        private bool _isHit;

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _boxCollider = GetComponent<BoxCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isHit)
                return;

            if (other.gameObject.TryGetComponent(out PlayerPinCollector collector))
            {
                _isHit = true;

                collector.CollectPin(1);

                // Launch when hit by the player
                Vector3 launchDirection = transform.position - other.transform.position + Vector3.down;
                float collectorSpeed = other.GetComponent<Rigidbody>().linearVelocity.magnitude;
                _rigidBody.AddForce(_impulseForceMultiplier * collectorSpeed * launchDirection, ForceMode.Impulse);

                Destroy(_boxCollider); // Remove the trigger to prevent further collisions

                Destroy(gameObject, _destroyDelay);
            }
        }
    }
}
