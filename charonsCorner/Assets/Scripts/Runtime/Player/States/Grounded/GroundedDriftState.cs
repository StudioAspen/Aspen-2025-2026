using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedDriftState : State<PlayerController>
    {
        private float driftTimer;

        public bool IsDrifting { get; private set; }

        private protected override void OnEnter()
        {
            IsDrifting = true;
            driftTimer = 0f;
        }

        private protected override void OnExit()
        {
            IsDrifting = false;

            context.RigidBody.isKinematic = false;
        }

        private protected override void OnUpdate()
        {
            driftTimer += Time.deltaTime;
        }

        private protected override void OnFixedUpdate()
        {
            
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (!context.Input.InputActions.Player.Drift.IsPressed())
                return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}
