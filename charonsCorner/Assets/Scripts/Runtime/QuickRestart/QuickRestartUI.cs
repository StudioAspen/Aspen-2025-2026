using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class QuickRestartUI : MonoBehaviour
    {
        [SerializeField, Required] private QuickRestarter _quickRestarter;
        [SerializeField, Required] private GameObject _quickRestartPanel;
        [SerializeField, Required] private Image _fillImage;

        private void OnEnable()
        {
            if(_quickRestarter.IsQuickRestartHeld)
                OnQuickRestartStarted();
            else
                OnQuickRestartEnded();
            
            _quickRestarter.OnQuickRestartStarted.AddListener(OnQuickRestartStarted);
            _quickRestarter.OnQuickRestartEnded.AddListener(OnQuickRestartEnded);
        }

        private void OnDisable()
        {
            OnQuickRestartEnded();
            
            _quickRestarter.OnQuickRestartStarted.RemoveListener(OnQuickRestartStarted);
            _quickRestarter.OnQuickRestartStarted.RemoveListener(OnQuickRestartEnded);
        }
        
        private void OnQuickRestartStarted()
        {
            _quickRestartPanel.SetActive(true);
        }       
        
        private void OnQuickRestartEnded()
        {
            _quickRestartPanel.SetActive(false);
        }

        private void Update()
        {
            _fillImage.fillAmount = _quickRestarter.HeldTimer / _quickRestarter.HoldDuration;
        }
    }
}