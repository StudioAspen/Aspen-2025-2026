using UnityEngine;
using DG.Tweening;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Enemy bumper for the soccer level: detects player, anticipates position, and moves toward them.
    /// Movement now uses FixedUpdate for physics consistency across Editor and Windows build.
    /// </summary>
    public class BumperEnemy : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _detectionRadius = 26f;
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

        // Cache movement direction computed in Update, applied in FixedUpdate
        private Vector3 _currentMoveDirection;

        private GameplayPlayerController gameplayPlayerController;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody != null)
            {
                // Smooth physics at varying frame rates in build
                _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }

            _meshRenderer = GetComponent<MeshRenderer>();
            if (_meshRenderer == null)
                _meshRenderer = GetComponentInChildren<MeshRenderer>();

            _initialScale = (_meshRenderer != null) ? _meshRenderer.transform.localScale : transform.localScale;

            gameplayPlayerController = FindFirstObjectByType<GameplayPlayerController>();
        }

        public void SetAggro(bool enabled, Transform player = null)
        {
            _forceAggro = enabled;
            if (enabled)
            {
                _playerTransform = player != null ? player : GameObject.FindGameObjectWithTag("Player")?.transform;
                if (_playerTransform != null)
                {
                    _lastPlayerPosition = _playerTransform.position;
                }
            }
            else
            {
                _playerTransform = null;
                _isPlayerInRadius = false;
                _currentMoveDirection = Vector3.zero;
            }
        }

        private void Update()
        {
            // Compute direction each frame (render loop), independent of physics step
            if (_forceAggro)
            {
                ComputeMoveDirection();
                return;
            }

            DetectPlayer();

            if (_isPlayerInRadius && _playerTransform != null)
            {
                ComputeMoveDirection();
            }
            else
            {
                _currentMoveDirection = Vector3.zero;
            }
        }

        private void FixedUpdate()
        {
            if (_currentMoveDirection.sqrMagnitude > 0.0001f)
            {
                // Apply physics movement at fixed timestep
                if (_rigidbody != null)
                {
                    var newPos = _rigidbody.position + _currentMoveDirection * _moveSpeed * Time.fixedDeltaTime;
                    _rigidbody.MovePosition(newPos);
                }
                else
                {
                    transform.position += _currentMoveDirection * _moveSpeed * Time.fixedDeltaTime;
                }
            }
        }

        private void ComputeMoveDirection()
        {
            if (_playerTransform == null)
            {
                _currentMoveDirection = Vector3.zero;
                return;
            }

            Vector3 playerVelocity = (_playerTransform.position - _lastPlayerPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 anticipatedPosition = _playerTransform.position + (_anticipationFactor * playerVelocity);
            _currentMoveDirection = (anticipatedPosition - transform.position).normalized;
            _lastPlayerPosition = _playerTransform.position;
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
                if (_meshRenderer != null)
                {
                    gameplayPlayerController?.BumperFeedbacks?.PlayFeedbacks();

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