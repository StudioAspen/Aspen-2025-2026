// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System.Runtime.CompilerServices;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Caches <see cref="AnimationClip.events"/> to reduce garbage allocations.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimationEventCache
    /// 
    public static class AnimationEventCache
    {
        /************************************************************************************************************************/

        private static readonly ConditionalWeakTable<AnimationClip, AnimationEvent[]>
            ClipToEvents = new();

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the <see cref="AnimationClip.events"/> and caches the result to avoid allocating more memory with
        /// each subsequent call.
        /// </summary>
        public static AnimationEvent[] GetCachedEvents(this AnimationClip clip)
        {
            if (!ClipToEvents.TryGetValue(clip, out var events))
            {
                events = clip.events;
                ClipToEvents.Add(clip, events);
            }

            return events;
        }

        /************************************************************************************************************************/

        /// <summary>Clears the cache.</summary>
        public static void Clear()
            => ClipToEvents.Clear();

        /************************************************************************************************************************/

        /// <summary>Removes the `clip` from the cache so its events will be retrieved again next time.</summary>
        public static void Remove(AnimationClip clip)
            => ClipToEvents.Remove(clip);

        /************************************************************************************************************************/
    }
}

#endif

