using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CameraSwitcher : MonoBehaviour
    {
        [SerializeField] private CinemachineBlendDefinition.Styles _blendType = CinemachineBlendDefinition.Styles.EaseOut;
        [SerializeField] private float _blendDuration = 0.5f;
        
        [Button("Switch", ButtonSizes.Large)]
        public void SwitchCamera(CinemachineCamera targetCamera)
        {
            CameraManager.Instance.ChangeActiveCamera(targetCamera, _blendType, _blendDuration);
        }
    }
}