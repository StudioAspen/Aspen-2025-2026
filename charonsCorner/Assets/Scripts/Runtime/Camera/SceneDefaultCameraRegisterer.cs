using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SceneDefaultCameraRegisterer : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private bool changeToActiveCamera = false;

        private void Start()
        {
            CameraManager.Instance.RegisterSceneDefaultCamera(cinemachineCamera, changeToActiveCamera);
        }
    }
}
