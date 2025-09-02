// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// An <see cref="AnimancerState"/> which blends multiple child states
    /// by allowing you to control their <see cref="AnimancerNode.Weight"/> manually.
    /// </summary>
    /// <remarks>
    /// This mixer type is similar to the Direct Blend Type in Mecanim Blend Trees.
    /// The official <see href="https://learn.unity.com/tutorial/5c5152bcedbc2a001fd5c696">Direct Blend Trees</see>
    /// tutorial explains their general concepts and purpose which apply to <see cref="ManualMixerState"/>s as well.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">
    /// Mixers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ManualMixerState
    /// 
    public partial class ManualMixerState : AnimancerState,
        ICopyable<ManualMixerState>,
        IParametizedState,
        IUpdatable
    {
        /************************************************************************************************************************/
        #region Properties
        /************************************************************************************************************************/

        /// <summary>The states connected to this mixer.</summary>
        /// <remarks>Only states up to the <see cref="ChildCount"/> should be assigned.</remarks>
        protected AnimancerState[] ChildStates { get; private set; }
            = Array.Empty<AnimancerState>();

        /************************************************************************************************************************/

        private int _ChildCount;

        /// <inheritdoc/>
        public sealed override int ChildCount
            => _ChildCount;

        /************************************************************************************************************************/

        /// <summary>The size of the internal array of <see cref="ChildStates"/>.</summary>
        /// <remarks>This value starts at 0 then expands to <see cref="ChildCapacity"/> when the first child is added.</remarks>
        public int ChildCapacity
        {
            get => ChildStates.Length;
            set
            {
                if (value == ChildStates.Length)
                    return;

#if UNITY_ASSERTIONS
                if (value <= 1 && OptionalWarning.MixerMinChildren.IsEnabled())
                    OptionalWarning.MixerMinChildren.Log(
                        $"The {nameof(ChildCapacity)} of '{this}' is being set to {value}." +
                        $" The purpose of a mixer is to mix multiple child states so this may be a mistake.",
                        Graph?.Component);
#endif

                var newChildStates = new AnimancerState[value];
                if (value > _ChildCount)// Increase size.
                {
                    Array.Copy(ChildStates, newChildStates, _ChildCount);
                }
                else// Decrease size.
                {
                    for (int i = value; i < _ChildCount; i++)
                        ChildStates[i].Destroy();

                    Array.Copy(ChildStates, newChildStates, value);
                    _ChildCount = value;
                }

                ChildStates = newChildStates;

                if (_Playable.IsValid())
                {
                    _Playable.SetInputCount(value);
                }
                else if (Graph != null)
                {
                    CreatePlayable();
                }

                OnChildCapacityChanged();
            }
        }

        /// <summary>Called when the <see cref="ChildCapacity"/> is changed.</summary>
        protected virtual void OnChildCapacityChanged() { }

        /// <summary><see cref="ChildCapacity"/> starts at 0 then expands to this value when the first child is added.</summary>
        /// <remarks>Default 8.</remarks>
        public static int DefaultChildCapacity { get; set; } = 8;

        /// <summary>
        /// Ensures that the remaining unused <see cref="ChildCapacity"/>
        /// is greater than or equal to the specified `minimumCapacity`.
        /// </summary>
        public void EnsureRemainingChildCapacity(int minimumCapacity)
        {
            minimumCapacity += _ChildCount;
            if (ChildCapacity < minimumCapacity)
            {
                var capacity = Math.Max(ChildCapacity, DefaultChildCapacity);
                while (capacity < minimumCapacity)
                    capacity *= 2;

                ChildCapacity = capacity;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public sealed override AnimancerState GetChild(int index)
            => ChildStates[index];

        /// <inheritdoc/>
        public sealed override FastEnumerator<AnimancerState> GetEnumerator()
            => new(ChildStates, _ChildCount);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnSetIsPlaying()
        {
            var isPlaying = IsPlaying;
            for (int i = _ChildCount - 1; i >= 0; i--)
                ChildStates[i].IsPlaying = isPlaying;
        }

        /************************************************************************************************************************/

        /// <summary>If greater than 0 then <see cref="IsLooping"/> is true.</summary>
        private int _LoopingChildCount;

        /// <summary>Are any child states looping?</summary>
        public override bool IsLooping
            => _LoopingChildCount > 0;

        /// <summary>Sets <see cref="IsLooping"/> and informs the <see cref="AnimancerNodeBase.Parent"/>s.</summary>
        private void AddIsLooping(int offset)
        {
            var wasLooping = IsLooping;

            _LoopingChildCount += offset;

            var isLooping = IsLooping;
            if (wasLooping != isLooping)
                OnIsLoopingChangedRecursive(isLooping);
        }

        /// <inheritdoc/>
        protected override void OnChildIsLoopingChanged(bool value)
            => AddIsLooping(value ? 1 : -1);

        /************************************************************************************************************************/

        /// <summary>The weighted average <see cref="AnimancerState.Time"/> of each child state.</summary>
        /// <remarks>
        /// If there are any <see cref="SynchronizedChildren"/>,
        /// only those states will be included in the getter calculation.
        /// </remarks>
        public override double RawTime
        {
            get
            {
                GetTimeDetails(out var totalWeight, out var normalizedTime, out var length);

                if (totalWeight == 0)
                    return base.RawTime;

                totalWeight *= totalWeight;
                return normalizedTime * length / totalWeight;
            }
            set
            {
                if (value == 0)
                    goto SetToZero;

                var length = Length;
                if (length == 0)
                    goto SetToZero;

                value /= length;// Normalize.

                for (int i = _ChildCount - 1; i >= 0; i--)
                    ChildStates[i].NormalizedTimeD = value;

                return;

                // If the value is 0, we can set the child times more efficiently.
                SetToZero:
                for (int i = _ChildCount - 1; i >= 0; i--)
                    ChildStates[i].TimeD = 0;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void MoveTime(double time, bool normalized)
        {
            base.MoveTime(time, normalized);

            for (int i = _ChildCount - 1; i >= 0; i--)
                ChildStates[i].MoveTime(time, normalized);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void GetEventDispatchInfo(
            out float length,
            out float normalizedTime,
            out bool isLooping)
        {
            GetTimeDetails(out _, out normalizedTime, out length);
            isLooping = _LoopingChildCount > 0;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Gets the time details based on the synchronized child states if any are active,
        /// otherwise recalculates based on all child states.
        /// </summary>
        private void GetTimeDetails(out float totalWeight, out float normalizedTime, out float length)
        {
            if (_SynchronizedChildren != null)
            {
                GetTimeDetails(
                    _SynchronizedChildren,
                    _SynchronizedChildren.Count,
                    out totalWeight,
                    out normalizedTime,
                    out length);
                if (totalWeight > MinimumSynchronizeChildrenWeight)
                    return;
            }

            GetTimeDetails(
                ChildStates,
                _ChildCount,
                out totalWeight,
                out normalizedTime,
                out length);
        }

        /// <summary>Gets the time details based on the `states`.</summary>
        private void GetTimeDetails(
            IList<AnimancerState> states,
            int count,
            out float totalWeight,
            out float normalizedTime,
            out float length)
        {
            totalWeight = 0;
            normalizedTime = 0;
            length = 0;

            for (int i = count - 1; i >= 0; i--)
            {
                var state = states[i];
                var weight = state.Weight;
                if (weight == 0)
                    continue;

                var stateLength = state.Length;
                if (stateLength == 0)
                    continue;

                totalWeight += weight;
                normalizedTime += state.Time / stateLength * weight;
                length += stateLength * weight;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The weighted average <see cref="AnimancerState.Length"/> of each child state.</summary>
        public override float Length
        {
            get
            {
                var length = 0f;
                var totalChildWeight = 0f;

                if (_SynchronizedChildren != null)
                {
                    for (int i = _SynchronizedChildren.Count - 1; i >= 0; i--)
                    {
                        var state = _SynchronizedChildren[i];
                        var weight = state.Weight;
                        if (weight == 0)
                            continue;

                        var stateLength = state.Length;
                        if (stateLength == 0)
                            continue;

                        totalChildWeight += weight;
                        length += stateLength * weight;
                    }
                }

                if (totalChildWeight > 0)
                    return length / totalChildWeight;

                totalChildWeight = CalculateTotalWeight(ChildStates, _ChildCount);
                if (totalChildWeight <= 0)
                    return 0;

                for (int i = _ChildCount - 1; i >= 0; i--)
                {
                    var state = ChildStates[i];
                    length += state.Length * state.Weight;
                }

                return length / totalChildWeight;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Initialization
        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="AnimationMixerPlayable"/> managed by this state.</summary>
        protected override void CreatePlayable(out Playable playable)
        {
            playable = AnimationMixerPlayable.Create(Graph._PlayableGraph, ChildCapacity);
        }

        /************************************************************************************************************************/

        /// <summary>Connects the `state` to this mixer at its <see cref="AnimancerNode.Index"/>.</summary>
        protected internal override void OnAddChild(AnimancerState state)
        {
            Validate.AssertGraph(state, Graph);

            var capacity = ChildCapacity;
            if (_ChildCount >= capacity)
                ChildCapacity = Math.Max(DefaultChildCapacity, capacity * 2);

            state.Index = _ChildCount;
            ChildStates[_ChildCount] = state;
            _ChildCount++;

            state.IsPlaying = IsPlaying;

            if (Graph != null)
                ConnectChildUnsafe(state.Index, state);

            if (SynchronizeNewChildren)
                Synchronize(state);

            if (state.IsLooping)
                AddIsLooping(1);

#if UNITY_ASSERTIONS
            _CachedToString = null;
#endif
        }

        /************************************************************************************************************************/

        /// <summary>Disconnects the `state` from this mixer at its <see cref="AnimancerNode.Index"/>.</summary>
        protected internal override void OnRemoveChild(AnimancerState state)
        {
            DontSynchronize(state);

            Validate.AssertCanRemoveChild(state, ChildStates, _ChildCount);

            // Shuffle all subsequent children down one place.
            if (Graph == null || !Graph._PlayableGraph.IsValid())
            {
                Array.Copy(
                    ChildStates, state.Index + 1,
                    ChildStates, state.Index,
                    _ChildCount - state.Index - 1);

                for (int i = state.Index; i < _ChildCount - 1; i++)
                    ChildStates[i].Index = i;
            }
            else
            {
                Graph._PlayableGraph.Disconnect(_Playable, state.Index);

                for (int i = state.Index + 1; i < _ChildCount; i++)
                {
                    var otherChild = ChildStates[i];
                    Graph._PlayableGraph.Disconnect(_Playable, otherChild.Index);
                    otherChild.Index = i - 1;
                    ChildStates[i - 1] = otherChild;
                    ConnectChildUnsafe(i - 1, otherChild);
                }
            }

            _ChildCount--;
            ChildStates[_ChildCount] = null;

            if (state.IsLooping)
                AddIsLooping(-1);

#if UNITY_ASSERTIONS
            _CachedToString = null;
#endif
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Destroy()
        {
            DestroyChildren();
            base.Destroy();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerState Clone(CloneContext context)
        {
            var clone = new ManualMixerState();
            clone.CopyFrom(this, context);
            return clone;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public sealed override void CopyFrom(AnimancerState copyFrom, CloneContext context)
            => this.CopyFromBase(copyFrom, context);

        /// <inheritdoc/>
        public virtual void CopyFrom(ManualMixerState copyFrom, CloneContext context)
        {
            base.CopyFrom(copyFrom, context);

            DestroyChildren();

            var synchronizeNewChildren = SynchronizeNewChildren;

            var childCount = copyFrom.ChildCount;
            EnsureRemainingChildCapacity(childCount);

            for (int i = 0; i < childCount; i++)
            {
                var child = copyFrom.ChildStates[i];
                SynchronizeNewChildren = copyFrom.IsSynchronized(child);
                child = context.Clone(child);
                Add(child);
            }

            SynchronizeNewChildren = synchronizeNewChildren;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Child Configuration
        /************************************************************************************************************************/

        /// <summary>Assigns the `state` as a child of this mixer.</summary>
        /// <remarks>This is the same as calling <see cref="AnimancerState.SetParent"/>.</remarks>
        public void Add(AnimancerState state)
            => state.SetParent(this);

        /// <summary>Creates and returns a new <see cref="ClipState"/> to play the `clip` as a child of this mixer.</summary>
        public ClipState Add(AnimationClip clip)
        {
            var state = new ClipState(clip);
            Add(state);
            return state;
        }

        /// <summary>Calls <see cref="AnimancerUtilities.CreateStateAndApply"/> then <see cref="Add(AnimancerState)"/>.</summary>
        public AnimancerState Add(ITransition transition)
        {
            var state = transition.CreateStateAndApply(Graph);
            Add(state);
            return state;
        }

        /// <summary>Calls one of the other <see cref="Add(object)"/> overloads as appropriate for the `child`.</summary>
        public AnimancerState Add(object child)
        {
            if (child is AnimationClip clip)
                return Add(clip);

            if (child is ITransition transition)
                return Add(transition);

            if (child is AnimancerState state)
            {
                Add(state);
                return state;
            }

            MarkAsUsed(this);
            throw new ArgumentException($"Failed to {nameof(Add)} '{AnimancerUtilities.ToStringOrNull(child)}'" +
                $" as child of '{this}' because it isn't an" +
                $" {nameof(AnimationClip)}, {nameof(ITransition)}, or {nameof(AnimancerState)}.");
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="Add(AnimationClip)"/> for each of the `clips`.</summary>
        public void AddRange(IList<AnimationClip> clips)
        {
            var count = clips.Count;
            EnsureRemainingChildCapacity(count);

            for (int i = 0; i < count; i++)
                Add(clips[i]);
        }

        /// <summary>Calls <see cref="Add(AnimationClip)"/> for each of the `clips`.</summary>
        public void AddRange(params AnimationClip[] clips)
            => AddRange((IList<AnimationClip>)clips);

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="Add(ITransition)"/> for each of the `transitions`.</summary>
        public void AddRange(IList<ITransition> transitions)
        {
            var count = transitions.Count;
            EnsureRemainingChildCapacity(count);

            for (int i = 0; i < count; i++)
                Add(transitions[i]);
        }

        /// <summary>Calls <see cref="Add(ITransition)"/> for each of the `transitions`.</summary>
        public void AddRange(params ITransition[] transitions)
            => AddRange((IList<ITransition>)transitions);

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="Add(object)"/> for each of the `children`.</summary>
        public void AddRange(IList<object> children)
        {
            var count = children.Count;
            EnsureRemainingChildCapacity(count);

            for (int i = 0; i < count; i++)
                Add(children[i]);
        }

        /// <summary>Calls <see cref="Add(object)"/> for each of the `children`.</summary>
        public void AddRange(params object[] children)
            => AddRange((IList<object>)children);

        /************************************************************************************************************************/

        /// <summary>Removes the child at the specified `index`.</summary>
        public void Remove(int index, bool destroy)
            => Remove(ChildStates[index], destroy);

        /// <summary>Removes the specified `child`.</summary>
        public void Remove(AnimancerState child, bool destroy)
        {
#if UNITY_ASSERTIONS
            if (child.Parent != this)
                Debug.LogWarning($"Attempting to remove a state which is not a child of this {GetType().Name}." +
                    $" This will remove the child from its actual parent so you should directly call" +
                    $" child.{nameof(child.Destroy)} or child.{nameof(child.SetParent)}(null, -1) instead." +
                    $"\n• Child: {child}" +
                    $"\n• Removing From: {this}" +
                    $"\n• Actual Parent: {child.Parent}",
                    Graph?.Component as Object);
#endif

            if (destroy)
                child.Destroy();
            else
                child.SetParent(null);
        }

        /************************************************************************************************************************/

        /// <summary>Replaces the `child` at the specified `index`.</summary>
        public void Set(int index, AnimancerState child, bool destroyPrevious)
        {
#if UNITY_ASSERTIONS
            if ((uint)index >= _ChildCount)
            {
                MarkAsUsed(this);
                MarkAsUsed(child);
                throw new IndexOutOfRangeException(
                    $"Invalid child index. Must be 0 <= index < {nameof(ChildCount)} ({ChildCount}).");
            }
#endif

            if (child.Parent != null)
                child.SetParent(null);

            var previousChild = ChildStates[index];
            DontSynchronize(previousChild);
            previousChild.SetParentInternal(null);

            child.SetGraph(Graph);
            ChildStates[index] = child;
            child.SetParentInternal(this, index);
            child.IsPlaying = IsPlaying;

            if (Graph != null)
            {
                Graph._PlayableGraph.Disconnect(_Playable, index);
                ConnectChildUnsafe(index, child);
            }

            var loopingOffset = 0;
            if (previousChild.IsLooping)
                loopingOffset--;
            if (child.IsLooping)
                loopingOffset++;
            if (loopingOffset != 0)
                AddIsLooping(loopingOffset);

            child.CopyIKFlags(this);

            if (SynchronizeNewChildren)
                Synchronize(child);

            if (destroyPrevious)
                previousChild.Destroy();

#if UNITY_ASSERTIONS
            _CachedToString = null;
#endif
        }

        /// <summary>Replaces the child at the specified `index` with a new <see cref="ClipState"/>.</summary>
        public ClipState Set(int index, AnimationClip clip, bool destroyPrevious)
        {
            var state = new ClipState(clip);
            Set(index, state, destroyPrevious);
            return state;
        }

        /// <summary>Replaces the child at the specified `index` with a <see cref="ITransition.CreateState"/>.</summary>
        public AnimancerState Set(int index, ITransition transition, bool destroyPrevious)
        {
            var state = transition.CreateStateAndApply(Graph);
            Set(index, state, destroyPrevious);
            return state;
        }

        /// <summary>Calls one of the other <see cref="Set(int, object, bool)"/> overloads as appropriate for the `child`.</summary>
        public AnimancerState Set(int index, object child, bool destroyPrevious)
        {
            if (child is AnimationClip clip)
                return Set(index, clip, destroyPrevious);

            if (child is ITransition transition)
                return Set(index, transition, destroyPrevious);

            if (child is AnimancerState state)
            {
                Set(index, state, destroyPrevious);
                return state;
            }

            MarkAsUsed(this);
            throw new ArgumentException($"Failed to {nameof(Set)} '{AnimancerUtilities.ToStringOrNull(child)}'" +
                $" as child of '{this}' because it isn't an" +
                $" {nameof(AnimationClip)}, {nameof(ITransition)}, or {nameof(AnimancerState)}.");
        }

        /************************************************************************************************************************/

        /// <summary>Returns the index of the specified `child` state.</summary>
        public int IndexOf(AnimancerState child)
            => Array.IndexOf(ChildStates, child, 0, _ChildCount);

        /************************************************************************************************************************/

        /// <summary>
        /// Destroys all <see cref="ChildStates"/> connected to this mixer. This operation cannot be undone.
        /// </summary>
        public void DestroyChildren()
        {
            for (int i = _ChildCount - 1; i >= 0; i--)
                ChildStates[i].Destroy();

            Array.Clear(ChildStates, 0, _ChildCount);
            _ChildCount = 0;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Jobs
        /************************************************************************************************************************/

        /// <summary>
        /// Creates an <see cref="AnimationScriptPlayable"/> to run the specified Animation Job instead of the usual
        /// <see cref="AnimationMixerPlayable"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// <strong>Example:</strong><code>
        /// void CreatePlayableExample(AnimancerComponent animancer)
        /// }
        ///     var job = new MyJob();// A struct that implements IAnimationJob.
        ///     var mixer = new WhateverMixerState();// e.g. LinearMixerState.
        ///     mixer.CreatePlayable(animancer, job);
        ///     // Use mixer.Initialize, CreateChild, and SetChild to configure the children as normal.
        /// }
        /// </code>
        /// See also: <seealso cref="CreatePlayable{T}(out Playable, T, bool)"/>
        /// </remarks>
        public AnimationScriptPlayable CreatePlayable<T>(
            AnimancerGraph graph,
            T job,
            bool processInputs = false)
            where T : struct, IAnimationJob
        {
            // Can't just use SetGraph normally because it would call the regular CreatePlayable method.
            SetGraph(null);

            Graph = graph;
            graph.States.Register(this);

            var playable = AnimationScriptPlayable.Create(graph._PlayableGraph, job, _ChildCount);

            if (!processInputs)
                playable.SetProcessInputs(false);

            for (int i = _ChildCount - 1; i >= 0; i--)
                ChildStates[i].SetGraph(graph);

            return playable;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates an <see cref="AnimationScriptPlayable"/> to run the specified Animation Job instead of the usual
        /// <see cref="AnimationMixerPlayable"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// <para></para>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/source/creating-custom-states">
        /// Creating Custom States</see>
        /// <para></para>
        /// <strong>Example:</strong><code>
        /// public class MyMixer : LinearMixerState
        /// {
        ///     protected override void CreatePlayable(out Playable playable)
        ///     {
        ///         CreatePlayable(out playable, new MyJob());
        ///     }
        /// 
        ///     private struct MyJob : IAnimationJob
        ///     {
        ///         public void ProcessAnimation(AnimationStream stream)
        ///         {
        ///         }
        /// 
        ///         public void ProcessRootMotion(AnimationStream stream)
        ///         {
        ///         }
        ///     }
        /// }
        /// </code>
        /// See also: <seealso cref="CreatePlayable{T}(AnimancerGraph, T, bool)"/>
        /// </remarks>
        protected void CreatePlayable<T>(
            out Playable playable,
            T job,
            bool processInputs = false)
            where T : struct, IAnimationJob
        {
            var scriptPlayable = AnimationScriptPlayable.Create(Graph._PlayableGraph, job, ChildCount);

            if (!processInputs)
                scriptPlayable.SetProcessInputs(false);

            playable = scriptPlayable;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Gets the Animation Job data from the <see cref="AnimationScriptPlayable"/>.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// This mixer was not initialized using <see cref="CreatePlayable{T}(AnimancerGraph, T, bool)"/>
        /// or <see cref="CreatePlayable{T}(out Playable, T, bool)"/>.
        /// </exception>
        public T GetJobData<T>()
            where T : struct, IAnimationJob
            => ((AnimationScriptPlayable)_Playable).GetJobData<T>();

        /// <summary>
        /// Sets the Animation Job data in the <see cref="AnimationScriptPlayable"/>.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// This mixer was not initialized using <see cref="CreatePlayable{T}(AnimancerGraph, T, bool)"/>
        /// or <see cref="CreatePlayable{T}(out Playable, T, bool)"/>.
        /// </exception>
        public void SetJobData<T>(T value)
            where T : struct, IAnimationJob
            => ((AnimationScriptPlayable)_Playable).SetJobData(value);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Updates
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public int UpdatableIndex { get; set; } = IUpdatable.List.NotInList;

        /// <summary>Recalculates the weights of child states and synchronizes their times if necessary.</summary>
        public virtual void Update()
        {
            if (!ApplySynchronizeChildren())
                Graph.CancelPreUpdate(this);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected internal override void UpdateEvents()
            => UpdateEventsRecursive(this);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void SetGraph(AnimancerGraph graph)
        {
            if (Graph == graph)
                return;

            Graph?.CancelPreUpdate(this);

            base.SetGraph(graph);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Synchronize Children
        /************************************************************************************************************************/

        /// <summary>Should newly added children be automatically added to the synchronization list? Default true.</summary>
        public static bool SynchronizeNewChildren { get; set; } = true;

        /// <summary>The minimum total weight of all children for their times to be synchronized. Default 0.01.</summary>
        public static float MinimumSynchronizeChildrenWeight { get; set; } = 0.01f;

        /************************************************************************************************************************/

        private List<AnimancerState> _SynchronizedChildren;

        /// <summary>A copy of the internal list of child states that will have their times synchronized.</summary>
        /// <remarks>
        /// If this mixer is a child of another mixer, its synchronized children will be managed by the parent.
        /// <para></para>
        /// The getter allocates a new array if <see cref="SynchronizedChildCount"/> is greater than zero.
        /// </remarks>
        public AnimancerState[] SynchronizedChildren
        {
            get => SynchronizedChildCount > 0
                ? _SynchronizedChildren.ToArray()
                : Array.Empty<AnimancerState>();
            set
            {
                if (_SynchronizedChildren == null)
                    _SynchronizedChildren = new();
                else
                    _SynchronizedChildren.Clear();

                for (int i = 0; i < value.Length; i++)
                    Synchronize(value[i]);
            }
        }

        /// <summary>The number of <see cref="SynchronizedChildren"/>.</summary>
        public int SynchronizedChildCount
            => _SynchronizedChildren != null
            ? _SynchronizedChildren.Count
            : 0;

        /************************************************************************************************************************/

        /// <summary>Is the `state` in the <see cref="SynchronizedChildren"/>?</summary>
        public bool IsSynchronized(AnimancerState state)
        {
            var synchronizer = GetParentMixer();
            return
                synchronizer._SynchronizedChildren != null &&
                synchronizer._SynchronizedChildren.Contains(state);
        }

        /************************************************************************************************************************/

        /// <summary>Adds the `state` to the <see cref="SynchronizedChildren"/>.</summary>
        /// <remarks>
        /// The `state` must be a child of this mixer.
        /// <para></para>
        /// If this mixer is a child of another mixer, the `state` will be added to the parent's
        /// <see cref="SynchronizedChildren"/> instead.
        /// </remarks>
        public void Synchronize(AnimancerState state)
        {
            if (state == null)
                return;

#if UNITY_ASSERTIONS
            if (!IsChildOf(state, this))
            {
                MarkAsUsed(this);
                throw new ArgumentException(
                    $"State is not a child of the mixer." +
                    $"\n• State: {state}" +
                    $"\n• Mixer: {this}",
                    nameof(state));
            }
#endif

            var synchronizer = GetParentMixer();
            synchronizer.SynchronizeDirect(state);
        }

        /// <summary>The internal implementation of <see cref="Synchronize"/>.</summary>
        private void SynchronizeDirect(AnimancerState state)
        {
            if (state == null)
                return;

            // If the state is a mixer, steal all its synchronized children instead of synchronizing the mixer itself.
            if (state is ManualMixerState mixer)
            {
                if (mixer._SynchronizedChildren != null)
                {
                    for (int i = 0; i < mixer._SynchronizedChildren.Count; i++)
                        Synchronize(mixer._SynchronizedChildren[i]);
                    mixer._SynchronizedChildren.Clear();
                }

                return;
            }

#if UNITY_ASSERTIONS
            if (OptionalWarning.MixerSynchronizeZeroLength.IsEnabled() && state.Length == 0)
                OptionalWarning.MixerSynchronizeZeroLength.Log(
                    $"Adding a state with zero {nameof(AnimancerState.Length)} to the synchronization list: '{state}'." +
                    $"\n\nSynchronization is based on the {nameof(NormalizedTime)}" +
                    $" which can't be calculated if the {nameof(Length)} is 0." +
                    $" Some state types can change their {nameof(Length)}, in which case you can just disable this warning." +
                    $" But otherwise, the indicated state probably shouldn't be added to the synchronization list.",
                    Graph?.Component);
#endif

            _SynchronizedChildren ??= new();

#if UNITY_ASSERTIONS
            if (_SynchronizedChildren.Contains(state))
                Debug.LogError($"{state} is already in the {nameof(SynchronizedChildren)} list.");
#endif

            _SynchronizedChildren.Add(state);
            Graph?.RequirePreUpdate(this);
        }

        /************************************************************************************************************************/

        /// <summary>Removes the `state` from the <see cref="SynchronizedChildren"/>.</summary>
        public void DontSynchronize(AnimancerState state)
        {
            var synchronizer = GetParentMixer();
            if (synchronizer._SynchronizedChildren != null &&
                synchronizer._SynchronizedChildren.Remove(state) &&
                state._Playable.IsValid())
                state._Playable.SetSpeed(state.Speed);
        }

        /************************************************************************************************************************/

        /// <summary>Removes all children of this mixer from the <see cref="SynchronizedChildren"/>.</summary>
        public void DontSynchronizeChildren()
        {
            var synchronizer = GetParentMixer();
            var synchronizedChildren = synchronizer._SynchronizedChildren;
            if (synchronizedChildren == null)
                return;

            if (synchronizer == this)
            {
                for (int i = synchronizedChildren.Count - 1; i >= 0; i--)
                {
                    var state = synchronizedChildren[i];
                    if (state._Playable.IsValid())
                        state._Playable.SetSpeed(state.Speed);
                }

                synchronizedChildren.Clear();
            }
            else
            {
                for (int i = synchronizedChildren.Count - 1; i >= 0; i--)
                {
                    var state = synchronizedChildren[i];
                    if (IsChildOf(state, this))
                    {
                        if (state._Playable.IsValid())
                            state._Playable.SetSpeed(state.Speed);
                        synchronizedChildren.RemoveAt(i);
                    }
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Initializes the internal <see cref="SynchronizedChildren"/> list.</summary>
        /// <remarks>
        /// The array can be null or empty. Any elements not in the array will be treated as <c>true</c>.
        /// <para></para>
        /// This method can only be called before any <see cref="SynchronizedChildren"/> are added and also before this
        /// mixer is made the child of another mixer.
        /// </remarks>
        public void InitializeSynchronizedChildren(params bool[] synchronizeChildren)
        {
            AnimancerUtilities.Assert(GetParentMixer() == this,
                $"{nameof(InitializeSynchronizedChildren)} cannot be used on a mixer that is a child of another mixer.");
            AnimancerUtilities.Assert(_SynchronizedChildren == null,
                $"{nameof(InitializeSynchronizedChildren)} cannot be used on a mixer already has synchronized children.");

            int flagCount;
            if (synchronizeChildren != null)
            {
                flagCount = synchronizeChildren.Length;
                for (int i = 0; i < flagCount; i++)
                    if (synchronizeChildren[i])
                        SynchronizeDirect(ChildStates[i]);
            }
            else flagCount = 0;

            for (int i = flagCount; i < _ChildCount; i++)
                SynchronizeDirect(ChildStates[i]);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the highest <see cref="ManualMixerState"/> in the hierarchy above this mixer
        /// or this mixer itself if there are none above it.
        /// </summary>
        public ManualMixerState GetParentMixer()
        {
            var mixer = this;

            var parent = Parent;
            while (parent != null)
            {
                if (parent is ManualMixerState parentMixer)
                    mixer = parentMixer;

                parent = parent.Parent;
            }

            return mixer;
        }

        /// <summary>Returns the highest <see cref="ManualMixerState"/> in the hierarchy above the `state` (inclusive).</summary>
        public static ManualMixerState GetParentMixer(AnimancerNodeBase node)
        {
            ManualMixerState mixer = null;

            while (node != null)
            {
                if (node is ManualMixerState parentMixer)
                    mixer = parentMixer;

                node = node.Parent;
            }

            return mixer;
        }

        /************************************************************************************************************************/

        /// <summary>Is the `child` a child of the `parent`?</summary>
        public static bool IsChildOf(AnimancerNodeBase child, AnimancerNodeBase parent)
        {
            while (true)
            {
                child = child.Parent;
                if (child == parent)
                    return true;
                else if (child == null)
                    return false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Synchronizes the <see cref="AnimancerState.NormalizedTime"/>s of the <see cref="SynchronizedChildren"/> by
        /// modifying their internal playable speeds.
        /// </summary>
        private bool ApplySynchronizeChildren()
        {
            if (Weight == 0 ||
                !IsPlaying ||
                _SynchronizedChildren == null ||
                _SynchronizedChildren.Count <= 1)
                return false;

            var deltaTime = AnimancerGraph.DeltaTime * CalculateRealEffectiveSpeed();
            if (deltaTime == 0)
                return true;

            var count = _SynchronizedChildren.Count;

            // Calculate the weighted average normalized time and normalized speed of all children.

            var totalWeight = 0f;
            var weightedNormalizedTime = 0f;
            var weightedNormalizedSpeed = 0f;

            for (int i = 0; i < count; i++)
            {
                var state = _SynchronizedChildren[i];

                var weight = CalculateRelativeWeight(state);
                if (weight == 0)
                    continue;

                var length = state.Length;
                if (length == 0)
                    continue;

                totalWeight += weight;

                weight /= length;

                weightedNormalizedTime += state.Time * weight;
                weightedNormalizedSpeed += state.Speed * weight;
            }

#if UNITY_ASSERTIONS
            if (!(totalWeight >= 0) || totalWeight == float.PositiveInfinity)// Reversed comparison includes NaN.
            {
                MarkAsUsed(this);
                throw new ArgumentOutOfRangeException(nameof(totalWeight), totalWeight,
                    $"Total weight {Strings.MustBeFinite} and must be positive");
            }
            if (!weightedNormalizedTime.IsFinite())
            {
                MarkAsUsed(this);
                throw new ArgumentOutOfRangeException(nameof(weightedNormalizedTime), weightedNormalizedTime,
                    $"Time {Strings.MustBeFinite}");
            }
            if (!weightedNormalizedSpeed.IsFinite())
            {
                MarkAsUsed(this);
                throw new ArgumentOutOfRangeException(nameof(weightedNormalizedSpeed), weightedNormalizedSpeed,
                    $"Speed {Strings.MustBeFinite}");
            }
#endif

            // If the total weight is too small, pretend they are all at Weight = 1.
            if (totalWeight < MinimumSynchronizeChildrenWeight)
            {
                weightedNormalizedTime = 0;
                weightedNormalizedSpeed = 0;

                var nonZeroCount = 0;
                for (int i = 0; i < count; i++)
                {
                    var state = _SynchronizedChildren[i];

                    var length = state.Length;
                    if (length == 0)
                        continue;

                    length = 1f / length;

                    weightedNormalizedTime += state.Time * length;
                    weightedNormalizedSpeed += state.Speed * length;

                    nonZeroCount++;
                }

                totalWeight = nonZeroCount;
            }

            // Increment that time value according to delta time.
            weightedNormalizedTime += deltaTime * weightedNormalizedSpeed;
            weightedNormalizedTime /= totalWeight;

            var inverseDeltaTime = 1f / deltaTime;

            // Modify the speed of all children to go from their current normalized time to the average in one frame.
            for (int i = 0; i < count; i++)
            {
                var state = _SynchronizedChildren[i];
                var length = state.Length;
                if (length == 0)
                    continue;

                var normalizedTime = state.Time / length;
                var speed = (weightedNormalizedTime - normalizedTime) * length * inverseDeltaTime;
                state._Playable.SetSpeed(speed);
            }

            // After this, all the playables will update and advance according to their new speeds this frame.

            return true;
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the weight of the `child` multiplied by its parents up to this mixer.</summary>
        private float CalculateRelativeWeight(AnimancerState child)
        {
            var weight = child.Weight;

            var parent = child.Parent;
            while (parent != this && parent != null)
            {
                weight *= parent.BaseWeight;
                parent = parent.Parent;
            }

            return weight;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The multiplied <see cref="PlayableExtensions.GetSpeed"/> of this mixer and its parents down the
        /// hierarchy to determine the actual speed its output is being played at.
        /// </summary>
        /// <remarks>
        /// This can be different from the <see cref="AnimancerNodeBase.EffectiveSpeed"/> because the
        /// <see cref="SynchronizedChildren"/> have their playable speed modified without setting their
        /// <see cref="AnimancerNodeBase.Speed"/>.
        /// </remarks>
        public float CalculateRealEffectiveSpeed()
        {
            var speed = _Playable.GetSpeed();

            var parent = Parent;
            while (parent != null)
            {
                speed *= parent.Playable.GetSpeed();
                parent = parent.Parent;
            }

            return (float)speed;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Inverse Kinematics
        /************************************************************************************************************************/

        private bool _ApplyAnimatorIK;

        /// <inheritdoc/>
        public override bool ApplyAnimatorIK
        {
            get => _ApplyAnimatorIK;
            set => base.ApplyAnimatorIK = _ApplyAnimatorIK = value;
        }

        /************************************************************************************************************************/

        private bool _ApplyFootIK;

        /// <inheritdoc/>
        public override bool ApplyFootIK
        {
            get => _ApplyFootIK;
            set => base.ApplyFootIK = _ApplyFootIK = value;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Other Methods
        /************************************************************************************************************************/

        /// <summary>Calculates the sum of the <see cref="AnimancerNode.Weight"/> of all `states`.</summary>
        public static float CalculateTotalWeight(AnimancerState[] states, int count)
        {
            var total = 0f;

            for (int i = count - 1; i >= 0; i--)
                total += states[i].Weight;

            return total;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets <see cref="AnimancerState.Time"/> for all <see cref="ChildStates"/>.
        /// </summary>
        public void SetChildrenTime(float value, bool normalized = false)
        {
            for (int i = _ChildCount - 1; i >= 0; i--)
            {
                var state = ChildStates[i];
                if (normalized)
                    state.NormalizedTime = value;
                else
                    state.Time = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sets the weight of all states after the `previousIndex` to 0.</summary>
        protected void DisableRemainingStates(int previousIndex)
        {
            for (int i = previousIndex + 1; i < _ChildCount; i++)
                Playable.SetChildWeight(ChildStates[i], 0);
        }

        /************************************************************************************************************************/

        private static float[] _TemporaryWeights = Array.Empty<float>();

        /// <summary>Returns an array at least as large as the `count`.</summary>
        /// <remarks>
        /// The same array is returned by subsequent calls as long as it's large enough
        /// and it isn't cleared between calls so it will contain the previous data.
        /// </remarks>
        public static float[] GetTemporaryFloatArray(int count)
        {
            if (_TemporaryWeights.Length < count)
            {
                if (count <= 16)
                    count = 16;
                else
                    count = Mathf.NextPowerOfTwo(count);

                _TemporaryWeights = new float[count];
            }

            return _TemporaryWeights;
        }

        /************************************************************************************************************************/

        /// <summary>Divides `weights` by the `totalWeight` and applies them to the child states.</summary>
        public void NormalizeAndApplyWeights(float totalWeight, float[] weights)
        {
            totalWeight = 1f / totalWeight;

            for (int i = _ChildCount - 1; i >= 0; i--)
            {
                var state = ChildStates[i];
                var weight = weights[i] * totalWeight;
                Playable.SetChildWeight(state, weight);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Gets a user-friendly key to identify the `state` in the Inspector.</summary>
        public virtual string GetDisplayKey(AnimancerState state)
            => $"[{state.Index}]";

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Vector3 AverageVelocity
        {
            get
            {
                var velocity = default(Vector3);

                for (int i = _ChildCount - 1; i >= 0; i--)
                {
                    var state = ChildStates[i];
                    velocity += state.AverageVelocity * state.Weight;
                }

                return velocity;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Recalculates the <see cref="AnimancerState.Duration"/> of all child states so that they add up to 1.
        /// </summary>
        public void NormalizeDurations()
        {
            int divideBy = 0;
            float totalDuration = 0f;

            // Count the number of states that exist and their total duration.
            for (int i = 0; i < _ChildCount; i++)
            {
                divideBy++;
                totalDuration += ChildStates[i].Duration;
            }

            // Calculate the average duration.
            totalDuration /= divideBy;

            // Set all states to that duration.
            for (int i = 0; i < _ChildCount; i++)
            {
                ChildStates[i].Duration = totalDuration;
            }
        }

        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        /// <summary>[Assert-Only] A string built by <see cref="ToString"/> to describe this mixer.</summary>
        private string _CachedToString;
#endif

        /// <summary>
        /// Returns a string describing the type of this mixer and the name of states connected to it.
        /// </summary>
        public override string ToString()
        {
#if UNITY_ASSERTIONS
            if (NameCache.TryToString(DebugName, out var name))
                return name;

            if (_CachedToString != null)
                return _CachedToString;
#endif

            // Gather child names.
            var childNames = ListPool.Acquire<string>();
            var allSimple = true;
            for (int i = 0; i < _ChildCount; i++)
            {
                var state = ChildStates[i];
                if (state == null)
                    continue;

                if (state.MainObject != null)
                {
                    childNames.Add(state.MainObject.name);
                }
                else
                {
                    childNames.Add(state.ToString());
                    allSimple = false;
                }
            }

            // If they all have a main object, check if they all have the same prefix so it doesn't need to be repeated.
            int prefixLength = 0;
            var count = childNames.Count;
            if (count <= 1 || !allSimple)
            {
                prefixLength = 0;
            }
            else
            {
                var prefix = childNames[0];
                var shortest = prefixLength = prefix.Length;

                for (int iName = 0; iName < count; iName++)
                {
                    var childName = childNames[iName];

                    if (shortest > childName.Length)
                    {
                        shortest = prefixLength = childName.Length;
                    }

                    for (int iCharacter = 0; iCharacter < prefixLength; iCharacter++)
                    {
                        if (childName[iCharacter] != prefix[iCharacter])
                        {
                            prefixLength = iCharacter;
                            break;
                        }
                    }
                }

                if (prefixLength < 3 ||// Less than 3 characters probably isn't an intentional prefix.
                    prefixLength >= shortest)
                    prefixLength = 0;
            }

            // Build the mixer name.
            var mixerName = StringBuilderPool.Instance.Acquire();

            var type = GetType().Name;
            if (type.EndsWith("State"))
                mixerName.Append(type, 0, type.Length - 5);
            else
                mixerName.Append(type);

            mixerName.Append('(');

            if (count > 0)
            {
                if (prefixLength > 0)
                    mixerName.Append(childNames[0], 0, prefixLength).Append('[');

                for (int i = 0; i < count; i++)
                {
                    if (i > 0)
                        mixerName.Append(", ");

                    var childName = childNames[i];
                    mixerName.Append(childName, prefixLength, childName.Length - prefixLength);
                }

                mixerName.Append(']');
            }
            ListPool.Release(childNames);

            mixerName.Append(')');

            var result = mixerName.ReleaseToString();

#if UNITY_ASSERTIONS
            _CachedToString = result;
#endif

            return result;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string separator)
        {
            base.AppendDetails(text, separator);

            text.Append(separator)
                .Append("SynchronizedChildren: ");

            if (SynchronizedChildCount == 0)
            {
                text.Append("0");
            }
            else
            {
                text.Append(_SynchronizedChildren.Count);
                separator += Strings.Indent;
                for (int i = 0; i < _SynchronizedChildren.Count; i++)
                {
                    text.Append(separator)
                        .Append(_SynchronizedChildren[i]);
                }
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void GatherAnimationClips(ICollection<AnimationClip> clips)
            => clips.GatherFromSource(ChildStates);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void GetParameters(List<StateParameterDetails> parameters) { }

        /// <inheritdoc/>
        public virtual void SetParameters(List<StateParameterDetails> parameters) { }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

