using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CameraRegisterer : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private CameraManager.CameraType cameraType;
        [SerializeField] private bool changeToActiveCamera = false;

        private void Start()
        {
            CameraManager.Instance.RegisterCamera(cameraType, cinemachineCamera, changeToActiveCamera);
        }
    }
}
