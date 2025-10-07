// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Animancer
{
    /// <summary>
    /// An <see cref="IEnumerator{T}"/> for any <see cref="IList{T}"/>
    /// which doesn't bother checking if the target has been modified.
    /// This gives it good performance but also makes it slightly less safe to use.
    /// </summary>
    /// <remarks>
    /// This struct also implements <see cref="IEnumerable{T}"/>
    /// so it can be used in <c>foreach</c> statements and <see cref="IList{T}"/>
    /// to allow the target collection to be modified without breaking the enumerator
    /// (though doing so is still somewhat dangerous so use with care).
    /// <para></para>
    /// <strong>Example:</strong><code>
    /// var numbers = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, };
    /// var count = 4;
    /// foreach (var number in new FastEnumerator&lt;int&gt;(numbers, count))
    /// {
    ///     Debug.Log(number);
    /// }
    /// 
    /// // Log Output:
    /// // 9
    /// // 8
    /// // 7
    /// // 6
    /// </code></remarks>
    public struct FastEnumerator<T> : IReadOnlyList<T>, IEnumerator<T>
    {
        /************************************************************************************************************************/

        /// <summary>The target <see cref="IList{T}"/>.</summary>
        private readonly IList<T> List;

        /************************************************************************************************************************/

        private int _Count;

        /// <summary>[<see cref="ICollection{T}"/>]
        /// The number of items in the <see cref="List"/> (which can be less than the
        /// <see cref="ICollection{T}.Count"/> of the <see cref="List"/>).
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _Count;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                AssertCount(value);
                _Count = value;
            }
        }

        /************************************************************************************************************************/

        private int _Index;

        /// <summary>The position of the <see cref="Current"/> item in the <see cref="List"/>.</summary>
        public int Index
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _Index;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                AssertIndex(value);
                _Index = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The item at the current <see cref="Index"/> in the <see cref="List"/>.</summary>
        public readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                AssertCount(_Count);
                AssertIndex(_Index);
                return List[_Index];
            }
        }

        /// <summary>The item at the current <see cref="Index"/> in the <see cref="List"/>.</summary>
        readonly object IEnumerator.Current
            => Current;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="FastEnumerator{T}"/>.</summary>
        /// <exception cref="NullReferenceException">
        /// The `list` is null. Use the <c>default</c> <see cref="FastEnumerator{T}"/> instead.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastEnumerator(IList<T> list)
            : this(list, list.Count)
        { }

        /// <summary>Creates a new <see cref="FastEnumerator{T}"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastEnumerator(IList<T> list, int count)
        {
            List = list;
            _Count = count;
            _Index = -1;
            AssertCount(count);
        }

        /************************************************************************************************************************/

        /// <summary>Moves to the next item in the <see cref="List"/> and returns true if there is one.</summary>
        /// <remarks>At the end of the <see cref="List"/> the <see cref="Index"/> is set to <see cref="int.MinValue"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            _Index++;
            if ((uint)_Index < (uint)_Count)
            {
                return true;
            }
            else
            {
                _Index = int.MinValue;
                return false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Moves to the previous item in the <see cref="List"/> and returns true if there is one.</summary>
        /// <remarks>At the end of the <see cref="List"/> the <see cref="Index"/> is set to <c>-1</c>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MovePrevious()
        {
            if (_Index > 0)
            {
                _Index--;
                return true;
            }
            else
            {
                _Index = -1;
                return false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>[<see cref="IEnumerator"/>] Reverts this enumerator to the start of the <see cref="List"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _Index = -1;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void IDisposable.Dispose() { }

        /************************************************************************************************************************/
        // IEnumerator.
        /************************************************************************************************************************/

        /// <summary>Returns <c>this</c>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly FastEnumerator<T> GetEnumerator() => this;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly IEnumerator IEnumerable.GetEnumerator() => this;

        /************************************************************************************************************************/
        // IList.
        /************************************************************************************************************************/

        /// <summary>[<see cref="IList{T}"/>] Returns the first index of the `item` in the <see cref="List"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(T item)
            => List.IndexOf(item);

        /// <summary>[<see cref="IList{T}"/>] The item at the specified `index` in the <see cref="List"/>.</summary>
        public readonly T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                AssertIndex(index);
                return List[index];
            }
        }

        /************************************************************************************************************************/
        // ICollection.
        /************************************************************************************************************************/

        /// <summary>[<see cref="ICollection{T}"/>] Does the <see cref="List"/> contain the `item`?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(T item) => List.Contains(item);

        /// <summary>[<see cref="ICollection{T}"/>] Copies the contents of the <see cref="List"/> into the `array`.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < _Count; i++)
                array[arrayIndex + i] = List[i];
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Only] Throws an exception unless 0 &lt;= `index` &lt; <see cref="Count"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void AssertIndex(int index)
        {
#if UNITY_ASSERTIONS
            if ((uint)index > (uint)_Count)
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"{nameof(FastEnumerator<T>)}.{nameof(Index)}" +
                    $" must be within 0 <= {nameof(Index)} ({index}) < {nameof(Count)} ({_Count}).");
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Only] Throws an exception unless 0 &lt; `count` &lt;= <see cref="ICollection{T}.Count"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        private readonly void AssertCount(int count)
        {
#if UNITY_ASSERTIONS
            if (List == null)
            {
                if (count != 0)
                    throw new ArgumentOutOfRangeException(nameof(count),
                        $"Must be 0 since the {nameof(List)} is null.");
            }
            else
            {
                if ((uint)count > (uint)List.Count)
                    throw new ArgumentOutOfRangeException(nameof(count),
                        $"Must be within 0 <= {nameof(count)} ({count}) < {nameof(List)}.{nameof(List.Count)} ({List.Count}).");
            }
#endif
        }

        /************************************************************************************************************************/
    }
}

