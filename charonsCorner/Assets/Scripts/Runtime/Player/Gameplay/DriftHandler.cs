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

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _chargeText;
        [SerializeField] private float _blinkSpeed = 10f;
        [SerializeField] private Color _blinkColor = Color.red;

        [Header("Drift Indicator")]
        [SerializeField] private LineRenderer _driftIndicatorLine;
        [SerializeField] private float _minIndicatorLength = 1f;
        [SerializeField] private float _maxIndicatorLength = 5f;
        [SerializeField] private float _minIndicatorWidth = 0.05f;
        [SerializeField] private float _maxIndicatorWidth = 0.2f;
        [SerializeField] private Color _minChargeColor = Color.white;
        [SerializeField] private Color _maxChargeColor = Color.red;

        [Header("Arrowhead")]
        [SerializeField] private MeshRenderer _arrowheadRenderer;
        [SerializeField] private float _minArrowheadSize = 0.2f;
        [SerializeField] private float _maxArrowheadSize = 0.6f;

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
            
            if (_chargeText != null)
                _chargeText.gameObject.SetActive(false);

            if (_driftIndicatorLine != null)
                _driftIndicatorLine.gameObject.SetActive(false);

            if (_arrowheadRenderer != null)
                _arrowheadRenderer.gameObject.SetActive(false);
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
                
                // If we're in the charge phase, update normally.
                if (_playerController.DriftSuperState.SubStateMachine.CurrentState == _playerController.DriftSuperState.DriftingChargeState)
                {
                    UpdateChargeText();
                    UpdateDriftIndicator();
                }
                // If we transition to boost, show the "BOOST" text once.
                // Since it's no longer updated every frame, it will stay 'locked'.
                else if (_chargeText != null)
                {
                    if (!_chargeText.text.StartsWith("BOOST"))
                    {
                        UpdateChargeText();
                    }
                    
                    if (_driftIndicatorLine != null && _driftIndicatorLine.gameObject.activeSelf)
                        _driftIndicatorLine.gameObject.SetActive(false);

                    if (_arrowheadRenderer != null && _arrowheadRenderer.gameObject.activeSelf)
                        _arrowheadRenderer.gameObject.SetActive(false);

                    BlinkBoostText();
                }
                
                return;
            }
            
            if(_driftCooldownTimer < _driftCooldownDuration)
            {
                _driftCooldownTimer += Time.unscaledDeltaTime;
                
                // Keep showing boost text and blinking until cooldown is over
                if (_chargeText != null && _chargeText.gameObject.activeSelf)
                {
                    BlinkBoostText();
                }
            }
            else
            {
                // Cooldown complete, hide UI
                if (_chargeText != null && _chargeText.gameObject.activeSelf)
                    _chargeText.gameObject.SetActive(false);
                
                if (_driftIndicatorLine != null && _driftIndicatorLine.gameObject.activeSelf)
                    _driftIndicatorLine.gameObject.SetActive(false);

                if (_arrowheadRenderer != null && _arrowheadRenderer.gameObject.activeSelf)
                    _arrowheadRenderer.gameObject.SetActive(false);
            }
        }

        private void BlinkBoostText()
        {
            if (_chargeText == null) return;
            
            float t = Mathf.PingPong(Time.unscaledTime * _blinkSpeed, 1f);
            _chargeText.color = Color.Lerp(Color.white, _blinkColor, t);
        }

        private void UpdateDriftIndicator()
        {
            if (_driftIndicatorLine == null) return;

            if (!_driftIndicatorLine.gameObject.activeSelf)
                _driftIndicatorLine.gameObject.SetActive(true);
            
            if (_arrowheadRenderer != null && !_arrowheadRenderer.gameObject.activeSelf)
                _arrowheadRenderer.gameObject.SetActive(true);

            float chargeRatio = _playerController.DriftSuperState.GetCurrentChargeRatio();
            Vector3 driftDir = _playerController.DriftSuperState.DriftingBoostState.GetDriftDirection();

            // If grounded, project the drift direction onto the ground/slope plane
            if (_playerController.IsGrounded)
            {
                Vector3 groundNormal = Vector3.up;
                if (_playerController.SlopeSensor != null && _playerController.SlopeSensor.Hit.normal != Vector3.zero)
                {
                    groundNormal = _playerController.SlopeSensor.Hit.normal;
                }
                
                driftDir = Vector3.ProjectOnPlane(driftDir, groundNormal).normalized;
            }

            Color currentColor = Color.Lerp(_minChargeColor, _maxChargeColor, chargeRatio);
            _driftIndicatorLine.startColor = currentColor;
            _driftIndicatorLine.endColor = currentColor;

            float currentLength = Mathf.Lerp(_minIndicatorLength, _maxIndicatorLength, chargeRatio);
            float currentWidth = Mathf.Lerp(_minIndicatorWidth, _maxIndicatorWidth, chargeRatio);

            _driftIndicatorLine.startWidth = currentWidth;
            _driftIndicatorLine.endWidth = currentWidth;

            _driftIndicatorLine.SetPosition(0, transform.position);
            Vector3 endPosition = transform.position + driftDir * currentLength;
            _driftIndicatorLine.SetPosition(1, endPosition);

            if (_arrowheadRenderer != null)
            {
                _arrowheadRenderer.transform.position = endPosition;
                _arrowheadRenderer.transform.rotation = Quaternion.LookRotation(driftDir);
                
                float currentArrowSize = Mathf.Lerp(_minArrowheadSize, _maxArrowheadSize, chargeRatio);
                _arrowheadRenderer.transform.localScale = Vector3.one * currentArrowSize;
                
                // Color Arrowhead:
                _propBlock.SetColor("_BaseColor", currentColor); // Common URP property
                _propBlock.SetColor("_Color", currentColor);     // Fallback for Standard/Legacy
                _arrowheadRenderer.SetPropertyBlock(_propBlock);
            }
        }

        private void UpdateChargeText()
        {
            if (_chargeText == null) return;
            
            if (!_chargeText.gameObject.activeSelf)
                _chargeText.gameObject.SetActive(true);

            float chargeRatio = _playerController.DriftSuperState.GetCurrentChargeRatio();
            
            if (_playerController.DriftSuperState.SubStateMachine.CurrentState == _playerController.DriftSuperState.DriftingChargeState)
            {
                _chargeText.text = $"Charge: {(chargeRatio * 100f):0}%";
                _chargeText.color = Color.white; // Ensure color is reset when charging
            }
            else
            {
                _chargeText.text = $"BOOST: {(chargeRatio * 100f):0}%";
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