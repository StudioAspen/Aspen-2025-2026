using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
namespace CharonsCorner.Runtime
{
    public class SwingingObstacle : MonoBehaviour
    {
        [SerializeField] private float _swingTime = 0.5f;
        [SerializeField] private float _delay = 0f;
        [SerializeField] private Ease _swingEase;
        [SerializeField] private float _swingAmount = 10f;
        
        void Start()
        {
            StartTween();
        }
        private void StartTween()
        {
            DOTween.Kill(this);
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, -_swingAmount);
            Vector3 swingAngle = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, _swingAmount);
            transform.DOLocalRotate(swingAngle, _swingTime).SetEase(_swingEase).SetLoops(-1, LoopType.Yoyo).SetDelay(_delay);
        }
    }
}
