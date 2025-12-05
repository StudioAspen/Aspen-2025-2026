using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhiteFlash : MonoBehaviour
{
    [Header("Effect Parameters")]
    [SerializeField] private float _flashDuration = 1.0f;
    [SerializeField] private float _holdDuration = 1.5f;
    [SerializeField] private float _fadeDuration = 2.0f;
    [SerializeField] private AnimationCurve _fadeInTransition;
    [SerializeField] private AnimationCurve _fadeOutTransition;


    private float startingIntensity = 0.0f;
    private const float MAX_INTENSITY = 1.0f;

    [SerializeField] private Material _flashMaterial;

    void Start()
    {
        _flashMaterial.SetFloat("_Intensity", startingIntensity);
        _flashMaterial.SetFloat("_Alpha", 0f);
    }

    void Update()
    {
        if(Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(ActivateFlash());
        }
    }

    private IEnumerator ActivateFlash()
    {
        _flashMaterial.SetFloat("_Intensity", startingIntensity);
        _flashMaterial.SetFloat("_Alpha", 0f);

        // First the white flash by quickly ramping intensity.
        float elapsedTime = 0.0f;
        while(elapsedTime <  _flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float lerpedIntensity = Mathf.Lerp(0f, MAX_INTENSITY, elapsedTime / _flashDuration);
            float lerpedAlpha = Mathf.Lerp(0.0f, 1.0f, elapsedTime / _flashDuration);
            // Debug.Log("Lerped Intensity: " + lerpedIntensity);
            _flashMaterial.SetFloat("_Intensity", lerpedIntensity);
            _flashMaterial.SetFloat("_Alpha", lerpedAlpha);
            yield return null;
        }

        yield return new WaitForSeconds(_holdDuration);

        elapsedTime = 0.0f;
        while(elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // I'm considering using a different interpolation function for a differnet transitional effect.
            float lerpedIntensity = _fadeOutTransition.Evaluate(elapsedTime);
            // Debug.Log("Lerped Intensity: " + lerpedIntensity);
            _flashMaterial.SetFloat("_Intensity", lerpedIntensity);
            yield return null;
        }

        _flashMaterial.SetFloat("_Alpha", 0f); // Reset alpha so the scene doesn't stay dark, but I wonder how this will work if we actually change scenes.
    }
}
