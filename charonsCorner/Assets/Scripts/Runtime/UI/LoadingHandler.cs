using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class LoadingHandler : MonoBehaviour
    {
        [SerializeField, Required] private Image image;

        [Header("Fade In Config")]
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private Ease fadeInEase = Ease.Linear;

        [Header("Fade Out Config")]
        [SerializeField] private float fadeOutDuration = 1f;
        [SerializeField] private Ease fadeOutEase = Ease.Linear;

        public async UniTask FadeIn()
        {
            image.SetImageAlpha(0f);
            gameObject.SetActive(true);

            await image.DOFade(1f, fadeInDuration).SetEase(fadeInEase).SetUpdate(true);
            image.SetImageAlpha(1f);
        }

        public async UniTask FadeOut()
        {
            gameObject.SetActive(true);

            await image.DOFade(0f, fadeOutDuration).SetEase(fadeOutEase).SetUpdate(true);
            image.SetImageAlpha(0f);

            gameObject.SetActive(false);
        }
    }
}
