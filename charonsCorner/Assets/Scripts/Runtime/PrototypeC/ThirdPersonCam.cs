using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ThirdPersonCam : MonoBehaviour
    {
        [field:SerializeField] public Transform Orientation { get; private set; }
    
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void Update()
        {
            // Rotate orientation towards camera
            Vector3 viewDir = Orientation.position - new Vector3(transform.position.x, Orientation.position.y, transform.position.z); 
            Orientation.forward = viewDir.normalized;
        }
    }

}
