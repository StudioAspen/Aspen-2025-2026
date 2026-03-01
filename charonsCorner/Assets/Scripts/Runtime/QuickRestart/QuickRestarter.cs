using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    public class QuickRestarter : MonoBehaviour
    {
        [field: SerializeField] public float HoldDuration { get; private set; } = 1f;
        [field: SerializeField, ReadOnly] public float HeldTimer { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsQuickRestartHeld { get; private set; }

        [field: SerializeField] public UnityEvent OnQuickRestartStarted { get; private set; } = new();
        [field: SerializeField] public UnityEvent OnQuickRestartEnded { get; private set; } = new();
        [field: SerializeField] public UnityEvent OnQuickRestartActivated { get; private set; } = new();
        
        private void OnEnable()
        {
            EndQuickRestart();
            InputManager.Instance.QuickRestart += InputManager_QuickRestart;
        }

        private void OnDisable()
        {
            EndQuickRestart();
            InputManager.Instance.QuickRestart += InputManager_QuickRestart;
        }

        private void InputManager_QuickRestart(bool started)
        {
            if(started)
                StartQuickRestart();
            else
                EndQuickRestart();
        }

        private void StartQuickRestart()
        {
            IsQuickRestartHeld = true;
            HeldTimer = 0f;
            
            OnQuickRestartStarted.Invoke();
        }

        private void EndQuickRestart()
        {
            IsQuickRestartHeld = false;
            HeldTimer = 0f;
            
            OnQuickRestartEnded.Invoke();
        }
        
        private void Update()
        {
            if (!IsQuickRestartHeld)
                return;
            
            HeldTimer += Time.deltaTime;
            if (HeldTimer >= HoldDuration)
            {
                EndQuickRestart();
                ActivateQuickRestart();
            }
        }

        [Button("Activate Quick Restart", ButtonSizes.Large)]
        private void ActivateQuickRestart()
        {
            OnQuickRestartActivated.Invoke();
        }
    }
}
