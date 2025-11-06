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
    [SerializeField] private float JumpBufferTimer= 0.3f;
    [SerializeField] private float rangeOfRaycast = 30f;

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


    private float TimeBetweenJumps => JumpDuration + JumpBufferTimer;

    private Rigidbody PinRigidBody;
    private Transform player;
    private bool isJumping = false;
    private Coroutine jumpCoroutine;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        PinRigidBody = GetComponent<Rigidbody>();
        PinRigidBody.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= DetectionRange && !isJumping)
        {
            jumpCoroutine = StartCoroutine(JumpSequence());
        }

        if (distanceToPlayer <= collisionRange)
        {
            StopAllCoroutines();
            StartCoroutine(CollisionSequence());
        }
    }

    private IEnumerator CollisionSequence()
    {
        PinRigidBody.isKinematic = false;
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }


    /// <summary>
    /// starts jump sequence for pin 
    /// </summary>
    private IEnumerator JumpSequence()
    {
        isJumping = true;
        int currentJumps = 0;

        while (currentJumps < TotalJumps)
        {
            Vector3 jumpDirection = (transform.position - player.position).normalized;
            jumpDirection.y = 0;

            if (TryDirectJump(jumpDirection, ref currentJumps) ||
                TryJumpInRotation(jumpDirection, rotationStepDegrees, ref currentJumps) ||
                TryJumpInRotation(jumpDirection, -rotationStepDegrees, ref currentJumps))
            {
                yield return new WaitForSeconds(TimeBetweenJumps);
            }
            else
            {
                Debug.LogWarning("SentientPin: No valid jump target found after all attempts. Retrying...");
                yield return new WaitForSeconds(0.75f);
            }
        }

        isJumping = false;
        jumpCoroutine = null;
        StartCoroutine(FinalJumpAndExit());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="currentJumps"></param>
    /// <returns></returns>
    private bool TryDirectJump(Vector3 direction, ref int currentJumps)
    {
        Vector3 directTarget = transform.position + direction * JumpDistance;
        if (FindValidJumpPosition(directTarget, out Vector3 validTarget))
        {
            currentJumps++;
            transform.LookAt(validTarget);
            StartCoroutine(JumpToTarget(transform.position, validTarget));
            return true;
        }
        return false;
    }


    /// <summary>
    /// attempts a jump using a different direction apart from the default
    /// </summary>
    /// <param name="initialDirection"> default direction</param>
    /// <param name="rotationStep">how much the direction changes by degrees</param>
    /// <param name="currentJumps">how many jumps are still available</param>
    /// <returns></returns>
    private bool TryJumpInRotation(Vector3 initialDirection, float rotationStep, ref int currentJumps)
    {
        Vector3 jumpDirection = initialDirection;

        for (int i = 0; i < maxRotationAttempts; i++)
        {
            jumpDirection = Quaternion.Euler(0, rotationStep * i, 0) * initialDirection;
            Vector3 proposedTarget = transform.position + jumpDirection.normalized * JumpDistance;

            if (FindValidJumpPosition(proposedTarget, out Vector3 validTarget))
            {
                currentJumps++;
                transform.LookAt(validTarget);
                StartCoroutine(JumpToTarget(transform.position, validTarget));
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Math for Jumping to target using StartPos and EndPos
    /// </summary>
    /// <param name="startPos">starting position of pin</param>
    /// <param name="endPos">target Position</param>
    /// <returns></returns>
    private IEnumerator JumpToTarget(Vector3 startPos, Vector3 endPos)
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
    private bool FindValidJumpPosition(Vector3 proposedPosition, out Vector3 validPosition)
    {
        validPosition = proposedPosition;
        Vector3 rayStart = proposedPosition + Vector3.up * raycastStartHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastStartHeight + 30f, groundLayerMask))
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
    private IEnumerator FinalJumpAndExit()
    {
        yield return new WaitForSeconds(delayBeforeFinalJump);

        PinRigidBody.useGravity = true;
        isJumping = true;

        Vector3 startPos = transform.position;
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        Vector3 endPos = startPos + randomDirection * finalJumpDistance;

        yield return StartCoroutine(JumpToTarget(startPos, endPos));
        isJumping = false;
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
    }
}