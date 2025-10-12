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
        [field: SerializeField] public GroundedDriftState DriftState { get; private set; } = new();

        [field: Header("Config")]
        [field: SerializeField] public float BaseGroundCheckDistance { get; private set; } = 0.2f;
        [field: SerializeField] public float BaseGroundCheckRadius { get; private set; } = 0.9f;
        [field: SerializeField] public LayerMask GroundLayerMask { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public bool IsGrounded { get; private set; }
        [field: SerializeField] public float JumpHeight { get; private set; } = 2f;

        private protected override void InitializeSubStates()
        {
            IdleState.Init(SubStateMachine, context);
            MoveState.Init(SubStateMachine, context);
            DriftState.Init(SubStateMachine, context);
        }

        private protected override void OnEnter()
        {
            context.Input.Jump += Input_Jump;
            context.Input.Drift += Input_Drift;
        }

        private protected override void OnExit()
        {
            if (context.Input != null)
            {
                context.Input.Jump -= Input_Jump;
                context.Input.Drift -= Input_Drift;
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
                return context.AirborneSuperState;

            return null;
        }

        public void CheckGrounded()
        {
            // Get current scale
            float scaleY = context.transform.localScale.y;
            float scaleX = context.transform.localScale.x;

            // Dynamically calculate ground check distance and radius
            float groundCheckDistance = BaseGroundCheckDistance * scaleY;
            float groundCheckRadius = BaseGroundCheckRadius * Mathf.Max(scaleX, scaleY);

            // Position the check sphere at the correct distance below the center
            Vector3 checkPosition = context.transform.position + groundCheckDistance * Vector3.down;

            IsGrounded = Physics.CheckSphere(
                checkPosition,
                groundCheckRadius,
                GroundLayerMask
            );
        }

        private void Input_Jump()
        {
            if (JumpHeight <= 0f)
                return;

            if (!IsGrounded)
                return;

            float jumpForce = Mathf.Sqrt(2 * JumpHeight * Mathf.Abs(Physics.gravity.y));
            context.RigidBody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            stateMachine.ChangeState(context.AirborneSuperState);
        }

        private void Input_Drift(bool isDrifting)
        {
            if (!isDrifting)
                return;

            if (context.Input.MoveDirection != Vector2.zero)
                SubStateMachine.ChangeState(DriftState);
        }
    }
}
