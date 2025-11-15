using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TeleportBumper : MonoBehaviour
    {
        [Header("Destination Parameters")]
        [field: SerializeField] public Transform TeleportDestination {get; private set;}
        [field: SerializeField] Color gizmoColor = Color.blue;



        private void OnDrawGizmos()
        {
            const float GIZMO_RADIUS = 1.0f;

            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(TeleportDestination.position, GIZMO_RADIUS);
        }
    }
}
