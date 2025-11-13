using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TeleportBumper : MonoBehaviour
    {
        [Header("Destination Parameters")]
        public Transform teleportDestination;
        public Color gizmoColor = Color.blue;        

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(teleportDestination.position, 1.0f);
        }
    }
}
