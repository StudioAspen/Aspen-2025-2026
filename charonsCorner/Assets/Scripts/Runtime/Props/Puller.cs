using CharonsCorner.Runtime;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Puller : MonoBehaviour
{
    [Header("Puller Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float curveStrength = 15f;
    [SerializeField] private float centripetalStrength = 8f;
    [SerializeField] private float slingshotMultiplier = 2f;
    [SerializeField] private LayerMask playerLayer;
    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private SphereCollider sphereCollider;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = radius;
    }

    private void OnTriggerStay(Collider other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        Vector3 toCenter = (transform.position - rb.position);
        Vector3 velocity = rb.linearVelocity;

        // Calculate the perpendicular direction to curve around the sphere
        Vector3 curveDir = Vector3.Cross(velocity.normalized, toCenter.normalized).normalized;

        // Apply centripetal force to keep the player from flying away
        rb.AddForce(toCenter.normalized * centripetalStrength, ForceMode.Acceleration);

        // Apply curve force to make the player orbit around the sphere
        rb.AddForce(curveDir * curveStrength, ForceMode.Acceleration);
    }

    private void OnTriggerExit(Collider other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        // Apply slingshot effect by multiplying velocity when exiting
        rb.linearVelocity *= slingshotMultiplier;

        // Try to get the player's movement state and temporarily raise speed cap
        var playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Example: set the cap to the new velocity magnitude for 1 second
            float newSpeedCap = rb.linearVelocity.magnitude;
            playerController.GroundedSuperState.MoveState.ActivateTemporaryMaxSpeed(newSpeedCap, 1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!debug) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * (radius + 0.5f),
            $"Puller\nRadius: {radius}\nCurve: {curveStrength}\nCentripetal: {centripetalStrength}\nSlingshot: {slingshotMultiplier}");
#endif
    }
}