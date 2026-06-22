using UnityEngine;
using Sirenix.OdinInspector;

namespace CharonsCorner.Runtime
{
    public class LerpMaterialOnDistance : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _active = true;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private float _minDistance = 2f;
        [SerializeField] private float _maxDistance = 10f;

        [Header("Shadow Color")]
        [SerializeField] private Color _shadowColorX = Color.black;
        [SerializeField] private Color _shadowColorY = Color.white;
        [SerializeField] private AnimationCurve _shadowColorCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Glow")]
        [SerializeField] private float _glowX = 0f;
        [SerializeField] private float _glowY = 1f;
        [SerializeField] private float _glowGapX = 0f;
        [SerializeField] private float _glowGapY = 0f;
        [SerializeField] private AnimationCurve _glowCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Glow Color")]
        [SerializeField] private Color _glowColorX = Color.black;
        [SerializeField] private Color _glowColorY = Color.white;
        [SerializeField] private AnimationCurve _glowColorCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Texture Glow Amount")]
        [SerializeField] private float _textureGlowAmountX = 0f;
        [SerializeField] private float _textureGlowAmountY = 1f;
        [SerializeField] private AnimationCurve _textureGlowAmountCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private Material _materialInstance;
        private HubPlayerController _playerController;
        private bool _wasActive;

        private Color _originalShadowColor;
        private float _originalGlow;
        private Color _originalGlowColor;
        private float _originalTextureGlowAmount;

        private static readonly int ShadowColorId = Shader.PropertyToID("_ShadowColor");
        private static readonly int GlowId = Shader.PropertyToID("_Glow");
        private static readonly int GlowColorId = Shader.PropertyToID("_GlowColor");
        private static readonly int TextureGlowAmountId = Shader.PropertyToID("_TextureGlowAmount");

        private void Awake()
        {
            if (_renderer != null)
            {
                _materialInstance = _renderer.material;
                StoreOriginalValues();
            }
            
            _wasActive = _active;
        }

        private void Update()
        {
            if (_active)
            {
                if (!_wasActive)
                {
                    // If it just became active, maybe we should re-store or just start lerping
                    // The requirement says: "If the script's active bool becomes false, it will return the material instance to whatever it was before the bool became true"
                    // This implies we should store state when it becomes true.
                    StoreOriginalValues();
                }

                LerpProperties();
            }
            else if (_wasActive)
            {
                RestoreOriginalValues();
            }

            _wasActive = _active;
        }

        private void StoreOriginalValues()
        {
            if (_materialInstance == null) return;
            
            if (_materialInstance.HasProperty(ShadowColorId)) _originalShadowColor = _materialInstance.GetColor(ShadowColorId);
            if (_materialInstance.HasProperty(GlowId)) _originalGlow = _materialInstance.GetFloat(GlowId);
            if (_materialInstance.HasProperty(GlowColorId)) _originalGlowColor = _materialInstance.GetColor(GlowColorId);
            if (_materialInstance.HasProperty(TextureGlowAmountId)) _originalTextureGlowAmount = _materialInstance.GetFloat(TextureGlowAmountId);
        }

        private void RestoreOriginalValues()
        {
            if (_materialInstance == null) return;

            if (_materialInstance.HasProperty(ShadowColorId)) _materialInstance.SetColor(ShadowColorId, _originalShadowColor);
            if (_materialInstance.HasProperty(GlowId)) _materialInstance.SetFloat(GlowId, _originalGlow);
            if (_materialInstance.HasProperty(GlowColorId)) _materialInstance.SetColor(GlowColorId, _originalGlowColor);
            if (_materialInstance.HasProperty(TextureGlowAmountId)) _materialInstance.SetFloat(TextureGlowAmountId, _originalTextureGlowAmount);
        }

        private void LerpProperties()
        {
            if (_materialInstance == null) return;

            if (_playerController == null)
            {
                _playerController = Object.FindAnyObjectByType<HubPlayerController>();
                if (_playerController == null) return;
            }

            float distance = Vector3.Distance(transform.position, _playerController.transform.position);
            
            // min distance: closer = 100% X
            // max distance: further = 100% Y
            float t = Mathf.InverseLerp(_minDistance, _maxDistance, distance);

            if (_materialInstance.HasProperty(ShadowColorId))
                _materialInstance.SetColor(ShadowColorId, Color.Lerp(_shadowColorX, _shadowColorY, _shadowColorCurve.Evaluate(t)));
            
            if (_materialInstance.HasProperty(GlowId))
            {
                float curveT = _glowCurve.Evaluate(t);
                float glowValue;
                if (_glowGapX == _glowGapY) // No gap or gap is a single point
                {
                    glowValue = Mathf.Lerp(_glowX, _glowY, curveT);
                }
                else
                {
                    // Calculate the "lengths" of the two segments
                    float segment1Length = Mathf.Abs(_glowGapX - _glowX);
                    float segment2Length = Mathf.Abs(_glowY - _glowGapY);
                    float totalLength = segment1Length + segment2Length;

                    if (totalLength > 0)
                    {
                        float splitT = segment1Length / totalLength;
                        if (curveT < splitT)
                        {
                            float normalizedT = curveT / splitT;
                            glowValue = Mathf.Lerp(_glowX, _glowGapX, normalizedT);
                        }
                        else
                        {
                            float normalizedT = (curveT - splitT) / (1f - splitT);
                            glowValue = Mathf.Lerp(_glowGapY, _glowY, normalizedT);
                        }
                    }
                    else
                    {
                        // If everything is same value, just pick one
                        glowValue = _glowX;
                    }
                }
                _materialInstance.SetFloat(GlowId, glowValue);
            }
            
            if (_materialInstance.HasProperty(GlowColorId))
                _materialInstance.SetColor(GlowColorId, Color.Lerp(_glowColorX, _glowColorY, _glowColorCurve.Evaluate(t)));
            
            if (_materialInstance.HasProperty(TextureGlowAmountId))
                _materialInstance.SetFloat(TextureGlowAmountId, Mathf.Lerp(_textureGlowAmountX, _textureGlowAmountY, _textureGlowAmountCurve.Evaluate(t)));
        }

        public void SetActive(bool active)
        {
            _active = active;
        }
    }
}
