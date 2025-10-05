// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Animancer
{
    /// <summary>
    /// A <see cref="callback"/> delegate
    /// paired with a <see cref="normalizedTime"/> which determines when to invoke it.
    /// </summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">
    /// Animancer Events</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    /// 
    public partial struct AnimancerEvent : IEquatable<AnimancerEvent>
    {
        /************************************************************************************************************************/
        #region Event
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerState.NormalizedTime"/> at which to invoke the <see cref="callback"/>.</summary>
        public float normalizedTime;

        /// <summary>The delegate to invoke when the <see cref="normalizedTime"/> passes.</summary>
        public Action callback;

        /************************************************************************************************************************/

        /// <summary>The largest possible float value less than 1.</summary>
        /// <remarks>
        /// This value is useful for placing events at the end of a looping animation since they do not allow the
        /// <see cref="normalizedTime"/> to be greater than or equal to 1.
        /// </remarks>
        public const float
            AlmostOne = 0.99999994f;

        /************************************************************************************************************************/

        /// <summary>The event name used for <see cref="Sequence.EndEvent"/>s.</summary>
        /// <remarks>
        /// This is a <see cref="StringReference.Unique"/> so that even if the same name happens
        /// to be used elsewhere, it would be treated as a different name.
        /// The reason for this is explained in <see cref="NamedEventDictionary.AssertNotEndEvent"/>.
        /// </remarks>
        public static readonly StringReference
            EndEventName = StringReference.Unique("EndEvent");

        /************************************************************************************************************************/

        /// <summary>Does nothing.</summary>
        /// <remarks>This delegate can be used for events which would otherwise have a <c>null</c> <see cref="callback"/>.</remarks>
        public static readonly Action
            DummyCallback = Dummy;

        /// <summary>Does nothing.</summary>
        /// <remarks>Used by <see cref="DummyCallback"/>.</remarks>
        private static void Dummy() { }

        /// <summary>Is the `callback` <c>null</c> or the <see cref="DummyCallback"/>?</summary>
        public static bool IsNullOrDummy(Action callback)
            => callback == null
            || callback == DummyCallback;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimancerEvent"/>.</summary>
        public AnimancerEvent(float normalizedTime, Action callback)
        {
            this.normalizedTime = normalizedTime;
            this.callback = callback;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a string describing the details of this event.</summary>
        public readonly override string ToString()
        {
            var text = StringBuilderPool.Instance.Acquire();
            text.Append($"{nameof(AnimancerEvent)}(");
            AppendDetails(text);
            text.Append(')');
            return text.ReleaseToString();
        }

        /************************************************************************************************************************/

        /// <summary>Appends the details of this event to the `text`.</summary>
        public readonly void AppendDetails(StringBuilder text)
        {
            text.Append("NormalizedTime: ")
                .Append(normalizedTime);

            var callbacks = AnimancerReflection.GetInvocationList(callback);
            if (callbacks != null)
            {
                text.Append(", Callbacks: [")
                    .Append(callbacks.Length)
                    .Append("] { ");

                for (int i = 0; i < callbacks.Length; i++)
                {
                    if (i > 0)
                        text.Append(", ");

                    text.AppendDelegate(callbacks[i]);
                }

                text.Append(" }");
            }
            else
            {
                text.Append(", Callback: ")
                    .AppendDelegate(callback);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Invocation
        /************************************************************************************************************************/

        /// <summary>The details of the event currently being triggered.</summary>
        /// <remarks>Cleared after the event is invoked.</remarks>
        // Having the underlying field here can cause type initialization errors due to circular dependencies.
        public static Invocation Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Invocation.Current;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// A cached delegate which calls <see cref="Invocation.InvokeBoundCallback"/>
        /// on the <see cref="Current"/>.
        /// </summary>
        public static readonly Action
            InvokeBoundCallback = InvokeCurrentBoundCallback;

        /// <summary>
        /// Calls <see cref="Invocation.InvokeBoundCallback"/> on the <see cref="Current"/>.
        /// </summary>
        private static void InvokeCurrentBoundCallback()
            => Current.InvokeBoundCallback();

        /************************************************************************************************************************/

        /// <summary>The custom parameter of the event currently being triggered.</summary>
        /// <remarks>Cleared after the event is finished.</remarks>
        public static object CurrentParameter { get; private set; }

        /// <summary>Calls <see cref="ConvertableUtilities.ConvertOrThrow"/> on the <see cref="CurrentParameter"/>.</summary>
        public static T GetCurrentParameter<T>()
            => ConvertableUtilities.ConvertOrThrow<T>(CurrentParameter);

        /// <summary>Returns a new delegate which invokes the `callback` using <see cref="GetCurrentParameter{T}"/>.</summary>
        /// <remarks>
        /// If <typeparamref name="T"/> is <see cref="string"/>,
        /// consider using <see cref="Parametize(Action{string})"/> instead of this.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The `callback` is <c>null</c>.</exception>
        public static Action Parametize<T>(Action<T> callback)
        {
#if UNITY_ASSERTIONS
            if (callback == null)
                throw new ArgumentNullException(
                    nameof(callback),
                    $"Can't {nameof(Parametize)} a null callback.");
#endif

            return () => callback(GetCurrentParameter<T>());
        }

        /// <summary>Returns a new delegate which invokes the `callback` using the <see cref="CurrentParameter"/>.</summary>
        /// <exception cref="ArgumentNullException">The `callback` is <c>null</c>.</exception>
        public static Action Parametize(Action<string> callback)
        {
#if UNITY_ASSERTIONS
            if (callback == null)
                throw new ArgumentNullException(
                    nameof(callback),
                    $"Can't {nameof(Parametize)} a null callback.");
#endif

            return () => callback(CurrentParameter?.ToString());
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Only]
        /// Logs an error if the `callback` doesn't contain a <see cref="Parameter{T}.Invoke"/>
        /// so that adding to it with <see cref="Parametize{T}(Action{T})"/> can use that parameter.
        /// </summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertContainsParameter<T>(Action callback)
        {
            if (!ContainsParameterInvoke<T>(callback))
                Debug.LogWarning(
                    $"Adding parametized callback will do nothing because the existing callback" +
                    $" doesn't contain a {typeof(T).GetNameCS()} parameter." +
                    $"\n• Existing Callback: {callback.ToStringDetailed()}");
        }

        /// <summary>Does the `callback` contain a <see cref="Parameter{T}.Invoke"/>?</summary>
        private static bool ContainsParameterInvoke<T>(Action callback)
        {
            if (callback == null)
                return false;

            if (IsParameterInvoke<T>(callback))
                return true;

            var invocations = AnimancerReflection.GetInvocationList(callback);

            if (invocations.Length == 1 && ReferenceEquals(invocations[0], callback))
                return false;

            for (int i = 0; i < invocations.Length; i++)
            {
                var invocation = invocations[i];
                if (IsParameterInvoke<T>(invocation))
                    return true;
            }

            return false;
        }

        /// <summary>Is the `callback` a call to <see cref="Parameter{T}.Invoke"/>?</summary>
        private static bool IsParameterInvoke<T>(Delegate callback)
            => callback.Target is IParameter parameter
            && callback.Method.Name == nameof(IInvokable.Invoke)
            && typeof(T).IsAssignableFrom(parameter.Value.GetType());

        /************************************************************************************************************************/

        /// <summary>
        /// Adds this event to the <see cref="Invoker"/>
        /// which will call <see cref="Invocation.Invoke"/> later in the current frame.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void DelayInvoke(
            StringReference eventName,
            AnimancerState state)
            => Invoker.Add(new(this, eventName, state));

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional]
        /// This method should be called when an animation is played.
        /// It asserts that either no event is currently being triggered
        /// or that the event is being triggered inside `playing`.
        /// Otherwise, it logs <see cref="OptionalWarning.EventPlayMismatch"/>.
        /// </summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertEventPlayMismatch(AnimancerGraph playing)
        {
#if UNITY_ASSERTIONS
            if (Current.State == null ||
                Current.State.Graph == playing ||
                OptionalWarning.EventPlayMismatch.IsDisabled())
                return;

            OptionalWarning.EventPlayMismatch.Log(
                $"An Animancer Event triggered by '{Current.State}' on '{Current.State.Graph}'" +
                $" was used to play an animation on a different character ('{playing}')." +
                $"\n\nThis most commonly happens when a Transition is shared by multiple characters" +
                $" and they all register their own callbacks to its events which leads to" +
                $" those events being triggered by the wrong character." +
                $" See the Shared Events page for more information: " +
                Strings.DocsURLs.SharedEventSequences +
                $"\n\n{Current}",
                playing.Component);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns either the <see cref="AnimancerGraph.DefaultFadeDuration"/>
        /// or the <see cref="AnimancerState.RemainingDuration"/>
        /// of the <see cref="Current"/> state (whichever is higher).
        /// </summary>
        public static float GetFadeOutDuration()
            => GetFadeOutDuration(Current.State, AnimancerGraph.DefaultFadeDuration);

        /// <summary>
        /// Returns either the `minDuration` or the <see cref="AnimancerState.RemainingDuration"/>
        /// of the <see cref="Current"/> state (whichever is higher).
        /// </summary>
        public static float GetFadeOutDuration(float minDuration)
            => GetFadeOutDuration(Current.State, minDuration);

        /// <summary>
        /// Returns either the `minDuration` or the <see cref="AnimancerState.RemainingDuration"/>
        /// of the `state` (whichever is higher).
        /// </summary>
        public static float GetFadeOutDuration(AnimancerState state, float minDuration)
        {
            if (state == null)
                return minDuration;

            var time = state.Time;
            var speed = state.EffectiveSpeed;
            if (speed == 0)
                return minDuration;

            float remainingDuration;
            if (state.IsLooping)
            {
                var previousTime = time - speed * Time.deltaTime;
                var inverseLength = 1f / state.Length;

                // If we just passed the end of the animation, the remaining duration would technically be the full
                // duration of the animation, so we most likely want to use the minimum duration instead.
                if (Math.Floor(time * inverseLength) != Math.Floor(previousTime * inverseLength))
                    return minDuration;
            }

            if (speed > 0)
            {
                remainingDuration = (state.Length - time) / speed;
            }
            else
            {
                remainingDuration = time / -speed;
            }

            return Math.Max(minDuration, remainingDuration);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Operators
        /************************************************************************************************************************/

        /// <summary>Are the <see cref="normalizedTime"/> and <see cref="callback"/> equal?</summary>
        public static bool operator ==(AnimancerEvent a, AnimancerEvent b)
            => a.Equals(b);

        /// <summary>Are the <see cref="normalizedTime"/> and <see cref="callback"/> not equal?</summary>
        public static bool operator !=(AnimancerEvent a, AnimancerEvent b)
            => !a.Equals(b);

        /************************************************************************************************************************/

        /// <summary>[<see cref="IEquatable{AnimancerEvent}"/>]
        /// Are the <see cref="normalizedTime"/> and <see cref="callback"/> of this event equal to `other`?
        /// </summary>
        public readonly bool Equals(AnimancerEvent other)
            => callback == other.callback
            && normalizedTime.IsEqualOrBothNaN(other.normalizedTime);

        /// <inheritdoc/>
        public readonly override bool Equals(object obj)
            => obj is AnimancerEvent animancerEvent
            && Equals(animancerEvent);

        /// <inheritdoc/>
        public readonly override int GetHashCode()
            => AnimancerUtilities.Hash(-78069441,
                normalizedTime.GetHashCode(),
                callback.SafeGetHashCode());

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

