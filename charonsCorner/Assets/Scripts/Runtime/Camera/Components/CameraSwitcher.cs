using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CameraSwitcher : MonoBehaviour
    {
        [SerializeField] private CinemachineBlendDefinition.Styles _blendType = CinemachineBlendDefinition.Styles.EaseOut;
        [SerializeField] private float _blendDuration = 0.5f;

        private CinemachineCamera _defaultCamera;

        public float BlendDuration
        {
            get => _blendDuration;
            set => _blendDuration = value;
        }

        private void Start()
        {
            _defaultCamera = CameraManager.Instance.CurrentCamera;
        }
        
        [Button("Switch", ButtonSizes.Large)]
        public void SwitchCamera(CinemachineCamera targetCamera)
        {
            CameraManager.Instance.ChangeActiveCamera(targetCamera, _blendType, _blendDuration);
        }

        [Button("Return To Default Camera", ButtonSizes.Large)]
        public void ReturnToDefaultCamera()
        {
            if (_defaultCamera != null)
            {
                CameraManager.Instance.ChangeActiveCamera(_defaultCamera, _blendType, _blendDuration);
            }
            else
            {
                Debug.LogWarning("CameraSwitcher: No default camera tracked. Returning to SceneDefaultCamera instead.", this);
                CameraManager.Instance.ResetActiveCamera();
            }
        }
    }
}