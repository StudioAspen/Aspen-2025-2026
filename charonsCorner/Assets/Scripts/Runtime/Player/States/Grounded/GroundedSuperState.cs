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
        [SerializeField] private float jumpHeight = 2f;
        public bool IsGrounded { get; private set; }

        private protected override void InitializeSubStates()
        {
            IdleState.Init(SubStateMachine, _context);
            MoveState.Init(SubStateMachine, _context);
        }

        private protected override void OnEnter()
        {
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

        }

        private protected override State<PlayerController> GetTransition()
        {
            if (!IsGrounded)
                return _context.AirborneSuperState;
            
            return null;
        }

        public void CheckGrounded()
        {
            IsGrounded = Physics.CheckSphere(_context.transform.position + GroundCheckDistance * Vector3.down, GroundCheckRadius, GroundLayerMask);
        }

        private void Input_Jump()
        {
            if (jumpHeight <= 0f)
                return;

            if (!IsGrounded)
                return;

            float jumpForce = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics.gravity.y)); // Equation to calculate jump force based on desired height
            _context.RigidBody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            _stateMachine.ChangeState(_context.AirborneSuperState);
        }
    }
}
