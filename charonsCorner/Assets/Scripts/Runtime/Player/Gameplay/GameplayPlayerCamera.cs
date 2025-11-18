using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class GameplayPlayerCamera : MonoBehaviour
    {
        [SerializeField] private Transform _orientation;
        
        public void Update()
        {
            // Rotate orientation towards camera
            Vector3 viewDir = _orientation.position - new Vector3(transform.position.x, _orientation.position.y, transform.position.z); 
            _orientation.forward = viewDir.normalized;
        }
    }
}
