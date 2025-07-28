using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class WorldSpaceCanvasLookAtCamera : MonoBehaviour
    {
        [SerializeField] private Transform targetCamera;

        private void LateUpdate()
        {
            transform.LookAt(transform.position + targetCamera.forward);
        }
    }
}
