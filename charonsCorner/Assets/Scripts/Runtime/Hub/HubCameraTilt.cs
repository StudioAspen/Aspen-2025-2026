using System;
using UnityEngine;
using CharonsCorner.Runtime;

public class HubCameraTilt : MonoBehaviour
{
    [Header("Tilt Settings")]
    [SerializeField] private float _tiltAngle = 15f;
    [SerializeField] private float _tiltSpeed = 5f;

    private GameManager _gameManager;
    private InputManager _input;
    private GameState _currentState = GameState.Title;

    private void Awake()
    {
        _input = InputManager.Instance;
        _gameManager = GameManager.Instance;

        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
            _currentState = _gameManager.CurrentGameState;
        }
    }

    private void OnDestroy()
    {
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged -= GameManager_OnGameStateChanged;
        }
    }

    private void GameManager_OnGameStateChanged(GameState newState)
    {
        _currentState = newState;
    }

    private void FixedUpdate()
    {
        float targetAngle = 0f;

        if (_currentState == GameState.Gameplay && _input != null)
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

        Vector3 currentRotation = transform.localEulerAngles;
        Quaternion targetRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, targetAngle);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.fixedDeltaTime * _tiltSpeed);
    }
}
