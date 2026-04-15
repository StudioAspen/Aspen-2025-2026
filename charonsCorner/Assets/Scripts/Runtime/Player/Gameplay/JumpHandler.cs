using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(GameplayPlayerController))]
    [RequireComponent(typeof(AudioSource))]
    public class JumpHandler : MonoBehaviour
    {   
        [Header("Landing Particle Setup")]
        // How long the player should be in the air before particle system triggers
        [SerializeField] private ParticleCloudPlayer _particleCloudPlayer;

        private GameplayPlayerController _player;
        private AudioSource _audioSource;

        [Header("Jump Settings")]
        [Tooltip("Force applied when jump is initiated")]
        [SerializeField] private float _jumpForce = 10f;
        [Tooltip("Grace period to jump after not being grounded")]
        [SerializeField] private float _coyoteTimeDuration = 0.5f;
        [Tooltip("Scale down upward velocity on early release by this value")]
        [SerializeField, Range(0f, 1f)] private float _earlyReleaseMultiplier = 0.5f;
        private bool _hasJumped;
        private bool _jumpIsHeld;

        [Header("Bounce Settings")]
        [SerializeField] private bool _enableBounce = true;
        [Tooltip("Minimum downward velocity to trigger bounce effect")]
        [SerializeField] private float _bounceThreshold = 1f; // e.g., only bounce if falling faster than 1 unit/s
        [Tooltip("Multiplier for bounce velocity")]
        [SerializeField] private float _bounceMultiplier = 0.5f; // e.g., bounce back up at 50% of impact velocity
        [Tooltip("Maximum upward velocity after bounce")]
        [SerializeField] private float _maxBounceVelocity = 20f; // cap the maximum bounce velocity
        
        [Header("Debug")]
        [SerializeField] private bool _showDebug = false;
        
        [Header("Audio Settings")]
        [SerializeField] private AudioClip _jumpSound;
        [SerializeField] private AudioClip _bounceSound;

        public bool HasJumped => _hasJumped;
        public bool JumpIsHeld => _jumpIsHeld;
        public float EarlyReleaseMultiplier => _earlyReleaseMultiplier;
        
        void Awake()
        {
            _player = GetComponent<GameplayPlayerController>();
            _particleCloudPlayer = GetComponent<ParticleCloudPlayer>();
            _audioSource = GetComponent<AudioSource>();
        }

        private float _lastJumpTime;
        [SerializeField, Tooltip("Cooldown before _hasJumped can be reset to false after a jump. Prevents double jump if IsGrounded is still true for a frame.")] 
        private float _jumpGroundedResetDelay = 0.15f;

        private void FixedUpdate()
        {
            // Reset _hasJumped when grounded and enough time has passed since the last jump.
            // On slopes, the upward velocity might be positive when moving uphill (due to gravity cancellation), 
            // so we don't strictly check for negative/zero vertical velocity if we are on a slope.
            bool velocityCondition = _player.Rb.linearVelocity.y <= 0.01f || _player.SlopeSensor.IsOnSlope;
            
            if (_player.IsGrounded && velocityCondition && Time.time > _lastJumpTime + _jumpGroundedResetDelay)
            {
                _hasJumped = false;
            }

            if (_enableBounce)
                Bounce();
        }

        private void OnEnable()
        {
            InputManager.Instance.JumpPressed += OnJumpPressed;
            InputManager.Instance.JumpReleased += OnJumpReleased;
        }

        private void OnDisable()
        {
            if (InputManager.Instance)
            {
                InputManager.Instance.JumpPressed -= OnJumpPressed;
                InputManager.Instance.JumpReleased -= OnJumpReleased;
            }
        }

        /*private void OnJumpPerformed()
        {
            // Backwards-compatible call: treat performed as an immediate press
            OnJumpPressed();
        }*/

        private void OnJumpPressed()
        {
            _jumpIsHeld = true;
            if (_hasJumped)
            {
                if(_showDebug)
                    Debug.LogWarning($"Jump failed because player has jumped already");
                return;
            }
            
            if (_player.StateMachine.CurrentState == _player.AirSuperState)
            {
                if (_player.AirSuperState.InAirTime <= _coyoteTimeDuration)
                {
                    if(_showDebug)
                        Debug.LogWarning($"Performing coyote jump after in air for {_player.AirSuperState.InAirTime}s/{_coyoteTimeDuration}s");
                    Jump();
                    return;
                }
                
                if(_showDebug)
                    Debug.LogWarning($"Jump failed because coyote time has passed and player is in air");
                return;
            }

            if (_player.StateMachine.CurrentState != _player.GroundSuperState)
            {
                if(_showDebug)
                    Debug.LogWarning($"Jump failed because player is not in Ground State and not in Coyote window");
                return;
            }
            
            Jump();
        }

        private void OnJumpReleased()
        {
            _jumpIsHeld = false;
        }

        private void Jump()
        {
            _hasJumped = true;
            _lastJumpTime = Time.time;
            
            // Debug.Log("Jump");
            _player.Rb.linearVelocity = new Vector3(_player.Rb.linearVelocity.x, 0f, _player.Rb.linearVelocity.z);
            _player.Rb.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
            
            _particleCloudPlayer.EnableParticleTrigger(true);
            
            if (_audioSource != null && _jumpSound != null)
            {
                _audioSource.PlayOneShot(_jumpSound);
            }

        }

        private void Bounce()
        {
            if (_player.JustLanded)
            {
                Vector3 velocity = _player.Rb.linearVelocity;

                if (_player.SlopeSensor.IsOnSlope) // If on slope, do not bounce
                {
                    return;
                }

                if (velocity.y < -_bounceThreshold) // only bounce if falling faster than threshold
                {
                    velocity.y = Mathf.Abs(velocity.y) * _bounceMultiplier; // bounce upward at 50% speed
                    if (velocity.y > _maxBounceVelocity)
                    {
                        velocity.y = _maxBounceVelocity; // cap max bounce velocity
                    }
                    _player.Rb.linearVelocity = velocity;
                }
            }
        }
    }
}
