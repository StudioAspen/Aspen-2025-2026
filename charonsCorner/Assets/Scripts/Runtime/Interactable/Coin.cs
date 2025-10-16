using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace CharonsCorner.Runtime
{
    public class Coin : MonoBehaviour
    {
        [Header("Coin Settings")]
        public LayerMask playerLayerMask;
        public Sprite coinSprite; // sprite UI

        [Header("Animation Settings")]
        public float spinSpeed = 180f; // degrees per second
        public float bobHeight = 0.2f;
        public float bobDuration = 0.7f;

        public float collectSpinSpeed = 720f;
        public float collectRiseAmount = 1.5f;
        public float collectDuration = 0.6f;
        public float fadeDuration = 0.3f;

        private Tween spinTween;
        private Tween bobTween;
        [SerializeField] private Renderer coinRenderer;
        private MaterialPropertyBlock propBlock;
        private float initialY;

        private void Start()
        {
            initialY = transform.position.y;
            propBlock = new MaterialPropertyBlock();

            // Continuous smooth spin on Y axis only
            spinTween = transform.DORotate(
                    new Vector3(0, 360, 0),
                    360f / spinSpeed,
                    RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);

            // Bobbing up and down
            bobTween = transform.DOMoveY(initialY + bobHeight, bobDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((1 << other.gameObject.layer & playerLayerMask.value) != 0)
            {
                // Update the UI
                CoinUIManager.Instance?.AddCoin(coinSprite);

                // Stop idle tweens
                spinTween?.Kill();
                bobTween?.Kill();

                // Start collect animation
                StartCoroutine(CollectAndDestroy());
            }
        }

        private IEnumerator CollectAndDestroy()
        {
            // Spin faster and move up
            Tween collectSpin = transform.DORotate(
                new Vector3(0, 360, 0), 360f / collectSpinSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

            Tween rise = transform.DOMoveY(transform.position.y + collectRiseAmount, collectDuration)
                .SetEase(Ease.OutCubic);

            // Fade out (if renderer/material supports _Color.a)
            float fadeStart = Time.time + collectDuration - fadeDuration;
            float fadeEnd = Time.time + collectDuration;

            float elapsed = 0f;
            while (elapsed < collectDuration)
            {
                elapsed += Time.deltaTime;

                // Fade out near the end
                if (coinRenderer != null && elapsed >= collectDuration - fadeDuration)
                {
                    float t = Mathf.InverseLerp(fadeStart, fadeEnd, Time.time);
                    coinRenderer.GetPropertyBlock(propBlock);
                    Color c = propBlock.GetColor("_Color");
                    c.a = Mathf.Lerp(1f, 0f, t);
                    propBlock.SetColor("_Color", c);
                    coinRenderer.SetPropertyBlock(propBlock);
                }

                yield return null;
            }

            collectSpin.Kill();
            rise.Kill();

            Destroy(gameObject);
        }
    }
}