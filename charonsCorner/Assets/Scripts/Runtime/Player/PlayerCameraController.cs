using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineOrbitalFollow _cinemachineOrbitalFollow;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private float _rotationSpeed = 5f;
        [SerializeField] private float _minVelocityThreshold = 0.05f;

        private void LateUpdate()
        {
            Vector3 velocity = _playerController.CurrentMovement;
            if (velocity.sqrMagnitude > _minVelocityThreshold * _minVelocityThreshold)
            {
                // horizontal
                float targetYaw = Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;
                float currentYaw = _cinemachineOrbitalFollow.HorizontalAxis.Value;
                float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * _rotationSpeed);
                _cinemachineOrbitalFollow.HorizontalAxis.Value = newYaw;

                // vertical
                // Project velocity on XZ to get horizontal magnitude
                float horizontalMag = new Vector2(velocity.x, velocity.z).magnitude;
                float targetPitch = -Mathf.Atan2(velocity.y, horizontalMag) * Mathf.Rad2Deg; 
                // negative so upward velocity tilts camera up
                float currentPitch = _cinemachineOrbitalFollow.VerticalAxis.Value;
                float newPitch = Mathf.LerpAngle(currentPitch, targetPitch, Time.deltaTime * _rotationSpeed);
                _cinemachineOrbitalFollow.VerticalAxis.Value = newPitch;
            }
        }
    }
}