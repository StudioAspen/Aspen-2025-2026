using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class NewPlayerController : MonoBehaviour
    {
        private Rigidbody rigidBody;
        private SphereCollider sphereCollider;

        [Header("Roll Config")]
        [SerializeField] private float rollAcceleration = 1f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayerMask;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            sphereCollider = GetComponent<SphereCollider>();
        }

        private void FixedUpdate()
        {
            
        }
    }
}
