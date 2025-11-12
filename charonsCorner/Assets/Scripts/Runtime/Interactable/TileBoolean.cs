using UnityEngine;
using System;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// The tile logic script is attached to the prefabs IncorrectTile/CorrectTile
    /// It will set the light on and any scripting that uses turning the light on/off can change the fade duration in/out and the light intensity itself
    /// </summary>
    /// 
    public class TileBoolean : MonoBehaviour
    {
        [Header("Tile Settings")]
        [field: SerializeField] public bool IsCorrectTile { get; private set; }

        [Header("Light Settings")]
        [SerializeField] private Light _tileLight; 
        [SerializeField] private float _lightOnIntensity = 2f; 
        [SerializeField] private float _lightOffIntensity = 0f;
        [SerializeField] private float _lightFadeDuration = 0.5f; 

        
        public static event Action OnWrongTileStepped;

        private void Awake()
        {
            if (_tileLight == null)
                _tileLight = GetComponentInChildren<Light>();

           
            if (_tileLight != null) // Ensure the light starts off
                _tileLight.intensity = _lightOffIntensity;
        }

        public void SetLightState(bool isOn)
        {
            if (_tileLight != null)
            {
                float targetIntensity = isOn ? _lightOnIntensity : _lightOffIntensity;
                StopAllCoroutines(); // Stop any ongoing fade coroutine
                StartCoroutine(FadeLightCoroutine(targetIntensity, _lightFadeDuration));
            }
        }

        private System.Collections.IEnumerator FadeLightCoroutine(float targetIntensity, float duration)
        {
            if (_tileLight == null)
                yield break;

            float startIntensity = _tileLight.intensity;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _tileLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / duration);
                yield return null;
            }

            _tileLight.intensity = targetIntensity;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out GameplayPlayerController player))
            {
                if (!IsCorrectTile)
                {
                    OnWrongTileStepped?.Invoke();
                }
            }
        }
    }
}