using UnityEngine;
using DG.Tweening;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// This script is on the enemy bumper blocks for the soccer level (Scene 1 Level 3). The enemy detects the player within a radius
    /// then is intended to move towards them by trying to anticipate their position, 'bullying' them. 
    /// When an enemy and player collide, the enemy has a 'squash' animation using DOTween
    /// Setaggro mode is intended to override the radius activation to made the bumpers pursue the player regardles of location 
    /// This script is WIP
    /// </summary>
    public class BumperEnemy : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _detectionRadius =26f;
        [SerializeField] private float _moveSpeed = 15f;
        [SerializeField] private float _anticipationFactor = 1.5f; 

        [Header("Squash & Stretch Settings")]
        [SerializeField] private float _squashDuration = 0.2f;
        [SerializeField] private float _stretchDuration = 0.2f;
        [SerializeField] private Vector3 _squashScale = new Vector3(1.2f, 0.7f, 1.2f);

        private Transform _playerTransform;
        private Vector3 _lastPlayerPosition;
        private bool _isPlayerInRadius = false;
        private bool _forceAggro = false;
        private MeshRenderer _meshRenderer;
        private Vector3 _initialScale;
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _meshRenderer = GetComponent<MeshRenderer>();
            if (_meshRenderer == null)
                _meshRenderer = GetComponentInChildren<MeshRenderer>();

            if (_meshRenderer != null)
                _initialScale = _meshRenderer.transform.localScale;
            else
                _initialScale = transform.localScale;
        }

        public void SetAggro(bool enabled, Transform player = null)
        {
            _forceAggro = enabled;
            if (enabled && player != null)
            {
                _playerTransform = player;
                _lastPlayerPosition = player.position;
            }
            if (!enabled)
            {
                _playerTransform = null;
                _isPlayerInRadius = false;
            }
        }
        private void MoveTowardsPlayer()
        {
            if (_playerTransform == null) return;
    
            Vector3 playerVelocity = (_playerTransform.position - _lastPlayerPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 anticipatedPosition = _playerTransform.position + (_anticipationFactor * playerVelocity);
            Vector3 direction = (anticipatedPosition - transform.position).normalized;
            if (_rigidbody != null)
                _rigidbody.MovePosition(transform.position + (_moveSpeed * Time.deltaTime) * direction);
            else
                transform.position += (_moveSpeed * Time.deltaTime) * direction;
            _lastPlayerPosition = _playerTransform.position;
        }

        private void Update()
        {
            if (_forceAggro)
            {
                MoveTowardsPlayer();
                return;
            }

            DetectPlayer();

            if (_isPlayerInRadius && _playerTransform != null)
            {
               MoveTowardsPlayer();
            }
        }

        private void DetectPlayer()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _detectionRadius);
            bool foundPlayer = false;

            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    foundPlayer = true;
                    if (_playerTransform == null)
                    {
                        _playerTransform = col.transform;
                        _lastPlayerPosition = _playerTransform.position;
                    }
                    break;
                }
            }

            if (!foundPlayer)
            {
                _playerTransform = null;
                _lastPlayerPosition = Vector3.zero;
                _isPlayerInRadius = false;
            }
            else
            {
                _isPlayerInRadius = true;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // Squash and stretch animation using DOTween
                if (_meshRenderer != null)
                {
                    Transform meshTransform = _meshRenderer.transform;
                    meshTransform.DOScale(Vector3.Scale(_initialScale, _squashScale), _squashDuration)
                        .OnComplete(() => meshTransform.DOScale(_initialScale, _stretchDuration));
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        }
    }
}