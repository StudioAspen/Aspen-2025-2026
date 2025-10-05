// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>Base class for <see cref="Playable"/> wrapper objects in an <see cref="AnimancerGraph"/>.</summary>
    /// <remarks>This is the base class of <see cref="AnimancerLayer"/> and <see cref="AnimancerState"/>.</remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerNode
    public abstract class AnimancerNode : AnimancerNodeBase,
        ICopyable<AnimancerNode>,
        IEnumerable<AnimancerState>,
        IEnumerator,
        IHasDescription
    {
        /************************************************************************************************************************/
        #region Playable
        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only] [Internal] Indicates whether the Inspector details for this node are expanded.</summary>
        internal bool _IsInspectorExpanded;
#endif

        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="Playable"/> managed by this node.</summary>
        /// <remarks>This method also applies the <see cref="AnimancerNodeBase.Speed"/> if it was set beforehand.</remarks>
        protected virtual void CreatePlayable()
        {
#if UNITY_ASSERTIONS
            if (Graph == null)
            {
                MarkAsUsed(this);
                throw new InvalidOperationException($"{nameof(AnimancerNode)}.{nameof(Graph)}" +
                    $" is null when attempting to create its {nameof(Playable)}: {this}" +
                    $"\nThe {nameof(Graph)} is generally set when you first play a state," +
                    $" so you probably just need to play it before trying to access it.");
            }

            if (_Playable.IsValid())
                Debug.LogWarning($"{nameof(AnimancerNode)}.{nameof(CreatePlayable)}" +
                    $" was called before destroying the previous {nameof(Playable)}: {this}", Graph?.Component as Object);
#endif

            CreatePlayable(out _Playable);

#if UNITY_ASSERTIONS
            if (!_Playable.IsValid())
                throw new InvalidOperationException(
                    $"{nameof(AnimancerNode)}.{nameof(CreatePlayable)}" +
                    $" did not create a valid {nameof(Playable)} for {this}");
#endif

            if (Speed != 1)
                _Playable.SetSpeed(Speed);
        }

        /// <summary>Creates and assigns the <see cref="Playable"/> managed by this node.</summary>
        protected abstract void CreatePlayable(out Playable playable);

        /************************************************************************************************************************/

        /// <summary>Destroys the <see cref="Playable"/>.</summary>
        public void DestroyPlayable()
        {
            if (_Playable.IsValid())
                Graph._PlayableGraph.DestroyPlayable(_Playable);
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="DestroyPlayable"/> and <see cref="CreatePlayable()"/>.</summary>
        public virtual void RecreatePlayable()
        {
            DestroyPlayable();
            CreatePlayable();
        }

        /// <summary>Calls <see cref="RecreatePlayable"/> on this node and all its children recursively.</summary>
        public void RecreatePlayableRecursive()
        {
            RecreatePlayable();

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i)?.RecreatePlayableRecursive();
        }

        /************************************************************************************************************************/

        /// <summary>Copies the details of `copyFrom` into this node, replacing its previous contents.</summary>
        public virtual void CopyFrom(AnimancerNode copyFrom, CloneContext context)
        {
            SetWeight(copyFrom._Weight);

            FadeGroup = context.WillCloneUpdatables
                ? null
                : copyFrom.FadeGroup?.CloneForSingleTarget(copyFrom, this);

            Speed = copyFrom.Speed;

            CopyIKFlags(copyFrom);

#if UNITY_ASSERTIONS
            DebugName = context.GetCloneOrOriginal(copyFrom.DebugName);
#endif
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Graph
        /************************************************************************************************************************/

        /// <summary>The index of the port this node is connected to on the parent's <see cref="Playable"/>.</summary>
        /// <remarks>
        /// A negative value indicates that it is not assigned to a port.
        /// <para></para>
        /// Indices are generally assigned starting from 0, ascending in the order they are connected to their layer.
        /// They will not usually change unless the <see cref="AnimancerNodeBase.Parent"/> changes or another state on
        /// the same layer is destroyed so the last state is swapped into its place to avoid shuffling everything down
        /// to cover the gap.
        /// <para></para>
        /// The setter is internal so user defined states cannot set it incorrectly. Ideally,
        /// <see cref="AnimancerLayer"/> should be able to set the port in its constructor and
        /// <see cref="AnimancerState.SetParent"/> should also be able to set it, but classes that further inherit from
        /// there should not be able to change it without properly calling that method.
        /// </remarks>
        public int Index { get; internal set; } = int.MinValue;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimancerNode"/>.</summary>
        protected AnimancerNode()
        {
#if UNITY_ASSERTIONS
            if (TraceConstructor)
                _ConstructorStackTrace = new(true);
#endif
        }

        /************************************************************************************************************************/
#if UNITY_ASSERTIONS
        /************************************************************************************************************************/

        /// <summary>[Assert-Only]
        /// Should a <see cref="System.Diagnostics.StackTrace"/> be captured in the constructor of all new nodes so
        /// <see cref="OptionalWarning.UnusedNode"/> can include it in the warning if that node ends up being unused?
        /// </summary>
        /// <remarks>This has a notable performance cost so it should only be used when trying to identify a problem.</remarks>
        public static bool TraceConstructor { get; set; }

        /************************************************************************************************************************/

        /// <summary>[Assert-Only]
        /// The stack trace of the constructor (or null if <see cref="TraceConstructor"/> was false).
        /// </summary>
        private System.Diagnostics.StackTrace _ConstructorStackTrace;

        /// <summary>[Assert-Only]
        /// Returns the stack trace of the constructor (or null if <see cref="TraceConstructor"/> was false).
        /// </summary>
        public static System.Diagnostics.StackTrace GetConstructorStackTrace(AnimancerNode node)
            => node._ConstructorStackTrace;

        /************************************************************************************************************************/

        /// <summary>[Assert-Only] Checks <see cref="OptionalWarning.UnusedNode"/>.</summary>
        ~AnimancerNode()
        {
            if (Graph != null ||
                Parent != null ||
                OptionalWarning.UnusedNode.IsDisabled())
                return;

            // ToString might throw an exception since finalizers arn't run on the main thread.
            string name = null;
            try { name = ToString(); }
            catch { name = GetType().FullName; }

            var message = $"The {nameof(Graph)} of '{name}'" +
                $" is null during finalization (garbage collection)." +
                $" This may have been caused by earlier exceptions, but otherwise it probably means" +
                $" that this node was never used for anything and should not have been created.";

            if (_ConstructorStackTrace != null)
                message += "\n\nThis node was created at:\n" + _ConstructorStackTrace;
            else
                message += $"\n\nEnable {nameof(AnimancerNode)}.{nameof(TraceConstructor)} on startup" +
                    $" to allow this warning to include the {nameof(System.Diagnostics.StackTrace)}" +
                    $" of when the node was constructed.";

            OptionalWarning.UnusedNode.Log(message);
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/

        /// <summary>Connects the `child`'s <see cref="Playable"/> to this node.</summary>
        /// <remarks>This method is NOT safe to call if the child was already connected.</remarks>
        protected internal void ConnectChildUnsafe(int index, AnimancerNode child)
        {
#if UNITY_ASSERTIONS
            if (index < 0)
            {
                MarkAsUsed(this);
                throw new InvalidOperationException(
                    $"Invalid {nameof(index)} when attempting to connect to its parent:" +
                    "\n• Child: " + child +
                    "\n• Parent: " + this);
            }

            Validate.AssertPlayable(child);
#endif

            Graph._PlayableGraph.Connect(_Playable, child._Playable, index, child._Weight);
        }

        /// <summary>Disconnects the <see cref="Playable"/> of the child at the specified `index` from this node.</summary>
        /// <remarks>This method is safe to call if the child was already disconnected.</remarks>
        protected void DisconnectChildSafe(int index)
        {
            if (_Playable.GetInput(index).IsValid())
                Graph._PlayableGraph.Disconnect(_Playable, index);
        }

        /************************************************************************************************************************/
        // IEnumerator for yielding in a coroutine to wait until animations have stopped.
        /************************************************************************************************************************/

        /// <summary>Is this node playing and not yet at its end?</summary>
        /// <remarks>
        /// This method is called by <see cref="IEnumerator.MoveNext"/> so this object can be used as a custom yield
        /// instruction to wait until it finishes.
        /// </remarks>
        public abstract bool IsPlayingAndNotEnding();

        bool IEnumerator.MoveNext()
            => IsPlayingAndNotEnding();

        object IEnumerator.Current
            => null;

        void IEnumerator.Reset() { }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Children
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected internal override AnimancerNode GetChildNode(int index)
            => GetChild(index);

        /// <summary>Returns the state connected to the specified `index` as a child of this node.</summary>
        /// <remarks>When overriding, don't call this base method because it throws an exception.</remarks>
        /// <exception cref="NotSupportedException">This node can't have children.</exception>
        public virtual AnimancerState GetChild(int index)
        {
            MarkAsUsed(this);
            throw new NotSupportedException(this + " can't have children.");
        }

        /// <summary>Called when a child is connected with this node as its <see cref="AnimancerNodeBase.Parent"/>.</summary>
        /// <remarks>When overriding, don't call this base method because it throws an exception.</remarks>
        /// <exception cref="NotSupportedException">This node can't have children.</exception>
        protected internal virtual void OnAddChild(AnimancerState state)
        {
            MarkAsUsed(this);
            state.SetParentInternal(null);
            throw new NotSupportedException(this + " can't have children.");
        }

        /************************************************************************************************************************/
        // IEnumerable for 'foreach' statements.
        /************************************************************************************************************************/

        /// <summary>Gets an enumerator for all of this node's child states.</summary>
        public virtual FastEnumerator<AnimancerState> GetEnumerator()
            => default;

        IEnumerator<AnimancerState> IEnumerable<AnimancerState>.GetEnumerator()
            => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Weight
        /************************************************************************************************************************/

        /// <summary>[Internal] The current blend weight of this node. Accessed via <see cref="Weight"/>.</summary>
        internal float _Weight;

        /************************************************************************************************************************/

        /// <summary>The current blend weight of this node which determines how much it affects the final output.</summary>
        /// 
        /// <remarks>
        /// 0 has no effect while 1 applies the full effect and values inbetween apply a proportional effect.
        /// <para></para>
        /// Setting this property cancels any fade currently in progress. If you don't wish to do that, you can use
        /// <see cref="SetWeight"/> instead.
        /// <para></para>
        /// <em>Animancer Lite only allows this value to be set to 0 or 1 in runtime builds.</em>
        /// </remarks>
        ///
        /// <example>
        /// Calling <see cref="AnimancerLayer.Play(AnimationClip)"/> immediately sets the weight of all states to 0
        /// and the new state to 1. Note that this is separate from other values like
        /// <see cref="AnimancerState.IsPlaying"/> so a state can be paused at any point and still show its pose on the
        /// character or it could be still playing at 0 weight if you want it to still trigger events (though states
        /// are normally stopped when they reach 0 weight so you would need to explicitly set it to playing again).
        /// <para></para>
        /// Calling <see cref="AnimancerLayer.Play(AnimationClip, float, FadeMode)"/> doesn't immediately change
        /// the weights, but instead calls <see cref="StartFade(float, float)"/> on every state to set their
        /// <see cref="TargetWeight"/> and <see cref="FadeSpeed"/>. Then every update each state's weight will move
        /// towards that target value at that speed.
        /// </example>
        public float Weight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _Weight;
            set
            {
                FadeGroup = null;
                SetWeight(value);
            }
        }

        /// <summary>
        /// Sets the current blend weight of this node which determines how much it affects the final output.
        /// 0 has no effect while 1 applies the full effect of this node.
        /// </summary>
        /// <remarks>
        /// This method allows any fade currently in progress to continue. If you don't wish to do that, you can set
        /// the <see cref="Weight"/> property instead.
        /// <para></para>
        /// <em>Animancer Lite only allows this value to be set to 0 or 1 in runtime builds.</em>
        /// </remarks>
        public virtual void SetWeight(float value)
            => SetWeightInternal(value);

        /// <summary>The internal non-<c>virtual</c> implementation of <see cref="SetWeight"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetWeightInternal(float value)
        {
            if (_Weight == value)
                return;

            Validate.AssertSetWeight(this, value);

            _Weight = value;

            if (Graph != null)
                Parent?.Playable.ApplyChildWeight(this);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected internal override float BaseWeight
            => Weight;

        /// <summary>
        /// The <see cref="Weight"/> of this state multiplied by the <see cref="Weight"/> of each of its parents down
        /// the hierarchy to determine how much this state affects the final output.
        /// </summary>
        public float EffectiveWeight
        {
            get
            {
                var weight = Weight;

                var parent = Parent;
                while (parent != null)
                {
                    weight *= parent.BaseWeight;
                    parent = parent.Parent;
                }

                return weight;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Fading
        /************************************************************************************************************************/

        internal FadeGroup _FadeGroup;

        /// <summary>The current fade being applied to this node (if any).</summary>
        public FadeGroup FadeGroup
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _FadeGroup;
            internal set
            {
                _FadeGroup?.Remove(this);
                _FadeGroup = value;
            }
        }

        /// <summary>
        /// The desired <see cref="Weight"/> which this node is fading towards according to the
        /// <see cref="FadeSpeed"/>.
        /// </summary>
        public float TargetWeight
            => FadeGroup != null
            ? FadeGroup.GetTargetWeight(this)
            : Weight;

        /// <summary>The speed at which this node is fading towards the <see cref="TargetWeight"/>.</summary>
        /// <remarks>
        /// This value isn't affected by this node's <see cref="AnimancerNodeBase.Speed"/>,
        /// but is affected by its parents.
        /// </remarks>
        public float FadeSpeed
            => FadeGroup != null
            ? FadeGroup.FadeSpeed
            : 0;

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="OnStartFade"/> and starts fading the <see cref="Weight"/> over the course
        /// of the <see cref="AnimancerGraph.DefaultFadeDuration"/> (in seconds).
        /// </summary>
        /// <remarks>
        /// If the `targetWeight` is 0 then <see cref="Stop"/> will be called when the fade is complete.
        /// <para></para>
        /// If the <see cref="Weight"/> is already equal to the `targetWeight` then the fade will end
        /// immediately.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartFade(float targetWeight)
            => StartFade(targetWeight, AnimancerGraph.DefaultFadeDuration);

        /// <summary>
        /// Calls <see cref="OnStartFade"/> and starts fading the <see cref="Weight"/>
        /// over the course of the `fadeDuration` (in seconds).
        /// </summary>
        /// <remarks>
        /// If the `targetWeight` is 0 then <see cref="Stop"/> will be called when the fade is complete.
        /// <para></para>
        /// If the <see cref="Weight"/> is already equal to the `targetWeight`
        /// then the fade will end immediately.
        /// <para></para>
        /// <em>Animancer Lite only allows a `targetWeight` of 0 or 1
        /// and the default `fadeDuration` (0.25 seconds) in runtime builds.</em>
        /// </remarks>
        public void StartFade(float targetWeight, float fadeDuration)
        {
            if (Weight == targetWeight && FadeGroup == null)
            {
                OnStartFade();
            }
            else if (fadeDuration > 0)
            {

                var fadeSpeed = Math.Abs(targetWeight - Weight) / fadeDuration;

                var fade = FadeGroup.Pool.Instance.Acquire();
                fade.SetFadeIn(this);
                fade.StartFade(targetWeight, fadeSpeed);
            }
            else
            {
                Weight = targetWeight;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Called by <see cref="StartFade(float, float)"/>.</summary>
        protected internal abstract void OnStartFade();

        /************************************************************************************************************************/

        /// <summary>Removes this node from the <see cref="FadeGroup"/>.</summary>
        public void CancelFade()
            => _FadeGroup?.Remove(this);

        /// <summary>[Internal] Called by <see cref="FadeGroup.Remove"/>.</summary>
        /// <remarks>Not called when a fade fully completes.</remarks>
        protected internal virtual void InternalClearFade()
        {
            _FadeGroup = null;
        }

        /************************************************************************************************************************/

        /// <summary>Stops the animation and makes it inactive immediately so it no longer affects the output.</summary>
        /// <remarks>
        /// Sets <see cref="Weight"/> = 0 by default unless overridden.
        /// <para></para>
        /// Note that playing something new will automatically stop the old animation.
        /// </remarks>
        public void Stop()
        {
            FadeGroup = null;
            SetWeightInternal(0);
            StopWithoutWeight();
        }

        /// <summary>[Internal] Stops this node without setting its <see cref="Weight"/>.</summary>
        protected internal virtual void StopWithoutWeight() { }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Inverse Kinematics
        /************************************************************************************************************************/

        /// <summary>
        /// Should setting the <see cref="AnimancerNodeBase.Parent"/>
        /// also set this node's <see cref="ApplyAnimatorIK"/> to match it?
        /// Default is true.
        /// </summary>
        public static bool ApplyParentAnimatorIK { get; set; } = true;

        /// <summary>
        /// Should setting the <see cref="AnimancerNodeBase.Parent"/>
        /// also set this node's <see cref="ApplyFootIK"/> to match it?
        /// Default is true.
        /// </summary>
        public static bool ApplyParentFootIK { get; set; } = true;

        /************************************************************************************************************************/

        /// <summary>
        /// Copies the IK settings from `copyFrom` into this node:
        /// <list type="bullet">
        /// <item>If <see cref="ApplyParentAnimatorIK"/> is true, copy <see cref="ApplyAnimatorIK"/>.</item>
        /// <item>If <see cref="ApplyParentFootIK"/> is true, copy <see cref="ApplyFootIK"/>.</item>
        /// </list>
        /// </summary>
        public virtual void CopyIKFlags(AnimancerNodeBase copyFrom)
        {
            if (Graph == null)
                return;

            if (ApplyParentAnimatorIK)
                ApplyAnimatorIK = copyFrom.ApplyAnimatorIK;

            if (ApplyParentFootIK)
                ApplyFootIK = copyFrom.ApplyFootIK;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool ApplyAnimatorIK
        {
            get
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state.ApplyAnimatorIK)
                        return true;
                }

                return false;
            }
            set
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    state.ApplyAnimatorIK = value;
                }
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool ApplyFootIK
        {
            get
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state.ApplyFootIK)
                        return true;
                }

                return false;
            }
            set
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    state.ApplyFootIK = value;
                }
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Descriptions
        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        /// <summary>[Assert-Only] The Inspector display name of this node.</summary>
        /// <remarks>Set using <see cref="SetDebugName"/>.</remarks>
        public object DebugName { get; private set; }
#endif

        /// <summary>[Assert-Conditional] Sets the <see cref="DebugName"/> to display in the Inspector.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public void SetDebugName(object name)
        {
#if UNITY_ASSERTIONS
            DebugName = name;
#endif
        }

        /// <summary>The Inspector display name of this node.</summary>
        public override string ToString()
        {
#if UNITY_ASSERTIONS
            if (NameCache.TryToString(DebugName, out var name))
                return name;
#endif

            return base.ToString();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public void AppendDescription(StringBuilder text, string separator = "\n")
        {

            text.Append(ToString());

            AppendDetails(text, separator);

            if (ChildCount > 0)
            {
                text.AppendField(separator, nameof(ChildCount), ChildCount);
                var indentedSeparator = separator + Strings.Indent;

                var i = 0;
                foreach (var child in this)
                {
                    text.Append(separator)
                        .Append('[')
                        .Append(i++)
                        .Append("] ")
                        .AppendDescription(child, indentedSeparator, true);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Called by <see cref="AppendDescription"/> to append the details of this node.</summary>
        protected virtual void AppendDetails(StringBuilder text, string separator)
        {
            text.AppendField(separator, "Playable", _Playable.IsValid()
                ? _Playable.GetPlayableType().ToString()
                : "Invalid");

            var parent = Parent;
            var isConnected =
                parent != null &&
                parent.Playable.GetInput(Index).IsValid();

            text.AppendField(separator, "Connected", isConnected);

            text.AppendField(separator, nameof(Index), Index);
            if (Index < 0)
                text.Append(" (No Parent)");

            text.AppendField(separator, nameof(Speed), Speed);

            var realSpeed = _Playable.IsValid()
                ? _Playable.GetSpeed()
                : Speed;

            if (realSpeed != Speed)
                text.Append(" (Real ").Append(realSpeed).Append(')');

            text.AppendField(separator, nameof(Weight), Weight);

            if (Weight != TargetWeight)
            {
                text.AppendField(separator, nameof(TargetWeight), TargetWeight);
                text.AppendField(separator, nameof(FadeSpeed), FadeSpeed);
            }

            AppendIKDetails(text, separator, this);

#if UNITY_ASSERTIONS
            if (_ConstructorStackTrace != null)
                text.AppendField(separator, "ConstructorStackTrace", _ConstructorStackTrace);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Appends the details of <see cref="AnimancerNodeBase.ApplyAnimatorIK"/> and
        /// <see cref="AnimancerNodeBase.ApplyFootIK"/>.
        /// </summary>
        public static void AppendIKDetails(StringBuilder text, string separator, AnimancerNodeBase node)
        {
            if (!node.Playable.IsValid())
                return;

            text.Append(separator)
                .Append("InverseKinematics: ");

            if (node.ApplyAnimatorIK)
            {
                text.Append("OnAnimatorIK");
                if (node.ApplyFootIK)
                    text.Append(", FootIK");
            }
            else if (node.ApplyFootIK)
            {
                text.Append("FootIK");
            }
            else
            {
                text.Append("None");
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns the hierarchy path of this node through its <see cref="AnimancerNodeBase.Parent"/>s.</summary>
        public string GetPath()
        {
            var path = StringBuilderPool.Instance.Acquire();

            if (Parent is AnimancerNode parent)
            {
                AppendPath(path, parent);
                AppendPortAndType(path);
            }
            else
            {
                AppendPortAndType(path);
            }

            return path.ReleaseToString();
        }

        /// <summary>Appends the hierarchy path of this state through its <see cref="AnimancerNodeBase.Parent"/>s.</summary>
        private static void AppendPath(StringBuilder path, AnimancerNode parent)
        {
            if (parent != null)
            {
                if (parent.Parent is AnimancerNode grandParent)
                {
                    AppendPath(path, grandParent);
                }
                else
                {
                    var layer = parent.Layer;
                    if (layer != null)
                    {
                        path.Append("Layers[")
                            .Append(parent.Layer.Index)
                            .Append("].States");
                    }
                    else
                    {
                        path.Append("NoLayer -> ")
                            .Append(parent.ToString());
                    }

                    return;
                }
            }

            if (parent is AnimancerState state)
            {
                state.AppendPortAndType(path);
            }
            else
            {
                path.Append(" -> ")
                    .Append(parent.GetType());
            }
        }

        /// <summary>Appends "[Index] -> ToString()".</summary>
        private void AppendPortAndType(StringBuilder path)
        {
            path.Append('[')
                .Append(Index)
                .Append("] -> ")
                .Append(ToString());
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

