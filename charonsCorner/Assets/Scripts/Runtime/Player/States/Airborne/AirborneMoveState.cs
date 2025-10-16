using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirborneMoveState : State<PlayerController>
    {
        [field: SerializeField] public float Damp { get; private set; } = 0.02f;
        [field: SerializeField] public float airMoveForce { get; private set; } = 6f;
        [field: SerializeField] public float airControlMultiplier { get; private set; } = 0.3f;
        [field: SerializeField] public float waterMoveMultiplier { get; private set; } = 1f;

        private protected override void OnEnter() { }
        private protected override void OnExit() { }
        private protected override void OnUpdate() { }

        private protected override void OnFixedUpdate()
        {
            Vector2 moveInput = context.Input.MoveDirection;
            Vector3 moveDir = Vector3.zero;

            if (moveInput != Vector2.zero)
            {
                // Use camera-based movement direction for consistency
                moveDir = Utilities.GetCameraBasedMoveInput(
                    CameraManager.Instance.CurrentCamera.transform,
                    moveInput
                );
                moveDir.Normalize();

                // Check if player is in water (optional)
                bool inWater = false;
#if WATER_SUPPORT
                inWater = context.IsInWater;
#endif
                float moveMultiplier = inWater ? waterMoveMultiplier : airControlMultiplier;
                context.RigidBody.AddForce(moveDir * airMoveForce * moveMultiplier, ForceMode.Acceleration);
            }
            else
            {
                // Only dampen horizontal velocity when no input
                Vector3 velocity = context.RigidBody.linearVelocity;
                velocity.x *= 1f - Damp;
                velocity.z *= 1f - Damp;
                context.RigidBody.linearVelocity = velocity;
            }
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.Input.MoveDirection == Vector2.zero)
                return context.AirborneSuperState.IdleState;

            return null;
        }
    }
}