using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class GiantPinStare : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _isStaring = false;
        
        public void SetStaring(bool isStaring) => _isStaring = isStaring;
    }
}
