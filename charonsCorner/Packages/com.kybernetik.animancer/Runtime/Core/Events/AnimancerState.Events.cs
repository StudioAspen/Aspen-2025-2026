// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Runtime.CompilerServices;
using UnityEngine;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerState
    partial class AnimancerState
    {
        /************************************************************************************************************************/

        /// <summary>The system which manages the <see cref="SharedEvents"/>.</summary>
        private AnimancerEvent.Dispatcher _EventDispatcher;

        /************************************************************************************************************************/

        /// <summary>
        /// Events which will be triggered while this state plays
        /// based on its <see cref="NormalizedTime"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// This property tries to ensure that the event sequence is only referenced by this state.
        /// <list type="bullet">
        /// <item>
        /// If the reference was <c>null</c>,
        /// a new sequence will be created.
        /// </item>
        /// <item>
        /// If a reference was assigned to <see cref="SharedEvents"/>,
        /// it will be cloned so this state owns the clone.
        /// </item>
        /// </list>
        /// <para></para>
        /// Using <see cref="Events(object)"/> or <see cref="Events(object, out AnimancerEvent.Sequence)"/>
        /// is often safer than this property since they help detect if multiple scripts are using the same
        /// state which could lead to unexpected bugs if they each assign conflicting callbacks.
        /// <para></para>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">
        /// Animancer Events</see>
        /// </remarks>
        public AnimancerEvent.Sequence OwnedEvents
        {
            get
            {
                _EventDispatcher ??= new(this);
                _EventDispatcher.InitializeEvents(out var events);
                return events;
            }
            set
            {
                if (value != null)
                    (_EventDispatcher ??= new(this)).SetEvents(value, true);
                else
                    _EventDispatcher = null;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Events which will be triggered while this state plays
        /// based on its <see cref="NormalizedTime"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// This reference is <c>null</c> by default and once assigned it may be shared by multiple states.
        /// <para></para>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">
        /// Animancer Events</see>
        /// </remarks>
        public AnimancerEvent.Sequence SharedEvents
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _EventDispatcher?.Events;
            set
            {
                if (value != null)
                    (_EventDispatcher ??= new(this)).SetEvents(value, false);
                else
                    _EventDispatcher = null;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Have the <see cref="SharedEvents"/> or <see cref="OwnedEvents"/> been initialized?</summary>
        /// <remarks>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">
        /// Animancer Events</see>
        /// </remarks>
        public bool HasEvents
            => _EventDispatcher != null;

        /************************************************************************************************************************/

        /// <summary>Have the <see cref="OwnedEvents"/> been initialized?</summary>
        /// <remarks>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">
        /// Animancer Events</see>
        /// </remarks>
        public bool HasOwnedEvents
            => _EventDispatcher != null
            && _EventDispatcher.HasOwnEvents;

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="OwnedEvents"/> haven't been initialized yet,
        /// this method gets them and returns <c>true</c>.
        /// </summary>
        /// 
        /// <remarks>
        /// This method tries to ensure that the event sequence is only referenced by this state.
        /// <list type="bullet">
        /// <item>
        /// If the reference was <c>null</c>,
        /// a new sequence will be created.
        /// </item>
        /// <item>
        /// If a reference was assigned to <see cref="SharedEvents"/>,
        /// it will be cloned so this state owns the clone.
        /// </item>
        /// </list>
        /// In both of those cases, this method returns <c>true</c>
        /// to indicate that the caller should initialize their event callbacks.
        /// <para></para>
        /// Also calls <see cref="AssertOwnership"/>.
        /// <para></para>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">
        /// Animancer Events</see>
        /// <para></para>
        /// <strong>Example:</strong>
        /// <code>
        /// public static readonly StringReference EventName = "Event Name";
        /// 
        /// ...
        /// 
        /// AnimancerState state = animancerComponent.Play(animation);
        /// if (state.Events(this, out AnimancerEvent.Sequence events))
        /// {
        ///     events.SetCallback(EventName, OnAnimationEvent);
        ///     events.OnEnd = OnAnimationEnded;
        /// }
        /// </code>
        /// If multiple different owners need to take turns reusing the same state, 
        /// use <see cref="Events(ref AnimancerEvent.Sequence)"/> instead.
        /// <para></para>
        /// If you only need to initialize the End Event, 
        /// consider using <see cref="Events(object)"/> instead.
        /// </remarks>
        public bool Events(object owner, out AnimancerEvent.Sequence events)
        {
            AssertOwnership(owner);
            _EventDispatcher ??= new(this);
            return _EventDispatcher.InitializeEvents(out events);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="OwnedEvents"/> haven't been initialized yet,
        /// this method gets them and returns <c>true</c>.
        /// </summary>
        /// 
        /// <remarks>
        /// This method tries to ensure that the event sequence is only referenced by this state.
        /// <list type="bullet">
        /// <item>
        /// If the reference was <c>null</c>,
        /// a new sequence will be created.
        /// </item>
        /// <item>
        /// If a reference was assigned to <see cref="SharedEvents"/>,
        /// it will be cloned so this state owns the clone.
        /// </item>
        /// </list>
        /// <para></para>
        /// Also calls <see cref="AssertOwnership"/>.
        /// <para></para>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">
        /// Animancer Events</see>
        /// <para></para>
        /// <strong>Example:</strong>
        /// <code>
        /// AnimancerState state = animancerComponent.Play(animation);
        /// state.Events(this).OnEnd ??= OnAnimationEnded;
        /// </code>
        /// If multiple different owners need to take turns reusing the same state, 
        /// use <see cref="Events(ref AnimancerEvent.Sequence)"/> instead.
        /// <para></para>
        /// If you need to initialize more than just the End Event, 
        /// use <see cref="Events(object, out AnimancerEvent.Sequence)"/> instead.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AnimancerEvent.Sequence Events(object owner)
        {
            Events(owner, out var events);
            return events;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the `events` are <c>null</c>, this method assigns a <c>new</c> <see cref="AnimancerEvent.Sequence"/>
        /// and returns <c>true</c> to indicate that the caller should now initialize their event callbacks.
        /// Otherwise, this method simply assigns the provided `events` to this state and returns <c>false</c>.
        /// </summary>
        /// 
        /// <remarks>
        /// If this state already had events, the <c>new</c> <see cref="AnimancerEvent.Sequence"/>
        /// will be a copy of those events for the caller to own.
        /// <para></para>
        /// This method allows multiple callers to safely take turns using the same state
        /// as long as they each call this method to assign their own events.
        /// <para></para>
        /// Also calls <see cref="AssertOwnership"/>.
        /// <para></para>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">
        /// Animancer Events</see>
        /// <para></para>
        /// <strong>Example:</strong>
        /// <code>
        /// public static readonly StringReference EventName = "Event Name";
        /// 
        /// private AnimancerEvent.Sequence _Events;// Don't new() this.
        /// 
        /// ...
        /// 
        /// AnimancerState state = animancerComponent.Play(animation);
        /// 
        /// // The first time this is called it will assign a new event sequence
        /// // to the _Events and return true so you can initialize it.
        /// 
        /// // After that, it will just re-assign the _Events to the state.
        /// // and return false so you don't need to re-initialize the events.
        /// 
        /// if (state.Events(ref _Events))
        /// {
        ///     _Events.SetCallback(EventName, OnAnimationEvent);
        ///     _Events.OnEnd = OnAnimationEnded;
        /// }
        /// </code>
        /// </remarks>
        public bool Events(ref AnimancerEvent.Sequence events)
        {
            _EventDispatcher ??= new(this);

            var justInitialized = events == null;
            if (justInitialized)
                events = new(_EventDispatcher.Events);

#if UNITY_ASSERTIONS
            // Normally swapping owners is an error,
            // but with this method it's fine to swap between event sequences since each caller is responsible for its own.
            if (Owner != null &&
                Owner != events &&
                Owner is not AnimancerEvent.Sequence)
                AssertOwnership(events);
            else
                Owner = events;
#endif

            _EventDispatcher.SetEvents(events, false);
            return justInitialized;
        }

        /************************************************************************************************************************/

        /// <summary>Copies the contents of the <see cref="_EventDispatcher"/>.</summary>
        private void CopyEvents(AnimancerState copyFrom, CloneContext context)
        {
            if (copyFrom._EventDispatcher != null)
            {
                var original = copyFrom._EventDispatcher.Events;
                var events = context.GetOrCreateCloneOrOriginal(original);
                if (events != null)
                {
                    _EventDispatcher ??= new(this);
                    _EventDispatcher.SetEvents(events, true);
                    return;
                }
            }

            _EventDispatcher = null;
        }

        /************************************************************************************************************************/

        /// <summary>Should events be raised on a state which is currently fading out?</summary>
        /// <remarks>
        /// Default <c>false</c>.
        /// <para></para>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">
        /// Animancer Events</see>
        /// </remarks>
        public static bool RaiseEventsDuringFadeOut { get; set; }

        /// <summary>[Internal] Should this state check for events to invoke?</summary>
        internal bool ShouldRaiseEvents
            => TargetWeight > 0
            || RaiseEventsDuringFadeOut;

        /************************************************************************************************************************/

        /// <summary>
        /// Checks if any events should be invoked based on the current time of this state.
        /// </summary>
        protected internal virtual void UpdateEvents()
            => _EventDispatcher?.UpdateEvents(ShouldRaiseEvents);

        /// <summary>
        /// Checks if any events should be invoked on the `parent` and its children recursively.
        /// </summary>
        public static void UpdateEventsRecursive(AnimancerState parent)
            => UpdateEventsRecursive(
                parent,
                parent.ShouldRaiseEvents);

        /// <summary>
        /// Checks if any events should be invoked on the `parent` and its children recursively.
        /// </summary>
        public static void UpdateEventsRecursive(AnimancerState parent, bool raiseEvents)
        {
            parent._EventDispatcher?.UpdateEvents(raiseEvents);

            for (int i = parent.ChildCount - 1; i >= 0; i--)
                UpdateEventsRecursive(parent.GetChild(i), raiseEvents);
        }

        /************************************************************************************************************************/
#if UNITY_ASSERTIONS
        /************************************************************************************************************************/

        /// <summary>[Assert-Only]
        /// Returns <c>null</c> if Animancer Events will work properly on this type of state,
        /// or a message explaining why they might not work.
        /// </summary>
        protected internal virtual string UnsupportedEventsMessage
            => null;

        /************************************************************************************************************************/

        /// <summary>[Assert-Only] An optional reference to the object that owns this state.</summary>
        public object Owner { get; private set; }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional]
        /// Sets the <see cref="Owner"/> and asserts that it wasn't already set to a different object.
        /// </summary>
        /// <remarks>This helps detect if multiple scripts attempt to manage the same state.</remarks>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public void AssertOwnership(object owner)
        {
#if UNITY_ASSERTIONS
            if (Owner == owner)
                return;

            if (Owner != null)
            {
                Debug.LogError(
                    $"Multiple objects have asserted ownership over the state '{ToString()}':" +
                    $"\n• Old Owner: {AnimancerUtilities.ToStringOrNull(Owner)}" +
                    $"\n• New Owner: {AnimancerUtilities.ToStringOrNull(owner)}" +
                    $"\n• State: {GetPath()}" +
                    $"\n• Graph: {Graph?.GetDescription("\n• ")}",
                    Graph?.Component as Object);
            }

            Owner = owner;
#endif
        }

        /************************************************************************************************************************/
    }
}

