using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class WorldSpaceCanvasLookAtCamera : MonoBehaviour
    {
        [SerializeField] private Transform _targetCamera;

        private void LateUpdate()
        {
            transform.LookAt(transform.position + _targetCamera.forward);
        }
    }
}
