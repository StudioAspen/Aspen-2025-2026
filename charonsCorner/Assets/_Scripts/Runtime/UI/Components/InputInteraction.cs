using Animancer;
using MoreMountains.Feedbacks;
using UnityEngine;
using TMPro;

namespace CharonsCorner.Runtime
{
    public class InputInteraction : MonoBehaviour
    {
        [Header("Feedbacks")]
        [SerializeField] private MMF_Player _enterFeedback;
        [SerializeField] private MMF_Player _exitFeedback;

        [Header("Audio")]
        [SerializeField] private StringAsset _appearSfx;
        [SerializeField] private StringAsset _disappearSfx;

        [Header("Blinking Effect")]
        [SerializeField] private TMP_Text _textComponent;
        [SerializeField] private Color _startColor = Color.white;
        [SerializeField] private Color _endColor = Color.white;
        [SerializeField] private float _blinkRate = 1f;

        private void Update()
        {
            if (_textComponent != null)
            {
                float t = Mathf.PingPong(Time.time * _blinkRate, 1f);
                _textComponent.color = Color.Lerp(_startColor, _endColor, t);
            }
        }

        public void Appear()
        {
            if (_appearSfx != null)
            {
                AudioManager.Instance.Play(_appearSfx);
            }
            
            if (_enterFeedback != null)
            {
                _enterFeedback.PlayFeedbacks();
            }
        }

        public void Disappear()
        {
            if (_disappearSfx != null)
            {
                AudioManager.Instance.Play(_disappearSfx);
            }
            
            if (_exitFeedback != null)
            {
                _exitFeedback.PlayFeedbacks();
            }
        }
    }
}
