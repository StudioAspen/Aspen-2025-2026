// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Animancer
{
    /// <summary>
    /// Base class for all states in an <see cref="AnimancerGraph"/> graph which manages one or more
    /// <see cref="Playable"/>s.
    /// </summary>
    /// 
    /// <remarks>
    /// This class can be used as a custom yield instruction to wait until the animation either stops playing or
    /// reaches its end.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states">
    /// States</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerState
    /// 
    public abstract partial class AnimancerState : AnimancerNode,
        IAnimationClipCollection,
        ICloneable<AnimancerState>,
        ICopyable<AnimancerState>
    {
        /************************************************************************************************************************/
        #region Graph
        /************************************************************************************************************************/

        /// <summary>Sets the <see cref="AnimancerNodeBase.Graph"/>.</summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="AnimancerNodeBase.Parent"/> has a different <see cref="AnimancerNodeBase.Graph"/>.
        /// Setting the <see cref="AnimancerNodeBase.Parent"/>'s <see cref="AnimancerNodeBase.Graph"/>
        /// will apply to its children recursively because they must always match.
        /// </exception>
        public virtual void SetGraph(AnimancerGraph graph)
        {
            if (Graph == graph)
                return;

            RemoveFromOldGraph(graph);

            Graph = graph;

            AddToNewGraph();

            FadeGroup?.ChangeGraph(graph);
        }

        private void RemoveFromOldGraph(AnimancerGraph newGraph)
        {
            if (Graph == null)
            {
#if UNITY_ASSERTIONS
                if (Parent != null && Parent.Graph != newGraph)
                    throw new InvalidOperationException(
                        "Unable to set the Graph of a state which has a Parent." +
                        " Setting the Parent's Graph will apply to its children recursively" +
                        " because they must always match.");
#endif

                return;
            }

            Graph.States.Unregister(this);

            if (Parent != null && Parent.Graph != newGraph)
            {
                Parent.OnRemoveChild(this);
                Parent = null;

                Index = -1;
            }

            _Time = TimeD;

            DestroyPlayable();
        }

        private void AddToNewGraph()
        {
            if (Graph != null)
            {
                Graph.States.Register(this);

                CreatePlayable();
            }

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i)?.SetGraph(Graph);

            if (Parent != null)
                CopyIKFlags(Parent);
        }

        /************************************************************************************************************************/

        /// <summary>Connects this state to the `parent` at its next available child index.</summary>
        /// <remarks>If the `parent` is <c>null</c>, this state will be disconnected from everything.</remarks>
        public void SetParent(AnimancerNode parent)
        {
#if UNITY_ASSERTIONS
            if (Parent == parent)
                Debug.LogWarning(
                    $"{nameof(Parent)} is already set to {AnimancerUtilities.ToStringOrNull(parent)}.",
                    Graph?.Component as Object);
#endif

            if (Parent != null)
            {
                Parent.OnRemoveChild(this);
                Parent = null;
            }

            if (parent == null)
            {
                FadeGroup?.ChangeParent(this);
                Index = -1;
                return;
            }

            SetGraph(parent.Graph);
            Parent = parent;
            parent.OnAddChild(this);
            CopyIKFlags(parent);
            FadeGroup?.ChangeParent(this);
        }

        /// <summary>[Internal]
        /// Directly sets the <see cref="AnimancerNodeBase.Parent"/> and <see cref="AnimancerNode.Index"/>
        /// without triggering any other connection methods.
        /// </summary>
        internal void SetParentInternal(AnimancerNode parent, int index = -1)
        {
            Parent = parent;
            Index = index;
        }

        /************************************************************************************************************************/
        // Layer.
        /************************************************************************************************************************/

        /// <summary>
        /// The index of the <see cref="AnimancerLayer"/> this state is connected to
        /// (determined by the <see cref="AnimancerNodeBase.Parent"/>).
        /// </summary>
        /// <returns><c>-1</c> if this state isn't connected to a layer.</returns>
        public int LayerIndex
        {
            get
            {
                if (Parent == null)
                    return -1;

                var layer = Parent.Layer;
                if (layer == null)
                    return -1;

                return layer.Index;
            }
            set => SetParent(value >= 0
                ? Graph.Layers[value]
                : null);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Key and Clip
        /************************************************************************************************************************/

        internal object _Key;

        /// <summary>
        /// The object used to identify this state in the graph <see cref="AnimancerGraph.States"/> dictionary.
        /// Can be null.
        /// </summary>
        public object Key
        {
            get => _Key;
            set
            {
                if (Graph == null)
                {
                    _Key = value;
                }
                else
                {
                    Graph.States.Unregister(this);
                    _Key = value;
                    Graph.States.Register(this);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimationClip"/> which this state plays (if any).</summary>
        /// <exception cref="NotSupportedException">This state type doesn't have a clip and you try to set it.</exception>
        public virtual AnimationClip Clip
        {
            get => null;
            set
            {
                MarkAsUsed(this);
                throw new NotSupportedException($"{GetType()} doesn't support setting the {nameof(Clip)}.");
            }
        }

        /// <summary>The main object to show in the Inspector for this state (if any).</summary>
        /// <exception cref="NotSupportedException">This state type doesn't have a main object and you try to set it.</exception>
        /// <exception cref="InvalidCastException">This state can't use the assigned value.</exception>
        public virtual Object MainObject
        {
            get => null;
            set
            {
                MarkAsUsed(this);
                throw new NotSupportedException($"{GetType()} doesn't support setting the {nameof(MainObject)}.");
            }
        }

#if UNITY_EDITOR
        /// <summary>[Editor-Only] The base type which can be assigned to the <see cref="MainObject"/>.</summary>
        public virtual Type MainObjectType
            => null;
#endif

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the `currentObject` and calls <see cref="AnimancerNode.RecreatePlayable"/>.
        /// If the `currentObject` was being used as the <see cref="Key"/> then it is changed as well.
        /// </summary>
        /// <exception cref="ArgumentNullException">The `newObject` is null.</exception>
        protected bool ChangeMainObject<T>(ref T currentObject, T newObject)
            where T : Object
        {
            if (newObject == null)
            {
                MarkAsUsed(this);
                throw new ArgumentNullException(nameof(newObject));
            }

            if (ReferenceEquals(currentObject, newObject))
                return false;

            if (ReferenceEquals(_Key, currentObject))
                Key = newObject;

            currentObject = newObject;

            if (Graph != null)
                RecreatePlayable();

            return true;
        }

        /************************************************************************************************************************/

        /// <summary>The average velocity of the Root Motion caused by this state.</summary>
        public virtual Vector3 AverageVelocity => default;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Playing
        /************************************************************************************************************************/

        /// <summary>Is the <see cref="Time"/> automatically advancing?</summary>
        private bool _IsPlaying;

        /************************************************************************************************************************/

        /// <summary>Is the <see cref="Time"/> automatically advancing?</summary>
        ///
        /// <remarks>
        /// <strong>Example:</strong><code>
        /// void IsPlayingExample(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.States.GetOrCreate(clip);
        ///
        ///     if (state.IsPlaying)
        ///         Debug.Log(clip + " is playing");
        ///     else
        ///         Debug.Log(clip + " is paused");
        ///
        ///     state.IsPlaying = false;// Pause the animation.
        ///
        ///     state.IsPlaying = true;// Unpause the animation.
        /// }
        /// </code></remarks>
        public bool IsPlaying
        {
            get => _IsPlaying;
            set
            {
                SetIsPlaying(value);
                UpdateIsActive();
            }
        }

        /// <summary>
        /// Sets <see cref="IsPlaying"/> and applies it to the <see cref="Playable"/>
        /// without calling <see cref="UpdateIsActive"/>.
        /// </summary>
        protected internal void SetIsPlaying(bool isPlaying)
        {
            if (_IsPlaying == isPlaying)
                return;

            _IsPlaying = isPlaying;

            if (_Playable.IsValid())
            {
                if (_IsPlaying)
                    _Playable.Play();
                else
                    _Playable.Pause();
            }

            OnSetIsPlaying();
        }

        /// <summary>Called when the value of <see cref="IsPlaying"/> is changed.</summary>
        protected virtual void OnSetIsPlaying() { }

        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="Playable"/> managed by this state.</summary>
        /// <remarks>This method also applies the <see cref="AnimancerNodeBase.Speed"/> and <see cref="IsPlaying"/>.</remarks>
        protected sealed override void CreatePlayable()
        {
            base.CreatePlayable();

            if (Parent != null && (IsActive || Parent.KeepChildrenConnected))
                Graph._PlayableGraph.Connect(Parent.Playable, Playable, Index, Weight);

            if (!_IsPlaying)
                _Playable.Pause();

            RawTime = _Time;
        }

        /************************************************************************************************************************/

        /// <summary>Is this state playing and not fading out?</summary>
        /// <remarks>
        /// If true, this state will usually be the <see cref="AnimancerLayer.CurrentState"/> but that is not always
        /// the case.
        /// </remarks>
        public bool IsCurrent
            => _IsPlaying
            && TargetWeight > 0;

        /// <summary>Is this state not playing and at 0 <see cref="AnimancerNode.Weight"/>?</summary>
        public bool IsStopped
            => !_IsPlaying
            && Weight == 0;

        /************************************************************************************************************************/

        /// <summary>
        /// Plays this state immediately, without any blending and without affecting any other states.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="AnimancerLayer.Play(AnimancerState)"/>,
        /// this method only affects this state and won't stop any others that are playing.
        /// <para></para>
        /// Sets <see cref="IsPlaying"/> = true and <see cref="AnimancerNode.Weight"/> = 1.
        /// <para></para>
        /// Doesn't change the <see cref="Time"/> so it will continue from its current value.
        /// </remarks>
        public void Play()
        {
            SetIsPlaying(true);
            Weight = 1;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected internal override void StopWithoutWeight()
        {
            SetIsPlaying(false);
            TimeD = 0;
            UpdateIsActive();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected internal override void OnStartFade()
        {
            UpdateIsActive();
        }

        /// <inheritdoc/>
        protected internal override void InternalClearFade()
        {
            base.InternalClearFade();
            UpdateIsActive();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Active
        /************************************************************************************************************************/

        /// <summary>
        /// The index of this state in its parent <see cref="AnimancerLayer.ActiveStates"/> (or -1 if inactive).
        /// </summary>
        /// <remarks>
        /// If this state's direct parent isn't a layer (such as a child of a mixer), this value simply uses 0 to
        /// indicate active.
        /// </remarks>
        internal int _ActiveIndex = ActiveList.NotInList;

        /************************************************************************************************************************/

        /// <summary>Is this state currently updating or affecting the animation output?</summary>
        /// <remarks>
        /// This property is true when <see cref="IsPlaying"/> or the <see cref="AnimancerNode.Weight"/> or
        /// <see cref="AnimancerNode.TargetWeight"/> are above 0.
        /// </remarks>
        public bool IsActive
            => _ActiveIndex >= 0;

        /// <summary>[Internal] Should <see cref="IsActive"/> be true based on the current details of this state?</summary>
        internal bool ShouldBeActive
        {
            get => IsPlaying
                || Weight > 0
                || FadeGroup != null;
            set => _ActiveIndex = value ? 0 : -1;
        }

        /// <summary>[Internal] If <see cref="IsActive"/> this method sets it to false and returns true.</summary>
        internal bool TryDeactivate()
        {
            if (_ActiveIndex < 0)
                return false;

            _ActiveIndex = ActiveList.NotInList;
            return true;
        }

        /// <summary>Called when <see cref="IsActive"/> might change.</summary>
        internal void UpdateIsActive()
        {
            var shouldBeActive = ShouldBeActive;
            if (IsActive == shouldBeActive)
                return;

            var parent = Parent;
            if (parent != null)
                parent.ApplyChildActive(this, shouldBeActive);
            else
                ShouldBeActive = ShouldBeActive;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public sealed override void SetWeight(float value)
        {
            base.SetWeight(value);
            UpdateIsActive();
        }

        /************************************************************************************************************************/

        /// <summary>[Internal] An <see cref="IIndexer{T}"/> based on <see cref="_ActiveIndex"/>.</summary>
        internal readonly struct Indexer : IIndexer<AnimancerState>
        {
            /************************************************************************************************************************/

            /// <summary>The <see cref="AnimancerNodeBase.Graph"/>.</summary>
            public readonly AnimancerGraph Graph;

            /// <summary>The <see cref="Playable"/> of the <see cref="AnimancerNodeBase.Parent"/>.</summary>
            public readonly Playable ParentPlayable;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Indexer"/>.</summary>
            public Indexer(AnimancerGraph graph, Playable parentPlayable)
            {
                Graph = graph;
                ParentPlayable = parentPlayable;
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public readonly int GetIndex(AnimancerState state)
                => state._ActiveIndex;

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public readonly void SetIndex(AnimancerState state, int index)
            {
                if (!Graph.KeepChildrenConnected && state._ActiveIndex < 0)
                {
                    Validate.AssertPlayable(state);
                    Graph._PlayableGraph.Connect(ParentPlayable, state._Playable, state.Index, state.Weight);
                }

                state._ActiveIndex = index;
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public readonly void ClearIndex(AnimancerState state)
            {
                if (!Graph.KeepChildrenConnected)
                    Graph._PlayableGraph.Disconnect(ParentPlayable, state.Index);

                state._ActiveIndex = ActiveList.NotInList;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        /// <summary>[Internal]
        /// An <see cref="IndexedList{TItem, TAccessor}"/> of <see cref="AnimancerState"/>s
        /// which tracks <see cref="IsActive"/>.
        /// </summary>
        internal class ActiveList : IndexedList<AnimancerState, Indexer>
        {
            /// <summary>The default <see cref="IndexedList{TItem, TIndexer}.Capacity"/> for newly created lists.</summary>
            /// <remarks>Default value is 4.</remarks>
            public static new int DefaultCapacity { get; set; } = 4;

            /// <summary>Creates a new <see cref="ActiveList"/> with the <see cref="DefaultCapacity"/>.</summary>
            public ActiveList(Indexer accessor)
                : base(DefaultCapacity, accessor)
            { }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Timing
        /************************************************************************************************************************/
        // Time.
        /************************************************************************************************************************/

        /// <summary>
        /// The current time of the <see cref="Playable"/>, retrieved by <see cref="Time"/> whenever the
        /// <see cref="_TimeFrameID"/> is different from the <see cref="AnimancerGraph.FrameID"/>.
        /// </summary>
        private double _Time;

        /// <summary>
        /// The <see cref="AnimancerGraph.FrameID"/> from when the <see cref="Time"/> was last retrieved from the
        /// <see cref="Playable"/>.
        /// </summary>
        private ulong _TimeFrameID;

        /************************************************************************************************************************/

        /// <summary>The number of seconds that have passed since the start of this animation.</summary>
        ///
        /// <remarks>
        /// This value continues increasing after the animation passes the end of its
        /// <see cref="Length"/>, regardless of whether it <see cref="IsLooping"/> or not.
        /// <para></para>
        /// The underlying <see cref="double"/> can be accessed via <see cref="TimeD"/>.
        /// <para></para>
        /// Setting this value will skip Events and Root Motion between the old and new time.
        /// Use <see cref="MoveTime(float, bool)"/> instead if you don't want that behaviour.
        /// <para></para>
        /// <em>Animancer Lite doesn't allow this value to be changed in runtime builds (except resetting it to 0).</em>
        /// <para></para>
        /// <strong>Example:</strong><code>
        /// void TimeExample(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.Play(clip);
        ///
        ///     // Skip 0.5 seconds into the animation:
        ///     state.Time = 0.5f;
        ///
        ///     // Skip 50% of the way through the animation (0.5 in a range of 0 to 1):
        ///     state.NormalizedTime = 0.5f;
        ///
        ///     // Skip to the end of the animation and play backwards:
        ///     state.NormalizedTime = 1;
        ///     state.Speed = -1;
        /// }
        /// </code></remarks>
        public float Time
        {
            get => (float)TimeD;
            set => TimeD = value;
        }

        /// <summary>The underlying <see cref="double"/> value of <see cref="Time"/>.</summary>
        public double TimeD
        {
            get
            {
                var graph = Graph;
                if (graph == null)
                    return _Time;

                var frameID = graph.FrameID;
                if (_TimeFrameID != frameID)
                {
                    _TimeFrameID = frameID;
                    _Time = RawTime;
                }

                return _Time;
            }
            set
            {
#if UNITY_ASSERTIONS
                if (!value.IsFinite())
                {
                    MarkAsUsed(this);
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        $"{nameof(Time)} {Strings.MustBeFinite}");
                }
#endif

                _Time = value;

                var graph = Graph;
                if (graph != null)
                {
                    _TimeFrameID = graph.FrameID;
                    RawTime = value;
                }

                _EventDispatcher?.OnSetTime();
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The internal implementation of <see cref="Time"/> which directly gets and sets the underlying value.
        /// </summary>
        /// <remarks>
        /// This property should generally not be accessed directly.
        /// <para></para>
        /// Setting this value will skip Events and Root Motion between the old and new time.
        /// Use <see cref="MoveTime(float, bool)"/> instead if you don't want that behaviour.
        /// </remarks>
        public virtual double RawTime
        {
            get
            {
                Validate.AssertPlayable(this);
                return _Playable.GetTime();
            }
            set
            {
                Validate.AssertPlayable(this);
                var time = value;
                _Playable.SetTime(time);
                _Playable.SetTime(time);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="Time"/> of this state as a portion of the animation's <see cref="Length"/>, meaning the
        /// value goes from 0 to 1 as it plays from start to end, regardless of how long that actually takes.
        /// </summary>
        /// 
        /// <remarks>
        /// This value continues increasing after the animation passes the end of its
        /// <see cref="Length"/>, regardless of whether it <see cref="IsLooping"/> or not.
        /// <para></para>
        /// The fractional part of the value (<c>NormalizedTime % 1</c>)
        /// is the percentage (0-1) of progress in the current loop
        /// while the integer part (<c>(int)NormalizedTime</c>)
        /// is the number of times the animation has been looped.
        /// <para></para>
        /// Setting this value will skip Events and Root Motion between the old and new time.
        /// Use <see cref="MoveTime(float, bool)"/> instead if you don't want that behaviour.
        /// <para></para>
        /// <em>Animancer Lite doesn't allow this value to be changed in runtime builds (except resetting it to 0).</em>
        /// <para></para>
        /// <strong>Example:</strong><code>
        /// void TimeExample(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.Play(clip);
        ///
        ///     // Skip 0.5 seconds into the animation:
        ///     state.Time = 0.5f;
        ///
        ///     // Skip 50% of the way through the animation (0.5 in a range of 0 to 1):
        ///     state.NormalizedTime = 0.5f;
        ///
        ///     // Skip to the end of the animation and play backwards:
        ///     state.NormalizedTime = 1;
        ///     state.Speed = -1;
        /// }
        /// </code></remarks>
        public float NormalizedTime
        {
            get => (float)NormalizedTimeD;
            set => NormalizedTimeD = value;
        }

        /// <summary>The underlying <see cref="double"/> value of <see cref="NormalizedTime"/>.</summary>
        public double NormalizedTimeD
        {
            get
            {
                var length = Length;
                if (length != 0)
                    return TimeD / length;
                else
                    return 0;
            }
            set => TimeD = value * Length;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the <see cref="Time"/> or <see cref="NormalizedTime"/>, but unlike those properties
        /// this method doesn't skip Events or Root Motion between the old and new time.
        /// </summary>
        /// <remarks>
        /// The Events and Root Motion will be applied during the next animation update.
        /// If you want to apply them immediately you can call <see cref="AnimancerGraph.Evaluate()"/>.
        /// <para></para>
        /// Events are triggered where <c>old time &lt;= event time &lt; new time</c>.
        /// <para></para>
        /// Avoid calling this method more than once per frame because doing so will cause
        /// Animation Events and Root Motion to be skipped due to an unfortunate design
        /// decision in the Playables API. Animancer Events would still be triggered,
        /// but only between the old time and the last new time you set
        /// (any other values would be ignored).
        /// </remarks>
        public void MoveTime(float time, bool normalized)
            => MoveTime((double)time, normalized);

        /// <summary>
        /// Sets the <see cref="Time"/> or <see cref="NormalizedTime"/>, but unlike those properties
        /// this method doesn't skip Events or Root Motion between the old and new time.
        /// </summary>
        /// <remarks>
        /// The Events and Root Motion will be applied during the next animation update.
        /// If you want to apply them immediately you can call <see cref="AnimancerGraph.Evaluate()"/>.
        /// <para></para>
        /// Avoid calling this method more than once per frame because doing so will cause
        /// Animation Events and Root Motion to be skipped due to an unfortunate design
        /// decision in the Playables API. Animancer Events would still be triggered,
        /// but only between the old time and the last new time you set
        /// (any other values would be ignored).
        /// </remarks>
        public virtual void MoveTime(double time, bool normalized)
        {
#if UNITY_ASSERTIONS
            if (!time.IsFinite())
            {
                MarkAsUsed(this);
                throw new ArgumentOutOfRangeException(nameof(time), time,
                    $"{nameof(Time)} {Strings.MustBeFinite}");
            }
#endif

            var graph = Graph;
            if (graph != null)
                _TimeFrameID = graph.FrameID;

            if (normalized)
                time *= Length;

            _Time = time;
            _Playable.SetTime(time);
        }

        /************************************************************************************************************************/
        // Duration.
        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="NormalizedTime"/> after which the
        /// <see cref="AnimancerEvent.Sequence.OnEnd"/> callback will be invoked every frame.
        /// </summary>
        /// <remarks>
        /// This is a wrapper around <see cref="AnimancerEvent.Sequence.NormalizedEndTime"/>
        /// so that if the value hasn't been set (<see cref="float.NaN"/>)
        /// it can be determined based on the <see cref="AnimancerNodeBase.EffectiveSpeed"/>:
        /// positive speed ends at 1 and negative speed ends at 0.
        /// </remarks>
        public float NormalizedEndTime
        {
            get
            {
                var events = SharedEvents;
                if (events != null)
                {
                    var time = events.NormalizedEndTime;
                    if (!float.IsNaN(time))
                        return time;
                }

                return AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(EffectiveSpeed);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The number of seconds the animation will take to play fully at its current
        /// <see cref="AnimancerNodeBase.EffectiveSpeed"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// For the time remaining from now until it reaches the end, use <see cref="RemainingDuration"/> instead.
        /// <para></para>
        /// Setting this value modifies the <see cref="AnimancerNodeBase.Speed"/>, not the <see cref="Length"/>.
        /// <para></para>
        /// <em>Animancer Lite doesn't allow this value to be changed in runtime builds.</em>
        /// <para></para>
        /// <strong>Example:</strong><code>
        /// void PlayAnimation(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.Play(clip);
        ///
        ///     state.Duration = 1;// Play fully in 1 second.
        ///     state.Duration = 2;// Play fully in 2 seconds.
        ///     state.Duration = 0.5f;// Play fully in half a second.
        ///     state.Duration = -1;// Play backwards fully in 1 second.
        ///     state.NormalizedTime = 1; state.Duration = -1;// Play backwards from the end in 1 second.
        /// }
        /// </code></remarks>
        public float Duration
        {
            get
            {
                var speed = EffectiveSpeed;
                if (speed == 0)
                    return float.PositiveInfinity;

                var events = SharedEvents;
                if (events != null)
                {
                    var endTime = events.NormalizedEndTime;
                    if (!float.IsNaN(endTime))
                    {
                        if (speed > 0)
                            return Length * endTime / speed;
                        else
                            return Length * (1 - endTime) / -speed;
                    }
                }

                return Length / Math.Abs(speed);
            }
            set
            {
                var length = Length;
                var events = SharedEvents;
                if (events != null)
                {
                    var endTime = events.NormalizedEndTime;
                    if (!float.IsNaN(endTime))
                    {
                        if (EffectiveSpeed > 0)
                            length *= endTime;
                        else
                            length *= 1 - endTime;
                    }
                }

                EffectiveSpeed = length / value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The number of seconds this state will take to go from its current <see cref="NormalizedTime"/> to the
        /// <see cref="NormalizedEndTime"/> at its current <see cref="AnimancerNodeBase.EffectiveSpeed"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// For the time it would take to play fully from the start, use the <see cref="Duration"/> instead.
        /// <para></para>
        /// Setting this value modifies the <see cref="AnimancerNodeBase.EffectiveSpeed"/>, not the <see cref="Length"/>.
        /// <para></para>
        /// <em>Animancer Lite doesn't allow this value to be changed in runtime builds.</em>
        /// <para></para>
        /// <strong>Example:</strong><code>
        /// void PlayAnimation(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.Play(clip);
        ///
        ///     state.RemainingDuration = 1;// Play from the current time to the end in 1 second.
        ///     state.RemainingDuration = 2;// Play from the current time to the end in 2 seconds.
        ///     state.RemainingDuration = 0.5f;// Play from the current time to the end in half a second.
        ///     state.RemainingDuration = -1;// Play from the current time away from the end.
        /// }
        /// </code></remarks>
        public float RemainingDuration
        {
            get => (Length * NormalizedEndTime - Time) / EffectiveSpeed;
            set => EffectiveSpeed = (Length * NormalizedEndTime - Time) / value;
        }

        /************************************************************************************************************************/
        // Length.
        /************************************************************************************************************************/

        /// <summary>
        /// The total time this state would take to play in seconds when <see cref="AnimancerNodeBase.Speed"/> = 1.
        /// </summary>
        public abstract float Length { get; }

        /// <summary>Will this state loop back to the start when it reaches the end?</summary>
        /// <remarks>
        /// Note that <see cref="Time"/> always continues increasing regardless of this value.
        /// See the comments on <see cref="Time"/> for more information.
        /// </remarks>
        public virtual bool IsLooping => false;

        /************************************************************************************************************************/

        /// <summary>
        /// Gets the details used to trigger <see cref="AnimancerEvent"/>s on this state:
        /// <see cref="Length"/>, <see cref="NormalizedTime"/>, and <see cref="IsLooping"/>.
        /// </summary>
        public virtual void GetEventDispatchInfo(
            out float length,
            out float normalizedTime,
            out bool isLooping)
        {
            length = Length;

            normalizedTime = length != 0
                ? Time / length
                : 0;

            isLooping = IsLooping;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Methods
        /************************************************************************************************************************/

        /// <summary>Destroys the <see cref="Playable"/> and cleans up this state.</summary>
        /// <remarks>
        /// This method is NOT called automatically when the whole <see cref="AnimancerGraph"/> is destroyed,
        /// so if you need to guarantee that things will get cleaned up you must use
        /// <see cref="AnimancerGraph.Disposables"/>.
        /// </remarks>
        public virtual void Destroy()
        {
            if (Parent != null)
            {
                Parent.OnRemoveChild(this);
                Parent = null;
            }

            FadeGroup = null;
            Index = -1;
            _EventDispatcher = null;

            var graph = Graph;
            if (graph != null)
            {
                graph.States.Unregister(this);

                // Destroying via the graph seems to be slightly faster than _Playable.Destroy().
                if (_Playable.IsValid() && graph._PlayableGraph.IsValid())
                    graph._PlayableGraph.DestroyPlayable(_Playable);
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public abstract AnimancerState Clone(CloneContext context);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public sealed override void CopyFrom(AnimancerNode copyFrom, CloneContext context)
            => this.CopyFromBase(copyFrom, context);

        /// <inheritdoc/>
        public virtual void CopyFrom(AnimancerState copyFrom, CloneContext context)
        {
            CopyFirstGraphAndKeyFrom(copyFrom, context);

            TimeD = copyFrom.TimeD;
            IsPlaying = copyFrom.IsPlaying;

            base.CopyFrom(copyFrom, context);

            CopyEvents(copyFrom, context);

            UpdateIsActive();
        }

        /************************************************************************************************************************/

        /// <summary>Sets the <see cref="AnimancerNodeBase.Graph"/> and <see cref="Key"/>.</summary>
        private void CopyFirstGraphAndKeyFrom(AnimancerState copyFrom, CloneContext context)
        {
            if (Graph != null)
                return;

            Graph = context.GetCloneOrOriginal(copyFrom.Graph);

            // If a clone is registered for the key, use it.
            // Otherwise, if the key is a state and we're cloning into a different graph, clone the key state.
            // Otherwise, just use the same key.
            _Key = copyFrom.Key is AnimancerState stateKey && stateKey.Graph != Graph
                ? context.GetOrCreateCloneOrOriginal(stateKey)
                : context.GetCloneOrOriginal(copyFrom.Key);

            // Each key can only be used once per graph,
            // so we can only use it if it's different or we have a different graph.
            if (_Key == copyFrom.Key && Graph == copyFrom.Graph)
                _Key = null;

            AddToNewGraph();
        }

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipCollection"/>] Gathers all the animations in this state.</summary>
        public virtual void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            clips.Gather(Clip);

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i).GatherAnimationClips(clips);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if the animation is playing and has not yet passed the
        /// <see cref="AnimancerEvent.Sequence.EndEvent"/>.
        /// </summary>
        /// <remarks>
        /// This method is called by <see cref="IEnumerator.MoveNext"/> so this object can be used as a custom yield
        /// instruction to wait until it finishes.
        /// </remarks>
        public override bool IsPlayingAndNotEnding()
        {
            if (!IsPlaying || !_Playable.IsValid())
                return false;

            var speed = EffectiveSpeed;
            if (speed > 0)
            {
                float endTime;
                var events = SharedEvents;
                if (events != null)
                {
                    endTime = events.NormalizedEndTime;
                    if (float.IsNaN(endTime))
                        endTime = Length;
                    else
                        endTime *= Length;
                }
                else endTime = Length;

                return Time <= endTime;
            }
            else if (speed < 0)
            {
                float endTime;
                var events = SharedEvents;
                if (events != null)
                {
                    endTime = events.NormalizedEndTime;
                    if (float.IsNaN(endTime))
                        endTime = 0;
                    else
                        endTime *= Length;
                }
                else endTime = 0;

                return Time >= endTime;
            }
            else return true;
        }

        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        private string _CachedToString;
#endif

        /// <summary>
        /// Returns the <see cref="AnimancerNode.DebugName"/> if one is set, otherwise a string describing the type of
        /// this state and the name of the <see cref="MainObject"/>.
        /// </summary>
        public override string ToString()
        {
#if UNITY_ASSERTIONS
            if (NameCache.TryToString(DebugName, out var cachedName))
                return cachedName;

            if (_CachedToString != null)
                return _CachedToString;
#endif

            string name;

            var type = GetType().Name;
            var mainObject = MainObject;
            if (mainObject != null)
            {
#if UNITY_ASSERTIONS
                name = mainObject.GetCachedName();
#else
                name = mainObject.name;
#endif
                name = $"{name} ({type})";
            }
            else
            {
                name = type;
            }

#if UNITY_ASSERTIONS
            _CachedToString = name;
#endif

            return name;
        }

        /************************************************************************************************************************/
        #region Descriptions
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string separator)
        {
            text.AppendField(separator, nameof(Key), _Key);
            text.AppendField(separator, "ActiveIndex", _ActiveIndex);

            var mainObject = MainObject;
            if (mainObject != _Key as Object)
                text.AppendField(separator, nameof(MainObject), mainObject);

#if UNITY_EDITOR
            if (mainObject != null)
                text.AppendField(separator, "AssetPath", AssetDatabase.GetAssetPath(mainObject));
#endif

            base.AppendDetails(text, separator);

            text.AppendField(separator, nameof(IsPlaying), IsPlaying);

            try
            {
                text.AppendField(separator, nameof(Time), TimeD)
                    .Append("s / ")
                    .Append(Length)
                    .Append("s = ")
                    .Append((NormalizedTime * 100).ToString("0.00"))
                    .Append('%');

                text.AppendField(separator, nameof(IsLooping), IsLooping);
            }
            catch (Exception exception)
            {
                text.Append(separator).Append(exception);
            }

            text.AppendField(separator, nameof(Events), SharedEvents?.DeepToString(false));

        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

