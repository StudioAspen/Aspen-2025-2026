using System;
using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Custom input controller for Cinemachine.
    /// InputManager and SensitivitySetting values are used here.
    /// </summary>
    public class CameraInputController : InputAxisControllerBase<CameraInputController.Reader>
    {
        void Update()
        { 
            if (!Application.isPlaying) 
                return;
            UpdateControllers();
        }

        [Serializable]
        public class Reader : IInputAxisReader
        {
            public float GetValue(UnityEngine.Object context, IInputAxisOwner.AxisDescriptor.Hints hint)
            {
                var inputManager = InputManager.Instance;
                if (inputManager == null || inputManager.InputActions == null)
                    return 0f;

                if (hint == IInputAxisOwner.AxisDescriptor.Hints.X)
                    return inputManager.LookDirection.x * SensitivitySetting.CurrentValue;

                if (hint == IInputAxisOwner.AxisDescriptor.Hints.Y)
                    return -inputManager.LookDirection.y * SensitivitySetting.CurrentValue;

                return 0f;
            }
        }
    }
}
