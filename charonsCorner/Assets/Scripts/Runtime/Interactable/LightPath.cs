using UnityEngine;
using UnityEngine.Splines;


namespace CharonsCorner.Runtime
{
    /// <summary>
    /// This script is attached to a LightSphere object. 
    /// The path needs to have the 'ToggleTransparencyLightPath' material added, which will render the 
    /// material invisible until the light's radius is updated. 
    /// You will need to create one spline, which is the ground path.
    /// Make sure to select the spline path as the spline container in the inspector under this script. 
    /// Additional note. On the LightSphere object, we have one child object. Global volume bloom, providing the glowing effect.
    /// </summary>
    public class LightPath : MonoBehaviour
    {
        [Header("Light Sphere Variables")]
        [field: SerializeField] public Transform Player { get; private set; }
        [field: SerializeField] public SplineContainer SplinePath { get; private set; }
        [field: SerializeField] public float MoveDistance { get; private set; } = 10f;
        [field: SerializeField] public float Speed { get; private set; } = 10f;
        [SerializeField] private float _transitionDuration = 0.5f; // Duration for smoothing between splines

        [Header("Shader Variables")]
        [SerializeField] private Material _revealMaterial;
        [SerializeField] private Transform _revealField;
        [SerializeField] private float _revealRadius = 5.0f;
        [SerializeField] private float _fadeWidth = 0.5f;
        [SerializeField] private bool _proximityEnabled = true;

        private int _currentSplineIndex = 0;
        private float _progress = 0f;
        private float _cachedSplineLength = 0f;
        private bool _isTransitioning = false;
        private Vector3 _transitionStartPos;
        private Quaternion _transitionStartRot;
        private Vector3 _transitionEndPos;
        private Quaternion _transitionEndRot;
        private float _transitionTimer = 0f;

        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private int _initialSplineIndex;
        private float _initialProgress;

        private void Start()
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _initialSplineIndex = _currentSplineIndex;
            _initialProgress = _progress;
        }

        private void OnEnable()
        {
            PlayerDeathHandler.OnPlayerDeath += ResetToStart;
        }

        private void OnDisable()
        {
            PlayerDeathHandler.OnPlayerDeath -= ResetToStart;
        }

        private void ResetToStart()
        {
            _currentSplineIndex = _initialSplineIndex;
            _progress = _initialProgress;
            _cachedSplineLength = 0f;
            _isTransitioning = false;
            _transitionTimer = 0f;

            transform.SetPositionAndRotation(_startPosition, _startRotation);
        }

        private void Update()
        {
            float distance = Vector3.Distance(transform.position, Player.position);

            if (distance <= MoveDistance && SplinePath != null && SplinePath.Splines.Count > 0)
            {
                if (_isTransitioning)
                {
                    _transitionTimer += Time.deltaTime;
                    float t = Mathf.Clamp01(_transitionTimer / _transitionDuration);
                    transform.position = Vector3.Lerp(_transitionStartPos, _transitionEndPos, t);
                    transform.rotation = Quaternion.Slerp(_transitionStartRot, _transitionEndRot, t);
                    if (t >= 1f)
                        _isTransitioning = false;

                    return;
                }

                Spline currentSpline = SplinePath.Splines[_currentSplineIndex];

                if (_cachedSplineLength == 0f)
                    _cachedSplineLength = SplineUtility.CalculateLength(currentSpline, SplinePath.transform.localToWorldMatrix);

                _progress += (Speed * Time.deltaTime) / _cachedSplineLength;

                if (_progress > 1f)
                {
                    int nextSplineIndex = _currentSplineIndex + 1;
                    if (nextSplineIndex >= SplinePath.Splines.Count)
                    {
                        _currentSplineIndex = SplinePath.Splines.Count - 1;
                        _progress = 1f;
                    }
                    else
                    {
                        // Prepare for smooth transition
                        Spline nextSpline = SplinePath.Splines[nextSplineIndex];
                        Vector3 endPos = currentSpline.EvaluatePosition(1f);
                        Vector3 endTangent = currentSpline.EvaluateTangent(1f);
                        endPos = SplinePath.transform.TransformPoint(endPos);
                        endTangent = SplinePath.transform.TransformDirection(endTangent);
                        Quaternion endRot = Quaternion.LookRotation(endTangent);

                        Vector3 startPos = nextSpline.EvaluatePosition(0f);
                        Vector3 startTangent = nextSpline.EvaluateTangent(0f);
                        startPos = SplinePath.transform.TransformPoint(startPos);
                        startTangent = SplinePath.transform.TransformDirection(startTangent);
                        Quaternion startRot = Quaternion.LookRotation(startTangent);

                        _transitionStartPos = endPos;
                        _transitionStartRot = endRot;
                        _transitionEndPos = startPos;
                        _transitionEndRot = startRot;
                        _transitionTimer = 0f;
                        _isTransitioning = true;

                        _currentSplineIndex = nextSplineIndex;
                        _progress = 0f;
                        return;
                    }
                }

                Vector3 position = currentSpline.EvaluatePosition(_progress);
                Vector3 tangent = currentSpline.EvaluateTangent(_progress);

                position = SplinePath.transform.TransformPoint(position);
                tangent = SplinePath.transform.TransformDirection(tangent);

                transform.position = position;
                transform.rotation = Quaternion.LookRotation(tangent);
            }

            if (_revealMaterial != null)
            {
                _revealMaterial.SetFloat("_ProximityEnabled", _proximityEnabled ? 1.0f : 0.0f);

                if (_proximityEnabled && _revealField != null)
                {
                    _revealMaterial.SetVector("_RevealPosition", _revealField.position);
                    _revealMaterial.SetFloat("_RevealRadius", _revealRadius);
                    _revealMaterial.SetFloat("_FadeWidth", _fadeWidth);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_proximityEnabled && _revealField != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_revealField.position, _revealRadius);
            }
        }
    }
}