using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedDriftState : State<PlayerController>
    {
        [SerializeField] private CinemachineOrbitalFollow orbitalCamera;
        [SerializeField] private float acceleration = 2f;
        [SerializeField] private float driftAngleAdjustSpeed = 50f;
        [SerializeField] private float maxDriftAngle = 90f;
        [SerializeField] private float driftVisualRotationMultiplier = 2f;
        [SerializeField] private bool useCamera = true;
        private float initialSpeed;
        private float currentDriftAngle;
        private float driftDirectionSign;
        private float driftTimer;

        public bool IsDrifting { get; private set; }

        private protected override void OnEnter()
        {
            IsDrifting = true;
            driftTimer = 0f;

            driftDirectionSign = 0f;

            currentDriftAngle = 0f;
            initialSpeed = context.RigidBody.linearVelocity.magnitude;
        }

        private protected override void OnExit()
        {
            IsDrifting = false;

            context.RigidBody.isKinematic = false;
        }

        private protected override void OnUpdate()
        {
            // First direction chosen will be the drift direction
            if(driftDirectionSign == 0)
                driftDirectionSign = Mathf.Abs(context.Input.MoveDirection.x) > 0 ? Mathf.Sign(context.Input.MoveDirection.x) : 0;

            driftTimer += Time.deltaTime;

            float driftAngleDelta = driftAngleAdjustSpeed * context.Input.MoveDirection.x * Time.deltaTime;
            if (driftDirectionSign > 0)
                currentDriftAngle = Mathf.Clamp(currentDriftAngle + driftAngleDelta, 0, maxDriftAngle);
            else if(driftDirectionSign < 0)
                currentDriftAngle = Mathf.Clamp(currentDriftAngle + driftAngleDelta, -maxDriftAngle, 0);

            RotateCamera();
        }

        private protected override void OnFixedUpdate()
        {
            Vector3 desiredDirection = Quaternion.Euler(0f, currentDriftAngle, 0f) * context.RigidBody.linearVelocity;
            Vector3 torque = acceleration * Vector3.Cross(Vector3.up, desiredDirection.normalized);

            context.RigidBody.AddTorque(torque, ForceMode.VelocityChange);

            // To prevent y-axis spin
            context.RigidBody.angularVelocity = Vector3.ProjectOnPlane(context.RigidBody.angularVelocity, Vector3.up);

            // Cap final speed
            if (context.RigidBody.linearVelocity.magnitude > initialSpeed)
                context.RigidBody.linearVelocity = Vector3.ClampMagnitude(context.RigidBody.linearVelocity, initialSpeed);

            Vector3 rotationAxis = GetRotationAxisFromDirection(desiredDirection);
            context.VisualObject.transform.Rotate(rotationAxis, driftVisualRotationMultiplier * context.RigidBody.angularVelocity.magnitude, Space.World);

            if(useCamera)
                orbitalCamera.HorizontalAxis.Value += acceleration * currentDriftAngle * Time.fixedDeltaTime;
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (!context.Input.InputActions.Player.Drift.IsPressed())
                return context.GroundedSuperState.IdleState;

            return null;
        }

        private Vector3 GetRotationAxisFromDirection(Vector3 direction) => -Vector3.Cross(direction.normalized, Vector3.up);

        private void RotateCamera()
        {
            if (!useCamera)
                return;

            Vector3 flatVel = context.RigidBody.linearVelocity.WithY(0).normalized;
            float targetYaw = Mathf.Atan2(flatVel.x, flatVel.z) * Mathf.Rad2Deg;

            float current = orbitalCamera.HorizontalAxis.Value;
            float newValue = Mathf.LerpAngle(current, targetYaw, Time.deltaTime);
            orbitalCamera.HorizontalAxis.Value = newValue;
        }
    }
}
