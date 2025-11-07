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

        /// <summary>
        /// Enter the drift boost state: reset the state timer, record this sub-state on the controller, and apply an instantaneous forward impulse to the player.
        /// </summary>
        /// <remarks>
        /// The impulse is applied in the direction of <c>driftDirection.forward</c> with magnitude <c>boostAmount</c> using <see cref="UnityEngine.ForceMode.VelocityChange"/>.
        /// </remarks>
        private protected override void OnEnter()
        {
            timer = 0f;
            _context.CurrentSubState = GetType().Name;
            _context.Rb.AddForce(driftDirection.forward * boostAmount, ForceMode.VelocityChange);
        }

        /// <summary>
        /// Cleans up the drift boost state: resets and hides the drift direction transform, restores camera FOV and global time scale, and clears the completion flag.
        /// </summary>
        private protected override void OnExit()
        {
            driftDirection.localEulerAngles = Vector3.zero;
            driftDirection.gameObject.SetActive(false);
            _context.PlayerCamera.Lens.FieldOfView = baseFOV;
            Time.timeScale = 1;
            IsComplete = false;
        }

        /// <summary>
        /// Updates camera field of view and global time scale toward their targets, advances the internal timer using unscaled delta time, and marks the state complete when its duration is reached.
        /// </summary>
        private protected override void OnUpdate()
        {
            _context.PlayerCamera.Lens.FieldOfView = Mathf.Lerp(_context.PlayerCamera.Lens.FieldOfView, baseFOV, fovSpeed * Time.unscaledDeltaTime);
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, timeScaleSpeed * Time.unscaledDeltaTime);
            timer += Time.unscaledDeltaTime;
            IsComplete = timer >= stateDuration;
        }

        /// <summary>
        /// Performs no fixed-timestep physics updates for the drift boost state.
        /// </summary>
        private protected override void OnFixedUpdate()
        {

        }

        /// <summary>
        /// Determines the next state to transition to from this drift boost substate; this implementation does not define an automatic transition.
        /// </summary>
        /// <returns>null to indicate no automatic state transition is provided.</returns>
        private protected override State<GameplayPlayerController> GetTransition()
        {
            return null;
        }
    }
}