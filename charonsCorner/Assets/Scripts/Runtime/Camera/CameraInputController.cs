using System;
using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CameraInputController : InputAxisControllerBase<CameraInputController.Reader>
    {
        void Update()
        {
            if (!Application.isPlaying) return;
            UpdateControllers();
        }

        [Serializable]
        public class Reader : IInputAxisReader
        {
            public float GetValue(UnityEngine.Object context, IInputAxisOwner.AxisDescriptor.Hints hint)
            {
                if(hint == IInputAxisOwner.AxisDescriptor.Hints.X)
                    return InputManager.Instance.LookDirection.x * SensitivitySetting.CurrentValue;

                if(hint == IInputAxisOwner.AxisDescriptor.Hints.Y)
                    return -InputManager.Instance.LookDirection.y * SensitivitySetting.CurrentValue;

                return 0f;
            }
        }
    }
}
