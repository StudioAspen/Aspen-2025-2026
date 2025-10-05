// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>[Pro-Only] An object that can be updated during Animancer's animation updates.</summary>
    ///
    /// <remarks>
    /// <strong>Example:</strong>
    /// Register to receive updates using <see cref="AnimancerGraph.RequirePreUpdate"/> or
    /// <see cref="AnimancerGraph.RequirePostUpdate"/> and stop
    /// receiving updates using <see cref="AnimancerGraph.CancelPreUpdate"/> or
    /// <see cref="AnimancerGraph.CancelPostUpdate"/>.
    /// <para></para><code>
    /// public sealed class MyUpdatable : IUpdatable
    /// {
    ///     // Implement IUpdatable.
    ///     // You can avoid this by inheriting from Updatable instead.
    ///     int IUpdatable.UpdatableIndex { get; set; } = IUpdatable.List.NotInList;
    ///     
    ///     private AnimancerComponent _Animancer;
    ///
    ///     public void StartUpdating(AnimancerComponent animancer)
    ///     {
    ///         _Animancer = animancer;
    ///         
    ///         // If you want Update to be called before the playables get updated.
    ///         _Animancer.Graph.RequirePreUpdate(this);
    ///         
    ///         // If you want Update to be called after the playables get updated.
    ///         _Animancer.Graph.RequirePostUpdate(this);
    ///     }
    ///
    ///     public void StopUpdating()
    ///     {
    ///         // If you used RequirePreUpdate.
    ///         _Animancer.Graph.CancelPreUpdate(this);
    ///         
    ///         // If you used RequirePostUpdate.
    ///         _Animancer.Graph.CancelPostUpdate(this);
    ///     }
    ///
    ///     void IUpdatable.Update()
    ///     {
    ///         // Called during every animation update.
    ///         
    ///         // AnimancerGraph.Current can be used to access the system it is being updated by.
    ///     }
    /// }
    /// </code></remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/IUpdatable
    /// 
    public interface IUpdatable
    {
        /************************************************************************************************************************/

        /// <summary>The index of this object in its <see cref="IndexedList{TItem, TIndexer}"/>.</summary>
        /// <remarks>Must be initialized to -1 to indicate that this object is not yet in a list.</remarks>
        int UpdatableIndex { get; set; }

        /// <summary>Updates this object.</summary>
        void Update();

        /************************************************************************************************************************/

        /// <summary>An <see cref="IIndexer{T}"/> for <see cref="IUpdatable"/>.</summary>
        public readonly struct Indexer : IIndexer<IUpdatable>
        {
            /************************************************************************************************************************/

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly int GetIndex(IUpdatable item)
                => item.UpdatableIndex;

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void SetIndex(IUpdatable item, int index)
                => item.UpdatableIndex = index;

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void ClearIndex(IUpdatable item)
                => item.UpdatableIndex = -1;

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        /// <summary>An <see cref="IndexedList{TItem, TAccessor}"/> of <see cref="IUpdatable"/>.</summary>
        public class List : IndexedList<IUpdatable, Indexer>
        {
            /************************************************************************************************************************/

            /// <summary>The default <see cref="IndexedList{TItem, TIndexer}.Capacity"/> for newly created lists.</summary>
            /// <remarks>Default value is 4.</remarks>
            public static new int DefaultCapacity { get; set; } = 4;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="List"/> with the <see cref="DefaultCapacity"/>.</summary>
            public List()
                : base(DefaultCapacity, new())
            { }

            /************************************************************************************************************************/

            /// <summary>Calls <see cref="Update"/> on all items in this list.</summary>
            /// <remarks>
            /// Uses <see cref="Debug.LogException(Exception, Object)"/> to handle exceptions and continues executing
            /// the remaining items if any occur.
            /// </remarks>
            public void UpdateAll()
            {
                BeginEnumeraton();
                ContinueEnumeration:
                try
                {
                    while (TryEnumerateNext())
                    {
                        Current.Update();
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, AnimancerGraph.Current?.Component as Object);
                    goto ContinueEnumeration;
                }
            }

            /************************************************************************************************************************/

            /// <summary>Clones any <see cref="ICloneable{T}"/> items.</summary>
            public void CloneFrom(
                List copyFrom,
                CloneContext context)
            {
                var count = copyFrom.Count;
                for (int i = 0; i < count; i++)
                    if (copyFrom[i] is ICloneable<object> cloneable &&
                        context.GetOrCreateClone(cloneable) is IUpdatable clone)
                        Add(clone);
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }

    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerUtilities
    public static partial class AnimancerUtilities
    {
        /************************************************************************************************************************/

        /// <summary>Is the `updatable` currently in a list?</summary>
        public static bool IsInList(this IUpdatable updatable)
            => updatable.UpdatableIndex >= 0;

        /************************************************************************************************************************/
    }
}

