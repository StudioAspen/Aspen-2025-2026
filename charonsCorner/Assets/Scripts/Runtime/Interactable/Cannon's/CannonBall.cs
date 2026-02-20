using Sirenix.OdinInspector;
using UnityEngine;
using Unity.Cinemachine;

namespace CharonsCorner.Runtime
{
    public class CannonBall : MonoBehaviour
    {
        [Header("Projectile Parameters")]
        [SerializeField] private float _acceleration = -9.81f;
        [SerializeField] private float _launchVelocity = 25f;
        [SerializeField] private float _currentHeight = 0f;

        public float Acceleration => _acceleration;
        public float LaunchVelocity => _launchVelocity;
        public float CurrentHeight => _currentHeight;

        [Header("Shot Parameters")]
        [SerializeField] private float _shotAngle = 45f;
        [SerializeField] private float _shotPower = 1f;
        [SerializeField] private float _shotLoadTime = 1f;

        public float ShotAngle { get => _shotAngle; set => _shotAngle = value; }
        public float ShotPower => _shotPower;
        public float ShotLoadTime => _shotLoadTime;

        [Header("Pillar Movement")]
        [SerializeField] private bool _movingPillar = false;
        [SerializeField] private float _shotAngleMin = 20f;
        [SerializeField] private float _shotAngleMax = 70f;
        [SerializeField] private float _pillarSpeed = 0.5f;

        public bool MovingPillar => _movingPillar;
        public float ShotAngleMin => _shotAngleMin;
        public float ShotAngleMax => _shotAngleMax;
        public float PillarSpeed => _pillarSpeed;

        [HideInInspector]
        public float currentShotAngle;

        [Header("Transforms")]
        [SerializeField] private Transform _cannonBase;
        [SerializeField] private Transform _cannonPillar;
        [SerializeField] private Transform _launchDirection;

        public Transform CannonBase => _cannonBase;
        public Transform CannonPillar => _cannonPillar;
        public Transform LaunchDirection => _launchDirection;

        [Header("Gizmos")]
        [SerializeField] private int _numPointsGizmos = 100;
        [SerializeField] private float _timeStepGizmos = 0.1f;

        [Header("Play Mode")]
        [SerializeField] private int _numPoints = 10;
        [SerializeField] private float _timeStep = 0.1f;

        public int NumPoints => _numPoints;
        public float TimeStep => _timeStep;

        [Header("Camera Target")]
        [SerializeField] private bool _useCamera = true;
        [SerializeField, ShowIf("_useCamera")] private CinemachineCamera _cinemachineCamera;

        public bool UseCamera => _useCamera;
        public CinemachineCamera CinemachineCamera => _cinemachineCamera;

        private void OnDrawGizmos()
        {
            if (CannonBase == null) return;
            Vector3 startPosition = CannonBase.position;
            Vector3 forward = LaunchDirection ? LaunchDirection.forward : transform.forward;

            Quaternion angleRotation = Quaternion.AngleAxis(ShotAngle, CannonBase.right);
            Vector3 direction = angleRotation * forward;
            Vector3 velocity = direction.normalized * -LaunchVelocity;
            Vector3 previousPosition = startPosition;

            Gizmos.color = Color.yellow;
            for (int i = 1; i <= _numPointsGizmos; i++)
            {
                float t = i * _timeStepGizmos;
                Vector3 calculatedPosition = startPosition + (velocity * t);
                calculatedPosition.y += (0.5f * Acceleration * (t * t));
                Gizmos.DrawLine(previousPosition, calculatedPosition);
                previousPosition = calculatedPosition;
            }
        }
    }
}
