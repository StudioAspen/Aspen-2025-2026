// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>Base class for objects that manage a <see cref="UnityEngine.Playables.Playable"/>.</summary>
    /// <remarks>This is the base class of <see cref="AnimancerGraph"/> and <see cref="AnimancerNode"/>.</remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerNodeBase
    public abstract class AnimancerNodeBase
    {
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerGraph"/> containing this node.</summary>
        public AnimancerGraph Graph { get; internal set; }

        /************************************************************************************************************************/

        /// <summary>The object which receives the output of the <see cref="Playable"/>.</summary>
        /// <remarks>
        /// This leads from <see cref="AnimancerState"/> to <see cref="AnimancerLayer"/> to
        /// <see cref="AnimancerGraph"/> to <c>null</c>.
        /// </remarks>
        public AnimancerNodeBase Parent { get; protected set; }

        /************************************************************************************************************************/

        /// <summary>The root <see cref="AnimancerLayer"/> which this node is connected to (if any).</summary>
        public virtual AnimancerLayer Layer
            => Parent?.Layer;

        /************************************************************************************************************************/

        /// <summary>The number of nodes using this as their <see cref="Parent"/>.</summary>
        public virtual int ChildCount
            => 0;

        /// <summary>Returns the node connected to the specified `index` as a child of this node.</summary>
        /// <remarks>When overriding, don't call this base method because it throws an exception.</remarks>
        /// <exception cref="NotSupportedException">This node can't have children.</exception>
        protected internal virtual AnimancerNode GetChildNode(int index)
        {
            MarkAsUsed(this);
            throw new NotSupportedException(this + " can't have children.");
        }

        /// <summary>Should child playables stay connected to the graph at all times?</summary>
        /// <remarks>
        /// If false, playables will be disconnected from the graph while they are inactive to stop it from
        /// evaluating them every frame which usually improves performance.
        /// </remarks>
        /// <seealso cref="AnimancerGraph.KeepChildrenConnected"/>
        public virtual bool KeepChildrenConnected
            => true;

        /************************************************************************************************************************/

        /// <summary>Called when a child's <see cref="AnimancerState.IsLooping"/> value changes.</summary>
        protected virtual void OnChildIsLoopingChanged(bool value) { }

        /// <summary>[Internal] Calls <see cref="OnChildIsLoopingChanged"/> for each <see cref="Parent"/> recursively.</summary>
        protected internal void OnIsLoopingChangedRecursive(bool value)
        {
            var parent = Parent;

            while (parent != null)
            {
                parent.OnChildIsLoopingChanged(value);

                parent = parent.Parent;
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Internal] Called when a child's <see cref="Parent"/> is changed from this node.</summary>
        /// <remarks>When overriding, don't call this base method because it throws an exception.</remarks>
        /// <exception cref="NotSupportedException">This node can't have children.</exception>
        protected internal virtual void OnRemoveChild(AnimancerState state)
        {
            MarkAsUsed(this);
            state.SetParentInternal(null);
            throw new NotSupportedException(this + " can't have children.");
        }

        /************************************************************************************************************************/

        /// <summary>[Internal] The <see cref="Playable"/>.</summary>
        protected internal Playable _Playable;

        /// <summary>The internal object this node manages in the <see cref="PlayableGraph"/>.</summary>
        /// <remarks>
        /// Must be set by <see cref="AnimancerNode.CreatePlayable()"/>. Failure to do so will throw the following
        /// exception throughout the system when using this node: "<see cref="ArgumentException"/>: The playable passed
        /// as an argument is invalid. To create a valid playable, please use the appropriate Create method".
        /// </remarks>
        public Playable Playable => _Playable;

        /************************************************************************************************************************/

        /// <summary>The current blend weight of this node which determines how much it affects the final output.</summary>
        protected internal virtual float BaseWeight => 1;

        /************************************************************************************************************************/
        #region Speed
        /************************************************************************************************************************/

        private float _Speed = 1;

        /// <summary>[Pro-Only] How fast the <see cref="AnimancerState.Time"/> is advancing every frame (default 1).</summary>
        /// 
        /// <remarks>
        /// A negative value will play the animation backwards.
        /// <para></para>
        /// To pause an animation, consider setting <see cref="AnimancerState.IsPlaying"/> to false instead of setting
        /// this value to 0.
        /// <para></para>
        /// <em>Animancer Lite doesn't allow this value to be changed in runtime builds.</em>
        /// <para></para>
        /// <strong>Example:</strong><code>
        /// void SpeedExample(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.Play(clip);
        ///
        ///     state.Speed = 1;// Normal speed.
        ///     state.Speed = 2;// Double speed.
        ///     state.Speed = 0.5f;// Half speed.
        ///     state.Speed = -1;// Normal speed playing backwards.
        ///     state.NormalizedTime = 1;// Start at the end to play backwards from there.
        /// }
        /// </code></remarks>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">The value is not finite.</exception>
        public virtual float Speed
        {
            get => _Speed;
            set
            {
                if (_Speed == value)
                    return;

#if UNITY_ASSERTIONS
                if (!value.IsFinite())
                {
                    MarkAsUsed(this);
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        $"{nameof(Speed)} {Strings.MustBeFinite}");
                }
#endif
                _Speed = value;

                if (_Playable.IsValid())
                    _Playable.SetSpeed(value);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="Speed"/> of this node multiplied by the <see cref="Speed"/> of each of its parents to
        /// determine the actual speed it's playing at.
        /// </summary>
        public float EffectiveSpeed
        {
            get => Speed * ParentEffectiveSpeed;
            set => Speed = value / ParentEffectiveSpeed;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The multiplied <see cref="Speed"/> of each of the <see cref="Parent"/> down the hierarchy,
        /// excluding the root <see cref="Speed"/>.
        /// </summary>
        private float ParentEffectiveSpeed
        {
            get
            {
                var parent = Parent;
                if (parent == null)
                    return 1;

                var speed = parent.Speed;

                while ((parent = parent.Parent) != null)
                {
                    speed *= parent.Speed;
                }

                return speed;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <summary>
        /// Should Unity call <c>OnAnimatorIK</c> on the animated object while this object and its children have any
        /// <see cref="AnimancerNode.Weight"/>?
        /// </summary>
        /// <remarks>
        /// This is equivalent to the "IK Pass" toggle in Animator Controller layers, except that due to limitations in
        /// the Playables API the <c>layerIndex</c> will always be zero.
        /// <para></para>
        /// This value starts false by default, but can be automatically changed by
        /// <see cref="AnimancerNode.CopyIKFlags"/> when the <see cref="Parent"/> is set.
        /// <para></para>
        /// IK only takes effect while at least one <see cref="ClipState"/> has a <see cref="AnimancerNode.Weight"/>
        /// above zero. Other node types either store the value to apply to their children or don't support IK.
        /// <para></para>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/ik#ik-pass">
        /// IK Pass</see>
        /// </remarks>
        public abstract bool ApplyAnimatorIK { get; set; }

        /************************************************************************************************************************/

        /// <summary>Should this object and its children apply IK to the character's feet?</summary>
        /// <remarks>
        /// This is equivalent to the "Foot IK" toggle in Animator Controller states.
        /// <para></para>
        /// This value starts true by default for <see cref="ClipState"/>s (false for others), but can be automatically
        /// changed by <see cref="AnimancerNode.CopyIKFlags"/> when the <see cref="Parent"/> is set.
        /// <para></para>
        /// IK only takes effect while at least one <see cref="ClipState"/> has a <see cref="AnimancerNode.Weight"/>
        /// above zero. Other node types either store the value to apply to their children or don't support IK.
        /// <para></para>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/ik#foot-ik">
        /// Foot IK</see>
        /// </remarks>
        public abstract bool ApplyFootIK { get; set; }

        /************************************************************************************************************************/

        /// <summary>[Internal] Applies a change to a child's <see cref="AnimancerState.IsActive"/>.</summary>
        protected internal virtual void ApplyChildActive(AnimancerState child, bool setActive)
            => child.ShouldBeActive = setActive;

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Prevents the `node` from causing <see cref="OptionalWarning.UnusedNode"/>.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void MarkAsUsed(AnimancerNodeBase node)
        {
#if UNITY_ASSERTIONS
            if (node.Graph == null)
                GC.SuppressFinalize(node);
#endif
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Adds functions to show and set <see cref="ApplyAnimatorIK"/> and
        /// <see cref="ApplyFootIK"/>.
        /// </summary>
        public static void AddContextMenuIK(UnityEditor.GenericMenu menu, AnimancerNodeBase ik)
        {
#if UNITY_IMGUI
            menu.AddItem(new("Inverse Kinematics/Apply Animator IK ?"),
                ik.ApplyAnimatorIK,
                () => ik.ApplyAnimatorIK = !ik.ApplyAnimatorIK);
            menu.AddItem(new("Inverse Kinematics/Apply Foot IK ?"),
                ik.ApplyFootIK,
                () => ik.ApplyFootIK = !ik.ApplyFootIK);
#endif
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}

