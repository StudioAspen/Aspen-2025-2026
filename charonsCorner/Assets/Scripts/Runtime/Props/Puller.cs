using UnityEngine;
using CharonsCorner.Runtime;
using DG.Tweening;

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
    [Header("Visual Pulse")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private float minPulseScale = 0.8f;
    [SerializeField] private float maxPulseScale = 1.3f;
    [SerializeField] private float minPulseDuration = 0.08f;
    [SerializeField] private float maxPulseDuration = 0.18f;
    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private SphereCollider sphereCollider;
    private readonly System.Collections.Generic.Dictionary<Collider, float> pullTimers = new();

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = radius;

        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        StartCoroutine(PulseCore());
    }

    private System.Collections.IEnumerator PulseCore()
    {
        while (true)
        {
            float targetScale = Random.Range(minPulseScale, maxPulseScale);
            float duration = Random.Range(minPulseDuration, maxPulseDuration);

            // Tween the local scale of the mesh's transform
            meshRenderer.transform.DOScale(Vector3.one * targetScale, duration)
                .SetEase(Ease.InOutFlash);

            yield return new WaitForSeconds(duration);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        if (!pullTimers.ContainsKey(other))
            pullTimers[other] = 0f;
        pullTimers[other] += Time.deltaTime;

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
            var playerController = other.GetComponent<GameplayPlayerController>();
            /*if (playerController != null && playerController.GroundedSuperState.DriftState.IsDrifting)
            {
                Vector3 boostDir = flatVelocity.sqrMagnitude > 0.01f ? flatVelocity.normalized : playerController.transform.forward;
                float pullerBoost = flatVelocity.magnitude * (slingshotMultiplier - 1f);
                rb.AddForce(boostDir * pullerBoost, ForceMode.VelocityChange);
            }
            else if (playerController != null)
            {
                flatVelocity *= slingshotMultiplier;
                rb.linearVelocity = new Vector3(flatVelocity.x, rb.linearVelocity.y, flatVelocity.z);
            }*/
        }

        pullTimers.Remove(other);
    }

    private void OnDrawGizmosSelected()
    {
        if (!debug) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}