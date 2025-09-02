// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

//#define ANIMANCER_LOG_OBJECT_POOLING

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    /// <summary>A simple object pooling system.</summary>
    /// <remarks><typeparamref name="T"/> must not inherit from <see cref="Component"/> or <see cref="ScriptableObject"/>.</remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ObjectPool_1
    public class ObjectPool<T>
        where T : class
    {
        /************************************************************************************************************************/

        /// <summary>An error message for when something has been modified after being released to the pool.</summary>
        /// <remarks>
        /// <strong>Example:</strong>
        /// <c>AnimancerUtilities.Assert($"A pooled list is not empty. {NotClearError}");</c>
        /// </remarks>
        public const string
            NotResetError = " It must be reset to its default values" +
                " before being released to the pool and not modified after that.";

        /************************************************************************************************************************/

        private readonly List<T>
            Items = new();

#if ANIMANCER_LOG_OBJECT_POOLING
        private int _TotalItemsCreated;
#endif

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>
        /// Creates a new <see cref="ObjectPool{T}"/>
        /// and asserts that there isn't a more derived type which should be used instead.
        /// </summary>
        public ObjectPool()
        {
            AnimancerUtilities.AssertDerivedPoolType<T>(GetType());
#if ANIMANCER_LOG_OBJECT_POOLING
            AllPools.Add(this);
#endif
        }
#endif

        /************************************************************************************************************************/

        /// <summary>Returns the <see cref="Static{T}.Instance"/>.</summary>
        public static ObjectPool<T> DefaultInstance
            => Static<ObjectPool<T>>.Instance;

        /************************************************************************************************************************/

        /// <summary>The number of spare items currently in this pool.</summary>
        public int Count
        {
            get => Items.Count;
            set
            {
                var count = Items.Count;
                if (count < value)
                {
                    if (Items.Capacity < value)
                        Items.Capacity = Mathf.NextPowerOfTwo(value);

                    do
                    {
                        Items.Add(New());
                        count++;
#if ANIMANCER_LOG_OBJECT_POOLING
                        _TotalItemsCreated++;
#endif
                    }
                    while (count < value);

#if ANIMANCER_LOG_OBJECT_POOLING
                    Debug.Log("Created multiple new items from " + ToString());
#endif
                }
                else if (count > value)
                {
                    Items.RemoveRange(value, count - value);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Increases the <see cref="Count"/> to equal the `count` if it was lower.</summary>
        public void IncreaseCountTo(int count)
        {
            if (Count < count)
                Count = count;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="List{T}.Capacity"/> of the internal list of spare items.</summary>
        public int Capacity
        {
            get => Items.Capacity;
            set
            {
                if (Items.Count > value)
                    Items.RemoveRange(value, Items.Count - value);
                Items.Capacity = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Increases the <see cref="Capacity"/> to equal the `capacity` if it was lower.</summary>
        public void IncreaseCapacityTo(int capacity)
        {
            if (Capacity < capacity)
                Capacity = capacity;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a <c>new()</c> instance of <typeparamref name="T"/>.</summary>
        protected virtual T New()
            => Activator.CreateInstance<T>();

        /************************************************************************************************************************/

        /// <summary>Returns a spare item if there are any, or creates a new one.</summary>
        /// <remarks>Remember to <see cref="Release(T)"/> it when you are done.</remarks>
        public virtual T Acquire()
        {
            var count = Items.Count;
            if (count == 0)
            {
#if ANIMANCER_LOG_OBJECT_POOLING
                _TotalItemsCreated++;
                var item = New();
                Debug.Log($"Created new item {item.GetHashCode()} from {ToString()}");
                return item;
#else
                return New();
#endif
            }
            else
            {
                count--;
                var item = Items[count];
                Items.RemoveAt(count);

#if ANIMANCER_LOG_OBJECT_POOLING
                Debug.Log($"Acquired spare item {item.GetHashCode()} from {ToString()}");
#endif

                return item;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Adds the `item` to the list of spares so it can be reused.</summary>
        public virtual void Release(T item)
        {
#if UNITY_ASSERTIONS
            AnimancerUtilities.Assert(item != null,
                $"Null objects must not be released into an {nameof(ObjectPool<T>)}.");

            // Don't want to check the whole list, but just checking the last item adds a bit of safety.
            if (Items.Count > 0 && Items[^1] == item)
                throw new InvalidOperationException(
                    $"Attempted to {nameof(Release)} an item which was already in the {nameof(ObjectPool<T>)}: {item}");
#endif
#if ANIMANCER_LOG_OBJECT_POOLING
            if (Items.Count + 1 >= Items.Capacity)
                Debug.LogWarning("Expanding " + ToString());
#endif

            Items.Add(item);

#if ANIMANCER_LOG_OBJECT_POOLING
            Debug.Log($"Released item {item.GetHashCode()} to {ToString()}");
#endif
        }

        /************************************************************************************************************************/

        /// <summary>Adds the `item` to the list of spares so it can be reused and sets it to <c>null</c>.</summary>
        public void Release(ref T item)
        {
            Release(item);
            item = null;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a description of the state of this pool.</summary>
        public override string ToString()
            => $"{nameof(ObjectPool<T>)}<{typeof(T).FullName}>" +
                $"({nameof(Count)} = {Items.Count}" +
                $", {nameof(Capacity)} = {Items.Capacity}" +
#if ANIMANCER_LOG_OBJECT_POOLING
                $", Created = {_TotalItemsCreated}" +
#endif
                ")";

        /************************************************************************************************************************/

#if ANIMANCER_LOG_OBJECT_POOLING
        private static readonly List<ObjectPool<T>>
            AllPools = new();

        static ObjectPool()
        {
            UnityEditor.EditorApplication.playModeStateChanged += change =>
            {
                for (int i = 0; i < AllPools.Count; i++)
                    Debug.Log($"{change}: {AllPools[i]}");
            };
        }
#endif

        /************************************************************************************************************************/
        #region Disposables
        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="ObjectPool{T}.Disposable"/> and calls <see cref="Acquire()"/>
        /// to set the <see cref="ObjectPool{T}.Disposable.Item"/> and `item`.
        /// </summary>
        public Disposable Acquire(out T item)
            => new(this, out item);

        /************************************************************************************************************************/

        /// <summary>
        /// An <see cref="IDisposable"/> to allow pooled objects to be acquired and released within <c>using</c>
        /// statements instead of needing to manually release everything.
        /// </summary>
        public readonly struct Disposable : IDisposable
        {
            /************************************************************************************************************************/

            /// <summary>The <see cref="ObjectPool{T}"/> which the <see cref="Item"/> is acquired from.</summary>
            public readonly ObjectPool<T> Pool;

            /// <summary>The object acquired from the <see cref="Pool"/>.</summary>
            public readonly T Item;

            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Disposable"/> and calls <see cref="Acquire()"/>
            /// to set the <see cref="Item"/> and `item`.
            /// </summary>
            public Disposable(ObjectPool<T> pool, out T item)
            {
                Pool = pool;
                Item = item = pool.Acquire();
            }

            /************************************************************************************************************************/

            /// <summary><see cref="Release(T)"/> the <see cref="Item"/>.</summary>
            public void Dispose()
                => Pool.Release(Item);

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/
#if UNITY_EDITOR
    /************************************************************************************************************************/

    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerUtilities
    public static partial class AnimancerUtilities
    {
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Asserts that the `type` isn't a base <see cref="ObjectPool{T}"/> with a more derived type available.
        /// </summary>
        internal static void AssertDerivedPoolType<T>(Type type)
            where T : class
        {
            if (type != typeof(ObjectPool<T>))
                return;

            if (BaseToDerivedPoolType.TryGetValue(type, out var derived))
                Debug.LogWarning(
                    $"A base {type.GetNameCS()} was created even though a more derived type exists" +
                    $" and should be used instead: {derived.GetNameCS()}");
        }

        /************************************************************************************************************************/

        private static Dictionary<Type, Type> _BaseToDerivedPoolType;

        /// <summary>
        /// An automatically gathered dictionary which maps base <see cref="ObjectPool{T}"/>
        /// types to any other types which inherit from them.
        /// </summary>
        private static Dictionary<Type, Type> BaseToDerivedPoolType
        {
            get
            {
                if (_BaseToDerivedPoolType != null)
                    return _BaseToDerivedPoolType;

                _BaseToDerivedPoolType = new();

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int iAssembly = 0; iAssembly < assemblies.Length; iAssembly++)
                {
                    var types = assemblies[iAssembly].GetTypes();
                    for (int iType = 0; iType < types.Length; iType++)
                    {
                        var type = types[iType];
                        if (TryGetBasePoolType(type, out var pool))
                            _BaseToDerivedPoolType[pool] = type;
                    }
                }

                return _BaseToDerivedPoolType;
            }
        }

        /************************************************************************************************************************/

        private static bool TryGetBasePoolType(Type derived, out Type pool)
        {
            pool = null;
            if (!derived.IsClass ||
                derived.IsAbstract)
                return false;

            pool = derived;
            while ((pool = pool.BaseType) != null)
                if (pool.IsGenericType && pool.GetGenericTypeDefinition() == typeof(ObjectPool<>))
                    return true;

            return false;
        }

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/
#endif
    /************************************************************************************************************************/
}

