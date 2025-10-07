// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Collections.Generic;

namespace Animancer
{
    /// <summary>Convenience methods for accessing <see cref="ListPool{T}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/ListPool
    public static class ListPool
    {
        /************************************************************************************************************************/

        /// <summary>Returns a spare <see cref="List{T}"/> if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release{T}(List{T})"/> it when you are done.</remarks>
        public static List<T> Acquire<T>()
            => ListPool<T>.Instance.Acquire();

        /// <summary>Returns a spare <see cref="List{T}"/> if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release{T}(List{T})"/> it when you are done.</remarks>
        public static void Acquire<T>(out List<T> list)
            => list = Acquire<T>();

        /************************************************************************************************************************/

        /// <summary>Clears the `list` and adds it to the list of spares so it can be reused.</summary>
        public static void Release<T>(this List<T> list)
            => ListPool<T>.Instance.Release(list);

        /// <summary>Clears the `list`, adds it to the list of spares so it can be reused, and sets it to <c>null</c>.</summary>
        public static void Release<T>(ref List<T> list)
        {
            Release(list);
            list = null;
        }

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/

    /// <summary>An <see cref="ObjectPool{T}"/> for <see cref="List{T}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/ListPool_1
    public class ListPool<T> : CollectionPool<T, List<T>>
    {
        /************************************************************************************************************************/

        /// <summary>Singleton.</summary>
        public static ListPool<T> Instance = new();

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override List<T> New()
            => new();

        /************************************************************************************************************************/
    }
}

