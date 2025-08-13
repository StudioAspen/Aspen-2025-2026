using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedSuperState : HierarchicalState<PlayerController>
    {
        [field: SerializeField] public IdleState IdleState { get; private set; } = new();
        [field: SerializeField] public RollState RollState { get; private set; } = new();

        [field: Header("Config")]
        [field: SerializeField] public float JumpHeight { get; private set; } = 2f;

        private protected override void InitializeSubStates()
        {
            Debug.Log(IdleState);
            IdleState.Init(SubStateMachine, context);
            RollState.Init(SubStateMachine, context);
        }

        public override void Enter()
        {
            InputManager.Instance.Jump += InputManager_Jump;

            SubStateMachine.ChangeState(IdleState, true);
        }

        public override void Exit()
        {
            if(InputManager.Instance != null)
                InputManager.Instance.Jump -= InputManager_Jump;
        }

        public override void Update()
        {

        }

        public override void FixedUpdate()
        {

        }

        private void InputManager_Jump()
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
