using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineOrbitalFollow _cinemachineOrbitalFollow;
        [SerializeField] private Rigidbody _playerRigidBody;
        [SerializeField, Range(0f, 20f)] private float _rotationSpeed = 5f;
        [SerializeField, Range(0f, 0.1f)] private float _minVelocityThreshold = 0.05f;

        private void LateUpdate()
        {
            Vector3 velocity = _playerRigidBody.linearVelocity;
            velocity.y = 0f; // Ignore vertical motion for camera rotation

            if (velocity.sqrMagnitude > _minVelocityThreshold * _minVelocityThreshold)
            {
                // Calculate angle relative to world forward
                float targetAngle = Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;

                // Smoothly rotate camera horizontal axis
                float currentAngle = _cinemachineOrbitalFollow.HorizontalAxis.Value;
                float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * _rotationSpeed);

                _cinemachineOrbitalFollow.HorizontalAxis.Value = newAngle;
            }
        }
    }
}