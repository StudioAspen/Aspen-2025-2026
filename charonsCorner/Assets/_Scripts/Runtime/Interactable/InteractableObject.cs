using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class InteractableObject : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        [Header("References")]
        [SerializeField] private Collider _triggerCollider;
        [SerializeField] private GameObject _inputDisplayerCanvasObject;

        [Header("Settings")]
        [SerializeField] private float _interactCooldown = 1f;

        [Space]
        [SerializeField, ReadOnly] private bool _isOverlapping;
        [SerializeField, ReadOnly] private bool _isOnCooldown;

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
            this.MMEventStartListening<MMGameEvent>();
        }

        private void OnDisable()
        {
            if(InputManager.Instance != null)
                InputManager.Instance.Interact -= InputManager_Interact;

            if(GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;
            
            this.MMEventStopListening<MMGameEvent>();
        }

        public void OnMMEvent(MMGameEvent gameEvent)
        {
            if (gameEvent.EventName == "OnDialogueEnd")
            {
                StartCooldown().Forget();
            }
        }

        private async UniTaskVoid StartCooldown()
        {
            _isOnCooldown = true;
            await UniTask.Delay((int)(_interactCooldown * 1000));
            _isOnCooldown = false;
        }

        private void InputManager_Interact()
        {
            if (!_isOverlapping || _isOnCooldown)
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
