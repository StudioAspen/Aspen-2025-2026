using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Debugging component to track when a Cinemachine camera becomes active or inactive.
    /// Attach this to any Cinemachine camera you want to monitor.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CinemachineCameraDebugger : MonoBehaviour
    {
        private CinemachineCamera _vcam;
        private bool _isLive;

        private void Awake()
        {
            _vcam = GetComponent<CinemachineCamera>();
        }

        private void Update()
        {
            bool currentlyLive = CinemachineCore.IsLive(_vcam);

            if (currentlyLive != _isLive)
            {
                _isLive = currentlyLive;
                if (_isLive)
                {
                    Debug.Log($"[CinemachineDebugger] <color=green>CAMERA LIVE:</color> {_vcam.name} (Priority: {_vcam.Priority.Value})", gameObject);
                }
                else
                {
                    Debug.Log($"[CinemachineDebugger] <color=red>CAMERA INACTIVE:</color> {_vcam.name} (Current Priority: {_vcam.Priority.Value})", gameObject);
                }
            }
        }

        private void OnEnable()
        {
            if (_vcam == null) _vcam = GetComponent<CinemachineCamera>();
            Debug.Log($"[CinemachineDebugger] Camera {_vcam.name} enabled. Priority: {_vcam.Priority.Value}", gameObject);
        }
    }
}
