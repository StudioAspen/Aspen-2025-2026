using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Handles movement of an obstacle along a defined set of waypoints.
    /// 
    /// The obstacle moves between Transform points specified in the inspector, 
    /// starting from its initial position. The movement pattern can be changed 
    /// between Lerp, MoveTowards, or SmoothDamp for different movement feels.
    /// 
    /// Once it reaches the end of its path, it reverses direction (rubberbands) 
    /// and moves back along the same path, looping indefinitely.
    /// 
    /// This script supports custom speed settings and optional facing 
    /// towards the current movement direction.
    /// </summary>

    public class MovingObstacle : MonoBehaviour
    {
        public enum MovementType
        {
            Lerping,
            MovingTowards,
            SmoothDamp
        }

        [Tooltip("Path Points for the Moving Obstacle")]
        [SerializeField] private List<Transform> _pathPoints; 
        private List<Vector3> _pathPositions = new List<Vector3>();
        private int _currentPointIndex = 1;
        private bool _rubberBanding = false;

        private GameObject _startPoint;

        [Header("Movement Settings")]
        [Tooltip("Speed of the Moving Obstacle")]
        [SerializeField] private float _speed = 2f;
        [Tooltip("Type of movement interpolation")]
        [SerializeField] private MovementType _movementType = MovementType.Lerping;
        [Tooltip("Smoothing time for SmoothDamp movement type")]
        [SerializeField] private float _smoothTime = 0.3f; // Adjust this value to control smoothing strength
        [Tooltip("Should the obstacle face towards its movement direction?")]
        [SerializeField] private bool _faceTowards = true;
        private Vector3 _smoothDampVelocity = Vector3.zero;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_pathPoints == null || _pathPoints.Count == 0)
            {
                Debug.LogError(this.name + " requires at least one path point to be assigned!");
                enabled = false;
                return;
            }

            _startPoint = new GameObject("StartPoint");
            _startPoint.transform.parent = this.transform;
            _pathPoints.Insert(0, _startPoint.transform);
            _pathPoints[0].position = transform.position;

            foreach (Transform point in _pathPoints)
            {
                _pathPositions.Add(point.position);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_pathPositions.Count < 2) // need to have points to go to
            {
                Debug.LogWarning(this.name + " does not have path points assigned! Will not move.");
                return;
            }

            // Move towards the current point
            DetermineMoveType();

            // Check if reached current point and switch to next/previous waypoint
            if (Vector3.Distance(transform.position, _pathPositions[_currentPointIndex]) < 0.01f)
            {
                if (_currentPointIndex + 1 < _pathPoints.Count && !_rubberBanding)
                {
                    _currentPointIndex++; // Move to the next point   
                }
                else
                {
                    _rubberBanding = true;
                }

                if (_currentPointIndex - 1 > -1 && _rubberBanding)
                {
                    _currentPointIndex--; // Move to the previous point
                }
                else
                {
                    _rubberBanding = false;
                }
            }
            if (_faceTowards)
            {
                FaceTowards();
            }
        }

        private void OnDestroy()
        {
            if (_startPoint != null)
            {
                Destroy(_startPoint);
            }
        }

        private void DetermineMoveType()
        {
            if (_movementType == MovementType.Lerping)
            {
                transform.position = Vector3.Lerp(transform.position, _pathPositions[_currentPointIndex], Mathf.Clamp01(_speed * Time.deltaTime));
            }
            else if (_movementType == MovementType.MovingTowards)
            {
                transform.position = Vector3.MoveTowards(transform.position, _pathPositions[_currentPointIndex], _speed * Time.deltaTime);
            }
            else if (_movementType == MovementType.SmoothDamp)
            {
                transform.position = Vector3.SmoothDamp(transform.position, _pathPositions[_currentPointIndex], ref _smoothDampVelocity, _smoothTime, _speed);
            }
        }

        // Make the obstacle face towards the next point it is moving to
        private void FaceTowards()
        {
            Vector3 direction = _pathPositions[_currentPointIndex] - transform.position;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _speed);
            }
        }

    }
}

