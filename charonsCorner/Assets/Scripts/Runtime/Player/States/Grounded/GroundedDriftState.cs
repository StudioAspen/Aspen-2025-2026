using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedDriftState : State<PlayerController>
    {
        [field: SerializeField] public float Drift { get; private set; } = 0.1f;
        public bool IsDrifting { get; private set; }

        public override void Enter()
        {
            IsDrifting = true;
        }

        public override void Exit()
        {
            IsDrifting = false;
        }

        public override void Update()
        {
            if (!InputManager.Instance.InputActions.Player.Drift.IsPressed())
            {
                stateMachine.ChangeState(context.GroundedSuperState.IdleState);
                return;
            }
        }

        public override void FixedUpdate()
        {

        }
    }
}
