// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>
    /// A layer on which animations can play with their states managed independantly of other layers while blending the
    /// output with those layers.
    /// </summary>
    ///
    /// <remarks>
    /// This class can be used as a custom yield instruction to wait until all animations finish playing.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/layers">
    /// Layers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerLayer
    /// 
    public class AnimancerLayer : AnimancerNode,
        IAnimationClipCollection,
        ICopyable<AnimancerLayer>
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        /// <summary>[Internal] Creates a new <see cref="AnimancerLayer"/>.</summary>
        protected internal AnimancerLayer(AnimancerGraph graph, int index)
        {
            Graph = graph;
            Parent = graph;
            Index = index;

            if (ApplyParentAnimatorIK)
                _ApplyAnimatorIK = graph.ApplyAnimatorIK;
            if (ApplyParentFootIK)
                _ApplyFootIK = graph.ApplyFootIK;

            CreatePlayable();

            ActiveStatesInternal = new(new(Graph, Playable));
        }

        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="AnimationMixerPlayable"/> managed by this layer.</summary>
        protected override void CreatePlayable(out Playable playable)
            => playable = AnimationMixerPlayable.Create(Graph._PlayableGraph, _Capacity);

        /************************************************************************************************************************/

        /// <summary>A layer is its own root.</summary>
        public override AnimancerLayer Layer
            => this;

        /// <inheritdoc/>
        public override bool KeepChildrenConnected
            => Graph.KeepChildrenConnected;

        /************************************************************************************************************************/

        /// <summary>The animation states connected to this layer.</summary>
        private readonly List<AnimancerState>
            States = new();

        /************************************************************************************************************************/

        private readonly AnimancerState.ActiveList
            ActiveStatesInternal;

        /// <summary>The states connected to this layer which are <see cref="AnimancerState.IsActive"/>.</summary>
        public IReadOnlyIndexedList<AnimancerState> ActiveStates
            => ActiveStatesInternal;

        /************************************************************************************************************************/

        /// <summary>The default <see cref="Capacity"/> is 8 unless changed.</summary>
        /// <remarks>
        /// This value only affects newly created layers.
        /// <para></para>
        /// This value should be set high enough to include all states a layer is likely to have. It's generally not
        /// particularly important though since expanding the capacity is fairly fast.
        /// </remarks>
        public static int DefaultCapacity = 8;

        private int _Capacity = DefaultCapacity;

        /// <summary>
        /// The number of states that can be connected to this layer before its <see cref="Playable"/> needs to
        /// allocate more inputs.
        /// </summary>
        /// <remarks>Starts at the <see cref="DefaultCapacity"/> and doubles each time it needs to expand.</remarks>
        /// <exception cref="ArgumentException">This value cannot be set lower than the <see cref="ChildCount"/>.</exception>
        public int Capacity
        {
            get => _Capacity;
            set
            {
                if (value < ChildCount)
                    throw new ArgumentException(
                        $"{nameof(Capacity)} ({value}) cannot be smaller than {nameof(ChildCount)} ({ChildCount}).");

                _Capacity = value;
                _Playable.SetInputCount(value);
            }
        }

        /************************************************************************************************************************/

        private AnimancerState _CurrentState;

        /// <summary>The state of the animation currently being played.</summary>
        /// <remarks>
        /// Specifically, this is the state that was most recently started using any of the Play or CrossFade methods
        /// on this layer. States controlled individually via methods in the <see cref="AnimancerState"/> itself will
        /// not register in this property.
        /// <para></para>
        /// Each time this property changes, the <see cref="CommandCount"/> is incremented.
        /// </remarks>
        public AnimancerState CurrentState
        {
            get => _CurrentState;
            private set
            {
                _CurrentState = value;
                IncrementCommandCount();
            }
        }

        /// <summary>The number of times the <see cref="CurrentState"/> has changed.</summary>
        /// <remarks>
        /// By storing this value and later comparing the stored value to the current value,
        /// you can determine whether the state has been changed since then, even it has changed back to the same state.
        /// </remarks>
        public int CommandCount { get; private set; }

        /// <summary>Increases the <see cref="CommandCount"/> by 1.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementCommandCount()
            => CommandCount++;

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Determines whether this layer is set to additive blending. Otherwise it will override any earlier layers.
        /// </summary>
        public bool IsAdditive
        {
            get => Graph.Layers.IsAdditive(Index);
            set => Graph.Layers.SetAdditive(Index, value);
        }

        /************************************************************************************************************************/

        /// <summary>[Internal] [Pro-Only] The mask that determines which bones this layer will affect.</summary>
        internal AvatarMask _Mask;

        /// <summary>[Pro-Only] The mask that determines which bones this layer will affect.</summary>
        /// <remarks>
        /// Don't assign the same mask repeatedly unless you have modified it.
        /// This property doesn't check if the mask is the same
        /// so repeatedly assigning the same thing will simply waste performance.
        /// </remarks>
        public AvatarMask Mask
        {
            get => _Mask;
            set => Graph.Layers.SetMask(Index, value);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The average velocity of the root motion of all currently playing animations, taking their current
        /// <see cref="AnimancerNode.Weight"/> into account.
        /// </summary>
        public Vector3 AverageVelocity
        {
            get
            {
                var velocity = default(Vector3);

                for (int i = ActiveStatesInternal.Count - 1; i >= 0; i--)
                {
                    var state = ActiveStatesInternal[i];
                    velocity += state.AverageVelocity * state.Weight;
                }

                return velocity;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Child States
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override int ChildCount
            => States.Count;

        /// <summary>Returns the state connected to the specified `index` as a child of this layer.</summary>
        /// <remarks>This method is identical to <see cref="this[int]"/>.</remarks>
        public override AnimancerState GetChild(int index)
            => States[index];

        /// <summary>Returns the state connected to the specified `index` as a child of this layer.</summary>
        /// <remarks>This indexer is identical to <see cref="GetChild(int)"/>.</remarks>
        public AnimancerState this[int index]
            => States[index];

        /************************************************************************************************************************/

        /// <summary>Connects the `state` to this layer at its <see cref="AnimancerNode.Index"/>.</summary>
        protected internal override void OnAddChild(AnimancerState state)
        {
            Validate.AssertGraph(state, Graph);

            var index = States.Count;
            state.Index = index;
            States.Add(state);

            if (_Capacity <= index)
            {
                _Capacity *= 2;
                _Playable.SetInputCount(_Capacity);
            }

            // If the state should be active, deactivate and properly add it to the active list.
            if (state.TryDeactivate())
                ActiveStatesInternal.Add(state);

            if (Graph.KeepChildrenConnected)
                ConnectChildUnsafe(state.Index, state);
        }

        /************************************************************************************************************************/

        /// <summary>Disconnects the `state` from this layer at its <see cref="AnimancerNode.Index"/>.</summary>
        protected internal override void OnRemoveChild(AnimancerState state)
        {
            var index = state.Index;
            Validate.AssertCanRemoveChild(state, States, States.Count);

            if (ActiveStatesInternal.Remove(state))
                state._ActiveIndex = 0;

            if (Graph._PlayableGraph.IsValid() &&
                _Playable.GetInput(index).IsValid())
                Graph._PlayableGraph.Disconnect(_Playable, index);

            // Swap the last state into the place of the one that was just removed.
            var last = States.Count - 1;
            if (index < last)
            {
                state = States[last];

                DisconnectChildSafe(last);

                States[index] = state;
                state.Index = index;

                if (state.IsActive || Graph.KeepChildrenConnected)
                    ConnectChildUnsafe(index, state);
            }

            States.RemoveAt(last);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected internal override void ApplyChildActive(AnimancerState state, bool setActive)
        {
            if (setActive)
                ActiveStatesInternal.Add(state);
            else
                ActiveStatesInternal.Remove(state);
        }

        /************************************************************************************************************************/

        /// <summary>[Internal] Connects all states in this layer.</summary>
        internal void ConnectAllStates()
        {
            for (int i = ChildCount - 1; i >= 0; i--)
                if (!_Playable.GetInput(i).IsValid())
                    ConnectChildUnsafe(i, States[i]);
        }

        /// <summary>[Internal] Disconnects all states which are not <see cref="AnimancerState.IsActive"/>.</summary>
        internal void DisconnectInactiveStates()
        {
            for (int i = ChildCount - 1; i >= 0; i--)
                if (!States[i].IsActive)
                    DisconnectChildSafe(i);
        }

        /************************************************************************************************************************/

        /// <summary>[Internal]
        /// Checks if any events should be invoked on any of the <see cref="ActiveStates"/>.
        /// </summary>
        internal void UpdateEvents()
        {
            if (Weight <= 0)
                return;

            if (FadeGroup != null &&
                FadeGroup.GetTargetWeight(this) == 0 &&
                !AnimancerState.RaiseEventsDuringFadeOut)
                return;

            for (int i = ActiveStatesInternal.Count - 1; i >= 0; i--)
                ActiveStatesInternal[i].UpdateEvents();
        }

        /************************************************************************************************************************/

        /// <summary>[Internal] Cancels the current <see cref="FadeGroup"/> so it can be object pooled.</summary>
        internal void OnGraphDestroyed()
        {
            CurrentState?.FadeGroup?.Cancel();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override FastEnumerator<AnimancerState> GetEnumerator()
            => new(States);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public sealed override void CopyFrom(AnimancerNode copyFrom, CloneContext context)
            => this.CopyFromBase(copyFrom, context);

        /// <summary>Copies the details of `copyFrom` into this layer.</summary>
        /// <remarks>Call <see cref="CopyStatesFrom"/> (as well) if you want to copy the states.</remarks>
        public virtual void CopyFrom(AnimancerLayer copyFrom, CloneContext context)
        {
            base.CopyFrom(copyFrom, context);

            IsAdditive = copyFrom.IsAdditive;
            Mask = copyFrom.Mask;
        }

        /************************************************************************************************************************/

        /// <summary>Copies the details of all states in `copyFrom` to their equivalent states in this layer.</summary>
        /// <remarks>
        /// Any states which do not have an equivalent in this layer will be cloned into this layer.
        /// <para></para>
        /// Call <see cref="CopyFrom(AnimancerLayer, CloneContext)"/> (as well)
        /// if you want to copy the details of the layer itself.
        /// </remarks>
        public void CopyStatesFrom(AnimancerLayer copyFrom, CloneContext context, bool includeInactive = false)
        {
            if (copyFrom == this)
                return;

            Debug.Assert(context.TryGetValue(copyFrom.Graph, out var thisGraph));
            Debug.Assert(thisGraph == Graph);

            for (int i = ActiveStatesInternal.Count - 1; i >= 0; i--)
                ActiveStatesInternal[i].Stop();

            IncrementCommandCount();

            if (!includeInactive && Weight == 0)
                return;

            IReadOnlyList<AnimancerState> copyFromStates = includeInactive
                ? copyFrom.States
                : copyFrom.ActiveStatesInternal;

            var stateCount = copyFromStates.Count;
            for (int i = 0; i < stateCount; i++)
            {
                var state = copyFromStates[i];

                // If the clone already exists, copy over the state details.
                if (context.TryGetClone(state, out var clone))
                    clone.CopyFrom(state, context);
                else// Otherwise, create a new clone.
                    clone = context.Clone(state);

                if (clone.Parent != this)
                    clone.SetParent(this);

                if (copyFrom.CurrentState == state)
                    CurrentState = clone;

                // This should prevent the clones from being one frame behind after the next animation update
                // but it seems to only work for some states and not others.
                // clone.Time += clone.EffectiveSpeed * Time.deltaTime;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Create State
        /************************************************************************************************************************/

        /// <summary>Creates and returns a new <see cref="ClipState"/> to play the `clip`.</summary>
        /// <remarks>
        /// <see cref="AnimancerGraph.GetKey"/> is used to determine the <see cref="AnimancerState.Key"/>.
        /// </remarks>
        public ClipState CreateState(AnimationClip clip)
            => CreateState(Graph.GetKey(clip), clip);

        /// <summary>
        /// Creates and returns a new <see cref="ClipState"/> to play the `clip` and registers it with the `key`.
        /// </summary>
        public ClipState CreateState(object key, AnimationClip clip)
        {
            var state = new ClipState(clip)
            {
                _Key = key,
            };
            state.SetParent(this);
            return state;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a state registered with the `key` and attached to this layer or null if none exist.</summary>
        /// <exception cref="ArgumentNullException">The `key` is null.</exception>
        /// <remarks>
        /// If a state is registered with the `key` but on a different layer, this method will use that state as the
        /// key and try to look up another state with it. This allows it to associate multiple states with the same
        /// original key.
        /// </remarks>
        public AnimancerState GetState(ref object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            // Check through any states backwards in the key chain.
            var earlierKey = key;
            while (earlierKey is AnimancerState keyState)
            {
                if (keyState.Parent == this)// If the state is on this layer, return it.
                {
                    key = keyState.Key;
                    return keyState;
                }
                else if (keyState.Parent == null)// If the state is on no layer, attach it to this one and return it.
                {
                    key = keyState.Key;
                    keyState.SetParent(this);
                    return keyState;
                }
                else// Otherwise the state is on a different layer.
                {
                    earlierKey = keyState.Key;
                }
            }

            while (true)
            {
                // If no state is registered with the key, return null.
                if (!Graph.States.TryGet(key, out var state))
                    return null;

                if (state.Parent == this)// If the state is on this layer, return it.
                {
                    return state;
                }
                else if (state.Parent == null)// If the state is on no layer, attach it to this one and return it.
                {
                    state.SetParent(this);
                    return state;
                }
                else// Otherwise the state is on a different layer.
                {
                    // Use it as the key and try to look up the next state in a chain.
                    key = state;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="GetOrCreateState(AnimationClip, bool)"/> for each of the specified clips.
        /// <para></para>
        /// If you only want to create a single state, use <see cref="CreateState(AnimationClip)"/>.
        /// </summary>
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1)
        {
            GetOrCreateState(clip0);
            GetOrCreateState(clip1);
        }

        /// <summary>
        /// Calls <see cref="GetOrCreateState(AnimationClip, bool)"/> for each of the specified clips.
        /// <para></para>
        /// If you only want to create a single state, use <see cref="CreateState(AnimationClip)"/>.
        /// </summary>
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2)
        {
            GetOrCreateState(clip0);
            GetOrCreateState(clip1);
            GetOrCreateState(clip2);
        }

        /// <summary>
        /// Calls <see cref="GetOrCreateState(AnimationClip, bool)"/> for each of the specified clips.
        /// <para></para>
        /// If you only want to create a single state, use <see cref="CreateState(AnimationClip)"/>.
        /// </summary>
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2, AnimationClip clip3)
        {
            GetOrCreateState(clip0);
            GetOrCreateState(clip1);
            GetOrCreateState(clip2);
            GetOrCreateState(clip3);
        }

        /// <summary>
        /// Calls <see cref="GetOrCreateState(AnimationClip, bool)"/> for each of the specified clips.
        /// <para></para>
        /// If you only want to create a single state, use <see cref="CreateState(AnimationClip)"/>.
        /// </summary>
        public void CreateIfNew(params AnimationClip[] clips)
        {
            if (clips == null)
                return;

            var count = clips.Length;
            for (int i = 0; i < count; i++)
            {
                var clip = clips[i];
                if (clip != null)
                    GetOrCreateState(clip);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="AnimancerGraph.GetKey"/> and returns the state registered with that key or
        /// creates one if it doesn't exist.
        /// <para></para>
        /// If the state already exists but has the wrong <see cref="AnimancerState.Clip"/>, the `allowSetClip`
        /// parameter determines what will happen. False causes it to throw an <see cref="ArgumentException"/> while
        /// true allows it to change the <see cref="AnimancerState.Clip"/>. Note that the change is somewhat costly to
        /// performance to use with caution.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public AnimancerState GetOrCreateState(AnimationClip clip, bool allowSetClip = false)
        {
            return GetOrCreateState(Graph.GetKey(clip), clip, allowSetClip);
        }

        /// <summary>
        /// Returns the state registered with the <see cref="IHasKey.Key"/> if there is one. Otherwise
        /// this method uses <see cref="ITransition.CreateState"/> to create a new one and registers it with
        /// that key before returning it.
        /// </summary>
        public AnimancerState GetOrCreateState(ITransition transition)
        {
            var key = transition.Key;
            var state = GetState(ref key);

            if (state == null)
            {
                state = transition.CreateState();
                state._Key = key;
                state.SetParent(this);
            }

            return state;
        }

        /// <summary>Returns the state registered with the `key` or creates one if it doesn't exist.</summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException">The `key` is null.</exception>
        /// <remarks>
        /// If the state already exists but has the wrong <see cref="AnimancerState.Clip"/>, the `allowSetClip`
        /// parameter determines what will happen. False causes it to throw an <see cref="ArgumentException"/> while
        /// true allows it to change the <see cref="AnimancerState.Clip"/>. Note that the change is somewhat costly to
        /// performance to use with caution.
        /// <para></para>
        /// See also: <see cref="AnimancerStateDictionary.GetOrCreate(object, AnimationClip, bool)"/>.
        /// </remarks>
        public AnimancerState GetOrCreateState(object key, AnimationClip clip, bool allowSetClip = false)
        {
            var state = GetState(ref key);
            if (state == null)
                return CreateState(key, clip);

            // If a state exists but has the wrong clip, either change it or complain.
            if (!ReferenceEquals(state.Clip, clip))
            {
                if (allowSetClip)
                {
                    state.Clip = clip;
                }
                else
                {
                    throw new ArgumentException(
                        AnimancerStateDictionary.GetClipMismatchError(key, state.Clip, clip));
                }
            }

            return state;
        }

        /// <summary>Returns the `state` if it's a child of this layer. Otherwise makes a clone of it.</summary>
        /// <remarks>The clone starts with 0 weight, no fade, and no events.</remarks>
        public AnimancerState GetOrCreateState(AnimancerState state)
        {
            var parent = state.Parent;
            if (parent == this)
                return state;

            if (parent == null)
            {
                state.SetParent(this);
                return state;
            }

            var key = state.Key;
            key ??= state;

            var stateOnThisLayer = GetState(ref key);

            if (stateOnThisLayer == null)
            {
                stateOnThisLayer = state.Clone();
                stateOnThisLayer._Weight = 0;
                stateOnThisLayer.SharedEvents = null;
                stateOnThisLayer.CancelFade();
                stateOnThisLayer.SetParent(this);
                stateOnThisLayer.Key = key;
            }

            return stateOnThisLayer;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The maximum <see cref="AnimancerNode.Weight"/> that <see cref="GetOrCreateWeightlessState"/>
        /// will treat as being weightless. Default = 0.1.
        /// </summary>
        /// <remarks>This allows states with very small weights to be reused instead of needing to create new ones.</remarks>
        public static float WeightlessThreshold { get; set; } = 0.1f;

        /// <summary>
        /// The maximum number of duplicate states that can be created for a single clip when trying to get a
        /// weightless state. Exceeding this limit will cause it to just use the state with the lowest weight.
        /// Default = 3.
        /// </summary>
        public static int MaxCloneCount { get; set; } = 3;

        /// <summary>
        /// If the `state`'s <see cref="AnimancerNode.Weight"/> is not currently low,
        /// this method finds or creates a copy of it which is low.
        /// The returned <see cref="AnimancerState.Time"/> is also set to 0.
        /// </summary>
        /// <remarks>
        /// If this method would exceed the <see cref="MaxCloneCount"/>, it returns the clone with the lowest weight.
        /// <para></para>
        /// "Low" weight is defined as less than or equal to the <see cref="WeightlessThreshold"/>.
        /// <para></para>
        /// The <see href="https://kybernetik.com.au/animancer/docs/manual/blending/fading/modes">Fade Modes</see> page
        /// explains why clones are created.
        /// </remarks>
        public AnimancerState GetOrCreateWeightlessState(AnimancerState state)
        {
            if (state.Parent == null)
            {
                state.Weight = 0;
                goto GotState;
            }

            if (state.Parent == this &&
                state.Weight <= WeightlessThreshold)
                goto GotState;

            float lowestWeight = float.PositiveInfinity;
            AnimancerState lowestWeightState = null;

            int cloneCount = 0;

            // Use any earlier state that is weightless.
            var keyState = state;
            while (true)
            {
                keyState = keyState.Key as AnimancerState;
                if (keyState == null)
                {
                    break;
                }
                else if (keyState.Parent == this)
                {
                    if (keyState.Weight <= WeightlessThreshold)
                    {
                        state = keyState;
                        goto GotState;
                    }
                    else if (lowestWeight > keyState.Weight)
                    {
                        lowestWeight = keyState.Weight;
                        lowestWeightState = keyState;
                    }
                }
                else if (keyState.Parent == null)
                {
                    keyState.SetParent(this);
                    goto GotState;
                }

                cloneCount++;
            }

            if (state.Parent == this)
            {
                lowestWeight = state.Weight;
                lowestWeightState = state;
            }

            keyState = state;

            // If that state is not at low weight,
            // get or create another state registered using the previous state as a key.
            // Keep going through states in this manner until you find one at low weight.
            while (true)
            {
                var key = (object)state;
                if (!Graph.States.TryGet(key, out state))
                {
                    if (cloneCount >= MaxCloneCount && lowestWeightState != null)
                    {
                        state = lowestWeightState;
                        goto GotState;
                    }
                    else
                    {
#if UNITY_ASSERTIONS
                        var cloneTimer = OptionalWarning.CloneComplexState.IsEnabled() && keyState is not ClipState
                            ? SimpleTimer.Start()
                            : SimpleTimer.Default;
#endif

                        state = keyState.Clone();
                        state.SetDebugName($"[{cloneCount + 1}] {keyState}");
                        state.Weight = 0;
                        state.Key = key;
                        if (state.Parent != this)
                            state.SetParent(this);

#if UNITY_ASSERTIONS
                        if (cloneTimer.Count() > 0)
                        {
                            var milliseconds = cloneTimer.TotalTimeSeconds * 1000;
                            OptionalWarning.CloneComplexState.Log(
                                $"A {keyState.GetType().Name} was cloned in {milliseconds} milliseconds." +
                                $" This performance cost may be notable and complex states generally have parameters" +
                                $" that need to be controlled which may result in undesired behaviour if your scripts" +
                                $" are only expecting to have one state to control so you may wish to avoid cloning." +
                                $"\n\nThe Fade Modes page explains why these clones are created:" +
                                $" {Strings.DocsURLs.FadeModes}",
                                Graph?.Component);
                        }
#endif

                        goto GotState;
                    }
                }
                else if (state.Parent == this)
                {
                    if (state.Weight <= WeightlessThreshold)
                    {
                        goto GotState;
                    }
                    else if (lowestWeight > state.Weight)
                    {
                        lowestWeight = state.Weight;
                        lowestWeightState = state;
                    }
                }
                else if (state.Parent == null)
                {
                    state.SetParent(this);
                    goto GotState;
                }

                cloneCount++;
            }

            GotState:

            state.TimeD = 0;

            return state;
        }

        /************************************************************************************************************************/

        /// <summary>Destroys all states connected to this layer.</summary>
        /// <remarks>This operation cannot be undone.</remarks>
        public void DestroyStates()
        {
            CurrentState?.FadeGroup?.Cancel();

            for (int i = States.Count - 1; i >= 0; i--)
                States[i].Destroy();

            States.Clear();
            CurrentState = null;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Play Management
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected internal override void OnStartFade()
        {
            IncrementCommandCount();
        }

        /************************************************************************************************************************/
        // Play Immediately.
        /************************************************************************************************************************/

        /// <summary>Stops all other animations on this layer, plays the `clip`, and returns its state.</summary>
        /// <remarks>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(clip).Time = 0;</c>.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the `clip` was already playing.
        /// </remarks>
        public AnimancerState Play(
            AnimationClip clip)
            => Play(GetOrCreateState(clip));

        /// <summary>Stops all other animations on the same layer, plays the `state`, and returns it.</summary>
        /// <remarks>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can use <c>...Play(state).Time = 0;</c>.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the `state` was already playing.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="AnimancerNodeBase.Parent"/> is another state (likely a <see cref="ManualMixerState"/>).
        /// It must be either null or a layer.
        /// </exception>
        public AnimancerState Play(
            AnimancerState state)
        {
#if UNITY_ASSERTIONS
            AnimancerEvent.AssertEventPlayMismatch(Graph);
            AnimancerState.AssertNotExpectingFade(state);

            if (state.Parent is AnimancerState)
                throw new InvalidOperationException(
                    $"A layer can't Play a state which is the child of another state." +
                    $"\n• State: {state}" +
                    $"\n• Parent: {state.Parent}" +
                    $"\n• Layer: {this}");
#endif

            // If the layer is at 0 weight and not fading, set it to 1.
            if (Weight == 0 && FadeGroup == null)
                Weight = 1;

            CurrentState?.FadeGroup?.Cancel();

            state = GetOrCreateState(state);

            CurrentState = state;

            for (int i = ActiveStatesInternal.Count - 1; i >= 0; i--)
            {
                var otherState = ActiveStatesInternal[i];
                if (otherState != state)
                    otherState.Stop();
            }

            // Similar to state.Play but more optimized.
            state.SetIsPlaying(true);
            state._Weight = 1;
            if (!state.IsActive)
                ActiveStatesInternal.Add(state);
            _Playable.ApplyChildWeight(state);

            return state;
        }

        /************************************************************************************************************************/
        // Cross Fade.
        /************************************************************************************************************************/

        /// <summary>
        /// Starts fading in the `clip` over the course of the `fadeDuration`
        /// while fading out all others in the same layer. Returns its state.
        /// </summary>
        /// <remarks>
        /// If the `state` was already playing and fading in with less time remaining than the `fadeDuration`,
        /// this method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>,
        /// this method will fade in the layer itself and simply <see cref="AnimancerState.Play"/> the `state`.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the `state` was already playing.
        /// <para></para>
        /// <em>Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </remarks>
        public AnimancerState Play(
            AnimationClip clip,
            float fadeDuration,
            FadeMode mode = default)
        {
            var key = Graph.GetKey(clip);
            var state = GetOrCreateState(key, clip);

            if (Graph.Transitions != null)
                fadeDuration = Graph.Transitions.GetFadeDuration(this, key, fadeDuration);

            return Play(state, fadeDuration, mode);
        }

        /// <summary>
        /// Starts fading in the `state` over the course of the `fadeDuration` while fading out all others in this
        /// layer. Returns the `state`.
        /// </summary>
        /// <remarks>
        /// If the `state` was already playing and fading in with less time remaining than the `fadeDuration`, this
        /// method will allow it to complete the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will fade in the layer itself
        /// and simply <see cref="AnimancerState.Play"/> the `state`.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the `state` was already playing.
        /// <para></para>
        /// <em>Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </remarks>
        public AnimancerState Play(
            AnimancerState state,
            float fadeDuration,
            FadeMode mode = default)
        {

            // Skip the fade if:
            if (fadeDuration <= 0 ||// There is no duration.
                (Graph.SkipFirstFade && Index == 0 && Weight == 0))// Or this is Layer 0 and it has no weight.
            {
                Weight = 1;
                AnimancerState.SkipNextExpectFade();
                state = Play(state);

                if (mode == FadeMode.FromStart ||
                    mode == FadeMode.NormalizedFromStart)
                    state.TimeD = 0;

                return state;
            }

            AnimancerEvent.AssertEventPlayMismatch(Graph);

            EvaluateFadeMode(mode, ref state, fadeDuration, out var stateFadeSpeed, out var layerFadeDuration);

            // If the layer is at or fading towards 0 weight, fade it in.
            // Otherwise it's probably at 1 weight or has been explicitly set or faded to some other weight.
            if (TargetWeight == 0)
            {
                StartFade(1, layerFadeDuration);

                // If the layer has to fade in, play the state immediately.
                if (Weight == 0)
                {
                    AnimancerState.SkipNextExpectFade();
                    return Play(state);
                }
            }

            state = GetOrCreateState(state);

            CurrentState = state;

            // If the state is already playing or will finish fading in faster than this new fade,
            // continue the existing fade.
            if (IsAlreadyFadingIn(state, fadeDuration))
            {
                IncrementCommandCount();// Still pretend the fade was restarted.
            }
            else// Otherwise fade in the target state and fade out all others.
            {
                state.IsPlaying = true;

                var fade = GetFade();
                fade.SetNodes(this, state, ActiveStatesInternal, Graph.KeepChildrenConnected);
                fade.StartFade(1, stateFadeSpeed);
            }

            return state;
        }

        /************************************************************************************************************************/

        /// <summary>Is the `state` already faded in or fading in with shorter than the given `fadeDuration`?</summary>
        private static bool IsAlreadyFadingIn(
            AnimancerState state,
            float fadeDuration)
        {
            if (!state.IsPlaying)
                return false;

            var fadeGroup = state.FadeGroup;
            if (fadeGroup == null)
                return state.Weight == 1;

            return
                fadeGroup.FadeIn.Node == state &&
                fadeGroup.TargetWeight == 1 &&
                fadeGroup.RemainingFadeDuration <= fadeDuration;
        }

        /************************************************************************************************************************/

        /// <summary>Clears and reuses the existing fade if there is one. Otherwise gets one from the object pool.</summary>
        private FadeGroup GetFade()
        {
            if (CurrentState != null)
            {
                var fade = CurrentState.FadeGroup;
                if (fade != null)
                {
                    fade.FadeOutInternal.Clear();
                    fade.Easing = null;
                    return fade;
                }
            }

            return FadeGroup.Pool.Instance.Acquire();
        }

        /************************************************************************************************************************/
        // Transition.
        /************************************************************************************************************************/

        /// <summary>
        /// Creates a state for the `transition` if it didn't already exist, then calls
        /// <see cref="Play(AnimancerState)"/> or <see cref="Play(AnimancerState, float, FadeMode)"/>
        /// depending on the <see cref="ITransition.FadeDuration"/>.
        /// </summary>
        /// <remarks>
        /// This method is safe to call repeatedly without checking whether the `transition` was already playing.
        /// </remarks>
        public AnimancerState Play(
            ITransition transition)
            => Graph.Transitions != null
            ? Graph.Transitions.Play(this, transition)
            : Play(transition, transition.FadeDuration, transition.FadeMode);

        /// <summary>
        /// Creates a state for the `transition` if it didn't already exist, then calls
        /// <see cref="Play(AnimancerState)"/> or <see cref="Play(AnimancerState, float, FadeMode)"/>
        /// depending on the <see cref="ITransition.FadeDuration"/>.
        /// </summary>
        /// <remarks>
        /// This method is safe to call repeatedly without checking whether the `transition` was already playing.
        /// </remarks>
        public AnimancerState Play(
            ITransition transition,
            float fadeDuration,
            FadeMode mode = default)
        {
            var state = GetOrCreateState(transition);
            state = Play(state, fadeDuration, mode);
            transition.Apply(state);
            return state;
        }

        /************************************************************************************************************************/
        // Try Play.
        /************************************************************************************************************************/

        /// <summary>
        /// Stops all other animations on this layer,
        /// plays the animation registered with the `key`,
        /// and returns the animation's state.
        /// </summary>
        /// <remarks>
        /// If no state is registered with the `key`, this method does nothing and returns null.
        /// <para></para>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can simply set the returned state's time to 0.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the animation was already playing.
        /// </remarks>
        public AnimancerState TryPlay(
            object key)
        {
            if (Graph.Transitions != null)
            {
                var transitionState = Graph.Transitions.TryPlay(this, key);
                if (transitionState != null)
                    return transitionState;
            }

            return Graph.States.TryGet(key, out var state)
                ? Play(state)
                : null;
        }

        /// <summary>
        /// Stops all other animations on this layer,
        /// plays the animation registered with the `key`,
        /// and returns the animation's state.
        /// </summary>
        /// <remarks>
        /// If no state is registered with the `key`, this method does nothing and returns null.
        /// <para></para>
        /// The animation will continue playing from its current <see cref="AnimancerState.Time"/>.
        /// To restart it from the beginning you can simply set the returned state's time to 0.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the animation was already playing.
        /// </remarks>
        public AnimancerState TryPlay(
            IHasKey hasKey)
            => TryPlay(hasKey.Key);

        /// <summary>
        /// Starts fading in the animation registered with the `key`
        /// while fading out all others in the same layer over the course of the `fadeDuration`
        /// and returns the animation's state.
        /// </summary>
        /// <remarks>
        /// If no state is registered with the `key`, this method does nothing and returns null.
        /// <para></para>
        /// If the `state` was already playing and fading in with less time remaining than the `fadeDuration`,
        /// this method allows it to continue the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will
        /// fade in the layer itself and simply <see cref="AnimancerState.Play"/> the `state`.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the animation was already playing.
        /// <para></para>
        /// <em>Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </remarks>
        public AnimancerState TryPlay(
            object key,
            float fadeDuration,
            FadeMode mode = default)
            => Graph.States.TryGet(key, out var state)
            ? Play(state, fadeDuration, mode)
            : null;

        /// <summary>
        /// Starts fading in the animation registered with the `key`
        /// while fading out all others in the same layer over the course of the `fadeDuration`
        /// and returns the animation's state.
        /// </summary>
        /// <remarks>
        /// If no state is registered with the `key`, this method does nothing and returns null.
        /// <para></para>
        /// If the `state` was already playing and fading in with less time remaining than the `fadeDuration`,
        /// this method allows it to continue the existing fade rather than starting a slower one.
        /// <para></para>
        /// If the layer currently has 0 <see cref="AnimancerNode.Weight"/>, this method will
        /// fade in the layer itself and simply <see cref="AnimancerState.Play"/> the `state`.
        /// <para></para>
        /// This method is safe to call repeatedly without checking whether the animation was already playing.
        /// <para></para>
        /// <em>Animancer Lite only allows the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </remarks>
        public AnimancerState TryPlay(
            IHasKey hasKey,
            float fadeDuration,
            FadeMode mode = default)
            => TryPlay(hasKey.Key, fadeDuration, mode);

        /************************************************************************************************************************/

        /// <summary>Manipulates the other parameters according to the `mode`.</summary>
        /// <exception cref="ArgumentException">
        /// The <see cref="AnimancerState.Clip"/> is null when using <see cref="FadeMode.FromStart"/> or
        /// <see cref="FadeMode.NormalizedFromStart"/>.
        /// </exception>
        private void EvaluateFadeMode(
            FadeMode mode,
            ref AnimancerState state,
            float fadeDuration,
            out float stateFadeSpeed,
            out float layerFadeDuration)
        {
            layerFadeDuration = fadeDuration;

            float fadeDistance;

            switch (mode)
            {
                case FadeMode.FixedSpeed:
                    fadeDistance = 1;
                    layerFadeDuration *= Math.Abs(1 - Weight);
                    break;

                case FadeMode.FixedDuration:
                    fadeDistance = Math.Abs(1 - state.Weight);
                    break;

                case FadeMode.FromStart:
                    state = GetOrCreateWeightlessState(state);
                    fadeDistance = 1;
                    break;

                case FadeMode.NormalizedSpeed:
                    {
                        var length = state.Length;
                        fadeDistance = 1;
                        fadeDuration *= length;
                        layerFadeDuration *= Math.Abs(1 - Weight) * length;
                    }
                    break;

                case FadeMode.NormalizedDuration:
                    {
                        var length = state.Length;
                        fadeDistance = Math.Abs(1 - state.Weight);
                        fadeDuration *= length;
                        layerFadeDuration *= length;
                    }
                    break;

                case FadeMode.NormalizedFromStart:
                    {
                        state = GetOrCreateWeightlessState(state);

                        var length = state.Length;
                        fadeDistance = 1;
                        fadeDuration *= length;
                        layerFadeDuration *= length;
                    }
                    break;

                default:
                    throw AnimancerUtilities.CreateUnsupportedArgumentException(mode);
            }

            stateFadeSpeed = fadeDistance / fadeDuration;
        }

        /************************************************************************************************************************/
        // Stopping
        /************************************************************************************************************************/

        /// <summary>
        /// Sets <see cref="AnimancerNode.Weight"/> = 0 and calls <see cref="AnimancerNode.Stop"/>
        /// on all animations to stop them from playing and rewind them to the start.
        /// </summary>
        protected internal override void StopWithoutWeight()
        {
            CurrentState = null;

            for (int i = ActiveStatesInternal.Count - 1; i >= 0; i--)
                ActiveStatesInternal[i].Stop();
        }

        /************************************************************************************************************************/
        // Checking
        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if the `clip` is currently being played by at least one state.
        /// </summary>
        public bool IsPlayingClip(AnimationClip clip)
        {
            for (int i = ActiveStatesInternal.Count - 1; i >= 0; i--)
            {
                var state = ActiveStatesInternal[i];
                if (state.Clip == clip && state.IsPlaying)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if at least one animation is being played.
        /// </summary>
        public bool IsAnyStatePlaying()
        {
            for (int i = ActiveStatesInternal.Count - 1; i >= 0; i--)
                if (ActiveStatesInternal[i].IsPlaying)
                    return true;

            return false;
        }

        /// <summary>
        /// Returns true if the <see cref="CurrentState"/> is playing and hasn't yet reached its end.
        /// <para></para>
        /// This method is called by <see cref="IEnumerator.MoveNext"/> so this object can be used as a custom yield
        /// instruction to wait until it finishes.
        /// </summary>
        public override bool IsPlayingAndNotEnding()
            => _CurrentState != null && _CurrentState.IsPlayingAndNotEnding();

        /************************************************************************************************************************/

        /// <summary>
        /// Calculates the total <see cref="AnimancerNode.Weight"/> of all states in this layer.
        /// </summary>
        public float GetTotalChildWeight()
        {
            float weight = 0;

            for (int i = ActiveStatesInternal.Count - 1; i >= 0; i--)
            {
                weight += ActiveStatesInternal[i].Weight;
            }

            return weight;
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
        #region Other
        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipCollection"/>]
        /// Gathers all the animations in this layer.
        /// </summary>
        public void GatherAnimationClips(ICollection<AnimationClip> clips)
            => clips.GatherFromSource(States);

        /************************************************************************************************************************/

        /// <summary>The Inspector display name of this layer.</summary>
        public override string ToString()
        {
#if UNITY_ASSERTIONS
            if (DebugName == null)
            {
                if (_Mask != null)
                    return _Mask.GetCachedName();

                SetDebugName(Index == 0
                    ? "Base Layer"
                    : "Layer " + Index);
            }

            return base.ToString();
#else
            return "Layer " + Index;
#endif
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string separator)
        {
            base.AppendDetails(text, separator);

            text.AppendField(separator, nameof(CurrentState), CurrentState?.GetPath());
            text.AppendField(separator, nameof(CommandCount), CommandCount);
            text.AppendField(separator, nameof(IsAdditive), IsAdditive);
            text.AppendField(separator, nameof(Mask), Mask);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

