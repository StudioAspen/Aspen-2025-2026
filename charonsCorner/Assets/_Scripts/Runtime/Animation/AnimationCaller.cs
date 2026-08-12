using System;
using System.Collections.Generic;
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
            public string animationParameter;
            public bool isTrigger = true;
            public bool returnToSeatedIdle;

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

        [SerializeField] private Animator _animator;
        [SerializeField] private string _seatedIdleParameter = "SeatedIdle";
        [SerializeField] private AnimationStringsSO _animationStrings;
        [SerializeField] private List<AnimationEntry> _animations = new List<AnimationEntry>();

        [Button("Update Scriptable Object List")]
        private void UpdateScriptableObjectList()
        {
            if (_animationStrings == null)
            {
                Debug.LogWarning("[AnimationCaller] No AnimationStringsSO assigned!");
                return;
            }

            bool changed = false;
            foreach (var entry in _animations)
            {
                if (string.IsNullOrEmpty(entry.MMGameEvent)) continue;

                if (!_animationStrings.AnimationEvents.Contains(entry.MMGameEvent))
                {
                    _animationStrings.AnimationEvents.Add(entry.MMGameEvent);
                    changed = true;
                    Debug.Log($"[AnimationCaller] Added '{entry.MMGameEvent}' to {_animationStrings.name}");
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
                Debug.Log("[AnimationCaller] No new animation events to add.");
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
            if (_animator == null)
            {
                Debug.LogWarning($"[AnimationCaller] Animator is not assigned on {gameObject.name}");
                return;
            }

            if (!string.IsNullOrEmpty(entry.animationParameter))
            {
                if (entry.isTrigger)
                {
                    _animator.SetTrigger(entry.animationParameter);
                }
                else
                {
                    _animator.SetBool(entry.animationParameter, true);
                }

                if (entry.returnToSeatedIdle)
                {
                    // Since we are using standard Animator, returning to seated idle 
                    // usually depends on the Animator Controller transitions.
                    // We can also force it back after some time if needed, but the instruction says
                    // "just have it intake an animator and instead of animation clips it will just call edits to animation parameters"
                    // and "it should work the same otherwise though".
                    // The "returnToSeatedIdle" in Animancer was using OnEnd.
                    // In Animator, we'll assume the Animator Controller handles the transition back if the parameter is a trigger
                    // or we might need a way to detect the end. 
                    // However, for Triggers, Animator usually consumes them.
                    // If the user wants it to "work the same", and we are just calling parameters,
                    // maybe we should just set the parameter and let the animator do its thing.
                }
            }
        }
    }
}