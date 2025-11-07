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

        /// <summary>
        /// Enters the drifting charge state: enables the drift-direction indicator, resets the state's timer, records this substate on the context, and subscribes to drift input events.
        /// </summary>
        private protected override void OnEnter()
        {
            driftDirection.gameObject.SetActive(true);
            timer = 0;
            _context.CurrentSubState = GetType().Name;
            InputManager.Instance.Drift += Drift;
        }


        /// <summary>
        /// Restore the global time scale to the state's configured value and unsubscribe the drift input handler when exiting the state.
        /// </summary>
        private protected override void OnExit()
        {
            Time.timeScale = desiredTimeScale;
            InputManager.Instance.Drift -= Drift;
        }

        /// <summary>
        /// Performs per-frame updates for the drift charge state: adjusts the camera FOV toward the drift target, rotates the drift direction based on horizontal input, advances the state's timer, and smooths the global time scale toward the configured target.
        /// </summary>
        /// <remarks>
        /// Updates use unscaled delta time so behavior is independent of the current Time.timeScale.
        /// </remarks>
        private protected override void OnUpdate()
        {
            _context.PlayerCamera.Lens.FieldOfView = Mathf.Lerp(_context.PlayerCamera.Lens.FieldOfView, driftDesiredFOV, fovSpeed * Time.unscaledDeltaTime);
            
            driftDirection.Rotate(InputManager.Instance.MoveDirection.x * driftDirectionSpeed * Time.unscaledDeltaTime * Vector3.up);
            
            timer += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(Time.timeScale, desiredTimeScale, timeScaleSpeed * Time.unscaledDeltaTime);
        }

        /// <summary>
        /// Called on the fixed physics update; intentionally performs no fixed-step logic for this state.
        /// </summary>
        private protected override void OnFixedUpdate()
        {
            
        }
        
        /// <summary>
        /// Handle drift input; when drifting ends, transition the drift super-state to its DriftingBoostState.
        /// </summary>
        /// <param name="drift">`true` if drift input is active; `false` if drift input has ended.</param>
        private void Drift(bool drift)
        {
            if (!drift)
            {
                _context.DriftSuperState.SubStateMachine.ChangeState(_context.DriftSuperState.DriftingBoostState);
            }
        }

        /// <summary>
        /// Determines whether this state should transition to the drifting boost substate based on elapsed time.
        /// </summary>
        /// <returns>The `DriftingBoostState` instance when `timer` is greater than or equal to `stateDuration`; otherwise `null`.</returns>
        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (timer >= stateDuration)
                return _context.DriftSuperState.DriftingBoostState;
            return null;
        }
    }
}