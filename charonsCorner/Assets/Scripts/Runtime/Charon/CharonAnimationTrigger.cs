using System.Collections.Generic;
using Animancer;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CharonAnimationTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharonController _charonController;

        [Header("Config")]
        [SerializeField] private List<ClipTransition> _animations;

        private void Awake()
        {
            if (_charonController == null)
            {
                _charonController = FindFirstObjectByType<CharonController>();
            }
        }

        /// <summary>
        /// Requests the CharonController to play an animation from the local _animations list based on the provided index.
        /// </summary>
        /// <param name="index">The index of the animation in the _animations list to play.</param>
        public void PlayAnimationByIndex(int index)
        {
            if (index < 0 || index >= _animations.Count)
            {
                Debug.LogWarning($"[CharonAnimationTrigger] Animation index {index} is out of range for the _animations list (Count: {_animations.Count}).");
                return;
            }

            ClipTransition clip = _animations[index];

            if (_charonController != null)
            {
                _charonController.PlayAnimation(clip);
            }
            else
            {
                // Try to find it again if it was missed in Awake (e.g. spawned later)
                _charonController = Object.FindFirstObjectByType<CharonController>();
                if (_charonController != null)
                {
                    _charonController.PlayAnimation(clip);
                }
                else
                {
                    Debug.LogWarning("[CharonAnimationTrigger] CharonController reference is missing and could not be found in the scene.");
                }
            }
        }
    }
}
