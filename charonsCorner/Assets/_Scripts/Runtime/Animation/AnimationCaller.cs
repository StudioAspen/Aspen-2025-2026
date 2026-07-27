using System;
using System.Collections.Generic;
using Animancer;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class AnimationCaller : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        [Serializable]
        public class AnimationEntry
        {
            public string MMGameEvent;
            public ClipTransition AnimationClip;

            [Button("Test")]
            private void TestAnimation()
            {
                AnimationCaller caller = UnityEngine.Object.FindAnyObjectByType<AnimationCaller>();
                if (caller != null)
                {
                    caller.PlayAnimation(this);
                }
            }
        }

        [SerializeField] private AnimancerComponent _animancer;
        [SerializeField] private float _fadeDuration = 0.2f;
        [SerializeField] private List<AnimationEntry> _animations = new List<AnimationEntry>();

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
            foreach (var entry in _animations)
            {
                if (entry.MMGameEvent == gameEvent.EventName)
                {
                    PlayAnimation(entry);
                    break;
                }
            }
        }

        public void PlayAnimation(AnimationEntry entry)
        {
            if (_animancer == null)
            {
                Debug.LogWarning($"[AnimationCaller] AnimancerComponent is not assigned on {gameObject.name}");
                return;
            }

            if (entry.AnimationClip != null)
            {
                _animancer.Play(entry.AnimationClip, _fadeDuration);
            }
        }
    }
}