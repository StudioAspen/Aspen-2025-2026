using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class Raycaster : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public float rayLength = 10f;

        void Start()
        {
            // Get or add a LineRenderer component
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            // Configure the LineRenderer
            lineRenderer.positionCount = 2; // Start and End point
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Or a custom material
            lineRenderer.startColor = Color.blue;
            lineRenderer.endColor = Color.blue;
        }

        void Update()
        {
            Vector3 rayOrigin = transform.position;
            Vector3 rayEnd = rayOrigin - transform.forward * rayLength;

            // Set the positions of the LineRenderer
            lineRenderer.SetPosition(0, rayOrigin);
            lineRenderer.SetPosition(1, rayEnd);

            // Example: If a raycast hits, adjust the end point
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, -transform.forward, out hit, rayLength))
            {
                lineRenderer.SetPosition(1, hit.point);
            }
        }
    }
}
