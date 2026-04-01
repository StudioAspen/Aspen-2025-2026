using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class LoadingCanvas : Singleton<LoadingCanvas>
    {
        [SerializeField] private UIPanel _loadingPanel;
        [SerializeField, Required] private Image _image;

        [Header("Fade In Config")]
        [SerializeField] private float _fadeInDuration = 1f;
        [SerializeField] private Ease _fadeInEase = Ease.Linear;

        [Header("Fade Out Config")]
        [SerializeField] private float _fadeOutDuration = 1f;
        [SerializeField] private Ease _fadeOutEase = Ease.Linear;

        public async UniTask FadeIn()
        {
            if (!DOTween.IsTweening(_image))
                _image.SetImageAlpha(0f);

            _image.DOKill();

            UIPanel.Focus(_loadingPanel);

            await _image.DOFade(1f, _fadeInDuration).SetEase(_fadeInEase).SetUpdate(true).OnComplete(() => { 
                _image.SetImageAlpha(1f); 
            });
        }

        public async UniTask FadeOut(UIPanel nextPanel)
        {
            if (!DOTween.IsTweening(_image))
                _image.SetImageAlpha(1f);

            _image.DOKill();

            gameObject.SetActive(true);

            await _image.DOFade(0f, _fadeOutDuration).SetEase(_fadeOutEase).SetUpdate(true).OnComplete(() => { 
                _image.SetImageAlpha(0f);
                
                if (nextPanel == null)
                {
                    UIPanel.CloseAll();
                }
                else
                {
                    UIPanel.Focus(nextPanel);
                }
            });
        }
        
        public void Show()
        {
            FadeIn().Forget();
        }

        public void Hide(UIPanel nextPanel)
        {
            FadeOut(nextPanel).Forget();
        }
    }
}