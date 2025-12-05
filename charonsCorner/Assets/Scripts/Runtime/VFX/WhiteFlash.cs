using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhiteFlash : MonoBehaviour
{
    [Header("Effect Parameters")]
    private float _flashDuration = 1.5f;
    private float _holdDuration = 1.0f;
    private float _fadeDuration = 2.0f;

    private float startingIntensity = 0.0f;
    private const float MAX_INTENSITY = 1.0f;

    [SerializeField] private Material _flashMaterial;

    void Start()
    {
        _flashMaterial.SetFloat("_Intensity", startingIntensity);
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
            float lerpedIntensity = Mathf.Lerp(MAX_INTENSITY, 0.0f, elapsedTime / _flashDuration);
            // Debug.Log("Lerped Intensity: " + lerpedIntensity);
            _flashMaterial.SetFloat("_Intensity", lerpedIntensity);
            yield return null;
        }

    }
}
