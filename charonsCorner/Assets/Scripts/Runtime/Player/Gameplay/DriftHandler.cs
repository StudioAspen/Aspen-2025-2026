using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(GameplayPlayerController))]
    public class DriftHandler : MonoBehaviour
    {
        [SerializeField, Required, ReadOnly] private GameplayPlayerController _playerController;
        
        [SerializeField] private float _driftCooldownDuration = 1f;
        [SerializeField, ReadOnly] private float _driftCooldownTimer;

        [Header("Glow Effect")]
        [SerializeField] private Renderer[] _glowRenderers;
        [SerializeField] private string _glowPropertyName = "_Glow";
        [SerializeField] private float _maxGlowValue = 5f;
        [SerializeField] private AnimationCurve _glowChargeCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _glowCooldownCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] private float _glowCooldownSpeed = 1f;

        [Header("Drift Boost VFX")]
        [SerializeField] private ParticleSystem _driftClouds;
        [SerializeField] private ParticleSystem _driftDust;
        [SerializeField] private float _particleDuration = 1f;

        [Header("Audio")]
        [SerializeField] private AudioSource _chargingAudioSource;
        [SerializeField] private AudioSource _boostReleaseAudioSource; 
 
        [ShowInInspector] public bool IsDrifting => _playerController != null && _playerController.StateMachine != null && _playerController.StateMachine.CurrentState == _playerController.DriftSuperState;
        [ShowInInspector] public bool IsOffCooldown => _playerController != null && _driftCooldownTimer >= _driftCooldownDuration;

        private float _capturedMinGlow;
        private float _currentGlow;
        private float _cooldownTimer;
        private float _glowAtStartOfCooldown;
        private bool _wasChargingLastFrame;
        private int _glowPropertyId;
        private bool _isSoundPlaying;

        private MaterialPropertyBlock _propBlock;

        private void OnValidate()
        {
            _playerController = GetComponent<GameplayPlayerController>();
        }
        
        private void Awake()
        {
            _propBlock = new MaterialPropertyBlock();
            _playerController = GetComponent<GameplayPlayerController>();
            _driftCooldownTimer = _driftCooldownDuration; // so we can immediately drift on start without waiting
            _glowPropertyId = Shader.PropertyToID(_glowPropertyName);

            // Capture initial glow from the first renderer
            _capturedMinGlow = 0f;
            if (_glowRenderers != null && _glowRenderers.Length > 0 && _glowRenderers[0] != null)
            {
                if (_glowRenderers[0].sharedMaterial.HasProperty(_glowPropertyId))
                {
                    _capturedMinGlow = _glowRenderers[0].sharedMaterial.GetFloat(_glowPropertyId);
                }
            }
            
            _currentGlow = _capturedMinGlow;
            _glowAtStartOfCooldown = _capturedMinGlow;
        }

        private void OnEnable()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.Drift += Drift;
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.Drift -= Drift;
        }

        private void Update()
        {
            bool isCharging = IsDrifting && _playerController.DriftSuperState.SubStateMachine.CurrentState == _playerController.DriftSuperState.DriftingChargeState;
           
             if (_chargingAudioSource != null)
            {
                if (_chargingAudioSource && !_isSoundPlaying)
                {
                    _chargingAudioSource.Play();
                    _isSoundPlaying = true; 
                }
                else if (!isCharging && _isSoundPlaying)
                {
                    _chargingAudioSource.Stop();
                    _isSoundPlaying = false;
                }
            }

            if (isCharging)
            {
                float chargeRatio = _playerController.DriftSuperState.GetCurrentChargeRatio();
                float evaluatedCharge = _glowChargeCurve.Evaluate(chargeRatio);
                _currentGlow = Mathf.Lerp(_capturedMinGlow, _maxGlowValue, evaluatedCharge);
                _wasChargingLastFrame = true;
                _cooldownTimer = 0f;
            }
            else
            {
                if (_wasChargingLastFrame)
                {
                    _glowAtStartOfCooldown = _currentGlow;
                    _wasChargingLastFrame = false;
                    _cooldownTimer = 0f;
                }

                _cooldownTimer += Time.deltaTime * _glowCooldownSpeed;
                float t = _glowCooldownCurve.Evaluate(_cooldownTimer);
                _currentGlow = Mathf.Lerp(_capturedMinGlow, _glowAtStartOfCooldown, t);
            }

            if (_glowRenderers != null)
            {
                foreach (var r in _glowRenderers)
                {
                    if (r != null)
                    {
                        r.GetPropertyBlock(_propBlock);
                        _propBlock.SetFloat(_glowPropertyId, _currentGlow);
                        r.SetPropertyBlock(_propBlock);
                    }
                }
            }

            if (IsDrifting)
            {
                _driftCooldownTimer = 0f;
                return;
            }
            
            if(_driftCooldownTimer < _driftCooldownDuration)
            {
                _driftCooldownTimer += Time.unscaledDeltaTime;
            }
        }

        private void Drift(bool drift)
        {
            // PRESS (start drift)
            if (drift)
            {
                if (!IsOffCooldown) 
                    return;
                if (IsDrifting) 
                    return;

                _playerController.StateMachine.ChangeState(_playerController.DriftSuperState, true);
                return;
            }
            
            // RELEASE (only relevant if currently drifting and in charge)
            if (!IsDrifting) 
                return;

            // Only allow release-to-boost if we're still in the charge substate
            if (_playerController.DriftSuperState.SubStateMachine.CurrentState
                == _playerController.DriftSuperState.DriftingChargeState)
            {
                _playerController.DriftSuperState.SubStateMachine.ChangeState(
                    _playerController.DriftSuperState.DriftingBoostState, true);
            }
            if(_boostReleaseAudioSource !=null)
            {
                _boostReleaseAudioSource.Play();
            }
        }


        public void PlayPersistentDriftParticles()
        {
            StartCoroutine(DriftParticlesRoutine());
        }

        private System.Collections.IEnumerator DriftParticlesRoutine()
        {
            if (_driftClouds != null)
            {
                var main = _driftClouds.main;
                main.stopAction = ParticleSystemStopAction.None;
                _driftClouds.Play();
            }

            if (_driftDust != null)
            {
                var main = _driftDust.main;
                main.stopAction = ParticleSystemStopAction.None;
                _driftDust.Play();
            }

            yield return new WaitForSeconds(_particleDuration);

            if (_driftClouds != null) _driftClouds.Stop();
            if (_driftDust != null) _driftDust.Stop();
        }
    }
}