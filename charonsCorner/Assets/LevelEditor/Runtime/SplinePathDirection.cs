using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace CharonsCorner.LevelEditor
{
    /// <summary>
    /// Utility for sampling travel direction along a SplinePath.
    /// Does not track state — poll this from a higher-level script.
    /// </summary>
    [RequireComponent(typeof(SplinePath))]
    public class SplinePathDirection : MonoBehaviour
    {
        public enum RoadDirection
        {
            Forward,  // Travel in the direction of increasing t (0 -> 1)
            Backward  // Travel in the direction of decreasing t (1 -> 0)
        }

        [Header("Config")]
        [SerializeField] private RoadDirection _correctDirection = RoadDirection.Forward;

        [Header("Detection")]
        [SerializeField, Range(0f, 1f)] private float _dotThreshold = 0.0f;

        private SplinePath _splinePath;
        private SplineContainer _splineContainer;

        public RoadDirection CorrectDirection => _correctDirection;

        private void OnValidate()
        {
            if (_splinePath != null)
                return;
            _splinePath = GetComponent<SplinePath>();
            _splineContainer = _splinePath.splineContainer;
        }

        private void Awake()
        {
            _splinePath = GetComponent<SplinePath>();
            _splineContainer = _splinePath.splineContainer;
        }

        /// <summary>
        /// Samples whether a velocity vector is travelling in the wrong direction
        /// at a given normalized t position along a specific spline.
        /// </summary>
        public bool CheckWrongWay(int splineIndex, float t, Vector3 velocity)
        {
            if (_splineContainer == null || velocity.sqrMagnitude < 0.001f)
                return false;

            Vector3 splineForward = SampleSplineForward(splineIndex, t);

            if (_correctDirection == RoadDirection.Backward)
                splineForward = -splineForward;

            Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z).normalized;

            return Vector3.Dot(flatVelocity, splineForward) < _dotThreshold;
        }

        /// <summary>
        /// Finds the closest point across ALL splines to a world position,
        /// then checks if the velocity is going the wrong way on that spline.
        /// </summary>
        public bool CheckWrongWayFromPosition(Vector3 worldPosition, Vector3 velocity)
        {
            if (_splineContainer == null)
                return false;

            float3 localPosition = _splineContainer.transform.InverseTransformPoint(worldPosition);

            float nearestDist = float.MaxValue;
            int nearestSplineIndex = 0;
            float nearestT = 0f;

            for (int i = 0; i < _splineContainer.Splines.Count; i++)
            {
                SplineUtility.GetNearestPoint(
                    _splineContainer.Splines[i],
                    localPosition,
                    out float3 nearestPoint,
                    out float t
                );

                float dist = math.distancesq(localPosition, nearestPoint);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestSplineIndex = i;
                    nearestT = t;
                }
            }

            return CheckWrongWay(nearestSplineIndex, nearestT, velocity);
        }

        /// <summary>
        /// Sample the spline's forward direction at a given t.
        /// </summary>
        private Vector3 SampleSplineForward(int splineIndex, float t)
        {
            _splineContainer.Evaluate(splineIndex, t, out _, out float3 forward, out _);

            if (((Vector3)forward).sqrMagnitude < 1e-6f)
            {
                float dt = 0.001f;
                float t0 = Mathf.Max(0f, t - dt);
                float t1 = Mathf.Min(1f, t + dt);

                _splineContainer.Evaluate(splineIndex, t0, out float3 p0, out _, out _);
                _splineContainer.Evaluate(splineIndex, t1, out float3 p1, out _, out _);
                forward = math.normalize(p1 - p0);
            }

            Vector3 flat = new Vector3(forward.x, 0f, forward.z);
            return flat.normalized;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_splineContainer == null) return;

            int steps = 25;
            for (int splineIndex = 0; splineIndex < _splineContainer.Splines.Count; splineIndex++)
            {
                for (int i = 0; i <= steps; i++)
                {
                    float t = i / (float)steps;
                    _splineContainer.Evaluate(splineIndex, t, out float3 pos, out _, out _);
                    Vector3 worldPos = (Vector3)pos;

                    Vector3 dir = SampleSplineForward(splineIndex, t);
                    if (_correctDirection == RoadDirection.Backward) dir = -dir;

                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(worldPos, dir * 2f);

                    Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
                    Gizmos.DrawRay(worldPos + dir * 2f, (-dir + right) * 0.5f);
                    Gizmos.DrawRay(worldPos + dir * 2f, (-dir - right) * 0.5f);
                }
            }
        }
#endif
    }
}