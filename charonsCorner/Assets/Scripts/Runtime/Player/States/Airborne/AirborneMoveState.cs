using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirborneMoveState : State<PlayerController>
    {
        [field: SerializeField] public float Damp { get; private set; } = 0.1f;

        public override void Enter()
        {

        }

        public override void Exit()
        {

        }

        public override void Update()
        {
            if (context.Input.MoveDirection == Vector2.zero)
            {
                stateMachine.ChangeState(context.AirborneSuperState.IdleState);
                return;
            }
        }

        public override void FixedUpdate()
        {
            
        }
    }
}
