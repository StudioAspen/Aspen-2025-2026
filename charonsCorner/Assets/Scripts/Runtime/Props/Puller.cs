using UnityEngine;
using CharonsCorner.Runtime;

[RequireComponent(typeof(SphereCollider))]
public class Puller : MonoBehaviour
{
    [Header("Puller Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float curveStrength = 15f;
    [SerializeField] private float centripetalStrength = 8f;
    [SerializeField] private float slingshotMultiplier = 2f;
    [SerializeField] private float minSlingshotSpeed = 8f;
    [SerializeField] private float maxPullTime = 2.5f;
    [SerializeField] private LayerMask playerLayer;
    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private SphereCollider sphereCollider;
    private readonly System.Collections.Generic.Dictionary<Collider, float> pullTimers = new();

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

        // Track time spent in puller
        if (!pullTimers.ContainsKey(other))
            pullTimers[other] = 0f;
        pullTimers[other] += Time.deltaTime;

        // Only apply forces if under maxPullTime
        if (pullTimers[other] <= maxPullTime)
        {
            Vector3 toCenter = (transform.position - rb.position);
            Vector3 velocity = rb.linearVelocity;
            toCenter.y = 0;
            velocity.y = 0;

            if (velocity.sqrMagnitude < 0.01f) return;

            Vector3 curveDir = Vector3.Cross(velocity.normalized, toCenter.normalized);
            curveDir.y = 0;
            curveDir = curveDir.normalized;

            rb.AddForce(toCenter.normalized * centripetalStrength, ForceMode.Acceleration);
            float speedFactor = Mathf.Clamp01(velocity.magnitude / 20f);
            rb.AddForce(curveDir * curveStrength * speedFactor, ForceMode.Acceleration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        Vector3 flatVelocity = rb.linearVelocity;
        flatVelocity.y = 0;
        if (flatVelocity.magnitude >= minSlingshotSpeed)
        {
            var playerController = other.GetComponent<PlayerController>();
            if (playerController != null && playerController.GroundedSuperState.DriftState.IsDrifting)
            {
                // Apply slingshot boost directly to velocity while drifting
                Vector3 boostDir = flatVelocity.sqrMagnitude > 0.01f ? flatVelocity.normalized : playerController.transform.forward;
                float pullerBoost = flatVelocity.magnitude * (slingshotMultiplier - 1f);
                rb.AddForce(boostDir * pullerBoost, ForceMode.VelocityChange);
            }
            else if (playerController != null)
            {
                // If not drifting, multiply velocity directly
                flatVelocity *= slingshotMultiplier;
                rb.linearVelocity = new Vector3(flatVelocity.x, rb.linearVelocity.y, flatVelocity.z);
            }
        }

        // Remove timer entry
        pullTimers.Remove(other);
    }

    private void OnDrawGizmosSelected()
    {
        if (!debug) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}