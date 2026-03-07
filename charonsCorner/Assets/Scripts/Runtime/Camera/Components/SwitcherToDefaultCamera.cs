using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SwitcherToDefaultCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineBlendDefinition.Styles _blendType = CinemachineBlendDefinition.Styles.EaseOut;
        [SerializeField] private float _blendDuration = 0.5f;
        
        [Button("Switch", ButtonSizes.Large)]
        public void SwitchToDefaultCamera()
        {
            CameraManager.Instance.ChangeActiveCamera(CameraManager.Instance.SceneDefaultCamera, _blendType, _blendDuration);
        }
    }
}