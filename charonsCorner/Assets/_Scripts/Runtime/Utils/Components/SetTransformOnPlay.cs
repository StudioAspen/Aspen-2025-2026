using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SetTransformOnPlay : MonoBehaviour
    {
        [SerializeField] protected bool _setPosition = false;
        [SerializeField] protected Vector3 _targetPosition = Vector3.zero;
        
        [SerializeField] protected bool _setRotation = false;
        [SerializeField] protected Vector3 _targetRotation = Vector3.zero;
        
        [SerializeField] protected bool _setScale = true;
        [SerializeField] protected Vector3 _targetScale = Vector3.zero;

        protected virtual void Awake()
        {
            if (_setPosition)
            {
                transform.localPosition = _targetPosition;
            }
            
            if (_setRotation)
            {
                transform.localRotation = Quaternion.Euler(_targetRotation);
            }
            
            if (_setScale)
            {
                transform.localScale = _targetScale;
            }
        }
    }
}
