using UnityEngine;
using MoreMountains.Feedbacks;

namespace CharonsCorner.Runtime
{
    public class Burster : SetTransformOnPlay
    {
        [SerializeField] private MMSpringScale _springScale;
        [SerializeField] private GlowController _glowController;
        
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
