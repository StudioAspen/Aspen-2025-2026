using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CameraShakerComponent : MonoBehaviour
    {
        [SerializeField] private float _duration = 1f;
        [SerializeField] private float _amplitude = 1f;
        [SerializeField] private float _frequency = 1f;

        [Button("Shake", ButtonSizes.Large)]
        public void ShakeCamera()
        {
            CameraManager.Instance.CameraShaker.ShakeCamera(_amplitude, _frequency, _duration);
        }
    }
}