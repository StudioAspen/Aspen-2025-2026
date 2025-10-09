// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

namespace Animancer
{
    /// <summary>An object which has an <see cref="AnimancerEvent.Sequence.Serializable"/>.</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">
    /// Animancer Events</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/IHasEvents
    public interface IHasEvents
    {
        /************************************************************************************************************************/

        /// <summary>Events which will be triggered as the animation plays.</summary>
        AnimancerEvent.Sequence Events { get; }

        /// <summary>Events which will be triggered as the animation plays.</summary>
        ref AnimancerEvent.Sequence.Serializable SerializedEvents { get; }

        /************************************************************************************************************************/
    }
}

