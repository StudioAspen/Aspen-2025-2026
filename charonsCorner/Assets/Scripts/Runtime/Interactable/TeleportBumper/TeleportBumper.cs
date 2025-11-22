using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TeleportBumper : MonoBehaviour
    {
        [Header("Destination Parameters")]
        [SerializeField] private Transform _teleportDestination;
        [SerializeField] private Color _gizmoColor = Color.blue;

        [Header("Post-Destination Boost Parameters")]
        [SerializeField] private float _boostSpeedMultiplier = 2;

        public Transform GetTeleportDestination() => _teleportDestination;
        public float GetBoostSpeedMultiplier() => _boostSpeedMultiplier;

        private void OnDrawGizmos()
        {
            if (_teleportDestination == null)
            {
                Debug.LogError("Missing teleport destination object");
                return;
            }

            const float GIZMO_RADIUS = 1.0f;

            Gizmos.color = _gizmoColor;
            Gizmos.DrawSphere(_teleportDestination.position, GIZMO_RADIUS);
            Gizmos.DrawRay(_teleportDestination.position, Vector3.back * 5f); 
        }

    }
}
