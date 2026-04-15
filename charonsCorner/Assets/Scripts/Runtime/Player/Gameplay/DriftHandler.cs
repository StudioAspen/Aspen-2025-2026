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

        [ShowInInspector] public bool IsDrifting => _playerController != null && _playerController.StateMachine != null && _playerController.StateMachine.CurrentState == _playerController.DriftSuperState;
        [ShowInInspector] public bool IsOffCooldown => _playerController != null && _driftCooldownTimer >= _driftCooldownDuration;

        private void OnValidate()
        {
            _playerController = GetComponent<GameplayPlayerController>();
        }
        
        private void Awake()
        {
            _playerController = GetComponent<GameplayPlayerController>();
            _driftCooldownTimer = _driftCooldownDuration; // so we can immediately drift on start without waiting
            
            if (_chargeText != null)
                _chargeText.gameObject.SetActive(false);

            if (_driftIndicatorLine != null)
                _driftIndicatorLine.gameObject.SetActive(false);
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
            _driftIndicatorLine.SetPosition(1, transform.position + driftDir * currentLength);
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
    }
}