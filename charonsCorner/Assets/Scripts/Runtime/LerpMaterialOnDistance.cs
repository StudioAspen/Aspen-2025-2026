using UnityEngine;
using Sirenix.OdinInspector;

namespace CharonsCorner.Runtime
{
    public class LerpMaterialOnDistance : MonoBehaviour
    {
        [Header("Target State")]
        [SerializeField] private GameState _targetGameState = GameState.Gameplay;

        [Header("References")]
        [SerializeField] private Renderer _renderer;
        
        [Header("Materials")]
        [SerializeField] private Material _materialX;
        [SerializeField] private Material _materialY;

        [Header("Distance Settings")]
        [SerializeField] private float _minDistance = 2f;
        [SerializeField] private float _maxDistance = 10f;

        private Material _originalMaterial;
        private Material _runtimeMaterial;
        private Transform _playerTransform;
        private bool _isInTargetState;

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();

            if (_renderer != null)
            {
                _originalMaterial = _renderer.sharedMaterial;
                // Create a runtime instance to lerp without affecting the project asset
                _runtimeMaterial = new Material(_materialX);
            }
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
                CheckState(GameManager.Instance.CurrentGameState);
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
            
            RestoreOriginalMaterial();
        }

        private void HandleGameStateChanged(GameState newState)
        {
            CheckState(newState);
        }

        private void CheckState(GameState state)
        {
            bool wasInTargetState = _isInTargetState;
            _isInTargetState = (state == _targetGameState);

            if (_isInTargetState && !wasInTargetState)
            {
                // Entering target state
                if (_renderer != null)
                {
                    _renderer.material = _runtimeMaterial;
                }
            }
            else if (!_isInTargetState && wasInTargetState)
            {
                // Exiting target state
                RestoreOriginalMaterial();
            }
        }

        private void RestoreOriginalMaterial()
        {
            if (_renderer != null && _originalMaterial != null)
            {
                _renderer.material = _originalMaterial;
            }
        }

        private void Update()
        {
            if (!_isInTargetState || _renderer == null || _runtimeMaterial == null)
                return;

            if (_playerTransform == null)
            {
                var player = Object.FindFirstObjectByType<GameplayPlayerController>();
                if (player != null)
                {
                    _playerTransform = player.transform;
                }
                else
                {
                    return;
                }
            }

            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            
            // t = 0 at minDistance (Material X), t = 1 at maxDistance (Material Y)
            float t = Mathf.InverseLerp(_minDistance, _maxDistance, distance);
            
            _runtimeMaterial.Lerp(_materialX, _materialY, t);
        }

        private void OnDestroy()
        {
            if (_runtimeMaterial != null)
            {
                Destroy(_runtimeMaterial);
            }
        }
    }
}
