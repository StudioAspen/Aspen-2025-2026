using DG.Tweening;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedDriftStartState : State<PlayerController>
    {
        [SerializeField] private float hopHeight = 0.1f;
        [SerializeField] private float hopDuration = 0.2f;

        private float timer;

        private protected override void OnEnter()
        {
            context.VisualObject.transform.DOLocalJump(Vector3.zero, hopHeight, 1, hopDuration);

            timer = 0f;
        }

        private protected override void OnExit()
        {
            
        }

        private protected override void OnUpdate()
        {
            timer += Time.deltaTime;
        }

        private protected override void OnFixedUpdate()
        {
            context.VisualObject.transform.rotation = context.RigidBody.rotation;
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (timer > hopDuration)
            {
                if(context.Input.MoveDirection.x == 0f)
                    return context.GroundedSuperState.IdleState;

                return context.GroundedSuperState.DriftState;
            }

            return null;
        }
    }
}
