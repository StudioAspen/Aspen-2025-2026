using UnityEngine;
using UnityEngine.Serialization;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirborneSuperState : SuperState<PlayerController>
    {
        public override State<PlayerController> InitialSubState => IdleState;

        [field: SerializeField] public AirborneIdleState IdleState { get; private set; } = new();
        [field: SerializeField] public AirborneMoveState MoveState { get; private set; } = new();
        [field: SerializeField] public AirborneDriftState DriftState { get; private set; } = new();
        [field: SerializeField] public AirborneDropDashState DropDashState { get; private set; } = new();

        public float MaxSpeed { get; private set; }
        private bool hasDropDashed;
        
        private protected override void InitializeSubStates()
        {
            IdleState.Init(SubStateMachine, context);
            MoveState.Init(SubStateMachine, context);
            DriftState.Init(SubStateMachine, context);
            DropDashState.Init(SubStateMachine, context);
        }

        private protected override void OnEnter()
        {
            hasDropDashed = false;
            context.Input.Jump += Input_Jump;
            
            UpdateMaxSpeed(context.RigidBody.linearVelocity.WithY(0).magnitude);
        }

        private protected override void OnExit()
        {
            if(context.Input != null)
                context.Input.Jump -= Input_Jump;
        }

        private protected override void OnUpdate()
        {

        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.IsGrounded)
                return context.GroundedSuperState;

            return null;
        }
        
        private void Input_Jump()
        {
            if (hasDropDashed)
                return;
            
            hasDropDashed = true;
            SubStateMachine.ChangeState(DriftState);
        }
        
        public void UpdateMaxSpeed(float newMaxSpeed) => MaxSpeed = newMaxSpeed;
    }
}
