using System;
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
        
        private protected override void InitializeSubStates()
        {
            IdleState.Init(SubStateMachine, _context);
            MoveState.Init(SubStateMachine, _context);
            DriftState.Init(SubStateMachine, _context);
            DropDashState.Init(SubStateMachine, _context);
        }

        private protected override void OnEnter()
        {
            _context.Input.Jump += Input_Jump;
        }

        private protected override void OnExit()
        {
            if(_context.Input != null)
                _context.Input.Jump -= Input_Jump;
        }

        private protected override void OnUpdate()
        {
            _context.ApplyGravity();
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<PlayerController> GetTransition()
        {
            if (_context.IsGrounded)
                return _context.GroundedSuperState;

            return null;
        }
        
        private void Input_Jump()
        {

        }

        public void ActivateCoyoteTime(float duration, Action jumpAction)
        {
            
        }
    }
}
