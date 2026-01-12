using System;
using UnityEngine;
using CharonsCorner.Runtime;

public class HubCameraTilt : MonoBehaviour
{
    [Header("Tilt Settings")]
    [SerializeField] private float _tiltAngle = 15f;
    [SerializeField] private float _tiltSpeed = 5f;

    private HubStateManager _stateManager;
    private InputManager _input;
    private HubState _currentState = HubState.TitleScreen;

    private void Awake()
    {
        _input = InputManager.Instance;
        _stateManager = FindFirstObjectByType<HubStateManager>(FindObjectsInactive.Include);

        if (_stateManager != null)
        {
            _stateManager.OnStateChanged += StateManager_OnStateChanged;
            _currentState = _stateManager.CurrentState;
        }
    }

    private void OnDestroy()
    {
        if (_stateManager != null)
        {
            _stateManager.OnStateChanged -= StateManager_OnStateChanged;
        }
    }

    private void StateManager_OnStateChanged(HubState newState)
    {
        _currentState = newState;
    }

    private void FixedUpdate()
    {
        float targetAngle = 0f;

        if (_currentState == HubState.Gameplay && _input != null)
        {
            float horizontalInput = _input.MoveDirection.x;
            if (horizontalInput > 0.01f)
            {
                targetAngle = _tiltAngle;
            }
            else if (horizontalInput < -0.01f)
            {
                targetAngle = -_tiltAngle;
            }
        }

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.fixedDeltaTime * _tiltSpeed);
    }
}
