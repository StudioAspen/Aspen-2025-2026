using log4net.Util;
using NaughtyAttributes;
using System;
using UnityEditorInternal;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedSuperState : HierarchicalState<PlayerController>
    {
        public override State<PlayerController> InitialSubState => IdleState;

        [field: SerializeField] public GroundedIdleState IdleState { get; private set; } = new();
        [field: SerializeField] public GroundedMoveState MoveState { get; private set; } = new();
        [field: SerializeField] public GroundedDriftState DriftState { get; private set; } = new();

        [field: Header("Config")]
        [field: SerializeField] public float GroundCheckDistance { get; private set; } = 0.2f;
        [field: SerializeField] public float GroundCheckRadius { get; private set; } = 0.9f;
        [field: SerializeField] public LayerMask GroundLayerMask { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public bool IsGrounded { get; private set; }
        [field: SerializeField] public float JumpHeight { get; private set; } = 2f;

        private protected override void InitializeSubStates()
        {
            IdleState.Init(SubStateMachine, context);
            MoveState.Init(SubStateMachine, context);
            DriftState.Init(SubStateMachine, context);
        }

        public override void Enter()
        {
            InputManager.Instance.Jump += Input_Jump;
        }

        public override void Exit()
        {
            if(InputManager.Instance != null)
                InputManager.Instance.Jump -= Input_Jump;
        }

        public override void Update()
        {
            if (InputManager.Instance.InputActions.Player.Drift.IsPressed())
            {
                SubStateMachine.ChangeState(DriftState);
                return;
            }
        }

        public override void FixedUpdate()
        {

        }

        public void CheckGrounded()
        {
            bool isGrounded = Physics.CheckSphere(context.transform.position + GroundCheckDistance * Vector3.down, GroundCheckRadius, GroundLayerMask);
            if (isGrounded != IsGrounded)
            {
                IsGrounded = isGrounded;
                stateMachine.ChangeState(IsGrounded ? context.GroundedSuperState : context.AirborneSuperState);
            }
        }

        private void Input_Jump()
        {
            if (JumpHeight <= 0f)
                return;

            if (!context.IsGrounded)
                return;

            float jumpForce = Mathf.Sqrt(2 * JumpHeight * Mathf.Abs(Physics.gravity.y)); // Equation to calculate jump force based on desired height
            context.RigidBody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            stateMachine.ChangeState(context.AirborneSuperState);
        }
    }
}
