// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

//#define DEBUG_INDEXED_LISTS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Animancer
{
    /// <summary>
    /// An <see cref="IReadOnlyList{T}"/> which can remove items in <c>O(1)</c> time without searching and an inbuilt
    /// enumerator which supports modifications at any time (including during enumeration).
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/IReadOnlyIndexedList_1
    public interface IReadOnlyIndexedList<T> : IReadOnlyList<T>
    {
        /************************************************************************************************************************/

        /// <summary>The number of items this list can contain before resizing is required.</summary>
        public int Capacity { get; set; }// Can't reduce the Count so it's safe for a Read-Only interface.

        /// <summary>Is the `item` currently in this list?</summary>
        bool Contains(T item);

        /// <summary>Is the `item` currently in this list at the specified `index`?</summary>
        bool Contains(T item, int index);

        /// <summary>Copies all the items from this list into the `array`, starting at the specified `index`.</summary>
        void CopyTo(T[] array, int index);

        /// <summary>Returns the index of the `item` in this list or <c>-1</c> if it's not in this list.</summary>
        int IndexOf(T item);

        /// <summary>Returns a string describing this list and its contents.</summary>
        string DeepToString(string separator = "\n• ");

        /************************************************************************************************************************/
    }

    /// <summary>An object which accesses the index of the items in an <see cref="IndexedList{TItem, TIndexer}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/IIndexer_1
    public interface IIndexer<T>
    {
        /************************************************************************************************************************/

        /// <summary>Returns the index of the `item`.</summary>
        /// <remarks>
        /// The index used by this method should be initialized at -1 and should not be modified by anything outside
        /// this indexer.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int GetIndex(T item);

        /// <summary>Sets the index of the `item`.</summary>
        /// <remarks>
        /// The index used by this method should be initialized at -1 and should not be modified by anything outside
        /// this indexer.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetIndex(T item, int index);

        /// <summary>Resets the index of the `item` to -1.</summary>
        /// <remarks>
        /// The index used by this method should be initialized at -1 and should not be modified by anything outside
        /// this indexer.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ClearIndex(T item);

        /************************************************************************************************************************/
    }

    /// <summary>
    /// A <see cref="List{T}"/> which can remove items in <c>O(1)</c> time without searching and an inbuilt
    /// enumerator which supports modifications at any time (including during enumeration).
    /// </summary>
    /// <remarks>
    /// This implementation has several restrictions compared to a regular <see cref="List{T}"/>:
    /// <list type="bullet">
    /// <item>Items cannot be <c>null</c>.</item>
    /// <item>
    /// Items can only be in one <see cref="IndexedList{TItem, TIndexer}"/>
    /// at a time and cannot appear multiple times in it.
    /// </item>
    /// </list>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/IndexedList_2
    public class IndexedList<TItem, TIndexer> :
        IList<TItem>,
        IReadOnlyIndexedList<TItem>,
        ICollection
        where TItem : class
        where TIndexer : IIndexer<TItem>
    {
        /************************************************************************************************************************/
        #region Fields and Accessors
        /************************************************************************************************************************/

        private const string
            SingleUse = "Each item can only be used in one " + nameof(IndexedList<TItem, TIndexer>) + " at a time.",
            NotFound = "The specified item does not exist in this " + nameof(IndexedList<TItem, TIndexer>) + ".";

        /// <summary>The index which indicates that an item isn't in a list.</summary>
        public const int NotInList = -1;

        /// <summary>The default <see cref="Capacity"/> which lists will expand to when their first item is added.</summary>
        public static int DefaultCapacity = 16;

        /************************************************************************************************************************/

        /// <summary>The <see cref="IIndexer{T}"/> used to access the details of items.</summary>
        public TIndexer Indexer;

        private TItem[] _Items;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="IndexedList{TItem, TIndexer}"/> using the default <see cref="List{T}"/> constructor.</summary>
        public IndexedList(TIndexer indexer = default)
        {
            Indexer = indexer;
            _Items = Array.Empty<TItem>();
        }

        /// <summary>Creates a new <see cref="IndexedList{TItem, TIndexer}"/> with the specified initial `capacity`.</summary>
        public IndexedList(int capacity, TIndexer indexer = default)
        {
            Indexer = indexer;
            _Items = new TItem[capacity];
        }

        // No copy constructor because the indices will not work if they are used in multiple lists at once.

        /************************************************************************************************************************/

        /// <summary>The number of items currently in the list.</summary>
        public int Count { get; private set; }

        /// <inheritdoc/>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _Items.Length;
            set
            {
                if (value < Count)
                    throw new ArgumentOutOfRangeException(nameof(Count),
                        $"{nameof(Capacity)} can't be less than {nameof(Count)}." +
                        $" Excess items must be removed before the {nameof(Capacity)} can be reduced.");

                Array.Resize(ref _Items, value);
            }
        }

        /************************************************************************************************************************/

        /// <summary>The item at the specified `index`.</summary>
        /// <remarks>This indexer has <c>O(1)</c> complexity</remarks>
        /// <exception cref="ArgumentException">The `value` was already in an <see cref="IndexedList{TItem, TIndexer}"/> (setter only).</exception>
        public TItem this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _Items[index];
            set
            {
                // Make sure it isn't already in a list.
                if (Indexer.GetIndex(value) != NotInList)
                    throw new ArgumentException(SingleUse);

                // Remove the old item at that index.
                Indexer.ClearIndex(_Items[index]);

                // Set the index of the new item and add it at that index.
                Indexer.SetIndex(value, index);
                _Items[index] = value;
                AssertContents();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Is the `item` currently in this list?</summary>
        /// <remarks>This method has <c>O(1)</c> complexity.</remarks>
        public bool Contains(TItem item)
            => item != null
            && Contains(item, Indexer.GetIndex(item));

        /// <summary>Is the `item` currently in this list at the specified `index`?</summary>
        /// <remarks>This method has <c>O(1)</c> complexity.</remarks>
        public bool Contains(TItem item, int index)
            => (uint)index < (uint)Count
            && _Items[index] == item;

        /************************************************************************************************************************/

        /// <summary>Returns the index of the `item` in this list or <c>-1</c> if it's not in this list.</summary>
        /// <remarks>This method has <c>O(1)</c> complexity.</remarks>
        public int IndexOf(TItem item)
        {
            if (item == null)
                return NotInList;

            var index = Indexer.GetIndex(item);
            if (Contains(item, index))
                return index;
            else
                return NotInList;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(TItem[] array, int index)
            => _Items.CopyTo(array, index);

        /// <summary>Copies all the items from this list into the `array`, starting at the specified `index`.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICollection.CopyTo(Array array, int index)
            => _Items.CopyTo(array, index);

        /************************************************************************************************************************/

        /// <summary>Returns false.</summary>
        bool ICollection<TItem>.IsReadOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        /// <summary>Is this list thread safe?</summary>
        bool ICollection.IsSynchronized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _Items.IsSynchronized;
        }

        /// <summary>An object that can be used to synchronize access to this <see cref="ICollection"/>.</summary>
        object ICollection.SyncRoot
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _Items.SyncRoot;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Add
        /************************************************************************************************************************/

        /// <summary>Adds the `item` to the end of this list.</summary>
        /// <remarks>
        /// This method has <c>O(1)</c> complexity if the <see cref="Capacity"/> doesn't need to be increased.
        /// Otherwise, it's <c>O(N)</c> since all existing items need to be copied into a new array.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The `item` was already in an <see cref="IndexedList{TItem, TIndexer}"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICollection<TItem>.Add(TItem item)
            => Add(item);

        /// <summary>Adds the `item` to the end of this list if it wasn't already in it and returns true if successful.</summary>
        /// <remarks>
        /// This method has <c>O(1)</c> complexity if the <see cref="Capacity"/> doesn't need to be increased.
        /// Otherwise, it's <c>O(N)</c> since all existing items need to be copied into a new array.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The `item` is already in a different list.
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// The `item` is already in a different list at an index larger than this list.
        /// </exception>
        public bool Add(TItem item)
        {
            var index = Indexer.GetIndex(item);

            // Make sure it isn't already in a list.
            if (index != NotInList)
            {
                if (_Items[index] == item)// If it's in this list, do nothing.
                    return false;
                else// Otherwise, it's in another list so we can't add it to this one.
                    throw new ArgumentException(SingleUse);
            }

            // Set the index of the new item and add it to the list.
            Indexer.SetIndex(item, Count);
            InternalAdd(item);
            AssertContents();
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>Adds the `item` to this list at the specified `index`.</summary>
        /// <remarks>
        /// This method has <c>O(1)</c> complexity.
        /// <para></para>
        /// This does not maintain the order of items, but is more efficient than <see cref="List{T}.Insert(int, T)"/>
        /// because it avoids the need to move every item after the target up one place.
        /// </remarks>
        public void Insert(int index, TItem item)
        {
            if (index >= Count)
            {
                Add(item);
                return;
            }

            var oldItem = _Items[index];

            Indexer.SetIndex(item, index);
            Indexer.SetIndex(oldItem, Count);

            _Items[index] = item;
            InternalAdd(oldItem);

            AssertContents();
        }

        /************************************************************************************************************************/

        private void InternalAdd(TItem item)
        {
            var count = Count;
            var capacity = Capacity;

            if (count == capacity)
            {
                if (capacity == 0)
                {
                    _Items = new TItem[DefaultCapacity];
                }
                else
                {
                    capacity *= 2;
                    if (capacity < DefaultCapacity)
                        capacity = DefaultCapacity;

                    var events = new TItem[capacity];

                    Array.Copy(_Items, 0, events, 0, count);

                    _Items = events;
                }
            }

            _Items[count] = item;

            Count = count + 1;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Remove
        /************************************************************************************************************************/

        /// <summary>Removes the item at the specified `index` by swapping the last item in this list into its place.</summary>
        /// <remarks>
        /// This method has <c>O(1)</c> complexity.
        /// <para></para>
        /// This does not maintain the order of items, but is more efficient than <see cref="List{T}.RemoveAt"/>
        /// because it avoids the need to move every item after the target down one place.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
            => RemoveAt(index, _Items[index]);

        /// <summary>Removes the item at the specified `index` by swapping the last item in this list into its place.</summary>
        /// <remarks>
        /// This method has <c>O(1)</c> complexity.
        /// <para></para>
        /// This does not maintain the order of items, but is more efficient than <see cref="List{T}.RemoveAt"/>
        /// because it avoids the need to move every item after the target down one place.
        /// </remarks>
        private void RemoveAt(int index, TItem item)
        {
            var lastIndex = Count - 1;

            // Adjust the enumerator if necessary.
            if (CurrentIndex > index)
            {
                CurrentIndex--;

                // If the removal index is ahead of the current enumeration,
                // swap the current item to that index and swap the last item to the current index.

                // Otherwise simply swapping the last item into that slot would mean that it gets covered again
                // when the enumerator reaches it.

                if (CurrentIndex > index)
                {
                    var lastItem = _Items[lastIndex];
                    var currentItem = _Items[CurrentIndex];

                    _Items[CurrentIndex] = lastItem;
                    _Items[index] = currentItem;
                    _Items[lastIndex] = null;

                    Count--;

                    Indexer.ClearIndex(item);
                    Indexer.SetIndex(currentItem, index);
                    Indexer.SetIndex(lastItem, CurrentIndex);

                    AssertContents();

                    return;
                }
            }

            // If it wasn't the last item, move the last item over it.
            if (lastIndex > index)
            {
                var lastItem = _Items[lastIndex];
                _Items[index] = lastItem;
                _Items[lastIndex] = null;

                Count--;

                Indexer.ClearIndex(item);
                Indexer.SetIndex(lastItem, index);
            }
            else// If it was the last item, just remove it.
            {
                _Items[lastIndex] = null;

                Count--;

                Indexer.ClearIndex(item);
            }

            AssertContents();
        }

        /************************************************************************************************************************/

        /// <summary>Removes the `item` by swapping the last item in this list into its place.</summary>
        /// <remarks>
        /// This method has <c>O(1)</c> complexity.
        /// <para></para>
        /// This method does not maintain the order of items, but is more efficient than <see cref="Remove"/> because
        /// it avoids the need to move every item after the target down one place.
        /// </remarks>
        public bool Remove(TItem item)
        {
            var index = Indexer.GetIndex(item);

            // If it isn't in this list, do nothing.
            if (!Contains(item, index))
                return false;

            // Remove the item.
            RemoveAt(index, item);
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>Removes all items from this list.</summary>
        /// <remarks>This method has <c>O(N)</c> complexity.</remarks>
        public void Clear()
        {
            for (int i = Count - 1; i >= 0; i--)
                Indexer.ClearIndex(_Items[i]);

            Array.Clear(_Items, 0, Count);
            Count = 0;

            CurrentIndex = NotInList;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Enumeration
        /************************************************************************************************************************/

        /// <summary>
        /// If something is currently enumerating through this list, this value holds the index it's currently up
        /// to. Otherwise, this value will be negative.
        /// </summary>
        public int CurrentIndex { get; private set; } = NotInList;

        /************************************************************************************************************************/

        /// <summary>The item at the <see cref="CurrentIndex"/>.</summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The <see cref="CurrentIndex"/> is negative so this list isn't currently being enumerated.
        /// </exception>
        public TItem Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _Items[CurrentIndex];
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Has <see cref="BeginEnumeraton"/> been called and <see cref="TryEnumerateNext"/> not yet been called
        /// enough times to go through all items?
        /// </summary>
        public bool IsEnumerating
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CurrentIndex != NotInList;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the <see cref="CurrentIndex"/> to the end of this list so that <see cref="TryEnumerateNext"/> can
        /// iterate backwards to the start.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This method was called multiple times without <see cref="TryEnumerateNext"/> going over all items. This
        /// list can only be enumerated by one thing at a time and it must fully complete before the next can begin.
        /// This limitation is necessary to allow items to be safely added and removed at any time.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BeginEnumeraton()
        {
            if (IsEnumerating)
                throw new InvalidOperationException(
                    $"{GetType().Name}<{typeof(TItem).Name}> was already enumerating." +
                    $" Recursive enumeration is not supported.");

            CurrentIndex = Count;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Moves the <see cref="CurrentIndex"/> so the <see cref="Current"/> property points to the next item in
        /// this list.
        /// </summary>
        /// <remarks>
        /// This method should only be called after <see cref="BeginEnumeraton"/> and it should be called
        /// repeatedly until it returns false. <see cref="CancelEnumeration"/> can be used to cancel the
        /// enumeration early.
        /// </remarks>
        /// <returns>False if there are no more items to move to. Otherwise true.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnumerateNext()
            => --CurrentIndex >= 0;

        /************************************************************************************************************************/

        /// <summary>
        /// Clears the <see cref="CurrentIndex"/> so that <see cref="BeginEnumeraton"/> can be used again without
        /// needing to call <see cref="TryEnumerateNext"/> repeatedly until it returns false.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CancelEnumeration()
            => CurrentIndex = NotInList;

        /************************************************************************************************************************/

        /// <summary>Returns an enumerator which iterates through this list.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastEnumerator<TItem> GetEnumerator()
            => new(_Items, Count);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
            => GetEnumerator();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Debugging
        /************************************************************************************************************************/

        /// <summary>Asserts that the indices stored in all items actually match their index in this list.</summary>
        [System.Diagnostics.Conditional("DEBUG_INDEXED_LISTS")]
        private void AssertContents(string name = null)
        {
            for (int i = 0; i < Count; i++)
                if (i != Indexer.GetIndex(_Items[i]))
                    throw new ArgumentException($"Index mismatch at {i} in {name} {DeepToString()}");
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public string DeepToString(string separator = "\n• ")
        {
            var text = StringBuilderPool.Instance.Acquire();

            text.Append(GetType().GetNameCS())
                .Append('[')
                .Append(Count)
                .Append(']');

            for (int i = 0; i < Count; i++)
            {
                var item = _Items[i];
                text.Append(separator)
                    .Append('[')
                    .Append(Indexer.GetIndex(item))
                    .Append("] ")
                    .Append(item);
            }

            return text.ReleaseToString();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

