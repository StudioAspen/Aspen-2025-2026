using UnityEngine;
using UnityEngine.Events;
using CharonsCorner.Runtime;
using UnityEngine.InputSystem;

public class StartDialogue : MonoBehaviour
{
    [SerializeField] private UnityEvent _onInputPressed;
    [SerializeField] private bool _triggerOnce = true;

    private bool _hasTriggered = false;
    private int _lastTriggerFrame = -1;

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Interact += HandleInput;
            InputManager.Instance.JumpPressed += HandleInput;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Interact -= HandleInput;
            InputManager.Instance.JumpPressed -= HandleInput;
        }
    }

    private void Update()
    {
        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                HandleInput();
            }
        }
    }

    private void HandleInput()
    {
        if (_triggerOnce && _hasTriggered) return;
        if (_lastTriggerFrame == Time.frameCount) return;

        _hasTriggered = true;
        _lastTriggerFrame = Time.frameCount;
        _onInputPressed?.Invoke();
    }
}
