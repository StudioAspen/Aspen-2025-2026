using System;
using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerController : MonoBehaviour
    {
        private InputManager _input; // Easy access to InputManager.Instance
        
        [Header("References")]
        [SerializeField] private Rigidbody _rigidBody;
        [SerializeField] private SphereCollider _sphereCollider;

        [Header("Ground Detection")] 
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _groundCheckYOffset = 0.25f;
        [SerializeField] private float _groundCheckDistance = 0.5f;
        [SerializeField] private float _regroundDelay = 0.05f;
        [SerializeField, ReadOnly] private bool _isGrounded;
        
        [Header("Jump")]
        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private float _coyoteTime = 0.4f;
        private bool _hasJumped;
        [SerializeField, ReadOnly] private float _inAirTime;
        
        [Header("Speed")]
        [SerializeField] private float _speed = 5f;

        private void Awake()
        {
            _input = InputManager.Instance;
        }

        private void OnEnable()
        {
            _input.Jump += HandleJumpInput;
        }

        private void OnDisable()
        {
            if(_input != null)
            {
                _input.Jump -= HandleJumpInput;
            }
        }

        private void OnDestroy()
        {
            
        }

        private void Update()
        {
            CheckGrounded();
            UpdateInAirTime();
        }

        private void FixedUpdate()
        {
            Vector3 moveDirection = Utilities.GetCameraBasedMoveInput(CameraManager.Instance.CurrentCamera.transform,
                _input.MoveDirection);
            _rigidBody.AddForce(_speed * moveDirection, ForceMode.Acceleration);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3 groundCheckStartPosition = transform.position + (_groundCheckYOffset - _sphereCollider.radius) * Vector3.up;
            Gizmos.DrawLine(groundCheckStartPosition, groundCheckStartPosition + _groundCheckDistance * Vector3.down);
        }

        private void CheckGrounded()
        {
            if (_inAirTime != 0f && _inAirTime < _regroundDelay)
            {
                _isGrounded = false;
                return;
            }
            
            _isGrounded = Physics.Raycast(transform.position + (_groundCheckYOffset - _sphereCollider.radius) * Vector3.up,
                    Vector3.down, out RaycastHit hit, _groundCheckDistance, _groundLayer);
        }
        
        private void HandleJumpInput()
        {
            if (_hasJumped)
                return;

            // Coyote time expired
            if (_inAirTime != 0f && _inAirTime > _coyoteTime)
                return;
            
            float jumpForce = Utilities.GetJumpForce(_jumpHeight);
            _rigidBody.AddForce((jumpForce - _rigidBody.linearVelocity.y) * Vector3.up, ForceMode.VelocityChange);

            _hasJumped = true;
            _isGrounded = false;
            _inAirTime = Mathf.Epsilon; // To activate the reground check and make _inAirTime > 0
        }

        private void UpdateInAirTime()
        {
            if (_isGrounded)
            {
                _inAirTime = 0f;
                _hasJumped = false;
                return;
            }
            
            _inAirTime += Time.deltaTime;
        }
    }
}