using System.Collections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    
public class SentientPin : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float _detectionRange = 10f;

    [Header("Jump Settings")]
    [SerializeField] private int _totalJumps = 5;
    [SerializeField] private float _jumpDistance = 5f;
    [SerializeField] private float _jumpHeight = 5f;
    [SerializeField] private float _jumpDuration = 1.2f;
    [SerializeField] private float _jumpBufferTimer = 0.3f;
    [SerializeField] private float _rangeOfRaycast = 30f;

    [Header("Rotation Settings")]
    [SerializeField] private int _maxRotationAttempts = 4;
    [SerializeField] private float _rotationStepDegrees = 30f;

    [Header("Offset Settings")]
    [SerializeField] private float _groundOffset = 0.5f;
    [SerializeField] private float _raycastStartHeight = 10f;

    [Header("End Behavior")]
    [SerializeField] private float _delayBeforeFinalJump = 2f;
    [SerializeField] private float _collisionRange = 2f;
    [SerializeField] private float _finalJumpDistance = 30f;

    [Header("Layer Mask")]
    [SerializeField] private LayerMask _groundLayerMask;


    private float TimeBetweenJumps => _jumpDuration + _jumpBufferTimer;

    private Rigidbody _pinRigidBody;
    private Transform _player;
    private bool _isJumping = false;
    private Coroutine _jumpCoroutine;

    private int _consecutiveFailedJumps = 0;
    private int _maxFailedJumpAttempts = 2;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _pinRigidBody = GetComponent<Rigidbody>();
        _pinRigidBody.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        if (distanceToPlayer <= _detectionRange && !_isJumping)
        {
            _jumpCoroutine = StartCoroutine(JumpSequence());
        }

        if (distanceToPlayer <= _collisionRange)
        {
            StopAllCoroutines();
            StartCoroutine(CollisionSequence());
        }
    }

    private IEnumerator CollisionSequence()
    {
        _pinRigidBody.isKinematic = false;
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }


    /// <summary>
    /// starts jump sequence for pin 
    /// </summary>
    private IEnumerator JumpSequence()
    {
        _isJumping = true;
        int currentJumps = 0;

        while (currentJumps < _totalJumps)
        {
            Vector3 jumpDirection = (transform.position - _player.position).normalized;
            jumpDirection.y = 0;

            if (TryDirectJump(jumpDirection, ref currentJumps) ||
                TryJumpInRotation(jumpDirection, _rotationStepDegrees, ref currentJumps) ||
                TryJumpInRotation(jumpDirection, -_rotationStepDegrees, ref currentJumps))
            {
                _consecutiveFailedJumps = 0;
                yield return new WaitForSeconds(TimeBetweenJumps);
            }
            else
            {
                _consecutiveFailedJumps++;
                if (_consecutiveFailedJumps >= _maxFailedJumpAttempts)
                {
                    Debug.LogWarning("SentientPin: No valid jump target found after all attempts. Retrying...");
                        
                    StartCoroutine(FinalJumpAndExit());
                    break;
                    // yield return new WaitForSeconds(0.75f);
                }
            }
        }

        _isJumping = false;
        _jumpCoroutine = null;
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
        Vector3 directTarget = transform.position + direction * _jumpDistance;
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

        for (int i = 0; i < _maxRotationAttempts; i++)
        {
            jumpDirection = Quaternion.Euler(0, rotationStep * i, 0) * initialDirection;
            Vector3 proposedTarget = transform.position + jumpDirection.normalized * _jumpDistance;

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
        while (elapsed < _jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _jumpDuration);

            float height = Mathf.Sin(t * Mathf.PI) * _jumpHeight;
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
        Vector3 rayStart = proposedPosition + Vector3.up * _raycastStartHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, _raycastStartHeight + 30f, _groundLayerMask))
        {
            validPosition = hit.point + Vector3.up * _groundOffset;
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
        yield return new WaitForSeconds(_delayBeforeFinalJump);

        _pinRigidBody.useGravity = true;
        _isJumping = true;

        Vector3 startPos = transform.position;
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        Vector3 endPos = startPos + randomDirection * _finalJumpDistance;

        yield return StartCoroutine(JumpToTarget(startPos, endPos));
        _isJumping = false;
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
    }
}
}
