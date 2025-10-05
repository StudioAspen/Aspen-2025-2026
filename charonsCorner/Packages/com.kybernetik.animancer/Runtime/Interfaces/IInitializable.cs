// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

namespace Animancer
{
    /// <summary>An object with an <see cref="Initialize(T)"/> method.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/IInitializable_1
    public interface IInitializable<T>
    {
        /************************************************************************************************************************/

        /// <summary>Initializes this object.</summary>
        void Initialize(T details);

        /************************************************************************************************************************/
    }
}

