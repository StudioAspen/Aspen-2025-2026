using Unity.Mathematics;
using UnityEngine;
namespace CharonsCorner.Runtime
{
    public class ParticleFollowPlayer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _parent;
        [SerializeField] private Rigidbody _parentRigidbody;
       
        // [Header("offsets")]
        [SerializeField] private bool _followDirectionOfParent;
        private Vector3 _offset;

        void OnEnable()
        {
            _offset = transform.position - _parent.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (_followDirectionOfParent && _parentRigidbody.linearVelocity.sqrMagnitude > 0.01f)
            {
                transform.position = _parent.position;
                transform.forward = _parentRigidbody.linearVelocity.normalized;
            }
            else
            {
                transform.position = _parent.position + _offset;
            }
        }
    }
}

