using UnityEngine;
using UnityEngine.Serialization;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class DriftingChargeState : State<GameplayPlayerController>
    {
        [Header("Drift Direction")]
        [SerializeField] private Transform driftDirection;
        [SerializeField] private float driftDirectionSpeed = 5;
        
        [Header("Camera FOV")]
        [SerializeField] private float driftDesiredFOV = 40;
        [SerializeField] private float fovSpeed = 5;
        
        [Header("Time Scale")]
        [SerializeField] private float desiredTimeScale = 0.1f;
        [SerializeField] private float timeScaleSpeed = 30;
        
        [SerializeField] private float stateDuration = 0.25f;
        private float timer;

        private protected override void OnEnter()
        {
            driftDirection.gameObject.SetActive(true);
            timer = 0;
            _context.CurrentSubState = GetType().Name;
        }

        private protected override void OnExit()
        {
            Time.timeScale = desiredTimeScale;
        }

        private protected override void OnUpdate()
        {
            _context.PlayerCamera.Lens.FieldOfView = Mathf.Lerp(_context.PlayerCamera.Lens.FieldOfView, driftDesiredFOV, fovSpeed * Time.unscaledDeltaTime);
            
            driftDirection.Rotate(InputManager.Instance.MoveDirection.x * driftDirectionSpeed * Time.unscaledDeltaTime * Vector3.up);
            
            timer += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(Time.timeScale, desiredTimeScale, timeScaleSpeed * Time.unscaledDeltaTime);
        }

        private protected override void OnFixedUpdate()
        {
            
        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (!Input.GetKey(KeyCode.LeftShift) || timer >= stateDuration)
                return _context.DriftSuperState.DriftingBoostState;
            return null;
        }
    }
}
