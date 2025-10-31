using System.Collections;
using UnityEngine;

public class SentientPin : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float DetectionRange = 10f;


    [Header("Jump Settings")]
    [SerializeField] private int TotalJumps = 5;
    private int currentJumps = 0;
    [SerializeField] private float JumpDistance = 5f;
    [SerializeField] private float JumpHeight = 5f;
    [SerializeField] private float JumpDuration = 1.2f;
    [SerializeField] private float TimeBetweenJumps = 1.5f;

    [Header("Layer Mask")]
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Offset Settings")]
    [SerializeField] private float groundOffset = 0.5f;
    [SerializeField] private float raycastStartHeight = 10f;

    [Header("End Behavior")]
    [SerializeField] private bool reuseInsteadOfDestroy = false; // disable instead of destroy
    [SerializeField] private float finalJumpHeight = 15f;
    [SerializeField] private float finalJumpDuration = 2f;

    private Transform player;
    private bool isPlayerInRange = false;
    private bool isJumping = false;

    private Coroutine jumpCoroutine;
    private Coroutine idleCoroutine;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
            Debug.LogError("Player not found! Make sure your player has the 'Player' tag.");
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= DetectionRange && !isPlayerInRange)
            PlayerDetected();
        else if (distanceToPlayer > DetectionRange && isPlayerInRange)
            PlayerLeftRange();
    }

    void PlayerDetected()
    {
        isPlayerInRange = true;
        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }

        if (!isJumping && jumpCoroutine == null)
            jumpCoroutine = StartCoroutine(JumpSequence());
    }

    void PlayerLeftRange()
    {
        isPlayerInRange = false;
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
            jumpCoroutine = null;
        }

        isJumping = false;
        idleCoroutine = StartCoroutine(IdleCountdown());
    }



    /// <summary>
    /// starts jump sequence for pin 
    /// </summary>
    /// <returns></returns>
    IEnumerator JumpSequence()
    {
        isJumping = true;

        while (isPlayerInRange && currentJumps < TotalJumps)
        {
            Vector3 jumpDirection = (transform.position - player.position).normalized;
            jumpDirection.y = 0;
            jumpDirection.Normalize();

            Vector3 proposedTarget = transform.position + jumpDirection * JumpDistance;

            if (FindValidJumpPosition(proposedTarget, out Vector3 targetPosition))
            {
                currentJumps++;

                transform.LookAt(targetPosition);

                // Perform the jump
                yield return StartCoroutine(JumpToTarget(transform.position, targetPosition));

                // Pause between jumps
                yield return new WaitForSeconds(TimeBetweenJumps);
            }
            else
            {
                // Retry after short delay if no valid ground found
                yield return new WaitForSeconds(0.5f);
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
    /// end sequence for pin
    /// not entirely necessary might delete later
    /// </summary>
    /// <returns></returns>
    IEnumerator IdleCountdown()
    {
        yield return new WaitForSeconds(15f);
        StartCoroutine(FinalJumpAndExit());
    }



    IEnumerator FinalJumpAndExit()
    {
        isJumping = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * finalJumpHeight;

        float elapsed = 0f;
        while (elapsed < finalJumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / finalJumpDuration);
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            transform.position = pos;
            yield return null;
        }

        // End of jump: disable or destroy
        if (reuseInsteadOfDestroy)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);

        isJumping = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
    }
}
