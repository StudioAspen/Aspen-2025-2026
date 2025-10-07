// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

namespace Animancer
{
    /// <summary>
    /// An object with a <see cref="Key"/> which can be used in dictionaries and hash sets.
    /// </summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/playing/keys">
    /// Keys</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/IHasKey
    public interface IHasKey
    {
        /************************************************************************************************************************/

        /// <summary>A key which can be used in dictionaries and hash sets.</summary>
        object Key { get; }

        /************************************************************************************************************************/
    }
}

