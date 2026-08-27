using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CameraSwitcher : MonoBehaviour
    {
        [SerializeField] private CinemachineBlendDefinition.Styles _blendType = CinemachineBlendDefinition.Styles.EaseOut;
        [SerializeField] private float _blendDuration = 0.5f;

        public float BlendDuration
        {
            get => _blendDuration;
            set => _blendDuration = value;
        }
        
        [Button("Switch", ButtonSizes.Large)]
        public void SwitchCamera(CinemachineCamera targetCamera)
        {
            Debug.Log($"[CameraSwitcher] {gameObject.name} switching to: {(targetCamera != null ? targetCamera.name : "Null")}");
            CameraManager.Instance.ChangeActiveCamera(targetCamera, _blendType, _blendDuration);
        }

        [Button("Return To Default Camera", ButtonSizes.Large)]
        public void ReturnToDefaultCamera()
        {
            Debug.Log($"[CameraSwitcher] {gameObject.name} returning to default camera.");
            CameraManager.Instance.ResetActiveCamera(_blendType, _blendDuration);
        }
    }
}