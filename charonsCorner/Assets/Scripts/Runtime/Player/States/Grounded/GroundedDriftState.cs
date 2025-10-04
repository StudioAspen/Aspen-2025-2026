using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedDriftState : State<PlayerController>
    {
        [field: SerializeField] public float AngleOffset { get; private set; } = 20f;
        [field: SerializeField] public float TurnResponsiveness { get; private set; } = 1f;
        [field: SerializeField] public float Friction { get; private set; } = 0.9f;
        [field: SerializeField] public float MinDriftBoost { get; private set; } = 5f;
        [field: SerializeField] public float MaxDriftBoost { get; private set; } = 15f;
        [field: SerializeField] public float MaxDriftTime { get; private set; } = 2.5f;
        [field: SerializeField] public float BoostChargeDuration { get; private set; } = 0.5f; // X seconds to charge

        private Vector3 driftDirection;
        [SerializeField, ReadOnly, AllowNesting] private float driftTimer;

        public bool IsDrifting { get; private set; }
        private bool boostCharging = false;
        private float boostChargeTimer = 0f;
        private bool boostReady = false;

        private protected override void OnEnter()
        {
            IsDrifting = true;
            boostCharging = false;
            boostChargeTimer = 0f;
            boostReady = false;
            driftTimer = 0f;

            driftDirection = Utilities.GetCameraBasedMoveInput(
                CameraManager.Instance.CurrentCamera.transform,
                context.Input.MoveDirection
            );

            float angle = AngleOffset * (context.Input.MoveDirection.x > 0 ? 1 : -1);
            driftDirection = Quaternion.AngleAxis(angle, Vector3.up) * driftDirection;
        }

        private protected override void OnExit()
        {
            IsDrifting = false;
            driftTimer = 0f;
            boostCharging = false;
            boostChargeTimer = 0f;
            boostReady = false;
        }

        private protected override void OnUpdate()
        {
            if (!boostCharging)
            {
                driftTimer += Time.deltaTime;
                // Only set boostReady if driftTimer exceeds MaxDriftTime
                if (driftTimer >= MaxDriftTime)
                {
                    boostReady = true;
                }
            }
            else
            {
                boostChargeTimer += Time.deltaTime;
                if (boostChargeTimer >= BoostChargeDuration)
                {
                    // Only apply boost if drift was held long enough
                    if (boostReady)
                    {
                        float boostAmount = Mathf.Lerp(MinDriftBoost, MaxDriftBoost, 1f);
                        Vector3 boost = context.transform.forward * boostAmount;
                        context.RigidBody.AddForce(boost, ForceMode.VelocityChange);
                        context.GroundedSuperState.MoveState.ActivateTemporaryMaxSpeed(boost.magnitude);
                    }
                    // Transition to move state after charge (regardless of boost)
                    stateMachine.ChangeState(context.GroundedSuperState.MoveState);
                }
            }
        }

        private protected override void OnFixedUpdate()
        {
            Vector3 desiredDirection = Utilities.GetCameraBasedMoveInput(
                CameraManager.Instance.CurrentCamera.transform,
                context.Input.MoveDirection
            );

            driftDirection = Vector3.Slerp(driftDirection, desiredDirection, TurnResponsiveness * 0.3f * Time.fixedDeltaTime);

            Vector3 turnTorque = Vector3.Cross(Vector3.up, driftDirection) * TurnResponsiveness;
            context.RigidBody.AddTorque(turnTorque, ForceMode.VelocityChange);

            Vector3 velocity = context.RigidBody.linearVelocity;
            Vector3 lateral = Vector3.ProjectOnPlane(velocity, driftDirection);
            context.RigidBody.linearVelocity = (velocity - lateral) + lateral * Friction;

            // Use MoveState's max speed for drift cap
            float moveMaxSpeed = context.GroundedSuperState.MoveState.MaxSpeed;
            if (context.RigidBody.linearVelocity.magnitude > moveMaxSpeed)
            {
                context.RigidBody.linearVelocity = context.RigidBody.linearVelocity.normalized * moveMaxSpeed;
            }
        }

        private protected override State<PlayerController> GetTransition()
        {
            // If drift button released, start boost charge
            if (!boostCharging && !context.Input.InputActions.Player.Drift.IsPressed())
            {
                boostCharging = true;
                boostChargeTimer = 0f;
                return null; // Stay in this state until boost charge completes
            }
            if (!boostCharging && context.Input.MoveDirection == Vector2.zero)
                return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}