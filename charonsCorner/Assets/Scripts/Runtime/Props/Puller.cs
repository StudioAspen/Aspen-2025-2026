using CharonsCorner.Runtime;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class Puller : MonoBehaviour
{
    [Header("Puller Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float curveStrength = 15f;
    [SerializeField] private float centripetalStrength = 8f;
    [SerializeField] private float slingshotMultiplier = 2f;
    [SerializeField] private float minSlingshotSpeed = 8f; // Only boost if above this speed
    [SerializeField] private float maxPullTime = 2.5f; // Max seconds to apply pull/curve
    [SerializeField] private LayerMask playerLayer;
    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private SphereCollider sphereCollider;
    private Dictionary<Collider, float> pullTimers = new();

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

            // Project toCenter and velocity onto XZ plane
            toCenter.y = 0;
            velocity.y = 0;

            if (velocity.sqrMagnitude < 0.01f) return; // Avoid division by zero

            // Calculate the perpendicular direction to curve around the sphere (XZ only)
            Vector3 curveDir = Vector3.Cross(velocity.normalized, toCenter.normalized);
            curveDir.y = 0;
            curveDir = curveDir.normalized;

            // Apply centripetal force to keep the player from flying away (XZ only)
            rb.AddForce(toCenter.normalized * centripetalStrength, ForceMode.Acceleration);

            // Apply curve force, scaled by speed for more effect at higher speeds (XZ only)
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
                // Add the extra boost to the drift state for stacking
                float pullerBoost = flatVelocity.magnitude * (slingshotMultiplier - 1f);
                playerController.GroundedSuperState.DriftState.AddExternalBoost(pullerBoost);
            }
            else if (playerController != null)
            {
                // If not drifting, multiply velocity directly
                flatVelocity *= slingshotMultiplier;
                rb.linearVelocity = new Vector3(flatVelocity.x, rb.linearVelocity.y, flatVelocity.z);
            }
        }

        // Remove timer entry
        if (pullTimers.ContainsKey(other))
            pullTimers.Remove(other);
    }

    private void OnDrawGizmosSelected()
    {
        if (!debug) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * (radius + 0.5f),
            $"Puller\nRadius: {radius}\nCurve: {curveStrength}\nCentripetal: {centripetalStrength}\nSlingshot: {slingshotMultiplier}\nMinSpeed: {minSlingshotSpeed}\nMaxPullTime: {maxPullTime}");
#endif
    }
}