using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TeleportBumper : MonoBehaviour
    {
        [Header("Destination Parameters")]
        [SerializeField] private Transform _teleportDestination;
        [SerializeField] private Color gizmoColor = Color.blue;

        [Header("Post-Destination Boost Parameters")]
        [SerializeField] private float _boostSpeedMultiplier = 2;
        [SerializeField] private float _boostSpeedDuration = 2;

        public Transform GetTeleportDestination() => _teleportDestination;
        public float GetBoostSpeedMultiplier() => _boostSpeedMultiplier;
        public float GetBoostSpeedDuration() => _boostSpeedDuration;


        private void OnDrawGizmos()
        {
            const float GIZMO_RADIUS = 1.0f;

            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(_teleportDestination.position, GIZMO_RADIUS);
            Gizmos.DrawRay(_teleportDestination.position, Vector3.back * 5f); 
        }

    }
}
