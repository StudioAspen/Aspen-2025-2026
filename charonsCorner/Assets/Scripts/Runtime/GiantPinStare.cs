using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class GiantPinStare : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _isStaring = false;
        [SerializeField] private Transform _pinTransform;
        
        private GameplayPlayerController _player;

        private void Start()
        {
            _player = Object.FindFirstObjectByType<GameplayPlayerController>();
        }

        private void Update()
        {
            if (!_isStaring || _player == null || _pinTransform == null) return;

            Vector3 direction = _player.transform.position - _pinTransform.position;
            direction.y = 0; // Only rotate on Y axis
            if (direction != Vector3.zero)
            {
                _pinTransform.forward = direction;
            }
        }

        public void SetStaring(bool isStaring) => _isStaring = isStaring;
    }
}
