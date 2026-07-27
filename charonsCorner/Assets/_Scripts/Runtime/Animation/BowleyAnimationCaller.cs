using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class BowleyAnimationCaller : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        [Serializable]
        public class BowleyAnimation
        {
            public string MMGameEvent;
            public float shakeSpeed;
            public float range;
            public Vector3 direction;

            [Button("Test")]
            private void TestAnimation()
            {
                BowleyAnimationCaller caller = UnityEngine.Object.FindAnyObjectByType<BowleyAnimationCaller>();
                if (caller != null)
                {
                    caller.PerformShake(this);
                }
            }
        }

        [SerializeField] private MMRotationShaker bowleyRotationShaker;
        [SerializeField] private List<BowleyAnimation> animations = new List<BowleyAnimation>();

        private void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
        }

        private void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
        }

        public void OnMMEvent(MMGameEvent gameEvent)
        {
            foreach (var anim in animations)
            {
                if (anim.MMGameEvent == gameEvent.EventName)
                {
                    PerformShake(anim);
                    break;
                }
            }
        }

        public void PerformShake(BowleyAnimation anim)
        {
            if (bowleyRotationShaker == null)
            {
                Debug.LogWarning($"[BowleyAnimationCaller] bowleyRotationShaker is not assigned on {gameObject.name}");
                return;
            }

            bowleyRotationShaker.ShakeSpeed = anim.shakeSpeed;
            bowleyRotationShaker.ShakeRange = anim.range;
            bowleyRotationShaker.ShakeMainDirection = anim.direction;
            bowleyRotationShaker.Play();
        }
    }
}
