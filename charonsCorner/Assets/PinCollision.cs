using UnityEngine;
using CharonsCorner.Runtime;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
public class PinCollision : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField] private float _impulseMagnitude = 10f;
    [SerializeField] private float _secondsToSubtract = 1f;
    [SerializeField] private float _secondsToSubtractPinHit = 0.5f;
    [SerializeField] private float _playerSpeedBoost = 5f;
    [SerializeField] private MMF_Player _hitFeedback;
    [SerializeField] private MMF_Player _pinOnPinHitFeedback;

    [Header("Glow Settings")]
    [SerializeField] private string _glowPropertyName = "_Glow";
    [SerializeField] private float _glowDistanceThreshold = 10f;
    [SerializeField] private float _minGlow = 0f;
    [SerializeField] private float _maxGlow = 1f;
    [SerializeField] private float _hitGlow = 2f;
    [SerializeField] private float _impactGlow = 5f;
    [SerializeField] private float _impactGlowDuration = 0.5f;
    [SerializeField] private Renderer _renderer;

    [Header("Reset (Testing Only)")]
    [SerializeField] private Transform _resetTarget;

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

        // Ensure Rigidbody is set up for initial state if starting from scratch
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.useGravity = false; // Start without gravity until hit, common for pins
    }

    [Button]
    public void ResetPin()
    {
        if (_resetTarget != null)
        {
            transform.position = _resetTarget.position;
            transform.rotation = _resetTarget.rotation;
        }

        _hasBeenHit = false;
        
        if (_impactGlowCoroutine != null)
        {
            StopCoroutine(_impactGlowCoroutine);
            _impactGlowCoroutine = null;
        }
        
        if (_rb != null)
        {
            _rb.useGravity = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.Sleep();
        }

        if (_hitFeedback != null)
        {
            _hitFeedback.StopFeedbacks();
        }

        if (_pinOnPinHitFeedback != null)
        {
            _pinOnPinHitFeedback.StopFeedbacks();
        }

        // Reset glow
        if (_renderer != null && _propBlock != null)
        {
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(_glowPropId, _minGlow);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }

    private void Update()
    {
        // Debug reset via Numpad 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ResetPin();
        }

        if (_hasBeenHit || _player == null) return;

        UpdateGlow();
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
        else if (collision.gameObject.TryGetComponent<PinCollision>(out var otherPin))
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
