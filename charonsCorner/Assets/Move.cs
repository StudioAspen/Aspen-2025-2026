using UnityEngine;
using DG.Tweening;

namespace CharonsCorner.Runtime
{
    public class MoveB : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float anticipationFactor = 1.5f; // How much to anticipate player's movement

        [Header("Squash & Stretch Settings")]
        [SerializeField] private float squashDuration = 0.2f;
        [SerializeField] private float stretchDuration = 0.2f;
        [SerializeField] private Vector3 squashScale = new Vector3(1.2f, 0.7f, 1.2f);

        private Transform playerTransform;
        private Vector3 lastPlayerPosition;
        private bool isPlayerInRadius = false;
        private bool forceAggro = false;
        private MeshRenderer meshRenderer;
        private Vector3 initialScale;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                meshRenderer = GetComponentInChildren<MeshRenderer>();

            if (meshRenderer != null)
                initialScale = meshRenderer.transform.localScale;
            else
                initialScale = transform.localScale;
        }

        public void SetAggro(bool enabled, Transform player = null)
        {
            forceAggro = enabled;
            if (enabled && player != null)
            {
                playerTransform = player;
                lastPlayerPosition = player.position;
            }
            if (!enabled)
            {
                playerTransform = null;
                isPlayerInRadius = false;
            }
        }

        private void Update()
        {
            if (forceAggro)
            {
                if (playerTransform != null)
                {
                    Vector3 playerVelocity = (playerTransform.position - lastPlayerPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
                    Vector3 anticipatedPosition = playerTransform.position + playerVelocity * anticipationFactor;
                    Vector3 direction = (anticipatedPosition - transform.position).normalized;
                    transform.position += direction * moveSpeed * Time.deltaTime;
                    lastPlayerPosition = playerTransform.position;
                }
                return;
            }

            DetectPlayer();

            if (isPlayerInRadius && playerTransform != null)
            {
                Vector3 playerVelocity = (playerTransform.position - lastPlayerPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
                Vector3 anticipatedPosition = playerTransform.position + playerVelocity * anticipationFactor;
                Vector3 direction = (anticipatedPosition - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
                lastPlayerPosition = playerTransform.position;
            }
        }

        private void DetectPlayer()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            bool foundPlayer = false;

            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    foundPlayer = true;
                    if (playerTransform == null)
                    {
                        playerTransform = col.transform;
                        lastPlayerPosition = playerTransform.position;
                    }
                    break;
                }
            }

            if (!foundPlayer)
            {
                playerTransform = null;
                isPlayerInRadius = false;
            }
            else
            {
                isPlayerInRadius = true;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // Squash and stretch animation using DOTween
                if (meshRenderer != null)
                {
                    Transform meshTransform = meshRenderer.transform;
                    meshTransform.DOScale(Vector3.Scale(initialScale, squashScale), squashDuration)
                        .OnComplete(() => meshTransform.DOScale(initialScale, stretchDuration));
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}