using MoreMountains.Feedbacks;
using UnityEngine;
using TMPro;

namespace CharonsCorner.Runtime
{
    public class InputInteraction : MonoBehaviour
    {
        [SerializeField] private MMF_Player _enterFeedback;
        [SerializeField] private MMF_Player _exitFeedback;

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
            if (_enterFeedback != null)
            {
                _enterFeedback.PlayFeedbacks();
            }
        }

        public void Disappear()
        {
            if (_exitFeedback != null)
            {
                _exitFeedback.PlayFeedbacks();
            }
        }
    }
}
