using CharonsCorner.LevelEditor;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Sets the CinemachineOrbitalFollow's horizontal axis to face the closest spline's
    /// travel direction at the player's current world position.
    /// </summary>
    public class PlayerForwardCameraDirectionHandler : MonoBehaviour
    {
        [SerializeField, Required] private GameplayPlayerController _player;
        [SerializeField, Required] private CinemachineOrbitalFollow _orbitalFollow;
        [SerializeField, Required] private SplinePathDirection _splinePathDirection;
        
        [Header("Animation Settings")]
        [SerializeField] private float _duration = 0.5f;

        [Button("Set Camera Direction", ButtonSizes.Large)]
        public void SetCameraDirection()
        {
            Vector3 splineTravelDirection = _splinePathDirection.GetTravelDirectionAtPosition(
                _player.transform.position
            );

            if (splineTravelDirection.sqrMagnitude < 0.001f)
            {
                Debug.LogWarning("[PlayerForwardCameraDirectionHandler] Could not determine spline travel direction.");
                return;
            }

            // Compute the world-space yaw (degrees) of the spline's travel direction.
            float targetYaw = Mathf.Atan2(splineTravelDirection.x, splineTravelDirection.z) * Mathf.Rad2Deg;

            _orbitalFollow.HorizontalAxis.Value = targetYaw;
            _orbitalFollow.VerticalAxis.Value = 0f;
        }
    }
}