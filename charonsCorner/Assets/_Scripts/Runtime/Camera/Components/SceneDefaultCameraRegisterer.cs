using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Helper component to make it easy to register the scene's default camera on Start().
    /// Attach this to the Cinemachine camera you want to be the default and assign references in the inspector.
    /// You can choose whether to immediately make it the active camera.
    /// </summary>
    public class SceneDefaultCameraRegisterer : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private bool _changeToActiveCamera = false;

        private void Awake()
        {
            if (_cinemachineCamera == null)
                _cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        private void Start()
        {
            if (_cinemachineCamera != null)
            {
                CameraManager.Instance.RegisterSceneDefaultCamera(_cinemachineCamera, _changeToActiveCamera);
            }
        }
    }
}
