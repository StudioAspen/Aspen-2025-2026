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
        [SerializeField] private float _groundCheckRadius = 1f;
        [SerializeField] private float _regroundDelay = 0.05f;
        [SerializeField, ReadOnly] private bool _isGrounded;
        
        [Header("Ground Stick")]
        [SerializeField] private float _groundStickCheckDistance = 30f;
        [SerializeField] private float _groundStickForce = 50f;
        private Vector3 _groundNormal;
        
        [Header("Jump")]
        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private float _coyoteTime = 0.4f;
        private bool _hasJumped;
        [SerializeField, ReadOnly] private float _inAirTime;
        
        [Header("Movement")]
        [SerializeField] private float _forwardAcceleration = 10f;
        [SerializeField] private float _idleDamp = 10f;
        [SerializeField] private float _stopThreshold = 0.25f;
        [SerializeField] private float _steerRotationSpeed = 200f;
        [SerializeField, ReadOnly] private float _currentSpeed;

        public Vector3 CurrentMovement { get; private set; }

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
            UpdateInAirTime(Time.deltaTime);
            HandleRotations(_input.MoveDirection, Time.deltaTime);
        }

        private void FixedUpdate()
        {
            HandleGroundStick(Time.fixedDeltaTime);
            HandleMovement(_input.MoveDirection, Time.fixedDeltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3 groundCheckStartPosition = transform.position + (_groundCheckYOffset - _sphereCollider.radius) * Vector3.up;
            Gizmos.DrawWireSphere(groundCheckStartPosition, _groundCheckRadius);
            Gizmos.DrawWireSphere(groundCheckStartPosition + _groundCheckDistance * Vector3.down, _groundCheckRadius);
            
            // Ground normal
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + 100f * _groundNormal);
            
            // Forward
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + 100f * transform.forward);
        }

        private void CheckGrounded()
        {
            if (_inAirTime != 0f && _inAirTime < _regroundDelay)
            {
                _isGrounded = false;
                return;
            }
            
            _isGrounded = Physics.SphereCast(transform.position + (_groundCheckYOffset - _sphereCollider.radius) * Vector3.up, _groundCheckRadius,
                Vector3.down, out RaycastHit groundCheckHit, _groundCheckDistance, _groundLayer);

            // Ground stick sphere cast
            Physics.SphereCast(transform.position + (_groundCheckYOffset - _sphereCollider.radius) * Vector3.up, _groundCheckRadius,
                Vector3.down, out RaycastHit groundStickHit, _groundStickCheckDistance, _groundLayer);
            _groundNormal = _isGrounded ? groundStickHit.normal : Vector3.up;
        }

        private void HandleGroundStick(float deltaTime)
        {
            if (!_isGrounded)
                return;
            
            _rigidBody.AddForce(_groundStickForce * -_groundNormal, ForceMode.Force);
        }

        private void HandleMovement(Vector2 moveDirection, float deltaTime)
        {
            if (moveDirection.y != 0)
            {
                _currentSpeed += _forwardAcceleration * moveDirection.y * deltaTime;
            }
            else
            {
                if (Mathf.Abs(_currentSpeed) < _stopThreshold)
                    _currentSpeed = 0f;
                else
                    _currentSpeed += _idleDamp * -Mathf.Sign(_currentSpeed) * deltaTime;
            }

            Vector3 projectedMovement = Vector3.ProjectOnPlane(transform.forward, _groundNormal);
            Vector3 displacement = _currentSpeed * deltaTime * projectedMovement;
            _rigidBody.MovePosition(_rigidBody.position + displacement);
            
            Debug.DrawLine(transform.position, transform.position + 10f * displacement, Color.green, Time.deltaTime);
            CurrentMovement = displacement + _rigidBody.linearVelocity.y * Vector3.up;
        }

        private void HandleRotations(Vector2 moveDirection, float deltaTime)
        {
            Quaternion tiltRotation = Quaternion.FromToRotation(transform.up, _groundNormal) * _rigidBody.rotation;
            if(!_isGrounded)
                tiltRotation = Quaternion.identity;
            
            float steeringAmount = moveDirection.x * _steerRotationSpeed * deltaTime;
            Quaternion steeringRotation = Quaternion.AngleAxis(steeringAmount, _groundNormal);
            
            Quaternion finalRotation = steeringRotation * tiltRotation;
            _rigidBody.MoveRotation(finalRotation);
        }
        
        private void HandleJumpInput()
        {
            if (_hasJumped)
                return;

            // Coyote time expired
            if (_inAirTime != 0f && _inAirTime > _coyoteTime)
                return;
            
            float jumpForce = Utilities.GetJumpForce(_jumpHeight, Mathf.Abs(Physics.gravity.y));
            _rigidBody.linearVelocity = jumpForce * Vector3.up;

            _hasJumped = true;
            _isGrounded = false;
            _inAirTime = Mathf.Epsilon; // To activate the reground check and make _inAirTime > 0
        }

        private void UpdateInAirTime(float deltaTime)
        {
            if (_isGrounded)
            {
                _inAirTime = 0f;
                _hasJumped = false;
                return;
            }
            
            _inAirTime += deltaTime;
        }
    }
}