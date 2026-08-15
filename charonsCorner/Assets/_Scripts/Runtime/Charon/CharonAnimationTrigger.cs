using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CharonAnimationTrigger : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private List<string> _animationEvents;

        /// <summary>
        /// Triggers an MMGameEvent based on the animation event string at the provided index.
        /// </summary>
        /// <param name="index">The index of the animation event in the _animationEvents list.</param>
        public void PlayAnimationByIndex(int index)
        {
            if (index < 0 || index >= _animationEvents.Count)
            {
                Debug.LogWarning($"[CharonAnimationTrigger] Animation event index {index} is out of range (Count: {_animationEvents.Count}).");
                return;
            }

            string eventName = _animationEvents[index];

            if (!string.IsNullOrEmpty(eventName))
            {
                MMGameEvent.Trigger(eventName);
            }
            else
            {
                Debug.LogWarning($"[CharonAnimationTrigger] Animation event at index {index} is null or empty.");
            }
        }
    }
}
