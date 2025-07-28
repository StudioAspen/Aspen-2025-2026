using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class InteractableObject : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Collider triggerCollider;
        [SerializeField] private GameObject inputDisplayerCanvasObject;

        [Space]
        [SerializeField, ReadOnly] private bool isOverlapping;

        [Space]
        public UnityEvent OnInteract = new UnityEvent();

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

            if (triggerCollider == null)
                triggerCollider = GetComponent<Collider>();

            if (triggerCollider != null && !triggerCollider.isTrigger)
                triggerCollider.isTrigger = true;
        }

        private void OnEnable()
        {
            InputManager.Instance.Interact += InputManager_Interact;

            inputDisplayerCanvasObject.SetActive(false);
        }

        private void OnDisable()
        {
            if(InputManager.Instance != null)
                InputManager.Instance.Interact -= InputManager_Interact;
        }

        private void InputManager_Interact()
        {
            if (!isOverlapping)
                return;

            Debug.Log($"Interacted with {gameObject.name}");
            OnInteract.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            isOverlapping = true; // No need to filter because this object only looks for player layer
            inputDisplayerCanvasObject.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            isOverlapping = false;
            inputDisplayerCanvasObject.SetActive(false);
        }
    }
}
