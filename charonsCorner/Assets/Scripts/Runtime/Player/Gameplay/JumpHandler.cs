using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(GameplayPlayerController))]
    public class JumpHandler : MonoBehaviour
    {   
        [Header("Landing Particle Setup")]
        // How long the player should be in the air before particle system triggers
        [SerializeField] private ParticleCloudPlayer _particleCloudPlayer;

        private GameplayPlayerController _player;

        [Header("Jump Settings")]
        [Tooltip("Force applied when jump is initiated")]
        [SerializeField] private float _jumpForce = 10f;
        [Tooltip("Scale down upward velocity on early release by this value")]
        [SerializeField, Range(0f, 1f)] private float _earlyReleaseMultiplier = 0.5f;

        [Header("Bounce Settings")]
        [SerializeField] private bool _enableBounce = true;
        [Tooltip("Minimum downward velocity to trigger bounce effect")]
        [SerializeField] private float _bounceThreshold = 1f; // e.g., only bounce if falling faster than 1 unit/s
        [Tooltip("Multiplier for bounce velocity")]
        [SerializeField] private float _bounceMultiplier = 0.5f; // e.g., bounce back up at 50% of impact velocity
        [Tooltip("Maximum upward velocity after bounce")]
        [SerializeField] private float _maxBounceVelocity = 20f; // cap the maximum bounce velocity

        void Awake()
        {
            _player = GetComponent<GameplayPlayerController>();
            _particleCloudPlayer = GetComponent<ParticleCloudPlayer>();
        }

        private void Update()
        {
            if (_enableBounce)
                Bounce();
        }

        private void OnEnable()
        {
            InputManager.Instance.JumpPressed += OnJumpPressed;
            InputManager.Instance.JumpReleased += OnJumpReleased;

            InputManager.Instance.Jump += OnJumpPerformed;
        }

        private void OnDisable()
        {
            if (InputManager.Instance)
            {
                InputManager.Instance.JumpPressed -= OnJumpPressed;
                InputManager.Instance.JumpReleased -= OnJumpReleased;
                InputManager.Instance.Jump -= OnJumpPerformed;
            }
        }

        private void OnJumpPerformed()
        {
            // Backwards-compatible call: treat performed as an immediate press
            OnJumpPressed();
        }

        private void OnJumpPressed()
        {
            if (_player.StateMachine.CurrentState != _player.GroundSuperState)
                return;
            
            Jump();
        }

        private void OnJumpReleased()
        {
            // if player released early while still moving upwards, reduce upward velocity so player falls earlier
            var lv = _player.Rb.linearVelocity;
            if (lv.y > 0f)
            {
                _player.Rb.linearVelocity = new Vector3(lv.x, lv.y * _earlyReleaseMultiplier, lv.z);
            }
        }

        private void Jump()
        {
            // Debug.Log("Jump");
            _player.Rb.linearVelocity = new Vector3(_player.Rb.linearVelocity.x, 0f, _player.Rb.linearVelocity.z);
            _player.Rb.AddForce(_player.Orientation.up * _jumpForce, ForceMode.VelocityChange);
            
            _particleCloudPlayer.EnableParticleTrigger(true);
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
