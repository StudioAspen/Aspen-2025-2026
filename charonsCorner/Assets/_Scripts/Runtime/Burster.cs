using UnityEngine;
using MoreMountains.Feedbacks;
using Animancer;

namespace CharonsCorner.Runtime
{
    public class Burster : SetTransformOnPlay
    {
        [SerializeField] private MMSpringScale _springScale;
        [SerializeField] private GlowController _glowController;
        [SerializeField] private StringAsset _burstAudioId;
        [SerializeField] private bool _playAtPosition = true;
        [SerializeField] private float _burstAudioRange = 20f;
        
        private Vector3 _originalScale;

        protected override void Awake()
        {
            _originalScale = transform.localScale;
            base.Awake();
            if (_springScale != null)
            {
                _springScale.MoveToInstant(_targetScale);
            }
        }

        public void Burst()
        {
            if (_burstAudioId != null && AudioManager.Instance != null)
            {
                if (_playAtPosition)
                {
                    AudioManager.Instance.Play(_burstAudioId, position: transform.position, maxDistance: _burstAudioRange);
                }
                else
                {
                    AudioManager.Instance.Play(_burstAudioId);
                }
            }

            if (_springScale != null)
            {
                _springScale.MoveTo(_originalScale);
            }

            if (_glowController != null)
            {
                _glowController.TurnOn();
            }
        }
    }
}
