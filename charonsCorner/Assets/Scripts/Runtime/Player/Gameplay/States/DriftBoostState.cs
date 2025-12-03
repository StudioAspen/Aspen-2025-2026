using UnityEngine;
using UnityEngine.Serialization;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class DriftBoostState : State<GameplayPlayerController>
    {
        [Header("Drift Direction")]
        [SerializeField] private Transform _driftDirection;

        [SerializeField] private float _boostAmount = 50f;
        
        [Header("Camera FOV")]
        [SerializeField] private float _baseFOV = 75f;
        [SerializeField] private float _fovSpeed = 10f;
        
        [Header("Time Scale")]
        [SerializeField] private float _timeScaleSpeed = 20f;
        
        [SerializeField] private float _stateDuration = 0.4f;

        [Header("Drift Boost VFX")]
        [SerializeField] private GameObject _BoostVFX;


        public bool IsComplete { get; private set; }
        private float _timer;

        private protected override void OnEnter()
        {
            _timer = 0f;
            _context.CurrentSubState = GetType().Name;
            _context.Rb.AddForce(_driftDirection.forward * _boostAmount, ForceMode.VelocityChange);
            _BoostVFX.SetActive(true);
        }

        private protected override void OnExit()
        {
            _driftDirection.localEulerAngles = Vector3.zero;
            _driftDirection.gameObject.SetActive(false);
            _context.PlayerCamera.Lens.FieldOfView = _baseFOV;
            Time.timeScale = 1;
            IsComplete = false;
            _BoostVFX.SetActive(false);
        }

        private protected override void OnUpdate()
        {
            _context.PlayerCamera.Lens.FieldOfView = Mathf.Lerp(_context.PlayerCamera.Lens.FieldOfView, _baseFOV, _fovSpeed * Time.unscaledDeltaTime);
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, _timeScaleSpeed * Time.unscaledDeltaTime);
            _timer += Time.unscaledDeltaTime;
            IsComplete = _timer >= _stateDuration;
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            return null;
        }
    }
}
