using Sirenix.OdinInspector;
using UnityEngine;
using Unity.Cinemachine;

namespace CharonsCorner.Runtime
{
    public class CannonBall : MonoBehaviour
    {
        [Header("Redesigned Cannon Parameters")]
        [SerializeField] private float _angleA = -45f;
        [SerializeField] private float _angleB = 45f;
        [SerializeField] private float _lerpRate = 1f;
        [SerializeField] private float _launchForce = 50f;
        [SerializeField] private float _controlDelay = 2f;
        [SerializeField] private Transform _launchObject;
        private LineRenderer _lineRenderer;

        public float AngleA => _angleA;
        public float AngleB => _angleB;
        public float LerpRate => _lerpRate;
        public float LaunchForce => _launchForce;
        public float ControlDelay => _controlDelay;
        public Transform LaunchObject => _launchObject;
        public LineRenderer LineRenderer => _lineRenderer;

        [Header("Cannon Effects")]
        [SerializeField] private Transform _barrelEnd;
        [SerializeField] private ParticleSystem _launchEffect;
        [SerializeField] private float _effectOffset = 0.5f;

        [Header("Transforms")]
        [SerializeField] private Transform _cannonPillar;

        public Transform CannonPillar => _cannonPillar;

        [Header("Camera Target")]
        [SerializeField] private CinemachineCamera _cinemachineCamera;

        public CinemachineCamera CinemachineCamera => _cinemachineCamera;

        private void Awake()
        {
            _lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            // Default LineRenderer settings
            _lineRenderer.startWidth = 0.1f;
            _lineRenderer.endWidth = 0.1f;
            _lineRenderer.positionCount = 0;
            _lineRenderer.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<GameplayPlayerController>(out var player))
            {
                var cannonSuper = player.CannonBallSuperState;
                
                // Don't activate if already in use
                if (!cannonSuper.LaunchCompleted && (cannonSuper.EntryState.IsInCannon || cannonSuper.PillarMoveState.IsInCannon || cannonSuper.FiredState.IsLaunching)) return;

                player.CannonBallSuperState.SetCannonReference(this);
                player.StateMachine.ChangeState(player.CannonBallSuperState, true);
            }
        }

        public void PlayLaunchEffect()
        {
            if (_launchEffect == null || _barrelEnd == null) return;
            // Calculate Effect Position and Rotation to front of the barrel end
            Vector3 effectPosition = _barrelEnd.position + (_barrelEnd.forward * _effectOffset);
            // Set Effect Position and Rotation
            _launchEffect.transform.position = effectPosition;    
           _launchEffect.transform.rotation = Quaternion.LookRotation(_barrelEnd.forward, Vector3.up);
           
            _launchEffect.Play();
        }
    }
}
