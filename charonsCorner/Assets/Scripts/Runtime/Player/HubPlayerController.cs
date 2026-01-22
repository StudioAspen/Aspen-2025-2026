using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Hub specific player controller with simple movement implementation and removed extra behavior.
    /// </summary>
    public class HubPlayerController : MonoBehaviour
    {
        private InputManager _input;
        private GameManager _gameManager;

        [Header("References")]
        [SerializeField] private Rigidbody _rigidBody;
        [SerializeField] private SphereCollider _sphereCollider;

        [Header("State")]
        [SerializeField] private GameState _currentState = GameState.Title;

        [Header("Title Screen Movement")]
        [SerializeField] private float _titleScreenIntensity = 0.5f;
        [SerializeField] private float _titleScreenDuration = 2f;
        private float _titleScreenTimer;

        [Header("Movement Attributes")]
        [SerializeField] private float _maxSpeed = 10f;
        [SerializeField] private float _acceleration = 2f;
        [SerializeField] private float _dampening = 0.25f;

        [Header("Lock Points")]
        [SerializeField] private float _minX = -10f;
        [SerializeField] private float _maxX = 10f;
        
        private void Awake()
        {
            _input = InputManager.Instance;
            _gameManager = GameManager.Instance;

            if (_gameManager != null)
            {
                _gameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
                SetState(_gameManager.CurrentGameState);
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
            SetState(newState);
        }

        private void FixedUpdate()
        {
            float horizontalInput = 0f;
            
            // Wobble Back and Forth during the TitleScreen, listen to the player input for Gameplay
            if (_currentState == GameState.Gameplay)
            {
                horizontalInput = _input.MoveDirection.x;
            }
            else if (_currentState == GameState.Title)
            {
                _titleScreenTimer += Time.fixedDeltaTime;
                float omega = Mathf.PI / _titleScreenDuration;
                horizontalInput = -Mathf.Cos(_titleScreenTimer * omega) * _titleScreenIntensity;
            }

            if (Mathf.Abs(horizontalInput) > 0.01f || _currentState == GameState.Title)
            {
                // Move only along X axis
                Vector3 desiredDirection = horizontalInput >= 0 ? Vector3.right : Vector3.left;

                // Propulsion torque based on speed
                float currentSpeed = Vector3.Project(_rigidBody.linearVelocity, desiredDirection).magnitude;
                if (Vector3.Dot(_rigidBody.linearVelocity, desiredDirection) < 0) currentSpeed = 0; // If moving opposite, we want full acceleration
                
                float speedFactor = Mathf.Clamp01(currentSpeed / _maxSpeed);
                Vector3 propulsionTorque = _acceleration * Vector3.Cross(Vector3.up, desiredDirection) * (1f - speedFactor);

                _rigidBody.AddTorque(propulsionTorque, ForceMode.VelocityChange);

                // To prevent y-axis spin and any movement not on X
                _rigidBody.angularVelocity = Vector3.ProjectOnPlane(_rigidBody.angularVelocity, Vector3.up);
                
                // Cap final speed
                if (_rigidBody.linearVelocity.magnitude > _maxSpeed)
                    _rigidBody.linearVelocity = Vector3.ClampMagnitude(_rigidBody.linearVelocity, _maxSpeed);
            }
            else
            {
                Vector3 torque = _dampening * -_rigidBody.angularVelocity;
                _rigidBody.AddTorque(torque, ForceMode.VelocityChange);
            }

            // Enforce lock points
            if (_currentState == GameState.Gameplay)
            {
                Vector3 position = transform.position;
                if (position.x < _minX)
                {
                    position.x = _minX;
                    _rigidBody.linearVelocity = new Vector3(Mathf.Max(0, _rigidBody.linearVelocity.x),
                        _rigidBody.linearVelocity.y, _rigidBody.linearVelocity.z);
                }
                else if (position.x > _maxX)
                {
                    position.x = _maxX;
                    _rigidBody.linearVelocity = new Vector3(Mathf.Min(0, _rigidBody.linearVelocity.x),
                        _rigidBody.linearVelocity.y, _rigidBody.linearVelocity.z);
                }

                transform.position = position;
            }

            // Constrain Z movement as well since we only want X
            Vector3 velocity = _rigidBody.linearVelocity;
            velocity.z = 0;
            _rigidBody.linearVelocity = velocity;
        }

        public void SetState(GameState state)
        {
            _currentState = state;
            
            // Set layer based on state to avoid detection from interactable objects
            if (state == GameState.Gameplay)
            {
                gameObject.layer = LayerMask.NameToLayer("Player");
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Default");
            }

            if (state == GameState.Title)
            {
                _titleScreenTimer = 0f;
            }
        }
    }
}
