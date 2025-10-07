// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Collections.Generic;

namespace Animancer
{
    /// <summary>An <see cref="ObjectPool{T}"/> for <see cref="ICollection{T}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/CollectionPool_2
    public abstract class CollectionPool<TItem, TCollection> : ObjectPool<TCollection>
        where TCollection : class, ICollection<TItem>// The non-generic ICollection doesn't have Count.
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override TCollection Acquire()
        {
            var collection = base.Acquire();
            AssertEmpty(collection);
            return collection;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Release(TCollection collection)
        {
            collection.Clear();
            base.Release(collection);
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Asserts that the `collection` is empty.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertEmpty(TCollection collection)
        {
#if UNITY_ASSERTIONS
            if (collection.Count != 0)
                throw new UnityEngine.Assertions.AssertionException(
                    $"A pooled {collection.GetType().GetNameCS()} is not empty.{NotResetError}",
                    null);
#endif
        }

        /************************************************************************************************************************/
    }
}

