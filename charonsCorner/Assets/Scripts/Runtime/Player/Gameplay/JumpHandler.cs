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

        [SerializeField] private float _jumpForce = 10f;
        
        void Awake()
        {
            _player = GetComponent<GameplayPlayerController>();
            _particleCloudPlayer = GetComponent<ParticleCloudPlayer>();
        }

        void OnEnable()
        {
            InputManager.Instance.Jump += OnJump;
        }
        
        void OnDisable()
        {
            InputManager.Instance.Jump -= OnJump;
        }

        private void OnJump()
        {
            if (_player.StateMachine.CurrentState != _player.GroundSuperState)
                return;
            
            Jump();
        }

        private void Jump()
        {
            // Debug.Log("Jump");
            _player.Rb.linearVelocity = new Vector3(_player.Rb.linearVelocity.x, 0f, _player.Rb.linearVelocity.z);
            _player.Rb.AddForce(_player.Orientation.up * _jumpForce, ForceMode.VelocityChange);
            
            _particleCloudPlayer.EnableParticleTrigger(true);
        }
    }
}
