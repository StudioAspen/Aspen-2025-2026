using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
   [System.Serializable]
    public class DriftSuperState : SuperState<GameplayPlayerController>
    {
        // Substates
        [field: SerializeField] public DriftingChargeState DriftingChargeState {get; private set;} = new();
        [field: SerializeField] public DriftBoostState DriftingBoostState {get; private set;} = new();

        public override State<GameplayPlayerController> InitialSubState => DriftingChargeState;
        
        public CinemachineOrbitalFollow CameraOrbitalFollow {get; private set;}
        public float CameraBaseOrbitalDistance {get; private set;}
        
        public Vector3 InitialVelocity { get; private set; }

        private protected override void InitializeSubStates()
        {
            DriftingChargeState.Init(SubStateMachine, _context);
            DriftingBoostState.Init(SubStateMachine, _context);
            
            CameraOrbitalFollow = _context.PlayerCamera.GetComponent<CinemachineOrbitalFollow>();
            CameraBaseOrbitalDistance = CameraOrbitalFollow.Radius;
        }

        private protected override void OnEnter()
        {
            InitialVelocity = _context.Rb.linearVelocity.WithY(0);
            _context.DriftActivationFeedbacks?.PlayFeedbacks();
        }

        private protected override void OnExit()
        {
            // Clean up drift effects or restore settings
        }

        private protected override void OnUpdate() { }
        private protected override void OnFixedUpdate() { }

        public float GetCurrentChargeRatio()
        {
            Vector3 driftDir = DriftingBoostState.GetDriftDirection();
            
            Vector3 currentDir = driftDir; 
            if (InitialVelocity.sqrMagnitude > 0.0001f)
                currentDir = InitialVelocity.normalized; 

            float angle = Vector3.Angle(currentDir, driftDir);
            float chargeRatio = Mathf.Clamp01(angle / Mathf.Max(0.0001f, DriftingBoostState.MaxAngle));
            
            return chargeRatio;
        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (DriftingBoostState.IsComplete)
                return _context.IsGrounded ? _context.GroundSuperState : _context.AirSuperState;
            return null;
        }
    }
}
