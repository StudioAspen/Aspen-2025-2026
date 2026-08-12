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
        [SerializeField] private AnimationStringsSO _animationStrings;
        [SerializeField] private List<BowleyAnimation> animations = new List<BowleyAnimation>();

        [Button("Update Scriptable Object List")]
        private void UpdateScriptableObjectList()
        {
            if (_animationStrings == null)
            {
                Debug.LogWarning("[BowleyAnimationCaller] No AnimationStringsSO assigned!");
                return;
            }

            bool changed = false;
            foreach (var anim in animations)
            {
                if (string.IsNullOrEmpty(anim.MMGameEvent)) continue;

                if (!_animationStrings.AnimationEvents.Contains(anim.MMGameEvent))
                {
                    _animationStrings.AnimationEvents.Add(anim.MMGameEvent);
                    changed = true;
                    Debug.Log($"[BowleyAnimationCaller] Added '{anim.MMGameEvent}' to {_animationStrings.name}");
                }
            }

            if (changed)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(_animationStrings);
                UnityEditor.AssetDatabase.SaveAssets();
#endif
            }
            else
            {
                Debug.Log("[BowleyAnimationCaller] No new animation events to add.");
            }
        }

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
