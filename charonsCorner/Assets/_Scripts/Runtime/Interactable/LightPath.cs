using UnityEngine;
using UnityEngine.Splines;
using MoreMountains.Feedbacks;


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
        [SerializeField] private float _lookAheadDistance = 5f;
        [SerializeField] private float _followSpeed = 5f;
        [SerializeField] private MMSpringScale _orbSpringScale;
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
            if (SplinePath == null || SplinePath.Splines.Count == 0 || Player == null)
            {
                UpdateShader();
                return;
            }

            if (_isTransitioning)
            {
                _transitionTimer += Time.deltaTime;
                float t = Mathf.Clamp01(_transitionTimer / _transitionDuration);
                transform.position = Vector3.Lerp(_transitionStartPos, _transitionEndPos, t);
                transform.rotation = Quaternion.Slerp(_transitionStartRot, _transitionEndRot, t);
                if (t >= 1f)
                    _isTransitioning = false;

                UpdateShader();
                return;
            }

            // 1. Find where the player is on the current spline
            Spline currentSpline = SplinePath.Splines[_currentSplineIndex];
            Vector3 localPlayerPos = SplinePath.transform.InverseTransformPoint(Player.position);
            
            SplineUtility.GetNearestPoint(currentSpline, localPlayerPos, out _, out float playerT);

            if (_cachedSplineLength <= 0f)
                _cachedSplineLength = SplineUtility.CalculateLength(currentSpline, SplinePath.transform.localToWorldMatrix);

            // 2. Calculate target progress (player progress + look ahead)
            float lookAheadT = _lookAheadDistance / _cachedSplineLength;
            float targetT = Mathf.Clamp01(playerT + lookAheadT);

            // 3. Smoothly move orb progress towards target
            // We use Lerp for simplicity, or we could use SmoothDamp for more control
            _progress = Mathf.Lerp(_progress, targetT, Time.deltaTime * _followSpeed);

            // 4. Handle spline transitions
            if (_progress > 0.99f && playerT > 0.95f)
            {
                int nextSplineIndex = _currentSplineIndex + 1;
                if (nextSplineIndex < SplinePath.Splines.Count)
                {
                    PrepareTransition(nextSplineIndex);
                    UpdateShader();
                    return;
                }
                else
                {
                    // End of all splines
                    if (_orbSpringScale != null && _progress > 0.999f)
                    {
                        _orbSpringScale.MoveTo(Vector3.zero);
                    }
                }
            }

            // 5. Update Orb Position and Rotation
            Vector3 position = currentSpline.EvaluatePosition(_progress);
            Vector3 tangent = currentSpline.EvaluateTangent(_progress);

            transform.position = SplinePath.transform.TransformPoint(position);
            transform.rotation = Quaternion.LookRotation(SplinePath.transform.TransformDirection(tangent));

            UpdateShader();
        }

        private void PrepareTransition(int nextSplineIndex)
        {
            Spline currentSpline = SplinePath.Splines[_currentSplineIndex];
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
            _cachedSplineLength = 0f; // Reset length for the next spline
        }

        private void UpdateShader()
        {
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