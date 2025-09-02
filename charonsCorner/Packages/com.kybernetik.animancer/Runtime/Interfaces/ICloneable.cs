// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

//#define ASSERT_CLONE_TYPES

using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    /// <summary>An object that can be cloned.</summary>
    /// <remarks>See <see cref="Clone(CloneContext)"/> for example usage.</remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ICloneable_1
    public interface ICloneable<out T>
    {
        /************************************************************************************************************************/

        /// <summary>Creates a new object with the same type and values this.</summary>
        /// 
        /// <remarks>
        /// Calling this method directly is not generally recommended.
        /// Use <see cref="CloneableExtensions.Clone{T}(ICloneable{T})"/> if you don't have a `context`
        /// or <see cref="CloneContext.GetOrCreateClone{T}(ICloneable{T})"/> if you do have one.
        /// <para></para>
        /// <strong>Example:</strong><code>
        /// class BaseClass : ICloneable
        /// {
        ///     // Explicit implementation so that the recommended methods will be used instead.
        ///     object ICloneable.Clone(CloneContext context)
        ///     {
        ///         var clone = (BaseClass)MemberwiseClone();
        ///         clone.CopyFrom(this, context);
        ///         return clone;
        ///     }
        ///     
        ///     // Protected method which should only be called by Clone.
        ///     protected virtual void InitializeClone(CloneContext context)
        ///     {
        ///         // Alter any necessary BaseClass fields according to the context.
        ///     }
        /// }
        /// 
        /// class DerivedClass : BaseClass
        /// {
        ///     protected override void InitializeClone(CloneContext context)
        ///     {
        ///         base.CopyFrom(copyFrom, context);
        ///         
        ///         var derived = (DerivedClass)copyFrom;
        ///         // Alter any necessary DerivedClass fields according to the context.
        ///     }
        /// }
        /// </code></remarks>
        T Clone(CloneContext context);

        /************************************************************************************************************************/
    }

    /// <summary>Extension methods for <see cref="ICloneable{T}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/CloneableExtensions
    public static partial class CloneableExtensions
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="ICloneable{T}.Clone"/> using a <see cref="CloneContext"/> from the
        /// <see cref="CloneContext.Pool"/> and casts the result.
        /// </summary>
        /// <remarks>
        /// Returns <c>null</c> if the `original` is <c>null</c>.
        /// <para></para>
        /// Use <see cref="CloneContext.GetOrCreateClone{T}(ICloneable{T})"/>
        /// if you already have a <see cref="CloneContext"/>.
        /// </remarks>
        public static T Clone<T>(this ICloneable<T> original)
        {
            if (original == null)
                return default;

            var context = CloneContext.Pool.Instance.Acquire();
            var clone = original.Clone(context);
            CloneContext.Pool.Instance.Release(context);
            return clone;
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Asserts that the `clone` has the same type as the `original`.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        internal static void AssertCloneType<T>(this ICloneable<T> original, object clone)
        {
#if UNITY_ASSERTIONS
            var cloneType = clone.GetType();
            var originalType = original.GetType();
            if (cloneType != originalType)
                Debug.LogError($"Cloned object type ({cloneType.FullName}" +
                    $" doesn't match original {originalType.FullName}." +
                    $"\n• Original: {original}" +
                    $"\n• Clone: {clone}");
#endif
        }

        /************************************************************************************************************************/
    }

    /// <summary>A dictionary which maps objects to their copies.</summary>
    /// <remarks>
    /// This class is used to clone complex object trees with potentially interconnected references so that
    /// references to original objects can be replaced with references to their equivalent clones.
    /// </remarks>
    public class CloneContext : Dictionary<object, object>
    {
        /************************************************************************************************************************/

        /// <summary>Will the <see cref="IUpdatable"/>s be cloned as part of the current operation?</summary>
        /// <remarks>
        /// This is used to prevent <see cref="AnimancerNode"/>s from cloning their <see cref="FadeGroup"/>s
        /// individually while cloning a whole <see cref="AnimancerGraph"/> because it will clone the whole groups
        /// after cloning all the nodes.
        /// </remarks>
        public bool WillCloneUpdatables { get; set; }

        /************************************************************************************************************************/
        #region Pooling
        /************************************************************************************************************************/

        /// <summary>An <see cref="ObjectPool{T}"/> for <see cref="CloneContext"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/Pool
        public class Pool : ObjectPool<CloneContext>
        {
            /************************************************************************************************************************/

            /// <summary>Singleton.</summary>
            public static Pool Instance = new();

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override CloneContext New()
                => new();

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override CloneContext Acquire()
            {
                var context = base.Acquire();
                CollectionPool<KeyValuePair<object, object>, CloneContext>.AssertEmpty(context);
                return context;
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void Release(CloneContext context)
            {
                context.Clear();
                base.Release(context);
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <summary>
        /// Returns the value registered using `original` as its key if there is one.
        /// Otherwise, calls <see cref="CloneableExtensions.Clone"/>, adds the clone to this dictionary, and returns it.
        /// </summary>
        public T GetOrCreateClone<T>(ICloneable<T> original)
        {
            if (original == null)
                return default;

            if (TryGetValue(original, out var value))
                return (T)value;

            return Clone(original);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the value registered using `original` as its key if there is one.
        /// Otherwise, if the `original` is <see cref="ICloneable{T}"/> it calls <see cref="CloneableExtensions.Clone"/>,
        /// adds the clone to this dictionary, and returns it.
        /// Otherwise, just returns the `original`.
        /// </summary>
        /// <remarks>
        /// If <typeparamref name="T"/> is <see cref="ICloneable{T}"/>,
        /// use <see cref="GetOrCreateClone{T}(ICloneable{T})"/> instead.
        /// </remarks>
        public T GetOrCreateCloneOrOriginal<T>(T original)
        {
            TryGetOrCreateCloneOrOriginal(original, out original);
            return original;
        }

        /// <summary>
        /// Returns <c>true</c> if there is a `clone` registered for the `original`.
        /// Otherwise, if the `original` is <see cref="ICloneable{T}"/> it calls <see cref="CloneableExtensions.Clone"/>,
        /// adds the `clone` to this dictionary, and returns <c>true</c>.
        /// Otherwise, outputs the `original` as the `clone` and returns <c>false</c>.
        /// </summary>
        /// <remarks>Outputs <c>null</c> and returns <c>true</c> if the `original` is <c>null</c>.</remarks>
        public bool TryGetOrCreateCloneOrOriginal<T>(T original, out T clone)
        {
            if (original == null)
            {
                clone = default;
                return true;
            }

            if (TryGetValue(original, out var value))
            {
                clone = (T)value;
                return true;
            }

            if (original is ICloneable<T> cloneable)
            {
                clone = Clone(cloneable);
                return true;
            }

            clone = original;
            return false;
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="ICloneable{T}.Clone"/> and registers the clone.</summary>
        /// <exception cref="System.ArgumentException">A clone is already registered for the `original`.</exception>
        public T Clone<T>(ICloneable<T> original)
        {
            var clone = original.Clone(this);
            if (clone != null)
            {
                original.AssertCloneType(clone);
                Add(original, clone);
            }
            return clone;
        }

        /************************************************************************************************************************/

        /// <summary>Returns the clone of the `original` if one was registered. Otherwise, throws.</summary>
        /// <exception cref="KeyNotFoundException">No clone of the `original` is registered.</exception>
        public T GetClone<T>(T original)
            => (T)this[original];

        /************************************************************************************************************************/

        /// <summary>Returns the clone of the `original` if one is registered. Otherwise, returns the `original`.</summary>
        public T GetCloneOrOriginal<T>(T original)
            => original != null && TryGetValue(original, out var value)
            ? (T)value
            : original;

        /************************************************************************************************************************/

        /// <summary>Replaces the `item` with its clone and returns true if one is registered.</summary>
        public bool TryGetClone<T>(ref T item)
        {
            if (item == null)
                return false;

            if (!TryGetValue(item, out var value) ||
                value is not T valueT)
            {
                item = default;
                return false;
            }

            item = valueT;
            return true;
        }

        /// <summary>Calls <see cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/> and casts the result.</summary>
        public bool TryGetClone<T>(T original, out T clone)
        {
            clone = original;
            return TryGetClone(ref clone);
        }

        /************************************************************************************************************************/
    }
}

