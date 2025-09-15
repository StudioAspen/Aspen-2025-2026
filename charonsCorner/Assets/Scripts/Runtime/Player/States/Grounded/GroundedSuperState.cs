using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedSuperState : SuperState<PlayerController>
    {
        public override State<PlayerController> InitialSubState => IdleState;

        [field: SerializeField] public GroundedIdleState IdleState { get; private set; } = new();
        [field: SerializeField] public GroundedMoveState MoveState { get; private set; } = new();

        [field: Header("Config")]
        [field: SerializeField] public float GroundCheckDistance { get; private set; } = 0.2f;
        [field: SerializeField] public float GroundCheckRadius { get; private set; } = 0.9f;
        [field: SerializeField] public LayerMask GroundLayerMask { get; private set; }
        [SerializeField] private float groundedYVelocity = -5f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float coyoteTime = 0.5f;
        private bool _hasJumped;
        public bool IsGrounded { get; private set; }

        private protected override void InitializeSubStates()
        {
            IdleState.Init(SubStateMachine, _context);
            MoveState.Init(SubStateMachine, _context);
        }

        private protected override void OnEnter()
        {
            _context.SetVelocity(_context.Velocity.WithY(groundedYVelocity));
            
            _hasJumped = false;
            
            _context.Input.Jump += Input_Jump;
        }

        private protected override void OnExit()
        {
            if(_context.Input != null)
            {
                _context.Input.Jump -= Input_Jump;
            }
        }

        private protected override void OnUpdate()
        {

        }

        private protected override void OnFixedUpdate()
        {
            _context.ApplyGravity();
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (!IsGrounded)
            {
                if(!_hasJumped)
                    _context.AirborneSuperState.ActivateCoyoteTime(coyoteTime, Input_Jump);
                return _context.AirborneSuperState;
            }
            
            return null;
        }

        public void CheckGrounded()
        {
            IsGrounded = Physics.CheckSphere(_context.transform.position + GroundCheckDistance * Vector3.down, GroundCheckRadius, GroundLayerMask);
        }

        /// <summary>
        /// Shoots a raycast downwards, detecting only ground layer colliders, and returns the ground normal.
        /// </summary>
        public Vector3 GetGroundNormal()
        {
            if (!Physics.Raycast(_context.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, GroundLayerMask))
                return Vector3.up;
            
            return hit.normal;
        }

        private void Input_Jump()
        {
            if (jumpHeight <= 0f)
                return;

            if (_hasJumped)
                return;

            float jumpForce = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics.gravity.y)); // Equation to calculate jump force based on desired height
            _context.SetVelocity(_context.Velocity.WithY(jumpForce));

            _hasJumped = true;

            _stateMachine.ChangeState(_context.AirborneSuperState);
        }
    }
}
