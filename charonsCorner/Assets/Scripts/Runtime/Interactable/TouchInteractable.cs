using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class TouchInteractable : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Collider triggerCollider;

        [Space]
        public UnityEvent OnTouch = new UnityEvent();

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
            if (gameObject.layer != LayerMask.NameToLayer("TouchInteractable"))
                gameObject.layer = LayerMask.NameToLayer("TouchInteractable");

            if (triggerCollider == null)
                triggerCollider = GetComponent<Collider>();

            if (triggerCollider != null && !triggerCollider.isTrigger)
                triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            // No need to filter because this object only looks for player layer
            OnTouch.Invoke();
        }
    }
}
