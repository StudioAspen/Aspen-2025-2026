using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SceneDefaultCameraRegisterer : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private bool _changeToActiveCamera = false;

        private void Start()
        {
            CameraManager.Instance.RegisterSceneDefaultCamera(_cinemachineCamera, _changeToActiveCamera);
        }
    }
}
