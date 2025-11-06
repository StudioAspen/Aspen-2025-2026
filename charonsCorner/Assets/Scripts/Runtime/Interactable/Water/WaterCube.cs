using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class WaterCube : MonoBehaviour
    {
        [Header("Buoyancy Settings")]
        public float liftForce = 10f;
        public float liftSmooth = 5f;
        public LayerMask playerLayerMask; // Select layers in Inspector

        [Header("Visual Settings")]
        public float waveSpeed = 2f;
        public float waveHeight = 0.1f;

        [SerializeField] private MeshRenderer meshRenderer;
        private Vector3 originalScale;
        private bool playerInside = false;
        private Rigidbody playerRb;

        private void Awake()
        {
            originalScale = transform.localScale;

            // Make sure the collider is a trigger
            var box = GetComponent<BoxCollider>();
            box.isTrigger = true;
        }

        private void Update()
        {
            // Animate the water surface (scale Y for simple wave effect)
            float wave = Mathf.Sin(Time.time * waveSpeed) * waveHeight;
            transform.localScale = new Vector3(
                originalScale.x,
                originalScale.y + wave,
                originalScale.z
            );
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & playerLayerMask.value) != 0)
            {
                playerInside = true;
                playerRb = other.GetComponent<Rigidbody>();
                other.GetComponent<PlayerController>().IsInWater = true; // Notify player controller
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (((1 << other.gameObject.layer) & playerLayerMask.value) != 0)
            {
                playerInside = false;
                playerRb = null;
                other.GetComponent<PlayerController>().IsInWater = false; // Notify player controller
            }
        }

        private void FixedUpdate()
        {
            if (playerInside && playerRb != null)
            {
                // Apply upward force to simulate buoyancy
                Vector3 velocity = playerRb.linearVelocity;
                velocity.y = Mathf.Lerp(velocity.y, liftForce, Time.fixedDeltaTime * liftSmooth);
                playerRb.linearVelocity = velocity;
            }
        }
    }
} 