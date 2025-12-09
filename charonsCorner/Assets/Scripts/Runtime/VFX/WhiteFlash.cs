using UnityEngine;

public class WhiteFlash : MonoBehaviour
{
    [Header("Effect Parameters")]
    [SerializeField] private Material _flashMaterial;
    [SerializeField] private Color flashColor;

    [SerializeField] private AnimationCurve _fadeInTransition;
    [SerializeField] private AnimationCurve _fadeOutTransition;

    [SerializeField] public float minRange = 10f;
    [SerializeField] public float maxRange = 50f;

    [SerializeField] private Transform playerTransform;

    private float _MAX_INTENSITY = 1.5f;

    void Start()
    {
        _flashMaterial.SetFloat("_Intensity", 0f);
        _flashMaterial.SetFloat("_Alpha", 0f);
        _flashMaterial.SetColor("_FlashColor", flashColor);
    }

    void OnDisable()
    {
        _flashMaterial.SetFloat("_Intensity", 0f);
        _flashMaterial.SetFloat("_Alpha", 0f);
    }

    void Update()
    {
        UpdateFlash();
    }

    private void UpdateFlash()
    {
        _flashMaterial.SetColor("_FlashColor", flashColor);

        float distance = Vector3.Distance(playerTransform.position, transform.position);
        float clampedDistance = Mathf.Clamp(distance, minRange, maxRange);
        float inverseProportion = 1 - Mathf.InverseLerp(minRange, maxRange, clampedDistance);
        
        float lerpedAlpha = Mathf.Lerp(0.0f, 1.0f, inverseProportion);
        float lerpedIntensity = Mathf.Lerp(0f, _MAX_INTENSITY, inverseProportion);

        _flashMaterial.SetFloat("_Intensity", lerpedIntensity);
        _flashMaterial.SetFloat("_Alpha", lerpedAlpha);

    }

}
