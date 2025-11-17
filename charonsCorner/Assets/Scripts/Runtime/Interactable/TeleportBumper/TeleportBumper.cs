using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TeleportBumper : MonoBehaviour
    {
        [Header("Destination Parameters")]
        [field: SerializeField] public Transform TeleportDestination {get; private set;}
        [field: SerializeField] Color gizmoColor = Color.blue;

        [Header("Post-Destination Boost Parameters")]
        [field: SerializeField] public float BoostSpeedMultiplier { get; private set;}
        [field: SerializeField] public float BoostSpeedDuration { get; private set; }


        private void OnDrawGizmos()
        {
            const float GIZMO_RADIUS = 1.0f;

            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(TeleportDestination.position, GIZMO_RADIUS);
        }

    }
}
