using UnityEngine;
using System;

namespace CharonsCorner.Runtime
{
    public class TileLogic : MonoBehaviour
    {
        [Header("Tile Settings")]
        public bool isCorrectTile; // Bool for swapping between correct and wrong tiles

        [Header("Light Settings")]
        [SerializeField] private Light tileLight; // Light associated with this tile
        [SerializeField] private float lightOnIntensity = 2f; // Intensity when the light is on
        [SerializeField] private float lightOffIntensity = 0f; // Intensity when the light is off
        [SerializeField] private float lightFadeDuration = 0.5f; // Duration for light fade in/out

        // Event triggered when the player steps on a wrong tile
        public static event Action OnWrongTileStepped;

        private void Awake()
        {
            if (tileLight == null)
                tileLight = GetComponentInChildren<Light>();

            // Ensure the light starts off
            if (tileLight != null)
                tileLight.intensity = lightOffIntensity;
        }

        public void SetLightState(bool isOn)
        {
            if (tileLight != null)
            {
                float targetIntensity = isOn ? lightOnIntensity : lightOffIntensity;
                StopAllCoroutines(); // Stop any ongoing fade coroutine
                StartCoroutine(FadeLight(targetIntensity, lightFadeDuration));
            }
        }

        private System.Collections.IEnumerator FadeLight(float targetIntensity, float duration)
        {
            if (tileLight == null)
                yield break;

            float startIntensity = tileLight.intensity;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                tileLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / duration);
                yield return null;
            }

            tileLight.intensity = targetIntensity;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out PlayerController player))
            {
                if (isCorrectTile)
                {
                    Debug.Log("Player stepped on the correct tile.");
                }
                else
                {
                    Debug.Log("Player stepped on the wrong tile!");
                    OnWrongTileStepped?.Invoke(); // Trigger the centralized event
                }
            }
        }
    }
}