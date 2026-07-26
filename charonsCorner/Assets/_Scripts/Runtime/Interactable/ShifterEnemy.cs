using UnityEngine;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using System.Collections;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Enemy that moves back and forth between specified Vector3 points along a curve in local space.
    /// Follows a ping-pong pattern: 0 -> 1 -> 2 -> 3 -> 2 -> 1 -> 0 and repeats.
    /// Uses MMTween instead of DOTween.
    /// </summary>
    public class ShifterEnemy : MonoBehaviour
    {
        [Header("Movement Settings")]
        [InfoBox("Specify the local positions the enemy should move through. It will ping-pong through them.")]
        [SerializeField] private Vector3[] _waypointPositions;
        
        [SerializeField] private float _moveDuration = 2f;
        [SerializeField] private float _delayBetweenShifts = 1f;
        [SerializeField] private MMTweenType _movementTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuadratic);

        private Coroutine _movementCoroutine;

        private void Start()
        {
            if (_waypointPositions == null || _waypointPositions.Length < 2)
            {
                Debug.LogWarning($"[ShifterEnemy] {gameObject.name} needs at least 2 waypoint positions to move.");
                return;
            }

            _movementCoroutine = StartCoroutine(MoveRoutine());
        }

        private IEnumerator MoveRoutine()
        {
            int currentIndex = 0;
            int direction = 1;

            while (true)
            {
                int nextIndex = currentIndex + direction;

                // Move from currentIndex to nextIndex
                yield return StartCoroutine(MoveBetweenWaypoints(currentIndex, nextIndex));

                currentIndex = nextIndex;

                // Ping-pong logic
                if (currentIndex >= _waypointPositions.Length - 1)
                {
                    direction = -1;
                }
                else if (currentIndex <= 0)
                {
                    direction = 1;
                }

                if (_delayBetweenShifts > 0)
                {
                    yield return new WaitForSeconds(_delayBetweenShifts);
                }
            }
        }

        private IEnumerator MoveBetweenWaypoints(int startIndex, int endIndex)
        {
            float elapsed = 0f;
            
            // For curved movement (Catmull-Rom), we need 4 points.
            // P0, P1, P2, P3. We interpolate between P1 and P2.
            Vector3 p0 = GetWaypoint(startIndex - 1);
            Vector3 p1 = GetWaypoint(startIndex);
            Vector3 p2 = GetWaypoint(endIndex);
            Vector3 p3 = GetWaypoint(endIndex + (endIndex > startIndex ? 1 : -1));

            // Divide duration by number of segments if we want _moveDuration to be for the WHOLE path,
            // but usually it's per segment in these requests. 
            // The previous DOTween implementation used _moveDuration for the ENTIRE path.
            float segmentDuration = _moveDuration / (_waypointPositions.Length - 1);

            while (elapsed < segmentDuration)
            {
                float t = elapsed / segmentDuration;
                float easedT = MMTween.Tween(t, 0f, 1f, 0f, 1f, _movementTween);
                
                transform.localPosition = GetCatmullRomPosition(easedT, p0, p1, p2, p3);
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = p2;
        }

        private Vector3 GetWaypoint(int index)
        {
            if (index < 0) return _waypointPositions[0] + (_waypointPositions[0] - _waypointPositions[1]);
            if (index >= _waypointPositions.Length) return _waypointPositions[_waypointPositions.Length - 1] + (_waypointPositions[_waypointPositions.Length - 1] - _waypointPositions[_waypointPositions.Length - 2]);
            return _waypointPositions[index];
        }

        private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        private void OnDestroy()
        {
            if (_movementCoroutine != null)
            {
                StopCoroutine(_movementCoroutine);
            }
        }

        [Button("Reset to Waypoint 0")]
        private void ResetToStart()
        {
            if (_waypointPositions != null && _waypointPositions.Length > 0)
            {
                transform.localPosition = _waypointPositions[0];
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_waypointPositions == null || _waypointPositions.Length == 0) return;

            Gizmos.color = Color.cyan;
            Vector3 parentPos = transform.parent != null ? transform.parent.position : Vector3.zero;
            Quaternion parentRot = transform.parent != null ? transform.parent.rotation : Quaternion.identity;
            Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;

            System.Func<Vector3, Vector3> toWorld = (localPos) => 
            {
                if (transform.parent == null) return localPos;
                return transform.parent.TransformPoint(localPos);
            };

            for (int i = 0; i < _waypointPositions.Length; i++)
            {
                Vector3 worldPos = toWorld(_waypointPositions[i]);
                Gizmos.DrawSphere(worldPos, 0.3f);
                
                if (i < _waypointPositions.Length - 1)
                {
                    // Draw curved path preview
                    Vector3 p0 = GetWaypoint(i - 1);
                    Vector3 p1 = GetWaypoint(i);
                    Vector3 p2 = GetWaypoint(i + 1);
                    Vector3 p3 = GetWaypoint(i + 2);

                    Vector3 lastPoint = toWorld(p1);
                    for (int step = 1; step <= 10; step++)
                    {
                        float t = step / 10f;
                        Vector3 currentPoint = toWorld(GetCatmullRomPosition(t, p0, p1, p2, p3));
                        Gizmos.DrawLine(lastPoint, currentPoint);
                        lastPoint = currentPoint;
                    }
                }
            }
        }
    }
}
