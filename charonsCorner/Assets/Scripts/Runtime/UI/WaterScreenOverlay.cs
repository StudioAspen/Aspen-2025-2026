using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CharonsCorner.Runtime
{
    public class WaterScreenOverlay : MonoBehaviour
    {
        [SerializeField] private Image overlayImage;
        [SerializeField] private float waveScaleAmount = 0.04f;
        [SerializeField] private float waveScaleSpeed = 1.2f;
        [SerializeField] private float waveAlphaMin = 0.3f;
        [SerializeField] private float waveAlphaMax = 0.45f;
        [SerializeField] private float waveAlphaSpeed = 1.5f;

        private Tween scaleTween;
        private Tween alphaTween;
        private Vector3 originalScale;

        private void Awake()
        {
            if (overlayImage != null)
                originalScale = overlayImage.rectTransform.localScale;
        }

        public void SetOverlayActive(bool active)
        {
            if (overlayImage == null) return;

            if (active)
            {
                // Set initial color and enable overlay
                var color = overlayImage.color;
                color.a = waveAlphaMin;
                overlayImage.color = color;
                overlayImage.enabled = true;

                // Wavy scale tween
                scaleTween?.Kill();
                scaleTween = overlayImage.rectTransform
                    .DOScale(originalScale * (1f + waveScaleAmount), waveScaleSpeed)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);

                // Wavy alpha tween
                alphaTween?.Kill();
                alphaTween = DOTween.To(
                        () => overlayImage.color.a,
                        a => {
                            var c = overlayImage.color;
                            c.a = a;
                            overlayImage.color = c;
                        },
                        waveAlphaMax,
                        waveAlphaSpeed)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
            else
            {
                // Stop tweens and hide overlay
                scaleTween?.Kill();
                alphaTween?.Kill();

                overlayImage.rectTransform.localScale = originalScale;
                var color = overlayImage.color;
                color.a = 0f;
                overlayImage.color = color;
                overlayImage.enabled = false;
            }
        }
    }
}