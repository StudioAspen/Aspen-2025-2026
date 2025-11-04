using System.Collections;
using UnityEngine;

public class SentientPin : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float DetectionRange = 10f;

    [Header("Jump Settings")]
    [SerializeField] private int TotalJumps = 5;
    [SerializeField] private float JumpDistance = 5f;
    [SerializeField] private float JumpHeight = 5f;
    [SerializeField] private float JumpDuration = 1.2f;
    [SerializeField] private float TimeBetweenJumps = 1.5f;

    [Header("Rotation Settings")]
    [SerializeField] private int maxRotationAttempts = 4;
    [SerializeField] private float rotationStepDegrees = 30f;

    [Header("Offset Settings")]
    [SerializeField] private float groundOffset = 0.5f;
    [SerializeField] private float raycastStartHeight = 10f;
    [Header("End Behavior")]
    [SerializeField] private float delayBeforeFinalJump = 2f;
    [SerializeField] private float collisionRange = 2f;
    [SerializeField] private float finalJumpDistance = 30f;

    [Header("Layer Mask")]
    [SerializeField] private LayerMask groundLayerMask;


    private Rigidbody PinRigidBody;
    private Transform player;
    private bool isPlayerInRange = false;
    private bool isJumping = false;
    private Coroutine jumpCoroutine;
    private Coroutine idleCoroutine;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        PinRigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= DetectionRange || isPlayerInRange)
            PlayerDetected();

        if (distanceToPlayer <= collisionRange)
        {
            StopAllCoroutines();
            StartCoroutine(CollisionSequence());
        }

    }

    void PlayerDetected()
    {
        isPlayerInRange = true;

        if (!isJumping && jumpCoroutine == null)
            jumpCoroutine = StartCoroutine(JumpSequence());
    }

    
    IEnumerator CollisionSequence()
    {
        PinRigidBody.isKinematic = false;
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);

    }
    
    /// <summary>
    /// starts jump sequence for pin 
    /// </summary>
    IEnumerator JumpSequence()
    {
        float currentJumps = 0f;
        isJumping = true;

        while (isPlayerInRange && currentJumps < TotalJumps)
        {
            Vector3 jumpDirection = (transform.position - player.position).normalized;
            jumpDirection.y = 0;
            jumpDirection.Normalize();

            bool foundValidTarget = false;

            for (int i = 0; i < maxRotationAttempts; i++)
            {
                Vector3 proposedTarget = transform.position + jumpDirection * JumpDistance;

                if (FindValidJumpPosition(proposedTarget, out Vector3 targetPosition))
                {
                    foundValidTarget = true;
                    currentJumps++;

                    transform.LookAt(targetPosition);

                    // Perform the jump
                    yield return StartCoroutine(JumpToTarget(transform.position, targetPosition));

                    // Pause between jumps
                    yield return new WaitForSeconds(TimeBetweenJumps);
                    break;
                }

                jumpDirection = Quaternion.Euler(0, rotationStepDegrees, 0) * jumpDirection;
                

                if (!foundValidTarget)
                {
                    Debug.LogWarning("SentientPin: No valid jump target found after rotating. Retrying...");
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        isJumping = false;
        jumpCoroutine = null;

        if (currentJumps >= TotalJumps)
            StartCoroutine(FinalJumpAndExit());
    }

    /// <summary>
    /// Math for Jumping to target using StartPos and EndPos
    /// </summary>
    /// <param name="startPos">starting position of pin</param>
    /// <param name="endPos">target Position</param>
    /// <returns></returns>
    IEnumerator JumpToTarget(Vector3 startPos, Vector3 endPos)
    {
        float elapsed = 0f;
        while (elapsed < JumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / JumpDuration);

            float height = Mathf.Sin(t * Mathf.PI) * JumpHeight;
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            pos.y += height;

            transform.position = pos;
            yield return null;
        }

        transform.position = endPos;
    }

    /// <summary>
    /// using a raycast is it even able to jump there
    /// </summary>
    /// <param name="proposedPosition">potential jump position</param>
    /// <param name="validPosition">returns valid position</param>
    /// <returns></returns>
    bool FindValidJumpPosition(Vector3 proposedPosition, out Vector3 validPosition)
    {
        validPosition = proposedPosition;
        Vector3 rayStart = proposedPosition + Vector3.up * raycastStartHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastStartHeight + 10f, groundLayerMask))
        {
            validPosition = hit.point + Vector3.up * groundOffset;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Final Jump needed for the pin to "die" but just disables for re use
    /// </summary>
    /// <returns></returns>
    IEnumerator FinalJumpAndExit()
    {
        yield return new WaitForSeconds(delayBeforeFinalJump);
        isJumping = true;

        Vector3 startPos = transform.position;
        Vector3 randomDirection = new Vector3
        (
            Random.Range(-1f, 1f),
            0,
            Random.Range(1f, -1f)
        ).normalized;

        Vector3 endPos = startPos + randomDirection * finalJumpDistance;

        yield return StartCoroutine(JumpToTarget(startPos, endPos));
        isJumping = false;
        gameObject.SetActive(false);
        
    }
    // void OnCollisionEnter(Collision collision)
    // {
    //     if (collision.gameObject.layer == 0)
    //     {
    //         gameObject.AddComponent<Rigidbody>();
    //     }
    // }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
    }

}
