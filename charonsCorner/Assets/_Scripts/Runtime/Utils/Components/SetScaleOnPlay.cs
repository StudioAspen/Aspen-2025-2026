using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SetTransformOnPlay : MonoBehaviour
    {
        [SerializeField] private bool _setPosition = false;
        [SerializeField] private Vector3 _targetPosition = Vector3.zero;
        
        [SerializeField] private bool _setRotation = false;
        [SerializeField] private Vector3 _targetRotation = Vector3.zero;
        
        [SerializeField] private bool _setScale = true;
        [SerializeField] private Vector3 _targetScale = Vector3.zero;

        private void Awake()
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
