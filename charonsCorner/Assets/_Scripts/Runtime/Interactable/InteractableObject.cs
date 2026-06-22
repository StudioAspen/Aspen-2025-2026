using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class InteractableObject : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Collider _triggerCollider;
        [SerializeField] private GameObject _inputDisplayerCanvasObject;

        [Space]
        [SerializeField, ReadOnly] private bool _isOverlapping;

        [Space]
        public UnityEvent OnInteract = new UnityEvent();
        public UnityEvent OnPlayerEnter = new UnityEvent();
        public UnityEvent OnPlayerExit = new UnityEvent();

        private void OnValidate()
        {
            ValidateObject();
        }

        private void Awake()
        {
            ValidateObject();
        }

        private void ValidateObject()
        {
            if (gameObject.layer != LayerMask.NameToLayer("Interactable"))
                gameObject.layer = LayerMask.NameToLayer("Interactable");

            if (_triggerCollider == null)
                _triggerCollider = GetComponent<Collider>();

            if (_triggerCollider != null && !_triggerCollider.isTrigger)
                _triggerCollider.isTrigger = true;
        }

        private void OnEnable()
        {
            InputManager.Instance.Interact += InputManager_Interact;
            GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
        }

        private void OnDisable()
        {
            if(InputManager.Instance != null)
                InputManager.Instance.Interact -= InputManager_Interact;

            if(GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;
        }

        private void InputManager_Interact()
        {
            if (!_isOverlapping)
                return;

            OnInteract.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!enabled)
                return;
            
            _isOverlapping = true; // No need to filter because this object only looks for player layer
            OnPlayerEnter.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            _isOverlapping = false;
            OnPlayerExit.Invoke();
        }

        private void GameManager_OnGameStateChanged(GameState newState)
        {
            if(newState == GameState.Gameplay && _isOverlapping)
            {
                OnPlayerEnter.Invoke();
                return;
            }
            OnPlayerExit.Invoke();
        }
    }
}
