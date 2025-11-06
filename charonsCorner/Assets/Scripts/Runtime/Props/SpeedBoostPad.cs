using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class SpeedBoostPad : MonoBehaviour
    {
        [Header("Boost Settings")]
        public Vector3 boostDirection = Vector3.forward;
        public float boostStrength = 30f;
        public LayerMask playerLayerMask;
        public bool setVelocity = false; // If true, sets velocity instead of adding force

        [Header("Debug")]
        public bool debugDrawGizmo = true;
        public float gizmoLength = 2f;
        public Color gizmoColor = Color.cyan;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & playerLayerMask.value) != 0)
            {
                var player = other.GetComponent<GameplayPlayerController>();
                if (player != null && player.Rb != null)
                {

                    Vector3 worldBoostDir = transform.TransformDirection(boostDirection.normalized);

                    if (setVelocity)
                    {
                        // Set velocity directly
                        player.Rb.linearVelocity = worldBoostDir * boostStrength;
                    }
                    else
                    {
                        // Add force for boost
                        player.Rb.AddForce(worldBoostDir * boostStrength, ForceMode.VelocityChange);
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!debugDrawGizmo) return;

            Gizmos.color = gizmoColor;
            Vector3 origin = transform.position;
            Vector3 dir = transform.TransformDirection(boostDirection.normalized) * gizmoLength;
            Gizmos.DrawLine(origin, origin + dir);

            // Draw arrow head
            Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, -150, 0) * Vector3.forward;
            Gizmos.DrawLine(origin + dir, origin + dir + right * 0.3f * gizmoLength);
            Gizmos.DrawLine(origin + dir, origin + dir + left * 0.3f * gizmoLength);
        }
    }
}