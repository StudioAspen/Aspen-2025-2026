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
        
        [SerializeField] private float _slowPlayerStrength = 5f;

        [SerializeField] private float _stateDuration = 0.5f;  
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
            InputManager.Instance.Drift -= Drift;
        }

        private protected override void OnUpdate()
        {
            _timer += Time.deltaTime;
            
            _context.PlayerCamera.Lens.FieldOfView = Mathf.Lerp(_context.PlayerCamera.Lens.FieldOfView, _driftDesiredFOV, _fovSpeed * Time.deltaTime);
            _context.CameraTargetFollowTarget.SetPositionOffset(new Vector3(0, Mathf.Lerp(_context.CameraTargetFollowTarget.PositionOffset.y, 1, _fovSpeed * Time.deltaTime), 0));
            
            _driftDirection.Rotate(InputManager.Instance.MoveDirection.x * _driftDirectionSpeed * Time.unscaledDeltaTime * Vector3.up);

            Vector3 horizontalVelocity = _context.Rb.linearVelocity.WithY(0f);

            Vector3 counterforce = -horizontalVelocity * _slowPlayerStrength;
            _context.Rb.AddForce(counterforce, ForceMode.Acceleration);
        }

        private protected override void OnFixedUpdate()
        {
            
        }
        
        private void Drift(bool drift)
        {
            // false means releasing
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
