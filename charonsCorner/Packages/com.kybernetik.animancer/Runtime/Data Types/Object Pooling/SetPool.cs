// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Collections.Generic;

namespace Animancer
{
    /// <summary>Convenience methods for accessing <see cref="SetPool{T}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/SetPool
    public static class SetPool
    {
        /************************************************************************************************************************/

        /// <summary>Returns a spare <see cref="HashSet{T}"/> if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release{T}(HashSet{T})"/> it when you are done.</remarks>
        public static HashSet<T> Acquire<T>()
            => SetPool<T>.Instance.Acquire();

        /// <summary>Returns a spare <see cref="HashSet{T}"/> if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release{T}(HashSet{T})"/> it when you are done.</remarks>
        public static void Acquire<T>(out HashSet<T> set)
            => set = Acquire<T>();

        /************************************************************************************************************************/

        /// <summary>Clears the `set` and adds it to the list of spares so it can be reused.</summary>
        public static void Release<T>(this HashSet<T> set)
            => SetPool<T>.Instance.Release(set);

        /// <summary>Clears the `set`, adds it to the list of spares so it can be reused, and sets it to <c>null</c>.</summary>
        public static void Release<T>(ref HashSet<T> set)
        {
            Release(set);
            set = null;
        }

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/

    /// <summary>An <see cref="ObjectPool{T}"/> for <see cref="HashSet{T}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/SetPool_1
    public class SetPool<T> : CollectionPool<T, HashSet<T>>
    {
        /************************************************************************************************************************/

        /// <summary>Singleton.</summary>
        public static SetPool<T> Instance = new();

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override HashSet<T> New()
            => new();

        /************************************************************************************************************************/
    }
}

