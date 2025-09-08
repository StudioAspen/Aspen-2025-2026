using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirborneDropDashState : State<PlayerController>
    {
        [SerializeField] private float dropForce = 10f;
        [SerializeField] private float boostForce = 15f;
        
        private protected override void OnEnter()
        {
            context.RigidBody.AddForce(dropForce * Vector3.down, ForceMode.VelocityChange);
        }

        private protected override void OnExit()
        {
            float cameraY = 0;
            if(CameraManager.Instance != null)
                cameraY = CameraManager.Instance.transform.rotation.eulerAngles.y;

            Vector3 boostDirection = Quaternion.Euler(0f, cameraY, 0f) * Vector3.forward;

            context.RigidBody.AddForce(boostForce * boostDirection - context.RigidBody.linearVelocity, ForceMode.VelocityChange);
        }

        private protected override void OnUpdate()
        {

        }

        private protected override void OnFixedUpdate()
        {

        }
    }
}