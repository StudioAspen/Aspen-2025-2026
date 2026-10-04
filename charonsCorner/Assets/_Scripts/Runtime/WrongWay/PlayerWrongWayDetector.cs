using CharonsCorner.LevelEditor;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(GameplayPlayerController))]
    public class PlayerWrongWayDetector : MonoBehaviour
    {
        [Header("Track Detection")]
        [SerializeField, Min(0.02f)] private float _checkRate = 0.1f;
        [SerializeField, Min(0f)] private float _minimumSpeed = 1f;
        [SerializeField, Range(-1f, 0f)] private float _wrongWayDotThreshold = -0.25f;

        [field: SerializeField, ReadOnly] public bool IsWrongWay { get; private set; }
        [field: SerializeField] public UnityEvent<bool> OnWrongWayChanged { get; private set; } = new();

        [ShowInInspector, ReadOnly] public SplinePathDirection CurrentPath { get; private set; }
        [ShowInInspector, ReadOnly] public int CurrentSplineIndex { get; private set; } = -1;
        [ShowInInspector, ReadOnly] public Vector3 TravelDirection { get; private set; }
        [ShowInInspector, ReadOnly] public float AlignmentDot { get; private set; }

        private GameplayPlayerController _playerController;
        private float _checkTimer;

        private void Awake() => _playerController = GetComponent<GameplayPlayerController>();
        private void OnEnable() => _checkTimer = 0f;
        private void OnDisable() => ClearTrack();

        private void Update()
        {
            _checkTimer += Time.deltaTime;
            if (_checkTimer < _checkRate) return;
            _checkTimer = 0f;
            CheckDirection();
        }

        private void CheckDirection()
        {
            SlopeSensor sensor = _playerController != null ? _playerController.SlopeSensor : null;
            if (_playerController == null || _playerController.Rb == null || sensor == null)
            {
                ClearTrack();
                return;
            }

            // The marble rotates while rolling; its transform/camera yaw is not its
            // travel heading. Compare actual motion with the road's local direction.
            RaycastHit hit = sensor.Hit;
            SplinePath path = hit.collider != null ? hit.collider.GetComponent<SplinePath>() : null;
            if (path == null || !path.isActiveAndEnabled || Vector3.Dot(hit.normal, Vector3.up) <= 0.1f)
            {
                ClearTrack();
                return;
            }

            SplinePathDirection directionSource = path.GetComponent<SplinePathDirection>();
            if (directionSource == null)
                directionSource = path.gameObject.AddComponent<SplinePathDirection>();

            if (!directionSource.isActiveAndEnabled ||
                !directionSource.TryGetTravelDirectionAtHit(hit, out Vector3 direction, out int splineIndex))
            {
                ClearTrack();
                return;
            }

            CurrentPath = directionSource;
            CurrentSplineIndex = splineIndex;
            TravelDirection = direction;
            Vector3 velocity = Vector3.ProjectOnPlane(_playerController.Rb.linearVelocity, Vector3.up);
            AlignmentDot = velocity.sqrMagnitude > 0.001f ? Vector3.Dot(velocity.normalized, direction) : 0f;
            ChangeWrongWay(velocity.sqrMagnitude >= _minimumSpeed * _minimumSpeed &&
                AlignmentDot < _wrongWayDotThreshold);
        }

        private void ClearTrack()
        {
            CurrentPath = null;
            CurrentSplineIndex = -1;
            TravelDirection = Vector3.zero;
            AlignmentDot = 0f;
            ChangeWrongWay(false);
        }

        private void ChangeWrongWay(bool isWrongWay)
        {
            if (IsWrongWay == isWrongWay) return;
            IsWrongWay = isWrongWay;
            OnWrongWayChanged.Invoke(isWrongWay);
        }
    }
}
