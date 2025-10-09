// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    partial struct AnimancerEvent
    {
        /// <summary>
        /// A system which triggers events in an <see cref="Sequence"/>
        /// based on a target <see cref="State"/>.
        /// </summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/Dispatcher
        public class Dispatcher : IHasDescription
        {
            /************************************************************************************************************************/

            /// <summary>The target state.</summary>
            public readonly AnimancerState State;

            /// <summary>
            /// <see cref="AnimancerState.OwnedEvents"/> and
            /// <see cref="AnimancerState.SharedEvents"/>.
            /// </summary>
            /// <remarks>Should never be null.</remarks>
            public Sequence Events { get; private set; }

            /// <summary><see cref="AnimancerState.HasOwnedEvents"/></summary>
            public bool HasOwnEvents { get; private set; }

            private float _PreviousNormalizedTime;
            private int _NextEventIndex = RecalculateEventIndex;
            private int _SequenceVersion = -1;// When version changes, next event index is invalid.
            private bool _WasPlayingForwards;// When direction changes, next event index is invalid.

            /// <summary>
            /// A special value for the <see cref="_NextEventIndex"/>
            /// which indicates that it needs to be recalculated.
            /// </summary>
            private const int RecalculateEventIndex = int.MinValue;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Dispatcher"/>.</summary>
            public Dispatcher(AnimancerState state)
            {

                State = state;
                _PreviousNormalizedTime = state.NormalizedTime;

#if UNITY_ASSERTIONS
                OptionalWarning.UnsupportedEvents.Log(state.UnsupportedEventsMessage, state.Graph?.Component);
#endif
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Setters for <see cref="AnimancerState.OwnedEvents"/>
            /// and <see cref="AnimancerState.SharedEvents"/>.
            /// </summary>
            public void SetEvents(Sequence events, bool isOwned)
            {
                Events = events;
                _NextEventIndex = RecalculateEventIndex;
                _SequenceVersion = Events.Version;
                HasOwnEvents = isOwned;
            }

            /************************************************************************************************************************/

            /// <summary>Sets <see cref="HasOwnEvents"/> to <c>false</c>.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void DismissEventOwnership()
                => HasOwnEvents = false;

            /************************************************************************************************************************/

            /// <summary><see cref="AnimancerState.Events(object, out Sequence)"/>.</summary>
            public bool InitializeEvents(out Sequence events)
            {
                if (HasOwnEvents)
                {
                    events = Events;
                    return false;
                }

                Events = events = new(Events);
                _NextEventIndex = RecalculateEventIndex;
                _SequenceVersion = Events.Version;
                HasOwnEvents = true;
                return true;
            }

            /************************************************************************************************************************/

            /// <summary>[Internal]
            /// Notifies this dispatcher that the target's <see cref="Time"/> has changed.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void OnSetTime()
            {
                // The Playable's time won't move in the same frame it was set,
                // so we'll just let the next frame grab its time.
                _PreviousNormalizedTime = float.NaN;
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public void UpdateEvents(bool raiseEvents)
            {
                State.GetEventDispatchInfo(out var length, out var normalizedTime, out var isLooping);

                // If we aren't raising events or don't have a previous time, just keep track of the time.
                if (!raiseEvents || float.IsNaN(_PreviousNormalizedTime))
                {
                    _PreviousNormalizedTime = normalizedTime;

                    // Since we aren't paying attention to the events,
                    // we also aren't paying attention to which index the time corresponds to.
                    _NextEventIndex = RecalculateEventIndex;

                    return;
                }

                // If the sequence is modified, we need to recalculate the next event index.
                var sequenceVersion = Events.Version;
                if (_SequenceVersion != sequenceVersion)
                {
                    _SequenceVersion = sequenceVersion;
                    _NextEventIndex = RecalculateEventIndex;
                }

                if (length > 0)
                {
                    if (_PreviousNormalizedTime == normalizedTime)
                        return;

                    CheckGeneralEvents(normalizedTime, isLooping);

                    CheckEndEvent(normalizedTime);

                    _PreviousNormalizedTime = normalizedTime;
                }
                else// Length zero, negative, or NaN.
                {
                    UpdateZeroLength();
                }
            }

            /************************************************************************************************************************/

            /// <summary>If the state has zero length, trigger its events every frame.</summary>
            private void UpdateZeroLength()
            {
                var speed = State.EffectiveSpeed;
                if (speed == 0)
                    return;

                if (Events.Count > 0)
                {
                    int playDirectionInt;
                    if (speed < 0)
                    {
                        playDirectionInt = -1;
                        if (_NextEventIndex == RecalculateEventIndex ||
                            _WasPlayingForwards)
                        {
                            _NextEventIndex = Events.Count - 1;
                            _WasPlayingForwards = false;
                        }
                    }
                    else
                    {
                        playDirectionInt = 1;
                        if (_NextEventIndex == RecalculateEventIndex ||
                            !_WasPlayingForwards)
                        {
                            _NextEventIndex = 0;
                            _WasPlayingForwards = true;
                        }
                    }

                    if (!InvokeAllEvents(Events, 1, playDirectionInt))
                        return;
                }

                var endEvent = Events.EndEvent;
                if (endEvent.callback != null)
                    endEvent.DelayInvoke(EndEventName, State);
            }

            /************************************************************************************************************************/

            /// <summary>General events are triggered on the frame when their time passes.</summary>
            /// <remarks>Looping animations trigger their events every loop.</remarks>
            private void CheckGeneralEvents(float currentTime, bool isLooping)
            {
                var count = Events.Count;
                if (count == 0)
                {
                    _NextEventIndex = 0;
                    return;
                }

                ValidateNextEventIndex(
                    isLooping,
                    ref currentTime,
                    out var playDirectionFloat,
                    out var playDirectionInt);

                if (isLooping)// Looping.
                {
                    var animancerEvent = Events[_NextEventIndex];
                    var eventTime = animancerEvent.normalizedTime * playDirectionFloat;

                    var loopDelta = GetLoopDelta(_PreviousNormalizedTime, currentTime, eventTime);
                    if (loopDelta == 0)
                        return;

                    // For each additional loop, invoke all events without needing to check their times.
                    if (!InvokeAllEvents(Events, loopDelta - 1, playDirectionInt))
                        return;

                    var loopStartIndex = _NextEventIndex;

                    Invoke:
                    animancerEvent.DelayInvoke(Events.GetName(_NextEventIndex), State);

                    if (!NextEventLooped(Events, playDirectionInt) ||
                        _NextEventIndex == loopStartIndex)
                        return;

                    animancerEvent = Events[_NextEventIndex];
                    eventTime = animancerEvent.normalizedTime * playDirectionFloat;
                    if (loopDelta == GetLoopDelta(_PreviousNormalizedTime, currentTime, eventTime))
                        goto Invoke;
                }
                else// Non-Looping.
                {
                    while ((uint)_NextEventIndex < (uint)count)
                    {
                        var animancerEvent = Events[_NextEventIndex];
                        var eventTime = animancerEvent.normalizedTime * playDirectionFloat;

                        if (currentTime <= eventTime)
                            return;

                        animancerEvent.DelayInvoke(Events.GetName(_NextEventIndex), State);

                        _NextEventIndex += playDirectionInt;
                    }
                }
            }

            /************************************************************************************************************************/

            private void ValidateNextEventIndex(
                bool isLooping,
                ref float currentTime,
                out float playDirectionFloat,
                out int playDirectionInt)
            {
                if (currentTime < _PreviousNormalizedTime)// Playing Backwards.
                {
                    var previousTime = _PreviousNormalizedTime;
                    _PreviousNormalizedTime = -previousTime;
                    currentTime = -currentTime;
                    playDirectionFloat = -1;
                    playDirectionInt = -1;

                    if (_NextEventIndex == RecalculateEventIndex ||
                        _WasPlayingForwards)
                    {
                        _NextEventIndex = Events.Count - 1;
                        _WasPlayingForwards = false;

                        if (isLooping)
                            previousTime = AnimancerUtilities.Wrap01(previousTime);

                        while (Events[_NextEventIndex].normalizedTime > previousTime)
                        {
                            _NextEventIndex--;

                            if (_NextEventIndex < 0)
                            {
                                if (isLooping)
                                    _NextEventIndex = Events.Count - 1;
                                break;
                            }
                        }

                        Events.AssertNormalizedTimes(State, isLooping);
                    }
                }
                else// Playing Forwards.
                {
                    playDirectionFloat = 1;
                    playDirectionInt = 1;

                    if (_NextEventIndex == RecalculateEventIndex ||
                        !_WasPlayingForwards)
                    {
                        _NextEventIndex = 0;
                        _WasPlayingForwards = true;

                        var previousTime = _PreviousNormalizedTime;
                        if (isLooping)
                            previousTime = AnimancerUtilities.Wrap01(previousTime);

                        var max = Events.Count - 1;
                        while (Events[_NextEventIndex].normalizedTime < previousTime)
                        {
                            _NextEventIndex++;

                            if (_NextEventIndex > max)
                            {
                                if (isLooping)
                                    _NextEventIndex = 0;
                                break;
                            }
                        }

                        Events.AssertNormalizedTimes(State, isLooping);
                    }
                }

                // This method could be slightly optimised for playback direction changes by using the current index
                // as the starting point instead of iterating from the edge of the sequence, but that would make it
                // significantly more complex for something that shouldn't happen very often and would only matter if
                // there are lots of events (in which case the optimisation would be tiny compared to the cost of
                // actually invoking all those events and running the rest of the application).
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Calculates the number of times an event at `eventTime` should be invoked when the
            /// <see cref="AnimancerState.NormalizedTime"/> goes from `previousTime` to `nextTime` on a looping animation.
            /// </summary>
            private static int GetLoopDelta(float previousTime, float nextTime, float eventTime)
            {
                previousTime -= eventTime;
                nextTime -= eventTime;

                var previousLoopCount = Mathf.FloorToInt(previousTime);
                var nextLoopCount = Mathf.FloorToInt(nextTime);

                var loopCount = nextLoopCount - previousLoopCount;

                // Previous time must be inclusive.
                // And next time must be exclusive.
                // So if the previous time is exactly on a looped increment of the event time, count one more.
                // And if the next time is exactly on a looped increment of the event time, count one less.
                if (previousTime == previousLoopCount)
                    loopCount++;
                if (nextTime == nextLoopCount)
                    loopCount--;

                return loopCount;
            }

            /************************************************************************************************************************/

            private static int _MaximumFullLoopCount = 3;

            /// <summary>
            /// The maximum number of times a looping animation can trigger all of its events in a single frame.
            /// Default 3, Minimum 1.
            /// </summary>
            /// <remarks>
            /// This limit should only ever be reached when a state has a very short length and high speed.
            /// </remarks>
            public static int MaximumFullLoopCount
            {
                get => _MaximumFullLoopCount;
                set => _MaximumFullLoopCount = Math.Max(value, 1);
            }

            private bool InvokeAllEvents(Sequence events, int count, int playDirectionInt)
            {
                if (count > _MaximumFullLoopCount)
                    count = _MaximumFullLoopCount;

                var loopStartIndex = _NextEventIndex;
                while (count-- > 0)
                {
                    do
                    {
                        events[_NextEventIndex].DelayInvoke(events.GetName(_NextEventIndex), State);

                        if (!NextEventLooped(events, playDirectionInt))
                            return false;
                    }
                    while (_NextEventIndex != loopStartIndex);
                }

                return true;
            }

            /************************************************************************************************************************/

            private bool NextEventLooped(Sequence events, int playDirectionInt)
            {
                _NextEventIndex += playDirectionInt;

                var count = events.Count;
                if (_NextEventIndex >= count)
                    _NextEventIndex = 0;
                else if (_NextEventIndex < 0)
                    _NextEventIndex = count - 1;

                return true;
            }

            /************************************************************************************************************************/

            /// <summary>End events are triggered every frame after their time passes.</summary>
            /// <remarks>
            /// This ensures that assigning the event after the time has passed
            /// will still trigger it rather than leaving it playing indefinitely.
            /// </remarks>
            private void CheckEndEvent(float normalizedTime)
            {
                var endEvent = Events.EndEvent;
                if (endEvent.callback == null)
                    return;

                if (normalizedTime > _PreviousNormalizedTime)// Playing Forwards.
                {
                    var eventTime = float.IsNaN(endEvent.normalizedTime)
                        ? 1
                        : endEvent.normalizedTime;

                    if (normalizedTime > eventTime)
                        endEvent.DelayInvoke(EndEventName, State);
                }
                else// Playing Backwards.
                {
                    var eventTime = float.IsNaN(endEvent.normalizedTime)
                        ? 0
                        : endEvent.normalizedTime;

                    if (normalizedTime < eventTime)
                        endEvent.DelayInvoke(EndEventName, State);
                }
            }

            /************************************************************************************************************************/

            /// <summary>Returns "<see cref="Dispatcher"/> (Target State)".</summary>
            public override string ToString()
                    => State != null
                    ? $"{nameof(Dispatcher)} ({State})"
                    : $"{nameof(Dispatcher)} (No Target State)";

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public void AppendDescription(StringBuilder text, string separator = "\n")
            {
                text.AppendField(separator, "State", State.GetPath());
                text.AppendField(separator, "IsLooping", State.IsLooping);
                text.AppendField(separator, "PreviousNormalizedTime", _PreviousNormalizedTime);
                text.AppendField(separator, "NextEventIndex", _NextEventIndex);
                text.AppendField(separator, "SequenceVersion", _SequenceVersion);
                text.AppendField(separator, "WasPlayingForwards", _WasPlayingForwards);
            }

            /************************************************************************************************************************/
        }
    }
}

