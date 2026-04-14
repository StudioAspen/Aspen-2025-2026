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

        [ShowInInspector] public bool IsDrifting => _playerController.StateMachine.CurrentState == _playerController.DriftSuperState;
        [ShowInInspector] public bool IsOffCooldown => _driftCooldownTimer >=  _driftCooldownDuration;

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
                }
                // If we transition to boost, show the "BOOST" text once.
                // Since it's no longer updated every frame, it will stay 'locked'.
                else if (_chargeText != null && !_chargeText.text.StartsWith("BOOST"))
                {
                    UpdateChargeText();
                }
                
                return;
            }
            
            if(_driftCooldownTimer < _driftCooldownDuration)
                _driftCooldownTimer += Time.unscaledDeltaTime;

            if (_chargeText != null && _chargeText.gameObject.activeSelf)
                _chargeText.gameObject.SetActive(false);
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