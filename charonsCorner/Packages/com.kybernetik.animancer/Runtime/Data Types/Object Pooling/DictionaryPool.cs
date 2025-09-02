// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Collections.Generic;

namespace Animancer
{
    /// <summary>Convenience methods for accessing <see cref="DictionaryPool{TKey,TValue}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/DictionaryPool
    public static class DictionaryPool
    {
        /************************************************************************************************************************/

        /// <summary>Returns a spare <see cref="Dictionary{TKey,TValue}"/> if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release{TKey,TValue}(Dictionary{TKey,TValue})"/> it when you are done.</remarks>
        public static Dictionary<TKey, TValue> Acquire<TKey, TValue>()
            => DictionaryPool<TKey, TValue>.Instance.Acquire();

        /// <summary>Returns a spare <see cref="Dictionary{TKey,TValue}"/> if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release{TKey,TValue}(Dictionary{TKey,TValue})"/> it when you are done.</remarks>
        public static void Acquire<TKey, TValue>(out Dictionary<TKey, TValue> dictionary)
            => dictionary = Acquire<TKey, TValue>();

        /************************************************************************************************************************/

        /// <summary>Clears the `dictionary` and adds it to the list of spares so it can be reused.</summary>
        public static void Release<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
            => DictionaryPool<TKey, TValue>.Instance.Release(dictionary);

        /// <summary>Clears the `dictionary`, adds it to the list of spares so it can be reused, and sets it to <c>null</c>.</summary>
        public static void Release<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary)
        {
            Release(dictionary);
            dictionary = null;
        }

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/

    /// <summary>An <see cref="ObjectPool{T}"/> for <see cref="Dictionary{TKey,TValue}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/DictionaryPool_2
    public class DictionaryPool<TKey, TValue> : CollectionPool<KeyValuePair<TKey, TValue>, Dictionary<TKey, TValue>>
    {
        /************************************************************************************************************************/

        /// <summary>Singleton.</summary>
        public static DictionaryPool<TKey, TValue> Instance = new();

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override Dictionary<TKey, TValue> New()
            => new();

        /************************************************************************************************************************/
    }
}

