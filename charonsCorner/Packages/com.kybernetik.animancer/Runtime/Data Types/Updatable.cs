// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

namespace Animancer
{
    /// <summary>[Pro-Only] An object that can be updated during Animancer's animation updates.</summary>
    /// <remarks>
    /// See <see cref="IUpdatable"/> for an example of how to use this class.
    /// Simply inherit from this instead of implementing <see cref="IUpdatable"/> directly.
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/Updatable
    public abstract class Updatable : IUpdatable
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        int IUpdatable.UpdatableIndex { get; set; } = IUpdatable.List.NotInList;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public abstract void Update();

        /************************************************************************************************************************/
    }
}

