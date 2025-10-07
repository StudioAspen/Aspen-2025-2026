// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>An <see cref="ITransition"/> with some additional details (mainly for the Unity Editor GUI).</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">
    /// Transitions</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ITransitionDetailed
    /// 
    public interface ITransitionDetailed : ITransition, IHasEvents
    {
        /************************************************************************************************************************/

        /// <summary>Can this transition create a valid <see cref="AnimancerState"/>?</summary>
        /// <remarks>
        /// Use <see cref="AnimancerUtilities.IsValid(ITransitionDetailed)"/> to check for <c>null</c> as well.
        /// </remarks>
        bool IsValid { get; }

        /// <summary>What will the value of <see cref="AnimancerState.IsLooping"/> be for the created state?</summary>
        bool IsLooping { get; }

        /// <summary>The <see cref="AnimancerState.NormalizedTime"/> to start the animation at.</summary>
        /// <remarks><see cref="float.NaN"/> allows the animation to continue from its current time.</remarks>
        float NormalizedStartTime { get; set; }

        /// <summary>The maximum amount of time the animation is expected to take (in seconds).</summary>
        /// <remarks>The actual duration can vary in states like <see cref="ManualMixerState"/>.</remarks>
        float MaximumDuration { get; }

        /// <summary>The <see cref="AnimancerNodeBase.Speed"/> to play the animation at.</summary>
        float Speed { get; set; }

        /************************************************************************************************************************/
    }

    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerUtilities
    public static partial class AnimancerUtilities
    {
        /************************************************************************************************************************/

        /// <summary>Is the `transition` not null and <see cref="ITransitionDetailed.IsValid"/>?</summary>
        public static bool IsValid(this ITransitionDetailed transition)
            => transition != null
            && transition.IsValid;

        /************************************************************************************************************************/

        /// <summary>Returns the <see cref="ITransitionDetailed.IsValid"/> with support for <see cref="IWrapper"/>.</summary>
        public static bool IsValid(this ITransition transition)
        {
            if (transition == null)
                return false;

            if (TryGetWrappedObject(transition, out ITransitionDetailed detailed))
                return detailed.IsValid;

            return true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the <see cref="ITransition.FadeDuration"/>
        /// or <see cref="float.NaN"/> if it's <c>null</c> or throws an exception.
        /// </summary>
        public static float TryGetFadeDuration(this ITransition transition)
        {
            if (transition == null)
                return float.NaN;

            try
            {
                return transition.FadeDuration;
            }
            catch
            {
                return float.NaN;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Outputs the <see cref="Motion.isLooping"/> or <see cref="ITransitionDetailed.IsLooping"/>.</summary>
        /// <remarks>Returns false if the `motionOrTransition` is null or an unsupported type.</remarks>
        public static bool TryGetIsLooping(object motionOrTransition, out bool isLooping)
        {
            if (motionOrTransition is Motion motion)
            {
                if (motion != null)
                {
                    isLooping = motion.isLooping;
                    return true;
                }
            }
            else if (TryGetWrappedObject(motionOrTransition, out ITransitionDetailed transition))
            {
                isLooping = transition.IsLooping;
                return true;
            }

            isLooping = false;
            return false;
        }

        /************************************************************************************************************************/

        /// <summary>Outputs the <see cref="AnimationClip.length"/> or <see cref="ITransitionDetailed.MaximumDuration"/>.</summary>
        /// <remarks>Returns false if the `motionOrTransition` is null or an unsupported type.</remarks>
        public static bool TryGetLength(object motionOrTransition, out float length)
        {
            if (motionOrTransition is AnimationClip clip)
            {
                if (clip != null)
                {
                    length = clip.length;
                    return true;
                }
            }
            else if (TryGetWrappedObject(motionOrTransition, out ITransitionDetailed transition))
            {
                length = transition.MaximumDuration;
                return true;
            }

            length = 0;
            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Tries to calculate the amount of time (in seconds)
        /// from the <see cref="ITransitionDetailed.NormalizedStartTime"/>
        /// too the <see cref="AnimancerEvent.Sequence.NormalizedEndTime"/>,
        /// including the <see cref="ITransitionDetailed.Speed"/>
        /// </summary>
        public static bool TryCalculateDuration(object transition, out float duration)
        {
            if (!TryGetWrappedObject(transition, out ITransitionDetailed detailed))
            {
                duration = 0;
                return false;
            }

            var speed = detailed.Speed;

            duration = detailed.MaximumDuration;

            var normalizedStartTime = detailed.NormalizedStartTime;
            if (!float.IsNaN(normalizedStartTime))
                duration *= 1 - normalizedStartTime;

            var normalizedEndTime = detailed.Events.NormalizedEndTime;
            if (!float.IsNaN(normalizedEndTime))
                duration *= normalizedEndTime;

            if (speed.IsFinite() && speed != 0)
                duration /= speed;

            return true;
        }

        /************************************************************************************************************************/
    }
}

