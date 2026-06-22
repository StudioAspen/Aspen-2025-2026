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

        public bool IsLoaded { get; private set; } = true;

        public async UniTask FadeIn()
        {
            IsLoaded = false;

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
            IsLoaded = false;

            if (!DOTween.IsTweening(_image))
                _image.SetImageAlpha(1f);

            _image.DOKill();

            gameObject.SetActive(true);

            await _image.DOFade(0f, _fadeOutDuration).SetEase(_fadeOutEase).SetUpdate(true).OnComplete(() => { 
                _image.SetImageAlpha(0f);
        
                if (nextPanel == null)
                {
                    // If we're finishing loading and no specific next panel was requested,
                    // we should clear all and let DefaultSceneUIPanelSetter handle it
                    // Debug.Log("[LoadingCanvas] FadeOut complete. nextPanel is null, calling CloseAll()");
                    UIPanel.CloseAll(true);
                }
                else
                {
                    // Debug.Log($"[LoadingCanvas] FadeOut complete. Focusing {nextPanel.name}");
                    UIPanel.Focus(nextPanel);
                }
                
                IsLoaded = true;
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