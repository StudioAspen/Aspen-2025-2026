// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using UnityEngine;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    partial struct AnimancerEvent
    {
        /// https://kybernetik.com.au/animancer/api/Animancer/Sequence
        partial class Sequence
        {
            /// <summary>
            /// Serializable data which can be used to construct an <see cref="Sequence"/> using
            /// <see cref="StringAsset"/>s and <see cref="IInvokable"/>s.
            /// </summary>
            /// https://kybernetik.com.au/animancer/api/Animancer/Serializable
            [Serializable]
            public class Serializable : ICloneable<Serializable>
#if UNITY_EDITOR
                , ISerializationCallbackReceiver
#endif
            {
                /************************************************************************************************************************/
                #region Fields and Properties
                /************************************************************************************************************************/

                [SerializeField]
                private float[] _NormalizedTimes;

                /// <summary>[<see cref="SerializeField"/>] The serialized <see cref="normalizedTime"/>s.</summary>
                /// <remarks>The last item is used for the <see cref="EndEvent"/>.</remarks>
                public ref float[] NormalizedTimes => ref _NormalizedTimes;

                /************************************************************************************************************************/

                [SerializeReference, Polymorphic]
                private IInvokable[] _Callbacks;

                /// <summary>[<see cref="SerializeField"/>] The serialized <see cref="callback"/>s.</summary>
                /// <remarks>
                /// This array only needs to be large enough to hold the last item that isn't null.
                /// <para></para>
                /// If this array is larger than the <see cref="NormalizedTimes"/>, the first item
                /// with no corresponding time will be used as the <see cref="OnEnd"/> callback
                /// and any others after that will be ignored.
                /// </remarks>
                public ref IInvokable[] Callbacks => ref _Callbacks;

                /************************************************************************************************************************/

                [SerializeField]
                private StringAsset[] _Names;

                /// <summary>[<see cref="SerializeField"/>] The serialized <see cref="Sequence.Names"/>.</summary>
                public ref StringAsset[] Names => ref _Names;

                /************************************************************************************************************************/
#if UNITY_EDITOR
                /************************************************************************************************************************/

                /// <summary>[Editor-Only] [Internal]
                /// The name of the array field which stores the <see cref="normalizedTime"/>s.
                /// </summary>
                internal const string NormalizedTimesField = nameof(_NormalizedTimes);

                /// <summary>[Editor-Only] [Internal]
                /// The name of the array field which stores the serialized <see cref="Callbacks"/>.
                /// </summary>
                internal const string CallbacksField = nameof(_Callbacks);

                /// <summary>[Editor-Only] [Internal]
                /// The name of the array field which stores the serialized <see cref="Names"/>.
                /// </summary>
                internal const string NamesField = nameof(_Names);

                /// <summary>[Editor-Only] Disable Inspector Gadgets Nested Object Drawers.</summary>
                private const bool NestedObjectDrawers = false;

                /************************************************************************************************************************/
#endif
                /************************************************************************************************************************/

                private Sequence _Events;

                /// <summary>Returns the <see cref="Events"/> or <c>null</c> if it wasn't yet initialized.</summary>
                public Sequence InitializedEvents
                    => _Events;

                /// <summary>
                /// The runtime <see cref="Sequence"/> compiled from this <see cref="Serializable"/>.
                /// Each call after the first will return the same reference.
                /// </summary>
                /// <remarks>
                /// Unlike <see cref="GetEventsOptional"/>, this property will create an empty
                /// <see cref="Sequence"/> instead of returning null if there are no events.
                /// </remarks>
                public Sequence Events
                {
                    get
                    {
                        if (_Events == null)
                        {
                            GetEventsOptional();
                            _Events ??= new();
                        }

                        return _Events;
                    }
                    set => _Events = value;
                }

                /************************************************************************************************************************/
                #endregion
                /************************************************************************************************************************/
                #region Initialization
                /************************************************************************************************************************/

                /// <summary>
                /// Returns the runtime <see cref="Sequence"/> compiled from this <see cref="Serializable"/>.
                /// Each call after the first will return the same reference.
                /// </summary>
                /// <remarks>
                /// This method returns null if the sequence would be empty anyway and is used by the implicit
                /// conversion from <see cref="Serializable"/> to <see cref="Sequence"/>.
                /// </remarks>
                public Sequence GetEventsOptional()
                {
                    if (_Events != null ||
                        _NormalizedTimes == null)
                        return _Events;

                    var timeCount = _NormalizedTimes.Length;
                    if (timeCount == 0)
                        return null;

                    var callbackCount = _Callbacks != null
                        ? _Callbacks.Length
                        : 0;

                    var callback = callbackCount >= timeCount--
                        ? GetInvoke(_Callbacks[timeCount])
                        : null;
                    var endEvent = new AnimancerEvent(_NormalizedTimes[timeCount], callback);

                    _Events = new(timeCount)
                    {
                        EndEvent = endEvent,
                        Count = timeCount,
                        Names = StringAsset.ToStringReferences(_Names),
                    };

                    var events = _Events._Events;
                    for (int i = 0; i < timeCount; i++)
                    {
                        callback = i < callbackCount
                            ? GetInvoke(_Callbacks[i])
                            : InvokeBoundCallback;

                        events[i] = new(_NormalizedTimes[i], callback);
                    }

                    return _Events;
                }

                /// <summary>Calls <see cref="GetEventsOptional"/>.</summary>
                public static implicit operator Sequence(Serializable serializable)
                    => serializable?.GetEventsOptional();

                /************************************************************************************************************************/

                /// <summary>
                /// Returns the <see cref="IInvokable.Invoke"/> if the `invokable` isn't <c>null</c>.
                /// Otherwise, returns <c>null</c>.
                /// </summary>
                public static Action GetInvoke(IInvokable invokable)
                    => invokable != null
                    ? invokable.Invoke
                    : InvokeBoundCallback;

                /************************************************************************************************************************/
                #endregion
                /************************************************************************************************************************/
                #region End Event
                /************************************************************************************************************************/

                /// <summary>Returns the <see cref="normalizedTime"/> of the <see cref="EndEvent"/>.</summary>
                /// <remarks>If the value is not set, the value is determined by <see cref="GetDefaultNormalizedEndTime"/>.</remarks>
                public float GetNormalizedEndTime(float speed = 1)
                {
                    return _NormalizedTimes.IsNullOrEmpty()
                        ? GetDefaultNormalizedEndTime(speed)
                        : _NormalizedTimes[^1];
                }

                /************************************************************************************************************************/

                /// <summary>Sets the <see cref="normalizedTime"/> of the <see cref="EndEvent"/>.</summary>
                public void SetNormalizedEndTime(float normalizedTime)
                {
                    if (_NormalizedTimes.IsNullOrEmpty())
                        _NormalizedTimes = new float[] { normalizedTime };
                    else
                        _NormalizedTimes[^1] = normalizedTime;
                }

                /************************************************************************************************************************/

                /// <summary>Sets the <see cref="callback"/> of the <see cref="EndEvent"/>.</summary>
                public void SetEndCallback(IInvokable callback = null)
                {
                    if (_NormalizedTimes.IsNullOrEmpty())
                        _NormalizedTimes = new float[] { float.NaN };

                    InsertOptionalItem(ref _Callbacks, _NormalizedTimes.Length - 1, callback);
                }

                /************************************************************************************************************************/

                /// <summary>Sets the data of the <see cref="EndEvent"/>.</summary>
                public void SetEndEvent(float normalizedTime = float.NaN, IInvokable callback = null)
                {
                    if (_NormalizedTimes.IsNullOrEmpty())
                        _NormalizedTimes = new float[] { normalizedTime };
                    else
                        _NormalizedTimes[^1] = normalizedTime;

                    InsertOptionalItem(ref _Callbacks, _NormalizedTimes.Length - 1, callback);
                }

                /************************************************************************************************************************/
                #endregion
                /************************************************************************************************************************/
                #region Other Events
                /************************************************************************************************************************/

                /// <summary>Adds an event to the serialized fields.</summary>
                public int AddEvent(float normalizedTime, IInvokable callback = null, StringAsset name = null)
                {
                    int index;

                    if (_NormalizedTimes.IsNullOrEmpty())
                    {
                        _NormalizedTimes = new float[] { normalizedTime, float.NaN };
                        index = 0;
                    }
                    else
                    {
                        index = _NormalizedTimes.Length - 1;

                        for (int i = 0; i < _NormalizedTimes.Length - 1; i++)
                        {
                            if (_NormalizedTimes[i] > normalizedTime)
                            {
                                index = i;
                                break;
                            }
                        }

                        AnimancerUtilities.InsertAt(ref _NormalizedTimes, index, normalizedTime);
                    }

                    InsertOptionalItem(ref _Callbacks, index, callback);
                    InsertOptionalItem(ref _Names, index, name);

                    return index;
                }

                /************************************************************************************************************************/

                /// <summary>Inserts an `item` at the specified `index` in an optional `array`.</summary>
                /// <remarks>
                /// If the `item` is <c>null</c> then the array only needs
                /// to be expanded if it was already larger than the `index`.
                /// </remarks>
                private static void InsertOptionalItem<T>(ref T[] array, int index, T item)
                    where T : class
                {
                    if (item == null &&
                        (array == null || array.Length < index))
                        return;

                    AnimancerUtilities.InsertAt(ref array, index, item);
                }

                /************************************************************************************************************************/

                /// <summary>Removes an event from the serialized fields.</summary>
                public void RemoveEvent(int index)
                {
                    if (_NormalizedTimes.IsNullOrEmpty())
                        return;

                    AnimancerUtilities.RemoveAt(ref _NormalizedTimes, index);

                    if (_Callbacks != null && _Callbacks.Length > index)
                        AnimancerUtilities.RemoveAt(ref _Callbacks, index);

                    if (_Names != null && _Names.Length > index)
                        AnimancerUtilities.RemoveAt(ref _Names, index);
                }

                /************************************************************************************************************************/

                /// <summary>Removes all events.</summary>
                public void Clear(bool keepEndEvent = false)
                {
                    if (keepEndEvent)
                    {
                        if (_NormalizedTimes != null && _NormalizedTimes.Length > 0)
                            _NormalizedTimes = new float[] { _NormalizedTimes[^1] };
                        else
                            _NormalizedTimes = null;

                        if (_Callbacks != null && _Callbacks.Length > 0)
                            _Callbacks = new IInvokable[] { _Callbacks[^1] };
                        else
                            _Callbacks = null;
                    }
                    else
                    {
                        _NormalizedTimes = null;
                        _Callbacks = null;
                    }

                    _Names = null;
                }

                /************************************************************************************************************************/
                #endregion
                /************************************************************************************************************************/
                #region Copying
                /************************************************************************************************************************/

                /// <summary>Creates a new <see cref="Serializable"/> and copies the contents of <c>this</c> into it.</summary>
                /// <remarks>To copy into an existing sequence, use <see cref="CopyFrom"/> instead.</remarks>
                public Serializable Clone()
                {
                    var clone = new Serializable();
                    clone.CopyFrom(this);
                    return clone;
                }

                /// <inheritdoc/>
                public Serializable Clone(CloneContext context)
                    => Clone();

                /************************************************************************************************************************/

                /// <inheritdoc/>
                public void CopyFrom(Serializable copyFrom)
                {
                    if (copyFrom == null)
                    {
                        _NormalizedTimes = default;
                        _Callbacks = default;
                        _Names = default;
                        return;
                    }

                    AnimancerUtilities.CopyExactArray(copyFrom._NormalizedTimes, ref _NormalizedTimes);
                    AnimancerUtilities.CopyExactArray(copyFrom._Callbacks, ref _Callbacks);
                    AnimancerUtilities.CopyExactArray(copyFrom._Names, ref _Names);
                }

                /************************************************************************************************************************/
                #endregion
                /************************************************************************************************************************/
                #region Serialization
                /************************************************************************************************************************/
#if UNITY_EDITOR
                /************************************************************************************************************************/

                /// <summary>[Editor-Only] Does nothing.</summary>
                void ISerializationCallbackReceiver.OnAfterDeserialize() { }

                /************************************************************************************************************************/

                /// <summary>[Editor-Only] [Internal]
                /// Called by <see cref="ISerializationCallbackReceiver.OnBeforeSerialize"/>.
                /// </summary>
                internal static event Action<Serializable> OnBeforeSerialize;

                /// <summary>[Editor-Only] Ensures that the events are sorted by time (excluding the end event).</summary>
                void ISerializationCallbackReceiver.OnBeforeSerialize()
                    => OnBeforeSerialize?.Invoke(this);

                /************************************************************************************************************************/

                /// <summary>[Editor-Only] [Internal]
                /// Should the arrays be prevented from reducing their size when their last elements are unused?
                /// </summary>
                internal static bool DisableCompactArrays { get; set; }

                /// <summary>[Editor-Only] [Internal]
                /// Removes empty data from the ends of the arrays to reduce the serialized data size.
                /// </summary>
                internal void CompactArrays()
                {
                    if (DisableCompactArrays)
                        return;

                    // If there is only one time and it is NaN, we don't need to store anything.
                    if (_NormalizedTimes == null ||
                        (_NormalizedTimes.Length == 1 &&
                        (_Callbacks == null || _Callbacks.Length == 0) &&
                        (_Names == null || _Names.Length == 0) &&
                        float.IsNaN(_NormalizedTimes[0])))
                    {
                        _NormalizedTimes = Array.Empty<float>();
                        _Callbacks = Array.Empty<IInvokable>();
                        _Names = Array.Empty<StringAsset>();
                        return;
                    }

                    Trim(ref _Callbacks, _NormalizedTimes.Length, callback => callback != null);
                    Trim(ref _Names, _NormalizedTimes.Length, name => name != null);
                }

                /************************************************************************************************************************/

                /// <summary>[Editor-Only] Removes unimportant values from the end of the `array`.</summary>
                private static void Trim<T>(ref T[] array, int maxLength, Func<T, bool> isImportant)
                {
                    if (array == null)
                        return;

                    var count = Math.Min(array.Length, maxLength);

                    while (count >= 1)
                    {
                        var item = array[count - 1];
                        if (isImportant(item))
                            break;
                        else
                            count--;
                    }

                    Array.Resize(ref array, count);
                }

                /************************************************************************************************************************/
#endif
                /************************************************************************************************************************/
                #endregion
                /************************************************************************************************************************/
            }
        }
    }
}

