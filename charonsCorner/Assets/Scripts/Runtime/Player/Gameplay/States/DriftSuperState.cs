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

        private protected override void InitializeSubStates()
        {
            DriftingChargeState.Init(SubStateMachine, _context);
            DriftingBoostState.Init(SubStateMachine, _context);
            
            CameraOrbitalFollow = _context.PlayerCamera.GetComponent<CinemachineOrbitalFollow>();
            CameraBaseOrbitalDistance = CameraOrbitalFollow.Radius;
        }

        private protected override void OnEnter()
        {
            
        }

        private protected override void OnExit()
        {
            // Clean up drift effects or restore settings
        }

        private protected override void OnUpdate() { }
        private protected override void OnFixedUpdate() { }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (DriftingBoostState.IsComplete)
                return _context.IsGrounded ? _context.GroundSuperState : _context.AirSuperState;
            return null;
        }
    }
}
