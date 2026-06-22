using TMPro;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace CharonsCorner.Runtime
{
    public class DriftUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DriftHandler _driftHandler;
        [SerializeField] private GameplayPlayerController _playerController;

        [Header("UI Text")]
        [SerializeField] private TextMeshProUGUI _chargeText;
        [SerializeField] private float _blinkSpeed = 10f;
        [SerializeField] private Color _blinkColor = Color.red;

        [Header("Drift Indicator")]
        [SerializeField] private LineRenderer _driftIndicatorLine;
        [SerializeField] private float _minIndicatorLength = 1f;
        [SerializeField] private float _maxIndicatorLength = 5f;
        [SerializeField] private float _minIndicatorWidth = 0.05f;
        [SerializeField] private float _maxIndicatorWidth = 0.2f;
        [SerializeField] private Color _minChargeColor = Color.white;
        [SerializeField] private Color _maxChargeColor = Color.red;

        [Header("Arrowhead")]
        [SerializeField] private MeshRenderer _arrowheadRenderer;
        [SerializeField] private float _minArrowheadSize = 0.2f;
        [SerializeField] private float _maxArrowheadSize = 0.6f;

        [Header("Skull Indicator")]
        [SerializeField] private RectTransform _skullParent;
        [SerializeField] private RectTransform _skullImage;
        [SerializeField] private RectTransform _centerImage;
        [SerializeField] private UnityEngine.UI.Image _radialFillImage;
        [SerializeField] private RectTransform _skullLine;
        [SerializeField] private Color _skullLineColor = Color.white;
        [SerializeField] private float _skullLineWidth = 2f;
        [SerializeField] private float _rotationRadius = 100f;
        [SerializeField] private float _skullLerpSpeed = 10f;
        [SerializeField] private Color _skullStartColor = Color.white;
        [SerializeField] private Color _skullEndColor = Color.red;
        [SerializeField] private Color _radialStartColor = Color.white;
        [SerializeField] private Color _radialEndColor = Color.red;
        [SerializeField] private Vector3 _minSkullScale = Vector3.one;
        [SerializeField] private Vector3 _maxSkullScale = Vector3.one * 1.5f;

        [Header("Identical Skull Indicator (360 degrees)")]
        [SerializeField] private RectTransform _identicalSkullParent;
        [SerializeField] private RectTransform _identicalSkullImage;
        [SerializeField] private RectTransform _identicalSkullLine;
        [SerializeField] private float _identicalSkullLineWidth = 2f;
        [SerializeField] private float _identicalRotationRadius = 100f;
        [SerializeField] private Vector3 _identicalMinSkullScale = Vector3.one;
        [SerializeField] private Vector3 _identicalMaxSkullScale = Vector3.one * 1.5f;

        [Header("Identical Springs")]
        [SerializeField] private MMSpringScale _identicalSkullSpringScale;
        [SerializeField] private MMSpringRectTransformPosition _identicalSkullSpringPosition;

        [Header("Springs")]
        [SerializeField] private MMSpringScale _skullSpringScale;
        [SerializeField] private MMSpringRectTransformPosition _skullSpringPosition;
        [SerializeField] private MMSpringScale _centerSpringScale;
        [SerializeField] private MMSpringScale _radialSpringScale;
        [SerializeField] private Vector3 _skullBumpScale = new Vector3(0.5f, 0.5f, 0.5f);
        [SerializeField] private Vector3 _radialBumpScale = new Vector3(0.5f, 0.5f, 0.5f);
        [SerializeField] private Vector3 _skullBumpPosition = new Vector3(0f, 20f, 0f);

        private MaterialPropertyBlock _propBlock;
        private float _currentSkullAngle;
        private float _lastTargetAngle;
        private float _currentIdenticalSkullAngle;
        private float _lastIdenticalTargetAngle;
        private float _launchedChargeRatio;
        private bool _isSkullActive;
        private bool _hasBumped;

        private void Awake()
        {
            _propBlock = new MaterialPropertyBlock();

            if (_chargeText != null)
                _chargeText.gameObject.SetActive(false);

            if (_driftIndicatorLine != null)
                _driftIndicatorLine.gameObject.SetActive(false);

            if (_arrowheadRenderer != null)
                _arrowheadRenderer.gameObject.SetActive(false);

            if (_skullParent != null)
            {
                _skullParent.gameObject.SetActive(true);
                _skullParent.localScale = Vector3.zero;
                if (_centerImage != null)
                {
                    _skullParent.localPosition = _centerImage.localPosition;
                }
            }
            else if (_skullImage != null)
            {
                _skullImage.gameObject.SetActive(true); // Keep it active for springs
                _skullImage.localScale = Vector3.zero;
                if (_centerImage != null)
                {
                    _skullImage.localPosition = _centerImage.localPosition;
                }
            }

            if (_centerImage != null)
            {
                _centerImage.gameObject.SetActive(true); // Keep active for springs
                _centerImage.localScale = Vector3.zero;
            }

            if (_radialFillImage != null)
            {
                _radialFillImage.gameObject.SetActive(true); // Keep active for springs
                _radialFillImage.transform.localScale = Vector3.zero;
            }

            if (_skullLine != null)
            {
                _skullLine.gameObject.SetActive(false);
                
                // Pre-configure line if it's an Image
                if (_skullLine.TryGetComponent<UnityEngine.UI.Image>(out var img))
                {
                    img.color = _skullLineColor;
                }
            }

            if (_identicalSkullParent != null)
            {
                _identicalSkullParent.gameObject.SetActive(true);
                _identicalSkullParent.localScale = Vector3.zero;
                if (_centerImage != null)
                {
                    _identicalSkullParent.localPosition = _centerImage.localPosition;
                }
            }
            else if (_identicalSkullImage != null)
            {
                _identicalSkullImage.gameObject.SetActive(true);
                _identicalSkullImage.localScale = Vector3.zero;
                if (_centerImage != null)
                {
                    _identicalSkullImage.localPosition = _centerImage.localPosition;
                }
            }

            if (_identicalSkullLine != null)
            {
                _identicalSkullLine.gameObject.SetActive(false);
                if (_identicalSkullLine.TryGetComponent<UnityEngine.UI.Image>(out var img))
                {
                    img.color = _skullLineColor;
                }
            }
        }

        private void HideSkullUI()
        {
            if (_skullParent != null)
            {
                _skullParent.localScale = Vector3.zero;
            }

            if (_skullSpringScale != null)
                _skullSpringScale.MoveTo(Vector3.zero);
            if (_skullSpringPosition != null && _centerImage != null)
                _skullSpringPosition.MoveTo(_centerImage.localPosition);
            
            if (_centerSpringScale != null)
                _centerSpringScale.MoveTo(Vector3.zero);
            if (_radialSpringScale != null)
                _radialSpringScale.MoveTo(Vector3.zero);
            
            _isSkullActive = false;
            _hasBumped = false;

            if (_skullLine != null && _skullLine.gameObject.activeSelf)
                _skullLine.gameObject.SetActive(false);

            if (_identicalSkullParent != null)
            {
                _identicalSkullParent.localScale = Vector3.zero;
            }

            if (_identicalSkullSpringScale != null)
                _identicalSkullSpringScale.MoveTo(Vector3.zero);
            if (_identicalSkullSpringPosition != null && _centerImage != null)
                _identicalSkullSpringPosition.MoveTo(_centerImage.localPosition);

            if (_identicalSkullLine != null && _identicalSkullLine.gameObject.activeSelf)
                _identicalSkullLine.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_driftHandler == null || _playerController == null) return;

            bool screenUIEnabled = !GameManager.IsScreenspaceUIHidden;

            if (_driftHandler.IsDrifting)
            {
                // If we're in the charge phase, update normally.
                if (_playerController.DriftSuperState.SubStateMachine.CurrentState == _playerController.DriftSuperState.DriftingChargeState)
                {
                    if (screenUIEnabled)
                    {
                        UpdateChargeText();
                        UpdateSkullIndicator();
                    }
                    else
                    {
                        if (_chargeText != null && _chargeText.gameObject.activeSelf) _chargeText.gameObject.SetActive(false);
                        HideSkullUI();
                    }
                    
                    UpdateDriftIndicator();
                    _hasBumped = false;
                }
                // If we transition to boost, show the "BOOST" text once.
                else 
                {
                    if (screenUIEnabled && _chargeText != null)
                    {
                        if (!_hasBumped)
                        {
                            _launchedChargeRatio = _playerController.DriftSuperState.GetCurrentChargeRatio();
                            UpdateChargeText();
                        }

                        // User wants a "bump" instead of immediate disappearance
                        if (!_hasBumped)
                        {
                            if (_skullSpringScale != null) _skullSpringScale.Bump(_skullBumpScale);
                            if (_skullSpringPosition != null) _skullSpringPosition.Bump(_skullBumpPosition);
                            if (_radialSpringScale != null) _radialSpringScale.Bump(_radialBumpScale);
                            _hasBumped = true;
                        }

                        BlinkBoostUI();
                    }
                    else
                    {
                        if (_chargeText != null && _chargeText.gameObject.activeSelf) _chargeText.gameObject.SetActive(false);
                        HideSkullUI();
                    }

                    if (_driftIndicatorLine != null && _driftIndicatorLine.gameObject.activeSelf)
                        _driftIndicatorLine.gameObject.SetActive(false);

                    if (_arrowheadRenderer != null && _arrowheadRenderer.gameObject.activeSelf)
                        _arrowheadRenderer.gameObject.SetActive(false);
                }

                return;
            }

            if (!_driftHandler.IsOffCooldown)
            {
                // Keep showing boost text and blinking until cooldown is over
                if (screenUIEnabled && _chargeText != null && _chargeText.gameObject.activeSelf)
                {
                    BlinkBoostUI();
                }
                else if (!screenUIEnabled)
                {
                    if (_chargeText != null && _chargeText.gameObject.activeSelf) _chargeText.gameObject.SetActive(false);
                    HideSkullUI();
                }
            }
            else
            {
                // Cooldown complete, hide UI
                if (_chargeText != null && _chargeText.gameObject.activeSelf)
                    _chargeText.gameObject.SetActive(false);

                if (_driftIndicatorLine != null && _driftIndicatorLine.gameObject.activeSelf)
                    _driftIndicatorLine.gameObject.SetActive(false);

                if (_arrowheadRenderer != null && _arrowheadRenderer.gameObject.activeSelf)
                    _arrowheadRenderer.gameObject.SetActive(false);

                HideSkullUI();
            }
        }

        private void LateUpdate()
        {
            if ((_isSkullActive || _hasBumped) && !GameManager.IsScreenspaceUIHidden)
            {
                UpdateSkullLine();
                UpdateIdenticalSkullLine();
            }
        }

        public void RefreshUIVisibility()
        {
            if (GameManager.IsScreenspaceUIHidden)
            {
                if (_chargeText != null) _chargeText.gameObject.SetActive(false);
                HideSkullUI();
            }
        }

        private void BlinkBoostUI()
        {
            float t = Mathf.PingPong(Time.unscaledTime * _blinkSpeed, 1f);

            if (_chargeText != null)
            {
                Color launchedColor = Color.Lerp(Color.white, _blinkColor, _launchedChargeRatio);
                _chargeText.color = Color.Lerp(Color.white, launchedColor, t);
            }

            if (_radialFillImage != null)
            {
                Color launchedColor = Color.Lerp(_radialStartColor, _radialEndColor, _launchedChargeRatio);
                _radialFillImage.color = Color.Lerp(_radialStartColor, launchedColor, t);
            }
        }

        private void UpdateDriftIndicator()
        {
            if (_driftIndicatorLine == null) return;

            if (!_driftIndicatorLine.gameObject.activeSelf)
                _driftIndicatorLine.gameObject.SetActive(true);

            if (_arrowheadRenderer != null && !_arrowheadRenderer.gameObject.activeSelf)
                _arrowheadRenderer.gameObject.SetActive(true);

            float chargeRatio = _playerController.DriftSuperState.GetCurrentChargeRatio();
            Vector3 driftDir = _playerController.DriftSuperState.DriftingBoostState.GetDriftDirection();

            // If grounded, project the drift direction onto the ground/slope plane
            if (_playerController.IsGrounded)
            {
                Vector3 groundNormal = Vector3.up;
                if (_playerController.SlopeSensor != null && _playerController.SlopeSensor.Hit.normal != Vector3.zero)
                {
                    groundNormal = _playerController.SlopeSensor.Hit.normal;
                }

                driftDir = Vector3.ProjectOnPlane(driftDir, groundNormal).normalized;
            }

            Color currentColor = Color.Lerp(_minChargeColor, _maxChargeColor, chargeRatio);
            _driftIndicatorLine.startColor = currentColor;
            _driftIndicatorLine.endColor = currentColor;

            float currentLength = Mathf.Lerp(_minIndicatorLength, _maxIndicatorLength, chargeRatio);
            float currentWidth = Mathf.Lerp(_minIndicatorWidth, _maxIndicatorWidth, chargeRatio);

            _driftIndicatorLine.startWidth = currentWidth;
            _driftIndicatorLine.endWidth = currentWidth;

            _driftIndicatorLine.SetPosition(0, _playerController.transform.position);
            Vector3 endPosition = _playerController.transform.position + driftDir * currentLength;
            _driftIndicatorLine.SetPosition(1, endPosition);

            if (_arrowheadRenderer != null)
            {
                _arrowheadRenderer.transform.position = endPosition;
                _arrowheadRenderer.transform.rotation = Quaternion.LookRotation(driftDir);

                float currentArrowSize = Mathf.Lerp(_minArrowheadSize, _maxArrowheadSize, chargeRatio);
                _arrowheadRenderer.transform.localScale = Vector3.one * currentArrowSize;

                // Color Arrowhead:
                _propBlock.SetColor("_BaseColor", currentColor); // Common URP property
                _propBlock.SetColor("_Color", currentColor);     // Fallback for Standard/Legacy
                _arrowheadRenderer.SetPropertyBlock(_propBlock);
            }
        }

        private void UpdateSkullIndicator()
        {
            if (_centerImage == null) return;
            RectTransform skullTarget = _skullParent != null ? _skullParent : _skullImage;
            if (skullTarget == null) return;

            if (!_isSkullActive)
            {
                if (!skullTarget.gameObject.activeSelf)
                    skullTarget.gameObject.SetActive(true);
                
                if (_skullSpringScale != null)
                    _skullSpringScale.MoveTo(Vector3.one);
                else
                    skullTarget.localScale = Vector3.one;

                if (_centerSpringScale != null)
                    _centerSpringScale.MoveTo(Vector3.one);

                if (_radialSpringScale != null)
                    _radialSpringScale.MoveTo(Vector3.one);
                
                _isSkullActive = true;
                _currentSkullAngle = 0f; // Start at due north
                _lastTargetAngle = 0f;
                _currentIdenticalSkullAngle = 0f;
                _lastIdenticalTargetAngle = 0f;
            }

            float chargeRatio = _playerController.DriftSuperState.GetCurrentChargeRatio();

            if (_skullParent != null)
            {
                _skullParent.localScale = Vector3.Lerp(_minSkullScale, _maxSkullScale, chargeRatio);
            }

            if (_radialFillImage != null)
            {
                // User wants: "fill up from 0 to .25 clockwise right and counter clockwise left as it turns"
                // 90 degrees corresponds to 0.25 fill amount (90/360).
                float fillRatio = Mathf.Abs(_currentSkullAngle) / 360f;
                _radialFillImage.fillAmount = fillRatio;
                _radialFillImage.fillClockwise = _currentSkullAngle <= 0;

                _radialFillImage.color = Color.Lerp(_radialStartColor, _radialEndColor, chargeRatio);
            }

            if (_skullImage != null && _skullImage.TryGetComponent<UnityEngine.UI.Image>(out var skullImg))
            {
                skullImg.color = Color.Lerp(_skullStartColor, _skullEndColor, chargeRatio);
            }

            Vector3 driftDir = _playerController.DriftSuperState.DriftingBoostState.GetDriftDirection();
            Vector3 initialVelDir = _playerController.DriftSuperState.InitialVelocity;

            float targetAngle = 0f;

            if (initialVelDir.sqrMagnitude >= 0.0001f)
            {
                initialVelDir.Normalize();

                // Project both onto XZ plane (though driftDir should already be planar)
                Vector2 initialDir2D = new Vector2(initialVelDir.x, initialVelDir.z);
                Vector2 driftDir2D = new Vector2(driftDir.x, driftDir.z);

                // Calculate signed angle between initial velocity and current drift direction
                float rawAngle = Vector2.SignedAngle(initialDir2D, driftDir2D);
                
                // --- Identical (Unclamped) logic first ---
                float rawIdenticalAngle = rawAngle;
                // To prevent flipping sides when crossing the 180-degree mark behind the player for identical:
                if (Vector2.Dot(initialDir2D, driftDir2D) < -0.1f)
                {
                    if (Mathf.Abs(rawIdenticalAngle - _lastIdenticalTargetAngle) > 180f)
                    {
                        rawIdenticalAngle = _lastIdenticalTargetAngle > 0 ? 180f : -180f;
                    }
                }
                _lastIdenticalTargetAngle = rawIdenticalAngle;
                float identicalTargetAngle = rawIdenticalAngle;

                // --- Original Clamped logic ---
                // To prevent flipping sides when crossing the 180-degree mark behind the player:
                // If the player is pointing "backwards" (Dot < 0), we check if the angle jumped significantly.
                // If it did, we force it to stay on the previous side.
                targetAngle = rawAngle;
                if (Vector2.Dot(initialDir2D, driftDir2D) < -0.1f) // pointing roughly backwards
                {
                    if (Mathf.Abs(rawAngle - _lastTargetAngle) > 180f)
                    {
                        // If it jumped from -180 to 180 (or vice versa), keep the sign of the last target angle.
                        targetAngle = _lastTargetAngle > 0 ? 180f : -180f;
                    }
                }

                _lastTargetAngle = targetAngle;

                // Clamp the target angle to respect the maximum drift angle (usually 90 degrees)
                float maxAngle = _playerController.DriftSuperState.DriftingBoostState.MaxAngle;
                targetAngle = Mathf.Clamp(targetAngle, -maxAngle, maxAngle);

                // Now use both angles for lerping
                _currentSkullAngle = Mathf.LerpAngle(_currentSkullAngle, targetAngle, _skullLerpSpeed * Time.deltaTime);
                _currentIdenticalSkullAngle = Mathf.LerpAngle(_currentIdenticalSkullAngle, identicalTargetAngle, _skullLerpSpeed * Time.deltaTime);
            }
            else
            {
                // Smoothly return both to zero if no initial velocity (shouldn't happen during drift usually)
                _currentSkullAngle = Mathf.LerpAngle(_currentSkullAngle, 0f, _skullLerpSpeed * Time.deltaTime);
                _currentIdenticalSkullAngle = Mathf.LerpAngle(_currentIdenticalSkullAngle, 0f, _skullLerpSpeed * Time.deltaTime);
            }

            // The user wants: "if they are staring forward while drifting in their initial velocity, 
            // it will be rotating it at due north".
            // "Due north" in UI is Up (0, 1, 0), which is 90 degrees in polar coordinates.
            float uiAngle = 90f + _currentSkullAngle; 
            
            float radians = uiAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0) * _rotationRadius;

            Vector3 targetPosition = _centerImage.localPosition + offset;
            if (_skullSpringPosition != null)
            {
                _skullSpringPosition.MoveTo(targetPosition);
            }
            else
            {
                skullTarget.localPosition = targetPosition;
            }
            
            // Optional: Rotate the skull itself to face away from center or follow direction
            skullTarget.localRotation = Quaternion.Euler(0, 0, _currentSkullAngle);

            UpdateIdenticalSkullIndicator();
        }

        private void UpdateIdenticalSkullIndicator()
        {
            RectTransform identicalTarget = _identicalSkullParent != null ? _identicalSkullParent : _identicalSkullImage;
            if (identicalTarget == null || _centerImage == null) return;

            if (_isSkullActive && !identicalTarget.gameObject.activeSelf)
            {
                identicalTarget.gameObject.SetActive(true);
                if (_identicalSkullSpringScale != null)
                    _identicalSkullSpringScale.MoveTo(Vector3.one);
                else
                    identicalTarget.localScale = Vector3.one;
            }

            float chargeRatio = _playerController.DriftSuperState.GetCurrentChargeRatio();

            if (_identicalSkullParent != null)
            {
                _identicalSkullParent.localScale = Vector3.Lerp(_identicalMinSkullScale, _identicalMaxSkullScale, chargeRatio);
            }

            if (_identicalSkullImage != null && _identicalSkullImage.TryGetComponent<UnityEngine.UI.Image>(out var skullImg))
            {
                skullImg.color = Color.Lerp(_skullStartColor, _skullEndColor, chargeRatio);
            }

            float uiAngle = 90f + _currentIdenticalSkullAngle;
            float radians = uiAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0) * _identicalRotationRadius;

            Vector3 targetPosition = _centerImage.localPosition + offset;
            if (_identicalSkullSpringPosition != null)
            {
                _identicalSkullSpringPosition.MoveTo(targetPosition);
            }
            else
            {
                identicalTarget.localPosition = targetPosition;
            }

            identicalTarget.localRotation = Quaternion.Euler(0, 0, _currentIdenticalSkullAngle);
        }

        private void UpdateIdenticalSkullLine()
        {
            RectTransform identicalTarget = _identicalSkullParent != null ? _identicalSkullParent : _identicalSkullImage;
            if (_identicalSkullLine == null || identicalTarget == null || _centerImage == null) return;

            if (!_identicalSkullLine.gameObject.activeSelf)
                _identicalSkullLine.gameObject.SetActive(true);

            Vector3 startPos = _centerImage.localPosition;
            Vector3 endPos = identicalTarget.localPosition;
            Vector3 diff = endPos - startPos;
            float distance = diff.magnitude;
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

            _identicalSkullLine.localPosition = startPos + diff * 0.5f;
            _identicalSkullLine.localRotation = Quaternion.Euler(0, 0, angle);
            _identicalSkullLine.sizeDelta = new Vector2(distance, _identicalSkullLineWidth);
        }

        private void UpdateSkullLine()
        {
            RectTransform skullTarget = _skullParent != null ? _skullParent : _skullImage;
            if (_skullLine == null || skullTarget == null || _centerImage == null) return;

            if (!_skullLine.gameObject.activeSelf)
                _skullLine.gameObject.SetActive(true);

            Vector3 startPos = _centerImage.localPosition;
            Vector3 endPos = skullTarget.localPosition;
            Vector3 diff = endPos - startPos;
            float distance = diff.magnitude;
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

            _skullLine.localPosition = startPos + diff * 0.5f;
            _skullLine.localRotation = Quaternion.Euler(0, 0, angle);
            _skullLine.sizeDelta = new Vector2(distance, _skullLineWidth);
        }

        private void UpdateChargeText()
        {
            if (_chargeText == null) return;

            if (!_chargeText.gameObject.activeSelf)
                _chargeText.gameObject.SetActive(true);

            float chargeRatio;

            if (_playerController.DriftSuperState.SubStateMachine.CurrentState == _playerController.DriftSuperState.DriftingChargeState)
            {
                chargeRatio = _playerController.DriftSuperState.GetCurrentChargeRatio();
                _chargeText.text = $"{(chargeRatio * 100f):0}%";
                _chargeText.color = Color.white; // Ensure color is reset when charging
            }
            else
            {
                chargeRatio = _launchedChargeRatio;
                _chargeText.text = $"{(chargeRatio * 100f):0}%";
            }
        }
    }
}
