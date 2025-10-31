using UnityEngine;
using UnityEngine.Serialization;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class DriftBoostState : State<GameplayPlayerController>
    {
        [Header("Drift Direction")]
        [SerializeField] private Transform driftDirection;

        [SerializeField] private float boostAmount;
        
        [Header("Camera FOV")]
        [SerializeField] private float baseFOV = 40;
        [SerializeField] private float fovSpeed = 5;
        
        [Header("Time Scale")]
        [SerializeField] private float timeScaleSpeed = 20;
        
        [SerializeField] private float stateDuration = 1f;
        public bool IsComplete { get; private set; }
        private float timer;

        private protected override void OnEnter()
        {
            timer = 0f;
            _context.CurrentSubState = GetType().Name;
            _context.Rb.AddForce(driftDirection.forward * boostAmount, ForceMode.VelocityChange);
        }

        private protected override void OnExit()
        {
            driftDirection.localEulerAngles = Vector3.zero;
            driftDirection.gameObject.SetActive(false);
            _context.PlayerCamera.Lens.FieldOfView = baseFOV;
            Time.timeScale = 1;
            IsComplete = false;
        }

        private protected override void OnUpdate()
        {
            _context.PlayerCamera.Lens.FieldOfView = Mathf.Lerp(_context.PlayerCamera.Lens.FieldOfView, baseFOV, fovSpeed * Time.unscaledDeltaTime);
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, timeScaleSpeed * Time.unscaledDeltaTime);
            timer += Time.unscaledDeltaTime;
            IsComplete = timer >= stateDuration;
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
