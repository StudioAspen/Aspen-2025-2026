// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#pragma warning disable IDE0016 // Use 'throw' expression.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    partial struct AnimancerEvent
    {
        /// <summary>
        /// A variable-size list of <see cref="AnimancerEvent"/>s which keeps itself sorted
        /// according to their <see cref="normalizedTime"/>.
        /// </summary>
        /// <remarks>
        /// <em>Animancer Lite doesn't allow events (except for <see cref="OnEnd"/>) in runtime builds.</em>
        /// <para></para>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">
        /// Animancer Events</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Sequence
        /// 
        public partial class Sequence :
            IEnumerable<AnimancerEvent>,
            ICloneable<Sequence>
        {
            /************************************************************************************************************************/
            #region Fields and Properties
            /************************************************************************************************************************/

            internal const string
                IndexOutOfRangeError = "index must be within the range of 0 <= index < " + nameof(Count);

#if UNITY_ASSERTIONS
            private const string
                NullCallbackError =
                nameof(AnimancerEvent) + " callbacks can't be null (except for End Events). Use " +
                nameof(AnimancerEvent) + "." + nameof(InvokeBoundCallback) + " or " +
                nameof(AnimancerEvent) + "." + nameof(DummyCallback) + " instead.";
#endif

            /************************************************************************************************************************/

            /// <summary>All of the events in this sequence, excluding the <see cref="EndEvent"/>.</summary>
            /// <remarks>This field should never be null. It should use <see cref="Array.Empty{T}"/> instead.</remarks>
            private AnimancerEvent[] _Events;

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] The number of events in this sequence, excluding the <see cref="EndEvent"/>.</summary>
            public int Count { get; private set; }

            /************************************************************************************************************************/

            /// <summary>Does this sequence have no events in it, including the <see cref="EndEvent"/>?</summary>
            public bool IsEmpty
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return
                        _EndEvent.callback == null &&
                        float.IsNaN(_EndEvent.normalizedTime) &&
                        Count == 0;
                }
            }

            /************************************************************************************************************************/

            /// <summary>The initial <see cref="Capacity"/> which will be used if another value is not specified.</summary>
            public const int DefaultCapacity = 4;

            /// <summary>[Pro-Only] The size of the internal array used to hold events.</summary>
            /// <remarks>
            /// When set, the array is re-allocated to the given size.
            /// <para></para>
            /// If not specified in the constructor, this value starts at 0
            /// and increases to the <see cref="DefaultCapacity"/> when the first event is added.
            /// </remarks>
            public int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _Events.Length;
                set
                {
                    if (value < Count)
                        throw new ArgumentOutOfRangeException(nameof(value),
                            $"{nameof(Capacity)} cannot be set lower than {nameof(Count)}");

                    if (value == _Events.Length)
                        return;

                    if (value > 0)
                    {
                        var newEvents = new AnimancerEvent[value];
                        if (Count > 0)
                            Array.Copy(_Events, 0, newEvents, 0, Count);
                        _Events = newEvents;
                    }
                    else
                    {
                        _Events = Array.Empty<AnimancerEvent>();
                    }
                }
            }

            /************************************************************************************************************************/

            /// <summary>
            /// The number of times the contents of this sequence have been modified.
            /// This applies to general events, but not the <see cref="EndEvent"/>.
            /// </summary>
            public int Version { get; private set; }

            /************************************************************************************************************************/
            #region End Event
            /************************************************************************************************************************/

            private AnimancerEvent _EndEvent = new(float.NaN, null);

            /// <summary>
            /// A <see cref="callback "/> which will be triggered <strong>every frame</strong>
            /// after the <see cref="normalizedTime"/> has passed as long as the animation is playing.
            /// </summary>
            ///
            /// <remarks>
            /// Interrupting the animation before it ends doesn't trigger this event.
            /// <para></para>
            /// By default, the <see cref="normalizedTime"/> will be <see cref="float.NaN"/>
            /// so that it chooses the correct value based on the current play direction:
            /// playing forwards ends at 1 and playing backwards ends at 0.
            /// <para></para>
            /// <strong>Documentation:</strong>
            /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/end">
            /// End Events</see>
            /// </remarks>
            /// 
            /// <seealso cref="OnEnd"/>
            /// <seealso cref="NormalizedEndTime"/>
            public ref AnimancerEvent EndEvent
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _EndEvent;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// A callback which will be triggered <strong>every frame</strong> after the
            /// <see cref="normalizedTime"/> has passed as long as the animation is playing.
            /// </summary>
            ///
            /// <remarks>
            /// Interrupting the animation before it ends doesn't trigger this event.
            /// <para></para>
            /// By default, the <see cref="normalizedTime"/> will be <see cref="float.NaN"/>
            /// so that it chooses the correct value based on the current play direction:
            /// playing forwards ends at 1 and playing backwards ends at 0.
            /// <para></para>
            /// <strong>Documentation:</strong>
            /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/end">
            /// End Events</see>
            /// </remarks>
            /// 
            /// <seealso cref="EndEvent"/>
            /// <seealso cref="NormalizedEndTime"/>
            public ref Action OnEnd
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _EndEvent.callback;
            }

            /************************************************************************************************************************/

            /// <summary>Shorthand for <c>EndEvent.normalizedTime</c>.</summary>
            /// <remarks>
            /// This value is <see cref="float.NaN"/> by default so that the actual time
            /// can be determined based on the <see cref="AnimancerNodeBase.EffectiveSpeed"/>:
            /// positive speed ends at 1 and negative speed ends at 0.
            /// <para></para>
            /// Use <see cref="AnimancerState.NormalizedEndTime"/> to access that value.
            /// </remarks>
            /// <seealso cref="EndEvent"/>
            /// <seealso cref="OnEnd"/>
            public ref float NormalizedEndTime
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _EndEvent.normalizedTime;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Returns the <see cref="NormalizedEndTime"/> but converts <see cref="float.NaN"/>
            /// to its corresponding default value: positive speed ends at 1 and negative speed ends at 0.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public float GetRealNormalizedEndTime(float speed = 1)
                => float.IsNaN(_EndEvent.normalizedTime)
                ? GetDefaultNormalizedEndTime(speed)
                : _EndEvent.normalizedTime;

            /************************************************************************************************************************/

            /// <summary>
            /// The default <see cref="AnimancerState.NormalizedTime"/> for an animation to start
            /// at when playing forwards is 0 (the start of the animation)
            /// and when playing backwards is 1 (the end of the animation).
            /// <para></para>
            /// `speed` 0 or <see cref="float.NaN"/> will also return 0.
            /// </summary>
            /// <remarks>
            /// This method has nothing to do with events, so it is only here because of
            /// <see cref="GetDefaultNormalizedEndTime"/>.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float GetDefaultNormalizedStartTime(float speed)
                => speed < 0 ? 1 : 0;

            /// <summary>
            /// The default <see cref="normalizedTime"/> for an <see cref="EndEvent"/>
            /// when playing forwards is 1 (the end of the animation)
            /// and when playing backwards is 0 (the start of the animation).
            /// <para></para>
            /// `speed` 0 or <see cref="float.NaN"/> will also return 1.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float GetDefaultNormalizedEndTime(float speed)
                => speed < 0 ? 0 : 1;

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Names
            /************************************************************************************************************************/

            private StringReference[] _Names = Array.Empty<StringReference>();

            /// <summary>The names of the events, excluding the <see cref="EndEvent"/>.</summary>
            /// <remarks>This array is empty by default and can never be <c>null</c>.</remarks>
            public StringReference[] Names
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _Names;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _Names = value ?? Array.Empty<StringReference>();
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Returns the name of the event at the specified `index`
            /// or <c>null</c> if it's outside of the <see cref="Names"/> array.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public StringReference GetName(int index)
                => (uint)_Names.Length > (uint)index
                ? _Names[index]
                : null;

            /************************************************************************************************************************/

            /// <summary>Sets the name of the event at the specified `index`.</summary>
            /// <remarks>
            /// If the <see cref="Names"/> did not previously include that `index`
            /// it will be resized with a size equal to the <see cref="Count"/>.
            /// </remarks>
            public void SetName(int index, StringReference name)
            {
                AnimancerUtilities.Assert((uint)index < (uint)Count, IndexOutOfRangeError);

                // Capacity can't be 0 at this point because the above assertion would have failed.

                if (_Names.Length <= index)
                    Array.Resize(ref _Names, Capacity);

                _Names[index] = name;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Returns the index of the first event with the specified `name`
            /// or <c>-1</c> if there is no such event.
            /// </summary>
            /// <seealso cref="Names"/>
            /// <seealso cref="GetName"/>
            /// <seealso cref="SetName"/>
            /// <seealso cref="IndexOfRequired(StringReference, int)"/>
            public int IndexOf(StringReference name, int startIndex = 0)
            {
                if (_Names.Length == 0)
                    return -1;

                var count = Mathf.Min(Count, _Names.Length);
                for (; startIndex < count; startIndex++)
                    if (_Names[startIndex] == name)
                        return startIndex;

                return -1;
            }

            /// <summary>Returns the index of the first event with the specified `name`.</summary>
            /// <exception cref="ArgumentException">There is no such event.</exception>
            /// <seealso cref="IndexOf(StringReference, int)"/>
            public int IndexOfRequired(StringReference name, int startIndex = 0)
            {
                startIndex = IndexOf(name, startIndex);
                if (startIndex >= 0)
                    return startIndex;

                throw new ArgumentException(
                    $"No event exists with the name '{name}'." +
                    $" If the specified event isn't required, use IndexOf which will return -1 if not found.");
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Constructors
            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Sequence"/> which starts at 0 <see cref="Capacity"/>.
            /// <para></para>
            /// Adding anything to the sequence will set the <see cref="Capacity"/> = <see cref="DefaultCapacity"/>
            /// and then double it whenever the <see cref="Count"/> would exceed the <see cref="Capacity"/>.
            /// </summary>
            public Sequence()
            {
                _Events = Array.Empty<AnimancerEvent>();
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Creates a new <see cref="Sequence"/> which starts with the specified
            /// <see cref="Capacity"/>. It will be initially empty, but will have room for the
            /// given number of elements before any reallocations are required.
            /// </summary>
            public Sequence(int capacity)
            {
                _Events = capacity > 0
                    ? new AnimancerEvent[capacity]
                    : Array.Empty<AnimancerEvent>();
            }

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Sequence"/> and copies the contents of `copyFrom` into it.</summary>
            /// <remarks>To copy into an existing sequence, use <see cref="CopyFrom"/> instead.</remarks>
            public Sequence(Sequence copyFrom)
            {
                _Events = Array.Empty<AnimancerEvent>();
                if (copyFrom != null)
                    CopyFrom(copyFrom);
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Iteration
            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Returns the event at the specified `index`.</summary>
            public AnimancerEvent this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    AnimancerUtilities.Assert((uint)index < (uint)Count, IndexOutOfRangeError);
                    return _Events[index];
                }
            }

            /// <summary>[Pro-Only] Returns the event with the specified `name`.</summary>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            public AnimancerEvent this[StringReference name]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this[IndexOfRequired(name)];
            }

            /************************************************************************************************************************/

            /// <summary>Returns a string containing the details of all events in this sequence.</summary>
            public string DeepToString(bool multiLine = true)
            {
                var text = StringBuilderPool.Instance.Acquire()
                    .Append(ToString())
                    .Append('[')
                    .Append(Count)
                    .Append(']');

                text.Append(multiLine
                    ? "\n{"
                    : " {");

                for (int i = 0; i < Count; i++)
                {
                    if (multiLine)
                        text.Append("\n   ");
                    else if (i > 0)
                        text.Append(',');

                    text.Append(" [");

                    text.Append(i)
                        .Append("] ");

                    this[i].AppendDetails(text);

                    var name = GetName(i);
                    if (name != null)
                    {
                        text.Append(", Name: '")
                            .Append(name)
                            .Append('\'');
                    }
                }

                if (multiLine)
                {
                    text.Append("\n    [End] ");
                }
                else
                {
                    if (Count > 0)
                        text.Append(',');
                    text.Append(" [End] ");
                }
                _EndEvent.AppendDetails(text);

                if (multiLine)
                    text.Append("\n}\n");
                else
                    text.Append(" }");

                return text.ReleaseToString();
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Returns a <see cref="FastEnumerator{T}"/> for the events in this sequence,
            /// excluding the <see cref="EndEvent"/>.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public FastEnumerator<AnimancerEvent> GetEnumerator()
                => new(_Events, Count);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<AnimancerEvent> IEnumerable<AnimancerEvent>.GetEnumerator()
                => GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Returns the index of the `animancerEvent` or <c>-1</c> if there is no such event.</summary>
            /// <seealso cref="IndexOfRequired(int, AnimancerEvent)"/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int IndexOf(AnimancerEvent animancerEvent)
                => IndexOf(Count / 2, animancerEvent);

            /// <summary>[Pro-Only] Returns the index of the `animancerEvent`.</summary>
            /// <exception cref="ArgumentException">There is no such event.</exception>
            /// <seealso cref="IndexOf(AnimancerEvent)"/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int IndexOfRequired(AnimancerEvent animancerEvent)
                => IndexOfRequired(Count / 2, animancerEvent);

            /// <summary>[Pro-Only] Returns the index of the `animancerEvent` or <c>-1</c> if there is no such event.</summary>
            /// <seealso cref="IndexOfRequired(int, AnimancerEvent)"/>
            public int IndexOf(int indexHint, AnimancerEvent animancerEvent)
            {
                if (Count == 0)
                    return -1;

                if (indexHint >= Count)
                    indexHint = Count - 1;

                var events = _Events;
                var otherEvent = events[indexHint];
                if (otherEvent == animancerEvent)
                    return indexHint;

                if (otherEvent.normalizedTime > animancerEvent.normalizedTime)
                {
                    while (--indexHint >= 0)
                    {
                        otherEvent = events[indexHint];
                        if (otherEvent.normalizedTime < animancerEvent.normalizedTime)
                            return -1;
                        else if (otherEvent.normalizedTime == animancerEvent.normalizedTime &&
                            otherEvent.callback == animancerEvent.callback)
                            return indexHint;
                    }
                }
                else
                {
                    while (otherEvent.normalizedTime == animancerEvent.normalizedTime)
                    {
                        indexHint--;
                        if (indexHint < 0)
                            break;

                        otherEvent = events[indexHint];
                    }

                    while (++indexHint < Count)
                    {
                        otherEvent = events[indexHint];
                        if (otherEvent.normalizedTime > animancerEvent.normalizedTime)
                            return -1;
                        else if (otherEvent.normalizedTime == animancerEvent.normalizedTime &&
                            otherEvent.callback == animancerEvent.callback)
                            return indexHint;
                    }
                }

                return -1;
            }

            /// <summary>[Pro-Only] Returns the index of the `animancerEvent`.</summary>
            /// <exception cref="ArgumentException">There is no such event.</exception>
            /// <seealso cref="IndexOf(int, AnimancerEvent)"/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int IndexOfRequired(int indexHint, AnimancerEvent animancerEvent)
            {
                indexHint = IndexOf(indexHint, animancerEvent);
                if (indexHint >= 0)
                    return indexHint;

                throw new ArgumentException($"Event not found in {nameof(Sequence)} '{animancerEvent}'.");
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Modification
            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Adds the given event to this sequence. The <see cref="Count"/> is increased by one
            /// and if required, the <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <remarks>
            /// This methods returns the index at which the event is added, which is determined by
            /// its <see cref="normalizedTime"/> to keep the sequence sorted in ascending order.
            /// If there are already any events with the same <see cref="normalizedTime"/>,
            /// the new event is added immediately after them.
            /// </remarks>
            /// <exception cref="ArgumentNullException">
            /// Use <see cref="DummyCallback"/> or <see cref="InvokeBoundCallback"/> instead of <c>null</c>.
            /// </exception>
            public int Add(AnimancerEvent animancerEvent)
            {
#if UNITY_ASSERTIONS
                if (animancerEvent.callback == null)
                    throw new ArgumentNullException($"{nameof(AnimancerEvent)}.{nameof(callback)}", NullCallbackError);
#endif

                var index = Insert(animancerEvent.normalizedTime);
                _Events[index] = animancerEvent;
                return index;
            }

            /// <summary>[Pro-Only]
            /// Adds the given event to this sequence. The <see cref="Count"/> is increased by one
            /// and if required, the <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <remarks>
            /// This methods returns the index at which the event is added, which is determined by
            /// its <see cref="normalizedTime"/> to keep the sequence sorted in ascending order.
            /// If there are already any events with the same <see cref="normalizedTime"/>,
            /// the new event is added immediately after them.
            /// </remarks>
            /// <exception cref="ArgumentNullException">
            /// Use <see cref="DummyCallback"/> or <see cref="InvokeBoundCallback"/> instead of <c>null</c>.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Add(float normalizedTime, Action callback)
                => Add(new(normalizedTime, callback));

            /// <summary>[Pro-Only]
            /// Adds the given event to this sequence. The <see cref="Count"/> is increased by one
            /// and if required, the <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <remarks>
            /// This methods returns the index at which the event is added, which is determined by
            /// its <see cref="normalizedTime"/> to keep the sequence sorted in ascending order.
            /// If there are already any events with the same <see cref="normalizedTime"/>,
            /// the new event is added immediately after them.
            /// </remarks>
            public int Add(int indexHint, AnimancerEvent animancerEvent)
            {
#if UNITY_ASSERTIONS
                if (animancerEvent.callback == null)
                    throw new ArgumentNullException($"{nameof(AnimancerEvent)}.{nameof(callback)}", NullCallbackError);
#endif

                indexHint = Insert(indexHint, animancerEvent.normalizedTime);
                _Events[indexHint] = animancerEvent;
                return indexHint;
            }

            /// <summary>[Pro-Only]
            /// Adds the given event to this sequence. The <see cref="Count"/> is increased by one
            /// and if required, the <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <remarks>
            /// This methods returns the index at which the event is added, which is determined by
            /// its <see cref="normalizedTime"/> to keep the sequence sorted in ascending order.
            /// If there are already any events with the same <see cref="normalizedTime"/>,
            /// the new event is added immediately after them.
            /// </remarks>
            /// <exception cref="ArgumentNullException">
            /// Use <see cref="DummyCallback"/> or <see cref="InvokeBoundCallback"/> instead of <c>null</c>.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Add(int indexHint, float normalizedTime, Action callback)
                => Add(indexHint, new(normalizedTime, callback));

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Adds every event in the `enumerable` to this sequence using <see cref="Add(AnimancerEvent)"/>.
            /// </summary>
            /// <exception cref="ArgumentNullException">
            /// Use <see cref="DummyCallback"/> or <see cref="InvokeBoundCallback"/> instead of <c>null</c>.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddRange(IEnumerable<AnimancerEvent> enumerable)
            {
                foreach (var item in enumerable)
                    Add(item);
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Adds the specified `callback` to the event at the specified `index`.</summary>
            /// <exception cref="ArgumentNullException">
            /// Use <see cref="DummyCallback"/> or <see cref="InvokeBoundCallback"/> instead of <c>null</c>.
            /// </exception>
            public void AddCallback(int index, Action callback)
            {
                ref var animancerEvent = ref _Events[index];
                if (animancerEvent.callback == DummyCallback)
                {
#if UNITY_ASSERTIONS
                    if (callback == null)
                        throw new ArgumentNullException(nameof(callback), NullCallbackError);
#endif

                    animancerEvent.callback = callback;
                }
                else
                {
                    animancerEvent.callback += callback;
                }
                Version++;
            }

            /// <summary>[Pro-Only] Adds the specified `callback` to the event with the specified `name`.</summary>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            /// <exception cref="ArgumentNullException">
            /// Use <see cref="DummyCallback"/> or <see cref="InvokeBoundCallback"/> instead of <c>null</c>.
            /// </exception>
            /// <seealso cref="AddCallbacks(StringReference, Action)"/>
            /// <seealso cref="IndexOfRequired(StringReference, int)"/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddCallback(StringReference name, Action callback)
                => AddCallback(IndexOfRequired(name), callback);

            /// <summary>[Pro-Only]
            /// Adds the specified `callback` to every event with the specified `name`
            /// and returns the number of events that were found.
            /// </summary>
            /// <exception cref="ArgumentNullException">
            /// Use <see cref="DummyCallback"/> or <see cref="InvokeBoundCallback"/> instead of <c>null</c>.
            /// </exception>
            /// <seealso cref="AddCallback(StringReference, Action)"/>
            /// <seealso cref="IndexOf(StringReference, int)"/>
            public int AddCallbacks(StringReference name, Action callback)
            {
                var count = 0;
                var index = -1;
                while (true)
                {
                    index = IndexOf(name, index + 1);
                    if (index < 0)
                        return count;

                    count++;
                    AddCallback(index, callback);
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Adds the specified `callback` to the event at the specified `index`.
            /// <see cref="GetCurrentParameter{T}"/> will be used to get the callback's parameter.
            /// </summary>
            /// <exception cref="ArgumentNullException">The `callback` is <c>null</c>.</exception>
            /// <seealso cref="AddCallback{T}(StringReference, Action{T})"/>
            /// <seealso cref="AddCallbacks{T}(StringReference, Action{T})"/>
            public Action AddCallback<T>(int index, Action<T> callback)
            {
                ref var animancerEvent = ref _Events[index];
                AssertContainsParameter<T>(animancerEvent.callback);
                var parametized = Parametize(callback);
                animancerEvent.callback += parametized;
                Version++;
                return parametized;
            }

            /// <summary>[Pro-Only]
            /// Adds the specified `callback` to the event with the specified `name`.
            /// <see cref="GetCurrentParameter{T}"/> will be used to get the callback's parameter.
            /// </summary>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            /// <exception cref="ArgumentNullException">The `callback` is <c>null</c>.</exception>
            /// <seealso cref="AddCallbacks{T}(StringReference, Action{T})"/>
            /// <seealso cref="IndexOfRequired(StringReference, int)"/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Action AddCallback<T>(StringReference name, Action<T> callback)
                => AddCallback(IndexOfRequired(name), callback);

            /// <summary>[Pro-Only]
            /// Adds the specified `callback` to every event with the specified `name`
            /// and returns the number of events that were found.
            /// <see cref="GetCurrentParameter{T}"/> will be used to get the callback's parameter.
            /// </summary>
            /// <exception cref="ArgumentNullException">The `callback` is <c>null</c>.</exception>
            /// <seealso cref="AddCallback{T}(StringReference, Action{T})"/>
            /// <seealso cref="IndexOf(StringReference, int)"/>
            public int AddCallbacks<T>(StringReference name, Action<T> callback)
            {
                Action parametized = null;

                var count = 0;
                var index = -1;
                while (true)
                {
                    index = IndexOf(name, index + 1);
                    if (index < 0)
                        return count;

                    AssertContainsParameter<T>(_Events[index].callback);

                    parametized ??= Parametize(callback);

                    count++;
                    AddCallback(index, parametized);
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Removes the specified `callback` from the event at the specified `index`.</summary>
            /// <remarks>
            /// If the <see cref="callback"/> would become <c>null</c>,
            /// it is instead set to the <see cref="DummyCallback"/> since they are not allowed to be <c>null</c>.
            /// </remarks>
            public void RemoveCallback(int index, Action callback)
            {
                ref var animancerEvent = ref _Events[index];
                animancerEvent.callback -= callback;
                animancerEvent.callback ??= DummyCallback;
                Version++;
            }

            /// <summary>[Pro-Only] Removes the specified `callback` from the event with the specified `name`.</summary>
            /// <remarks>
            /// If the <see cref="callback"/> would become <c>null</c>,
            /// it is instead set to the <see cref="DummyCallback"/> since they are not allowed to be <c>null</c>.
            /// </remarks>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            /// <seealso cref="RemoveCallbacks(StringReference, Action)"/>
            /// <seealso cref="IndexOfRequired(StringReference, int)"/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveCallback(StringReference name, Action callback)
                => RemoveCallback(IndexOfRequired(name), callback);

            /// <summary>[Pro-Only]
            /// Removes the specified `callback` from every event with the specified `name`
            /// and returns the number of events that were found.
            /// </summary>
            /// <remarks>
            /// If a <see cref="callback"/> would become <c>null</c>,
            /// it is instead set to the <see cref="DummyCallback"/> since they are not allowed to be <c>null</c>.
            /// </remarks>
            /// <seealso cref="RemoveCallback(StringReference, Action)"/>
            /// <seealso cref="IndexOfRequired(StringReference, int)"/>
            public int RemoveCallbacks(StringReference name, Action callback)
            {
                var count = 0;
                var index = -1;
                while (true)
                {
                    index = IndexOf(name, index + 1);
                    if (index < 0)
                        return count;

                    count++;
                    RemoveCallback(index, callback);
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Replaces the <see cref="callback"/> of the event at the specified `index`.</summary>
            /// <exception cref="ArgumentNullException">
            /// Use <see cref="DummyCallback"/> or <see cref="InvokeBoundCallback"/> instead of <c>null</c>.
            /// </exception>
            public void SetCallback(int index, Action callback)
            {
#if UNITY_ASSERTIONS
                if (callback == null)
                    throw new ArgumentNullException(nameof(callback), NullCallbackError);
#endif

                ref var animancerEvent = ref _Events[index];
                animancerEvent.callback = callback;
                Version++;
            }

            /// <summary>[Pro-Only] Replaces the <see cref="callback"/> of the event with the specified `name`.</summary>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            /// <exception cref="ArgumentNullException">
            /// Use <see cref="DummyCallback"/> or <see cref="InvokeBoundCallback"/> instead of <c>null</c>.
            /// </exception>
            /// <seealso cref="SetCallbacks(StringReference, Action)"/>
            /// <seealso cref="IndexOfRequired(StringReference, int)"/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetCallback(StringReference name, Action callback)
                => SetCallback(IndexOfRequired(name), callback);

            /// <summary>[Pro-Only]
            /// Replaces the <see cref="callback"/> of every event with the specified `name`
            /// and returns the number of events that were found.
            /// </summary>
            /// <exception cref="ArgumentNullException">
            /// Use <see cref="DummyCallback"/> or <see cref="InvokeBoundCallback"/> instead of <c>null</c>.
            /// </exception>
            /// <seealso cref="SetCallback(StringReference, Action)"/>
            /// <seealso cref="IndexOfRequired(StringReference, int)"/>
            public int SetCallbacks(StringReference name, Action callback)
            {
                var count = 0;
                var index = -1;
                while (true)
                {
                    index = IndexOf(name, index + 1);
                    if (index < 0)
                        return count;

                    count++;
                    SetCallback(index, callback);
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Sets the <see cref="normalizedTime"/> of the event at the specified `index`.</summary>
            /// <remarks>
            /// If multiple events have the same <see cref="normalizedTime"/>, this method will
            /// avoid re-arranging them where calling <see cref="Remove(int)"/> then
            /// <see cref="Add(AnimancerEvent)"/> would always re-add the moved event
            /// as the last one with that time.
            /// </remarks>
            public int SetNormalizedTime(int index, float normalizedTime)
            {
#if UNITY_ASSERTIONS
                if (!normalizedTime.IsFinite())
                    throw new ArgumentOutOfRangeException(nameof(normalizedTime), normalizedTime,
                        $"{nameof(normalizedTime)} {Strings.MustBeFinite}");
#endif

                var events = _Events;
                var animancerEvent = events[index];
                if (animancerEvent.normalizedTime == normalizedTime)
                    return index;

                var moveTo = index;
                if (animancerEvent.normalizedTime < normalizedTime)
                {
                    while (moveTo < Count - 1)
                    {
                        if (events[moveTo + 1].normalizedTime >= normalizedTime)
                            break;
                        else
                            moveTo++;
                    }
                }
                else
                {
                    while (moveTo > 0)
                    {
                        if (events[moveTo - 1].normalizedTime <= normalizedTime)
                            break;
                        else
                            moveTo--;
                    }
                }

                if (index != moveTo)
                {
                    var name = GetName(index);
                    Remove(index);

                    index = moveTo;

                    Insert(index);
                    if (!name.IsNullOrEmpty())
                        SetName(index, name);
                }

                animancerEvent.normalizedTime = normalizedTime;
                events[index] = animancerEvent;

                Version++;

                return index;
            }

            /// <summary>[Pro-Only] Sets the <see cref="normalizedTime"/> of the event with the specified `name`.</summary>
            /// <remarks>
            /// If multiple events have the same <see cref="normalizedTime"/>, this method will
            /// avoid re-arranging them where calling <see cref="Remove(int)"/> then
            /// <see cref="Add(AnimancerEvent)"/> would always re-add the moved event
            /// as the last one with that time.
            /// </remarks>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            /// <seealso cref="IndexOfRequired(StringReference, int)"/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int SetNormalizedTime(StringReference name, float normalizedTime)
                => SetNormalizedTime(IndexOfRequired(name), normalizedTime);

            /// <summary>[Pro-Only] Sets the <see cref="normalizedTime"/> of the matching `animancerEvent`.</summary>
            /// <remarks>
            /// If multiple events have the same <see cref="normalizedTime"/>, this method will
            /// avoid re-arranging them where calling <see cref="Remove(int)"/> then
            /// <see cref="Add(AnimancerEvent)"/> would always re-add the moved event
            /// as the last one with that time.
            /// </remarks>
            /// <exception cref="ArgumentException">There is no event matching the `animancerEvent`.</exception>
            /// <seealso cref="IndexOfRequired(AnimancerEvent)"/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int SetNormalizedTime(AnimancerEvent animancerEvent, float normalizedTime)
                => SetNormalizedTime(IndexOfRequired(animancerEvent), normalizedTime);

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Determines the index where a new event with the specified `normalizedTime` should
            /// be added in order to keep this sequence sorted, increases the <see cref="Count"/>
            /// by one, doubles the <see cref="Capacity"/> if required, moves any existing events
            /// to open up the chosen index, and returns that index.
            /// <para></para>
            /// </summary>
            /// <remarks>
            /// This overload starts searching for the desired index from the end of the sequence,
            /// based on the assumption that elements will usually be added in order.
            /// </remarks>
            private int Insert(float normalizedTime)
            {
                var index = Count;
                var events = _Events;
                while (index > 0 && events[index - 1].normalizedTime > normalizedTime)
                    index--;
                Insert(index);
                return index;
            }

            /// <summary>[Pro-Only]
            /// Determines the index where a new event with the specified `normalizedTime` should
            /// be added in order to keep this sequence sorted, increases the <see cref="Count"/>
            /// by one, doubles the <see cref="Capacity"/> if required, moves any existing events
            /// to open up the chosen index, and returns that index.
            /// <para></para>
            /// This overload starts searching for the desired index from the `hint`.
            /// </summary>
            private int Insert(int indexHint, float normalizedTime)
            {
                if (Count == 0)
                {
                    Count = 0;
                }
                else
                {
                    if (indexHint >= Count)
                        indexHint = Count - 1;

                    var events = _Events;
                    if (events[indexHint].normalizedTime > normalizedTime)
                    {
                        while (indexHint > 0 && events[indexHint - 1].normalizedTime > normalizedTime)
                            indexHint--;
                    }
                    else
                    {
                        while (indexHint < Count && events[indexHint].normalizedTime <= normalizedTime)
                            indexHint++;
                    }
                }

                Insert(indexHint);
                return indexHint;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Increases the <see cref="Count"/> by one, doubles the <see cref="Capacity"/> if required,
            /// and moves any existing events to open up the `index`.
            /// </summary>
            private void Insert(int index)
            {
                AnimancerUtilities.Assert((uint)index <= (uint)Count, IndexOutOfRangeError);

                var capacity = _Events.Length;
                if (Count == capacity)
                {
                    if (capacity == 0)
                    {
                        capacity = DefaultCapacity;
                        _Events = new AnimancerEvent[DefaultCapacity];
                    }
                    else
                    {
                        capacity *= 2;
                        if (capacity < DefaultCapacity)
                            capacity = DefaultCapacity;

                        var events = new AnimancerEvent[capacity];

                        Array.Copy(_Events, 0, events, 0, index);
                        if (Count > index)
                            Array.Copy(_Events, index, events, index + 1, Count - index);

                        _Events = events;
                    }
                }
                else if (Count > index)
                {
                    Array.Copy(_Events, index, _Events, index + 1, Count - index);
                }

                if (_Names.Length > 0)
                {
                    if (_Names.Length < capacity)
                    {
                        var names = new StringReference[capacity];

                        Array.Copy(_Names, 0, names, 0, Math.Min(_Names.Length, index));
                        if (index <= Count &&
                            index < _Names.Length)
                            Array.Copy(_Names, index, names, index + 1, Count - index);

                        _Names = names;
                    }
                    else
                    {
                        if (Count > index)
                            Array.Copy(_Names, index, _Names, index + 1, Count - index);

                        _Names[index] = null;
                    }
                }

                Count++;
                Version++;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Removes the event at the specified `index` from this sequence by decrementing the
            /// <see cref="Count"/> and copying all events after the removed one down one place.
            /// </summary>
            public void Remove(int index)
            {
                AnimancerUtilities.Assert((uint)index < (uint)Count, IndexOutOfRangeError);
                Count--;
                if (index < Count)
                {
                    Array.Copy(_Events, index + 1, _Events, index, Count - index);

                    if (_Names.Length > 0)
                    {
                        var nameCount = Mathf.Min(Count + 1, _Names.Length);
                        if (index + 1 < nameCount)
                            Array.Copy(_Names, index + 1, _Names, index, nameCount - index - 1);

                        _Names[nameCount - 1] = default;
                    }
                }
                else if ((uint)_Names.Length > (uint)index)
                {
                    _Names[index] = default;
                }

                _Events[Count] = default;
                Version++;
            }

            /// <summary>[Pro-Only]
            /// Removes the event with the specified `name` from this sequence by decrementing the
            /// <see cref="Count"/> and copying all events after the removed one down one place.
            /// Returns true if the event was found and removed.
            /// </summary>
            public bool Remove(StringReference name)
            {
                var index = IndexOf(name);
                if (index < 0)
                    return false;

                Remove(index);
                return true;
            }

            /// <summary>[Pro-Only]
            /// Removes the `animancerEvent` from this sequence by decrementing the
            /// <see cref="Count"/> and copying all events after the removed one down one place.
            /// Returns true if the event was found and removed.
            /// </summary>
            public bool Remove(AnimancerEvent animancerEvent)
            {
                var index = IndexOf(animancerEvent);
                if (index < 0)
                    return false;

                Remove(index);
                return true;
            }

            /************************************************************************************************************************/

            /// <summary>Removes all events, including the <see cref="EndEvent"/>.</summary>
            public void Clear()
            {
                Array.Clear(_Names, 0, _Names.Length);
                Array.Clear(_Events, 0, Count);
                Count = 0;
                Version++;

                _EndEvent = new(float.NaN, null);
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Copying
            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Sequence"/> and copies the contents of <c>this</c> into it.</summary>
            /// <remarks>To copy into an existing sequence, use <see cref="CopyFrom"/> instead.</remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Sequence Clone()
                => new(this);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Sequence Clone(CloneContext context)
                => new(this);

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public void CopyFrom(Sequence copyFrom)
            {
                if (copyFrom == null)
                {
                    Array.Clear(_Names, 0, _Names.Length);
                    Array.Clear(_Events, 0, Count);
                    Count = 0;
                    Capacity = 0;
                    _EndEvent = default;
                    return;
                }

                CopyNamesFrom(copyFrom._Names, copyFrom.Count);

                var sourceCount = copyFrom.Count;

                if (Count > sourceCount)
                    Array.Clear(_Events, Count, sourceCount - Count);
                else if (_Events.Length < sourceCount)
                    Capacity = sourceCount;

                Count = sourceCount;

                Array.Copy(copyFrom._Events, 0, _Events, 0, sourceCount);

                _EndEvent = copyFrom._EndEvent;
            }

            /************************************************************************************************************************/

            /// <summary>Copies the given array into the <see cref="Names"/>.</summary>
            private void CopyNamesFrom(StringReference[] copyFrom, int maxCount)
            {
                if (_Names.Length == 0)
                {
                    // Both empty.
                    if (copyFrom.Length == 0)
                        return;

                    // Copying into empty.
                    maxCount = Math.Min(copyFrom.Length, maxCount);
                    _Names = new StringReference[copyFrom.Length];
                    Array.Copy(copyFrom, _Names, maxCount);
                    return;
                }

                // Copying empty into not empty.
                if (copyFrom.Length == 0)
                {
                    Array.Clear(_Names, 0, _Names.Length);
                    return;
                }

                // Copying into large enough array.
                maxCount = Math.Min(copyFrom.Length, maxCount);
                if (_Names.Length >= maxCount)
                {
                    Array.Copy(copyFrom, _Names, maxCount);

                    Array.Clear(_Names, maxCount, _Names.Length - maxCount);
                }
                else// Need larger array.
                {
                    _Names = new StringReference[copyFrom.Length];
                    Array.Copy(copyFrom, _Names, maxCount);
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Copies the <see cref="AnimationClip.events"/> into this <see cref="Sequence"/>.</summary>
            /// <remarks>
            /// The <see cref="callback"/> of the new events will be empty and can be set by
            /// <see cref="SetCallback(StringReference, Action)"/>.
            /// <para></para>
            /// If you're going to play the `animation`, consider disabling <see cref="Animator.fireEvents"/>
            /// so the events copied by this method are not triggered as <see cref="AnimationEvent"/>s.
            /// Otherwise they would still trigger in addition to the <see cref="AnimancerEvent"/>s copied here.
            /// </remarks>
            public void AddAllEvents(AnimationClip animation)
            {
                if (animation == null)
                    return;

                var animationEvents = animation.events;
                if (animationEvents.Length == 0)
                    return;

                var capacity = Count + animationEvents.Length;
                if (Capacity < capacity)
                    Capacity = Mathf.Max(Mathf.NextPowerOfTwo(capacity), DefaultCapacity);

                if (_Names.Length < Capacity)
                    Array.Resize(ref _Names, Capacity);

                var length = animation.length;

                var index = -1;
                for (int i = 0; i < animationEvents.Length; i++)
                {
                    var animationEvent = animationEvents[i];
                    index = Add(index + 1, new(animationEvent.time / length, InvokeBoundCallback));
                    _Names[index] = animationEvent.functionName;
                }
            }

            /************************************************************************************************************************/

            /// <summary>[<see cref="ICollection{T}"/>] [Pro-Only]
            /// Copies all the events from this sequence into the `array`, starting at the `index`.
            /// </summary>
            public void CopyTo(AnimancerEvent[] array, int index)
            {
                Array.Copy(_Events, 0, array, index, Count);
            }

            /************************************************************************************************************************/

            /// <summary>Are all events in this sequence identical to the ones in the `other` sequence?</summary>
            public bool ContentsAreEqual(Sequence other)
            {
                if (other == null ||
                    _EndEvent != other._EndEvent)
                    return false;

                if (Count != other.Count)
                    return false;

                for (int i = Count - 1; i >= 0; i--)
                    if (this[i] != other[i])
                        return false;

                return true;
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Assertions
            /************************************************************************************************************************/

            /// <summary>[Assert-Conditional]
            /// Throws an <see cref="ArgumentOutOfRangeException"/>
            /// if any event is outside the range of <c>0 &lt;= normalizedTime &lt; 1</c>.
            /// </summary>
            /// <remarks>
            /// This excludes the <see cref="EndEvent"/> since it works differently to other events.
            /// </remarks>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            public void AssertNormalizedTimes(AnimancerState state)
            {
                if (Count == 0 ||
                    (_Events[0].normalizedTime >= 0 && _Events[Count - 1].normalizedTime < 1))
                    return;

                throw new ArgumentOutOfRangeException(nameof(normalizedTime),
                    "Events on looping animations are triggered every loop and must be" +
                    $" within the range of 0 <= {nameof(normalizedTime)} < 1.\n{state}\n{DeepToString()}");
            }

            /************************************************************************************************************************/

            /// <summary>[Assert-Conditional]
            /// Calls <see cref="AssertNormalizedTimes(AnimancerState)"/> if `isLooping` is true.
            /// </summary>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            public void AssertNormalizedTimes(AnimancerState state, bool isLooping)
            {
                if (isLooping)
                    AssertNormalizedTimes(state);
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
        }
    }
}

