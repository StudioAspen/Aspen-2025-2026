using UnityEngine;
using UnityEngine.Serialization;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class DriftingChargeState : State<GameplayPlayerController>
    {
        [Header("Drift Direction")]
        [SerializeField] private Transform _driftDirection;
        [SerializeField] private float _driftDirectionSpeed = 200;
        
        [Header("Camera FOV")]
        [SerializeField] private float _driftDesiredFOV = 40;
        [SerializeField] private float _fovSpeed = 5;
        
        [Header("Time Scale")]
        [SerializeField] private float _desiredTimeScale = 1f;
        [SerializeField] private float _timeScaleSpeed = 30f;
        
        [SerializeField] private float _stateDuration = 0.5f;  

        [SerializeField] private float _slowPlayerStrength = 5f;

        private float _timer;

        private protected override void OnEnter()
        {
            _driftDirection.gameObject.SetActive(true);
            _timer = 0;
            _context.CurrentSubState = GetType().Name;
            InputManager.Instance.Drift += Drift;
            
        }

        private protected override void OnExit()
        {
            // Time.timeScale = _desiredTimeScale;
            InputManager.Instance.Drift -= Drift;
        }

        private protected override void OnUpdate()
        {
            _context.PlayerCamera.Lens.FieldOfView = Mathf.Lerp(_context.PlayerCamera.Lens.FieldOfView, _driftDesiredFOV, _fovSpeed * Time.unscaledDeltaTime);
            _context.CameraTargetFollowTarget.SetPositionOffset(new Vector3(0, Mathf.Lerp(_context.CameraTargetFollowTarget.PositionOffset.y, 1, _fovSpeed * Time.unscaledDeltaTime), 0));
            
            _driftDirection.Rotate(InputManager.Instance.MoveDirection.x * _driftDirectionSpeed * Time.unscaledDeltaTime * Vector3.up);
            
            // _timer += Time.unscaledDeltaTime;
            // Time.timeScale = Mathf.Lerp(Time.timeScale, _desiredTimeScale, _timeScaleSpeed * Time.unscaledDeltaTime);

            Vector3 horizontalVelocity = new Vector3(_context.Rb.linearVelocity.x, 0f, _context.Rb.linearVelocity.z);

            Vector3 counterforce = -horizontalVelocity * _slowPlayerStrength;
            _context.Rb.AddForce(counterforce, ForceMode.Acceleration);
        }

        private protected override void OnFixedUpdate()
        {
            
        }
        
        private void Drift(bool drift)
        {
            if (!drift)
            {
                _context.DriftSuperState.SubStateMachine.ChangeState(_context.DriftSuperState.DriftingBoostState);
            }
        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (_timer >= _stateDuration)
                return _context.DriftSuperState.DriftingBoostState;
            return null;
        }
    }
}
