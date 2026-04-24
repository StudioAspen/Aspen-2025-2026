using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

namespace CharonsCorner.LevelEditor
{
    /// <summary>
    /// Utility for sampling travel direction along a SplinePath.
    /// Supports per-spline direction overrides configurable via Odin Inspector.
    /// Does not track state, poll this from a higher-level script.
    /// </summary>
    [RequireComponent(typeof(SplinePath))]
    public class SplinePathDirection : MonoBehaviour
    {
        public enum RoadDirection
        {
            Forward,  // Travel in the direction of increasing t (0 -> 1)
            Backward  // Travel in the direction of decreasing t (1 -> 0)
        }

        [System.Serializable]
        public class SplineDirectionEntry
        {
            [HideInInspector]
            public int SplineIndex;

#if UNITY_EDITOR
            [HorizontalGroup, LabelWidth(80)]
            [DisplayAsString, HideLabel]
            public string Label;

            [HorizontalGroup, HideLabel]
#endif
            public RoadDirection Direction = RoadDirection.Forward;
        }

        [Header("Config")]
        [Tooltip("Fallback direction used when no per-spline entry exists.")]
        [SerializeField] private RoadDirection _defaultDirection = RoadDirection.Forward;

        [Header("Per-Spline Direction Overrides")]
#if UNITY_EDITOR
        [ListDrawerSettings(IsReadOnly = true, ShowFoldout = true)]
#endif
        [SerializeField] private List<SplineDirectionEntry> _splineDirections = new List<SplineDirectionEntry>();

        [Header("Detection")]
        [SerializeField, Range(-1f, 1f)] private float _dotThreshold = -0.15f;

        [Tooltip("If enabled, compares full 3D velocity/tangent. Disable to compare on XZ plane only.")]
        [SerializeField] private bool _useFull3D = false;

        [Tooltip("Higher value prefers splines whose tangent aligns with movement at crossings/loops.")]
        [SerializeField, Min(0f)] private float _directionMatchWeight = 2f;

        private SplinePath _splinePath;
        private SplineContainer _splineContainer;

        public RoadDirection DefaultDirection => _defaultDirection;

#if UNITY_EDITOR
        [Button("Generate Per-Spline Direction List", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0.4f)]
        private void GenerateSplineDirectionList()
        {
            EnsureReferences();

            if (_splineContainer == null)
            {
                Debug.LogWarning("[SplinePathDirection] No SplineContainer found — cannot generate list.");
                return;
            }

            int count = _splineContainer.Splines.Count;
            var newList = new List<SplineDirectionEntry>(count);

            for (int i = 0; i < count; i++)
            {
                // Preserve existing direction if an entry already exists for this index
                RoadDirection existingDir = _defaultDirection;
                if (i < _splineDirections.Count)
                    existingDir = _splineDirections[i].Direction;

                newList.Add(new SplineDirectionEntry
                {
                    SplineIndex = i,
                    Label = $"Spline {i}",
                    Direction = existingDir
                });
            }

            _splineDirections = newList;
            Debug.Log($"[SplinePathDirection] Generated {count} spline direction entries.");

            UnityEditor.EditorUtility.SetDirty(this);
        }

        [Button("Validate List", ButtonSizes.Large), GUIColor(0.4f, 0.6f, 1f)]
        private void ValidateSplineDirectionList()
        {
            EnsureReferences();

            if (_splineContainer == null)
            {
                Debug.LogWarning("[SplinePathDirection] No SplineContainer found — cannot validate.");
                return;
            }

            int splineCount = _splineContainer.Splines.Count;
            int listCount = _splineDirections.Count;

            if (listCount == splineCount)
            {
                Debug.Log($"[SplinePathDirection] ✓ List is valid — {listCount} entries match {splineCount} splines.");
            }
            else
            {
                Debug.LogWarning(
                    $"[SplinePathDirection] ✗ Mismatch — list has {listCount} entries but container has {splineCount} splines. " +
                    $"Press 'Generate Per-Spline Direction List' to rebuild."
                );
            }
        }
#endif

        private void OnValidate()
        {
            EnsureReferences();
        }

        private void Awake()
        {
            EnsureReferences();
        }

        private void EnsureReferences()
        {
            if (_splinePath == null)
                _splinePath = GetComponent<SplinePath>();

            if (_splinePath != null && _splineContainer == null)
                _splineContainer = _splinePath.splineContainer;
        }

        /// <summary>
        /// Returns the configured direction for a given spline index,
        /// falling back to _defaultDirection if the list is missing or too short.
        /// </summary>
        private RoadDirection GetDirectionForSpline(int splineIndex)
        {
            if (_splineDirections != null && splineIndex < _splineDirections.Count)
                return _splineDirections[splineIndex].Direction;

            return _defaultDirection;
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

            if (GetDirectionForSpline(splineIndex) == RoadDirection.Backward)
                splineForward = -splineForward;

            Vector3 compareVelocity = NormalizeForMode(velocity);

            return Vector3.Dot(compareVelocity, splineForward) < _dotThreshold;
        }

        /// <summary>
        /// Finds the closest point across ALL splines to a world position,
        /// then checks if the velocity is going the wrong way on that spline.
        /// </summary>
        public bool CheckWrongWayFromPosition(Vector3 worldPosition, Vector3 velocity)
        {
            if (!TryGetNearestSpline(worldPosition, velocity, out int nearestSplineIndex, out float nearestT, out _))
                return false;

            return CheckWrongWay(nearestSplineIndex, nearestT, velocity);
        }

        public bool TryGetNearestDistanceSqr(Vector3 worldPosition, out float nearestDistanceSqr)
        {
            // Distance-only variant for external systems
            return TryGetNearestSpline(worldPosition, Vector3.zero, out _, out _, out nearestDistanceSqr);
        }

        private bool TryGetNearestSpline(Vector3 worldPosition, Vector3 velocity, out int nearestSplineIndex, out float nearestT, out float nearestDist)
        {
            nearestSplineIndex = 0;
            nearestT = 0f;
            nearestDist = float.MaxValue;

            if (_splineContainer == null || _splineContainer.Splines.Count == 0)
                return false;

            float3 localPosition = _splineContainer.transform.InverseTransformPoint(worldPosition);
            Vector3 compareVelocity = NormalizeForMode(velocity);
            bool useDirectionBias = compareVelocity.sqrMagnitude > 0.0001f && _directionMatchWeight > 0f;

            for (int i = 0; i < _splineContainer.Splines.Count; i++)
            {
                SplineUtility.GetNearestPoint(
                    _splineContainer.Splines[i],
                    localPosition,
                    out float3 nearestPoint,
                    out float t
                );

                float distSqr = math.distancesq(localPosition, nearestPoint);
                float score = distSqr;

                if (useDirectionBias)
                {
                    Vector3 forward = SampleSplineForward(i, t);
                    if (GetDirectionForSpline(i) == RoadDirection.Backward)
                        forward = -forward;

                    float absDot = Mathf.Abs(Vector3.Dot(compareVelocity, forward));
                    float directionPenalty = 1f - absDot; // 0 is best, 1 is worst
                    score += directionPenalty * _directionMatchWeight;
                }

                if (score < nearestDist)
                {
                    nearestDist = score;
                    nearestSplineIndex = i;
                    nearestT = t;
                }
            }

            return true;
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

            return NormalizeForMode((Vector3)forward);
        }

        private Vector3 NormalizeForMode(Vector3 v)
        {
            if (_useFull3D)
                return v.normalized;

            Vector3 flat = new Vector3(v.x, 0f, v.z);
            return flat.normalized;
        }

        /// <summary>
        /// Returns the configured travel direction vector (flat, normalized) for the
        /// spline closest to <paramref name="worldPosition"/>.
        /// </summary>
        public Vector3 GetTravelDirectionAtPosition(Vector3 worldPosition)
        {
            if (!TryGetNearestSpline(worldPosition, Vector3.zero, out int nearestSplineIndex, out float nearestT, out _))
                return Vector3.zero;

            Vector3 forward = SampleSplineForward(nearestSplineIndex, nearestT);

            if (GetDirectionForSpline(nearestSplineIndex) == RoadDirection.Backward)
                forward = -forward;

            return forward;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_splineContainer == null) return;

            int steps = 25;
            for (int splineIndex = 0; splineIndex < _splineContainer.Splines.Count; splineIndex++)
            {
                RoadDirection dir = GetDirectionForSpline(splineIndex);

                for (int i = 0; i <= steps; i++)
                {
                    float t = i / (float)steps;
                    _splineContainer.Evaluate(splineIndex, t, out float3 pos, out _, out _);
                    Vector3 worldPos = (Vector3)pos;

                    Vector3 forward = SampleSplineForward(splineIndex, t);
                    if (dir == RoadDirection.Backward) forward = -forward;

                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(worldPos, forward * 2f);

                    Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
                    Gizmos.DrawRay(worldPos + forward * 2f, (-forward + right) * 0.5f);
                    Gizmos.DrawRay(worldPos + forward * 2f, (-forward - right) * 0.5f);
                }
            }
        }
#endif
    }
}