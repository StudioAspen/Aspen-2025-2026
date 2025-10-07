// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    partial struct AnimancerEvent
    {
        /// <summary>An <see cref="AnimancerEvent"/> and other associated details used to invoke it.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/Invocation
        public readonly struct Invocation
        {
            /************************************************************************************************************************/

            /// <summary>The details of the event currently being triggered.</summary>
            /// <remarks>Cleared after the event is invoked.</remarks>
            public static Invocation Current { get; private set; }

            /************************************************************************************************************************/

            /// <summary>The <see cref="AnimancerEvent"/>.</summary>
            public readonly AnimancerEvent Event;

            /// <summary>The name of the <see cref="Event"/>.</summary>
            public readonly StringReference Name;

            /// <summary>The <see cref="AnimancerState"/> triggering the <see cref="Event"/>.</summary>
            public readonly AnimancerState State;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Invocation"/>.</summary>
            public Invocation(
                AnimancerEvent animancerEvent,
                StringReference eventName,
                AnimancerState state)
            {
                Event = animancerEvent;
                State = state;
                Name = eventName;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Sets the <see cref="Current"/>, invokes the <see cref="callback"/>,
            /// then reverts the <see cref="Current"/>.
            /// </summary>
            /// <remarks>This method catches and logs any exception thrown by the <see cref="callback"/>.</remarks>
            /// <exception cref="NullReferenceException">The <see cref="callback"/> is null.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Invoke()
            {
#if UNITY_ASSERTIONS
                var oldLayer = State.Layer;
                var oldCommandCount = oldLayer.CommandCount;
#endif

                var previous = Current;
                var parameter = CurrentParameter;

                Current = this;
                CurrentParameter = null;

                try
                {
                    Event.callback();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, State?.Graph?.Component as Object);
                }

                Current = previous;
                CurrentParameter = parameter;

#if UNITY_ASSERTIONS
                if (Name == EndEventName)
                    AssertEndEventInvoked(oldLayer, oldCommandCount);
#endif
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Returns the callback registered in the <see cref="AnimancerGraph.Events"/>
            /// with the <see cref="Name"/> (or null if there isn't one).
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly Action GetBoundCallback()
                => Name.IsNullOrEmpty()
                ? null
                : State.Graph._Events?.Get(Name);

            /************************************************************************************************************************/

            /// <summary>Returns a string describing the contents of this invocation.</summary>
            public override string ToString()
                => $"{nameof(AnimancerEvent)}.{nameof(Invocation)}(" +
                $"{nameof(Name)}={AnimancerUtilities.ToStringOrNull(Name)}, " +
                $"NormalizedTime={Event.normalizedTime:0.##}, " +
                $"Callback=({AnimancerReflection.ToStringDetailed(Event.callback)}), " +
                $"{nameof(State)}={AnimancerUtilities.ToStringOrNull(State)})";

            /************************************************************************************************************************/

            /// <summary>
            /// Invokes the callback bound to the <see cref="Name"/> in the <see cref="AnimancerGraph.Events"/>.
            /// </summary>
            /// <remarks>
            /// Logs <see cref="OptionalWarning.UselessEvent"/> if no callback is bound.
            /// </remarks>
            public void InvokeBoundCallback()
            {
                if (Name != null &&
                    State.Graph._Events != null &&
                    State.Graph._Events.TryGetValue(Name, out var callback))
                {
                    callback();
                }
#if UNITY_ASSERTIONS
                else if (OptionalWarning.UselessEvent.IsEnabled())
                {
                    OptionalWarning.UselessEvent.Log(
                        $"An {nameof(AnimancerEvent)} which does nothing was invoked." +
                        $" Most likely it wasn't configured correctly." +
                        $" Unused events should be removed to avoid wasting performance checking them." +
                        $"\n• Name: {AnimancerUtilities.ToStringOrNull(Name)}" +
                        $"\n• Normalized Time: {Event.normalizedTime}" +
                        $"\n• State: {State}" +
                        $"\n• Object: {AnimancerUtilities.ToStringOrNull(State.Graph?.Component)}",
                        State.Graph?.Component);
                }
#endif
            }

            /************************************************************************************************************************/
#if UNITY_ASSERTIONS
            /************************************************************************************************************************/

            /// <summary>[Assert-Only]
            /// Call after invoking an end event to assert <see cref="OptionalWarning.EndEventInterrupt"/>.
            /// </summary>
            private readonly void AssertEndEventInvoked(AnimancerLayer oldLayer, int oldCommandCount)
            {
                if (ShouldLogEndEventInterrupt(oldLayer, oldCommandCount))
                {
                    OptionalWarning.EndEventInterrupt.Log(
                        $"An End Event callback didn't stop the animation." +
                        $" Animancer doesn't handle End Events automatically," +
                        $" so the controlling script is responsible for stopping the animation," +
                        $" often by playing a different one." +
                        $"\n• State: {State}" +
                        $"\n• Callback: {Event.callback.ToStringDetailed()}" +
                        $"\n• End Events are triggered every frame after their time has passed: {Strings.DocsURLs.EndEvents}" +
                        $"\n• To avoid this behaviour, use a regular Animancer Event instead: {Strings.DocsURLs.AnimancerEvents}",
                        State.Graph?.Component);

                    OptionalWarning.EndEventInterrupt.Disable();
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Assert-Only] Should <see cref="OptionalWarning.EndEventInterrupt"/> be logged?</summary>
            private readonly bool ShouldLogEndEventInterrupt(AnimancerLayer oldLayer, int oldCommandCount)
            {
                if (!OptionalWarning.EndEventInterrupt.IsEnabled())
                    return false;

                var events = State.SharedEvents;
                if (events == null ||
                    events.OnEnd != Event.callback)
                    return false;

                var newLayer = State.Layer;
                if (oldLayer != newLayer ||
                    oldCommandCount != newLayer.CommandCount ||
                    !State.Graph.IsGraphPlaying ||
                    !State.IsPlaying)
                    return false;

                var speed = State.EffectiveSpeed;
                if (speed > 0)
                    return State.NormalizedTime > State.NormalizedEndTime;
                else if (speed < 0)
                    return State.NormalizedTime < State.NormalizedEndTime;
                else// Speed 0.
                    return false;
            }

            /************************************************************************************************************************/
#endif
            /************************************************************************************************************************/
        }
    }
}

