using UnityEngine;
using CharonsCorner.Runtime;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
public class PinCollision : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField] private bool _stareAtPlayer = false;
    [SerializeField] private float _impulseMagnitude = 10f;
    [SerializeField] private float _secondsToSubtract = 1f;
    [SerializeField] private float _secondsToSubtractPinHit = 0.5f;
    [SerializeField] private float _playerSpeedBoost = 5f;
    [SerializeField] private bool _canHitOtherPins = true;
    [SerializeField] private MMF_Player _hitFeedback;
    [SerializeField] private MMF_Player _pinOnPinHitFeedback;

    [Header("Destruction")]
    [SerializeField] private bool _destroyOnHit = true;
    [SerializeField] private float _destructionDelay = 5f;
    [SerializeField] private GameObject _objectToDestroy;

    [Header("Glow Settings")]
    [SerializeField] private string _glowPropertyName = "_Glow";
    [SerializeField] private float _glowDistanceThreshold = 10f;
    [SerializeField] private float _minGlow = 0f;
    [SerializeField] private float _maxGlow = 1f;
    [SerializeField] private float _hitGlow = 2f;
    [SerializeField] private float _impactGlow = 5f;
    [SerializeField] private float _impactGlowDuration = 0.5f;
    [SerializeField] private Renderer _renderer;

    [Header("Ground Alignment")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _alignmentRayLength = 2f;
    [SerializeField] private float _snapOffset = 0.01f;

    private Rigidbody _rb;
    private bool _hasBeenHit = false;
    private MaterialPropertyBlock _propBlock;
    private int _glowPropId;
    private GameplayPlayerController _player;
    private Coroutine _impactGlowCoroutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _propBlock = new MaterialPropertyBlock();
        _glowPropId = Shader.PropertyToID(_glowPropertyName);
        
        // Find player - assuming one exists in the scene
        _player = Object.FindAnyObjectByType<GameplayPlayerController>();

        if (_objectToDestroy == null && transform.parent != null)
        {
            _objectToDestroy = transform.parent.gameObject;
        }
    }

    private void Start()
    {
        AlignToGround();
    }

    private void AlignToGround()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, _alignmentRayLength, _groundLayer))
        {
            // Position the pin exactly on the ground
            transform.position = hit.point + hit.normal * _snapOffset;

            // Align the pin's local Z-axis (forward) with the ground normal
            transform.rotation = Quaternion.FromToRotation(transform.forward, hit.normal) * transform.rotation;
        }
    }

    private void Update()
    {
        if (_hasBeenHit || _player == null) return;

        UpdateGlow();
        UpdateStareAtPlayer();
    }

    private void UpdateStareAtPlayer()
    {
        if (!_stareAtPlayer) return;

        Vector3 directionToPlayer = _player.transform.position - transform.position;

        // The user states Z is the vertical axis.
        // We want to rotate around Z so that the reverse of the X vector (-transform.right) looks at the player.
        // Project direction onto the plane perpendicular to the pin's local Z axis.
        Vector3 localDirection = transform.InverseTransformDirection(directionToPlayer);
        localDirection.z = 0; // Lock Z to keep it in the "horizontal" plane of the pin.

        if (localDirection.sqrMagnitude > 0.001f)
        {
            // We want -transform.right to point at the player.
            // In local space, -right is (-1, 0, 0).
            // Atan2(y, x) gives the angle from the local X axis.
            float angle = Mathf.Atan2(localDirection.y, localDirection.x) * Mathf.Rad2Deg;

            // angle is the rotation needed to align local X (1,0,0) with localDirection.
            // We want -X (-1,0,0) to align with localDirection.
            // -X is 180 degrees away from X.
            // So we add 180 to the angle.
            transform.Rotate(0, 0, angle + 180f, Space.Self);
        }
    }

    private void UpdateGlow()
    {
        float distance = Vector3.Distance(transform.position, _player.transform.position);

        if (distance < _glowDistanceThreshold)
        {
            // Map distance [threshold, 0] to glow [_minGlow, _maxGlow]
            float t = 1f - Mathf.Clamp01(distance / _glowDistanceThreshold);
            float currentGlow = Mathf.Lerp(_minGlow, _maxGlow, t);

            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(_glowPropId, currentGlow);
            _renderer.SetPropertyBlock(_propBlock);
        }
        else
        {
            // Reset glow if player moves out of range
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(_glowPropId, _minGlow);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the colliding object is the player
        if (collision.gameObject.TryGetComponent<GameplayPlayerController>(out var player))
        {
            HandleHit(collision, _secondsToSubtract, true);

            // Apply speed boost to player
            if (player.Rb != null)
            {
                Vector3 boostDir = player.Rb.linearVelocity.normalized;
                if (boostDir.sqrMagnitude < 0.0001f) boostDir = player.Orientation.forward;
                player.Rb.AddForce(boostDir * _playerSpeedBoost, ForceMode.VelocityChange);
            }
        }
        // Check if hit by another Pin (which must have been hit already to be moving)
        else if (_canHitOtherPins && collision.gameObject.TryGetComponent<PinCollision>(out var otherPin))
        {
            HandleHit(collision, _secondsToSubtractPinHit, false);
        }
    }

    private void HandleHit(Collision collision, float secondsToSubtract, bool isPlayer)
    {
        if (!_hasBeenHit)
        {
            _hasBeenHit = true;
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.WakeUp();

            // Trigger impact glow
            if (_impactGlowCoroutine != null) StopCoroutine(_impactGlowCoroutine);
            _impactGlowCoroutine = StartCoroutine(ImpactGlowRoutine());

            // Play hit feedback if assigned
            if (isPlayer)
            {
                if (_hitFeedback != null && !_hitFeedback.IsPlaying)
                {
                    _hitFeedback.PlayFeedbacks();
                }
                
                MMGameEvent.Trigger("PowEffect");
            }
            else
            {
                if (_pinOnPinHitFeedback != null && !_pinOnPinHitFeedback.IsPlaying)
                {
                    _pinOnPinHitFeedback.PlayFeedbacks();
                }
            }

            // Subtract time from RankingSystem
            PinScoring.OnPinScored?.Invoke(secondsToSubtract);

            if (_destroyOnHit && _objectToDestroy != null)
            {
                Destroy(_objectToDestroy, _destructionDelay);
            }
        }

        // Get the first contact point
        ContactPoint contact = collision.GetContact(0);

        // contact.normal points towards the object being hit (this pin) if hit by player/other object? 
        // Actually, collision.GetContact(0).normal points FROM the other object TOWARDS this object.
        Vector3 pushDirection = contact.normal;

        // Apply impulse away from the contact point
        _rb.AddForce(pushDirection * _impulseMagnitude, ForceMode.Impulse);
    }

    private System.Collections.IEnumerator ImpactGlowRoutine()
    {
        if (_renderer == null || _propBlock == null) yield break;

        // Set to impact glow
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(_glowPropId, _impactGlow);
        _renderer.SetPropertyBlock(_propBlock);

        yield return new WaitForSeconds(_impactGlowDuration);

        // Transition back to hit glow
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(_glowPropId, _hitGlow);
        _renderer.SetPropertyBlock(_propBlock);
        
        _impactGlowCoroutine = null;
    }
}
