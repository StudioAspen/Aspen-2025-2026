// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_ASSERTIONS
//#define ANIMANCER_ASSERT_FADE_GRAPH
#endif

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>A group of <see cref="AnimancerNode"/>s which are cross-fading.</summary>
    /// 
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/fading/custom">
    /// Custom Easing</see>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/FadeGroup
    /// 
    public partial class FadeGroup : Updatable,
        ICloneable<FadeGroup>,
        ICopyable<FadeGroup>,
        IHasDescription
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/
        // Parameters.
        /************************************************************************************************************************/

        /// <summary>The 0-1 progress of this fade.</summary>
        public float NormalizedTime { get; set; }

        /// <summary>The <see cref="AnimancerNode.Weight"/> which the <see cref="FadeIn"/> is moving towards.</summary>
        public float TargetWeight { get; set; }

        /// <summary>The speed at which the <see cref="NormalizedTime"/> increases.</summary>
        public float FadeSpeed { get; set; }

        /// <summary>The total amount of time this fade will take to complete (in seconds).</summary>
        public float FadeDuration
        {
            get => FadeSpeed != 0
                ? 1 / FadeSpeed
                : float.PositiveInfinity;
            set => FadeSpeed = value != 0
                ? 1 / value
                : float.PositiveInfinity;
        }

        /// <summary>The remaining amount of time this fade will take to complete (in seconds).</summary>
        public float RemainingFadeDuration
        {
            get => FadeSpeed != 0
                ? (1 - NormalizedTime) / FadeSpeed
                : float.PositiveInfinity;
            set => FadeSpeed = value != 0
                ? (1 - NormalizedTime) / value
                : float.PositiveInfinity;
        }

        /************************************************************************************************************************/
        // Parent.
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerNodeBase.Graph"/>.</summary>
        public AnimancerGraph Graph { get; private set; }

        /// <summary>The <see cref="AnimancerNodeBase.Graph"/>.</summary>
        public AnimancerNodeBase Parent { get; private set; }

        /// <summary>The <see cref="AnimancerNodeBase.Playable"/> of the <see cref="Parent"/>.</summary>
        public Playable ParentPlayable { get; private set; }

        /// <summary>Should the fading nodes always be connected to the <see cref="ParentPlayable"/>?</summary>
        public bool KeepChildrenConnected { get; private set; }

        /************************************************************************************************************************/
        // Nodes.
        /************************************************************************************************************************/

        /// <summary>The node which is fading towards the <see cref="TargetWeight"/>.</summary>
        public NodeWeight FadeIn { get; private set; }

        internal readonly List<NodeWeight> FadeOutInternal = new();

        /// <summary>The nodes which are fading out.</summary>
        public IReadOnlyList<NodeWeight> FadeOut => FadeOutInternal;

        /************************************************************************************************************************/
        // Custom Fade.
        /************************************************************************************************************************/

        private Func<float, float> _Easing;

        /// <summary>[Pro-Only] An optional function for modifying the fade curve.</summary>
        /// <remarks>
        /// The <see cref="NormalizedTime"/> is passed in and the return value is multiplied by the
        /// <see cref="TargetWeight"/> to set the <see cref="AnimancerNode.Weight"/> of the <see cref="FadeIn"/>.
        /// <para></para>
        /// <see cref="Animancer.Easing"/> has various common functions that could be used here.
        /// <para></para>
        /// Note that the <see cref="AnimancerNode.FadeGroup"/> may be <c>null</c>
        /// right after playing something if it was already playing, so
        /// <see cref="FadeGroupExtensions.SetEasing(FadeGroup, Easing.Function)"/>
        /// <see cref="FadeGroupExtensions.SetEasing(FadeGroup, Func{float, float})"/>
        /// can be used to avoid needing to null-check it.
        /// <para></para>
        /// <em>Animancer Lite ignores this property in runtime builds.</em>
        /// </remarks>
        public Func<float, float> Easing
        {
            get => _Easing;
            set
            {
                _Easing = value;
                AssertNormalizedBounds(value, nameof(Easing));
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Initialization
        /************************************************************************************************************************/

        /// <summary>Assigns the target nodes that will be faded.</summary>
        public void SetNodes(
            AnimancerNode parent,
            AnimancerNode fadeIn,
            IReadOnlyList<AnimancerNode> fadeOut,
            bool keepChildrenConnected)
        {
            Parent = parent;
            Graph = parent.Graph;
            ParentPlayable = parent.Playable;
            KeepChildrenConnected = keepChildrenConnected;

            FadeIn = new(fadeIn);

            if (fadeIn.FadeGroup != this)
                fadeIn.FadeGroup = this;

            var count = fadeOut.Count;
            for (int i = 0; i < count; i++)
            {
                var node = fadeOut[i];
                if (node != fadeIn)
                {
                    FadeOutInternal.Add(new(node));

                    if (node.FadeGroup != this)
                        node.FadeGroup = this;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Assigns the <see cref="FadeIn"/> with no <see cref="FadeOut"/>.</summary>
        public void SetFadeIn(AnimancerNode fadeIn)
        {
            Parent = fadeIn.Parent;
            if (Parent != null)
            {
                Graph = fadeIn.Graph;
                ParentPlayable = Parent.Playable;
                KeepChildrenConnected = Parent.KeepChildrenConnected;
            }

            FadeIn = new(fadeIn);
            fadeIn.FadeGroup = this;
        }

        /************************************************************************************************************************/

        /// <summary>Adds a node to the <see cref="FadeOut"/> list.</summary>
        public void AddFadeOut(AnimancerNode fadeOut)
        {
            FadeOutInternal.Add(new(fadeOut));
            fadeOut.FadeGroup = this;
        }

        /************************************************************************************************************************/

        /// <summary>Sets the starting values and registers this fade to be updated.</summary>
        public void StartFade(
            float targetWeight,
            float fadeSpeed)
        {
            NormalizedTime = 0;
            TargetWeight = targetWeight;
            FadeSpeed = fadeSpeed;

            StartFade();
        }

        /// <summary>Registers this fade to be updated.</summary>
        public void StartFade()
        {
            Graph?.RequirePreUpdate(this);

            FadeIn.Node?.OnStartFade();
            for (int i = FadeOutInternal.Count - 1; i >= 0; i--)
                FadeOutInternal[i].Node.OnStartFade();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Queries
        /************************************************************************************************************************/

        /// <summary>Should this fade continue?</summary>
        public bool IsValid
            => FadeSpeed > 0;

        /************************************************************************************************************************/

        /// <summary>Does this fade affect the `node`?</summary>
        public bool Contains(AnimancerNode node)
        {
            if (FadeIn.Node == node)
                return true;

            for (int i = 0; i < FadeOutInternal.Count; i++)
                if (FadeOutInternal[i].Node == node)
                    return true;

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the <see cref="TargetWeight"/> if the `node` is the <see cref="FadeIn"/>.
        /// Otherwise, returns 0.
        /// </summary>
        public float GetTargetWeight(AnimancerNode node)
        {
            return FadeIn.Node == node
                ? TargetWeight
                : 0;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Methods
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Update()
        {
            if (!IsValid)
            {
                Cancel();
                return;
            }

            AssertGraph();

            NormalizedTime += Math.Abs(AnimancerGraph.DeltaTime * Parent.EffectiveSpeed * FadeSpeed);

            if (NormalizedTime < 1)// Fade.
            {
                ApplyWeights();
            }
            else// End.
            {
                Finish();
            }
        }

        /************************************************************************************************************************/

        private void Finish()
        {
            NormalizedTime = 1;

            if (KeepChildrenConnected)
            {
                ApplyWeights(1);

                for (int i = FadeOutInternal.Count - 1; i >= 0; i--)
                    FadeOutInternal[i].Node.StopWithoutWeight();

                if (TargetWeight == 0)
                    FadeIn.Node.StopWithoutWeight();
            }
            else// Disconnect all faded out nodes and only apply the faded in weight.
            {
                for (int i = FadeOutInternal.Count - 1; i >= 0; i--)
                    StopAndDisconnect(FadeOutInternal[i].Node);

                FadeOutInternal.Clear();

                if (TargetWeight > 0)
                    ApplyWeight(FadeIn.Node, TargetWeight);
                else
                    StopAndDisconnect(FadeIn.Node);
            }

            Cancel();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Recalculates the node weights based on the <see cref="NormalizedTime"/>.
        /// </summary>
        public void ApplyWeights()
        {
            if (NormalizedTime < 1)// Fade.
            {
                var progress = NormalizedTime;
                if (_Easing != null)
                    progress = _Easing(progress);

                ApplyWeights(progress);
            }
            else// End.
            {
                Finish();
            }
        }

        private void ApplyWeights(float progress)
        {
            // Move FadeIn towards target (usually 1 or 0).

            ApplyWeight(FadeIn.Node, Mathf.LerpUnclamped(FadeIn.StartingWeight, TargetWeight, progress));

            // Move FadeOut towards 0.

            progress = 1 - progress;

            for (int i = FadeOutInternal.Count - 1; i >= 0; i--)
            {
                var node = FadeOutInternal[i];
                ApplyWeight(node.Node, node.StartingWeight * progress);
            }
        }

        private void ApplyWeight(AnimancerNode node, float weight)
        {
            node._Weight = weight;
            ParentPlayable.ApplyChildWeight(node);
        }

        private void StopAndDisconnect(AnimancerNode node)
        {
            // Don't InternalClearFade because it's virtual.
            node._FadeGroup = null;
            node.Stop();
        }

        /************************************************************************************************************************/

        private void Release()
        {
            FadeSpeed = 0;
            _Easing = null;
            Graph = null;
            Parent = null;

            if (FadeIn.Node != null)
            {
                FadeIn.Node.InternalClearFade();
                FadeIn = default;
            }

            for (int i = FadeOutInternal.Count - 1; i >= 0; i--)
                FadeOutInternal[i].Node.InternalClearFade();
            FadeOutInternal.Clear();

            Pool.Instance.Release(this);
        }

        /************************************************************************************************************************/

        /// <summary>Interrupts this fade and releases it to the <see cref="ObjectPool{T}"/>.</summary>
        public void Cancel()
        {
            Graph.CancelPreUpdate(this);
            Release();
        }

        /************************************************************************************************************************/

        /// <summary>Removes the `node` from this <see cref="FadeGroup"/> and returns true if successful.</summary>
        public bool Remove(AnimancerNode node)
        {
            if (FadeIn.Node == node)
            {
                FadeIn = default;

                if (FadeOutInternal.Count == 0)
                    FadeSpeed = 0;

                node.InternalClearFade();

                return true;
            }

            for (int i = FadeOutInternal.Count - 1; i >= 0; i--)
            {
                if (FadeOutInternal[i].Node == node)
                {
                    FadeOutInternal.RemoveAt(i);

                    if (FadeIn.Node == null && FadeOutInternal.Count == 0)
                        FadeSpeed = 0;

                    node.InternalClearFade();

                    return true;
                }
            }

            return false;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void AppendDescription(StringBuilder text, string separator = "\n")
        {
            text.Append(GetType().FullName);

            if (!IsValid)
            {
                text.Append("(Cancelled)");
                return;
            }

            if (!separator.StartsWithNewLine())
                separator = "\n" + separator;

            text.AppendField(separator, nameof(NormalizedTime), NormalizedTime);
            text.AppendField(separator, nameof(FadeSpeed), FadeSpeed);
            text.AppendField(separator, nameof(Easing), _Easing?.ToStringDetailed());

            text.Append(separator).Append($"{nameof(FadeIn)}: ");
            FadeIn.AppendDescription(text, TargetWeight);

            text.AppendField(separator, nameof(FadeOut), FadeOutInternal.Count);
            for (int i = 0; i < FadeOutInternal.Count; i++)
            {
                text.Append(separator)
                    .Append(Strings.Indent);
                FadeOutInternal[i].AppendDescription(text, 0);
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Checks <see cref="OptionalWarning.FadeEasingBounds"/>.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertNormalizedBounds(Func<float, float> easing, string name = "function")
        {
#if UNITY_ASSERTIONS
            if (easing != null && OptionalWarning.FadeEasingBounds.IsEnabled())
            {
                if (easing(0) != 0)
                    OptionalWarning.FadeEasingBounds.Log(name + "(0) != 0.");

                if (easing(1) != 1)
                    OptionalWarning.FadeEasingBounds.Log(name + "(1) != 1.");
            }
#endif
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Cloning
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual FadeGroup Clone(CloneContext context)
        {
            if (!IsValid)
                return null;

            var clone = new FadeGroup();
            clone.CopyFrom(this, context);
            return clone;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void CopyFrom(FadeGroup copyFrom, CloneContext context)
        {
            CopyNodesFrom(copyFrom, context);

            var node = FadeIn.Node;
            if (node == null)
            {
                if (FadeOut.Count == 0)
                    return;

                node = FadeOut[0].Node;
            }

            ChangeParent(node);
            CopyDetailsFrom(copyFrom);
        }

        /************************************************************************************************************************/

        private void CopyNodesFrom(FadeGroup copyFrom, CloneContext context)
        {
            FadeIn = new(copyFrom.FadeIn, context);
            FadeIn.Node.FadeGroup = this;

            FadeOutInternal.Clear();

            var count = copyFrom.FadeOutInternal.Count;
            for (int i = 0; i < count; i++)
            {
                var nodeWeight = new NodeWeight(copyFrom.FadeOutInternal[i], context);
                if (nodeWeight.Node != null)
                {
                    FadeOutInternal.Add(nodeWeight);
                    nodeWeight.Node.FadeGroup = this;
                }
            }
        }

        /************************************************************************************************************************/

        internal void ChangeParent(AnimancerNode child)
        {
            var parent = child.Parent;
            if (Parent == parent)
                return;

            Parent = parent;
            if (Parent != null)
            {
                ParentPlayable = Parent.Playable;
                KeepChildrenConnected = Parent.KeepChildrenConnected;

                ChangeGraph(child.Graph);

                _AssertGraphNextFrame = true;
            }
        }

        /************************************************************************************************************************/

        internal void ChangeGraph(AnimancerGraph graph)
        {
            if (Graph == graph)
                return;

            Graph?.CancelPreUpdate(this);
            Graph = graph;
            Graph?.RequirePreUpdate(this);

            _AssertGraphNextFrame = true;
        }

        /************************************************************************************************************************/

        private bool _AssertGraphNextFrame;

        private void AssertGraph()
        {
            if (!_AssertGraphNextFrame)
                return;

            _AssertGraphNextFrame = false;

            if (FadeIn.Node != null && !AssertNode(FadeIn.Node))
                return;

            for (int i = 0; i < FadeOutInternal.Count; i++)
                if (!AssertNode(FadeOutInternal[i].Node))
                    return;
        }

        private bool AssertNode(AnimancerNode node)
        {
            string propertyName;
            string nodeValue, myValue;
            if (node.Graph == Graph)
            {
                if (node.Parent == Parent)
                    return true;

                propertyName = nameof(node.Parent);
                nodeValue = AnimancerUtilities.ToStringOrNull(node.Parent);
                myValue = AnimancerUtilities.ToStringOrNull(Parent);
            }
            else
            {
                propertyName = nameof(node.Graph);
                nodeValue = AnimancerUtilities.ToStringOrNull(node.Graph);
                myValue = AnimancerUtilities.ToStringOrNull(Graph);
            }

            var graph = Graph ?? node.Graph;
            Debug.LogWarning(
                $"{nameof(AnimancerNode)}.{propertyName} doesn't match {nameof(FadeGroup)}.{propertyName}." +
                $"\n• Node: {node.GetPath()}" +
                $"\n• Node.{propertyName}: {nodeValue}" +
                $"\n• This.{propertyName}: {myValue}" +
                $"\n• Graph: {graph?.GetDescription("\n• ")}");

            return false;
        }

        /************************************************************************************************************************/

        private void CopyDetailsFrom(FadeGroup copyFrom)
        {
            NormalizedTime = copyFrom.NormalizedTime;
            FadeSpeed = copyFrom.FadeSpeed;
            TargetWeight = copyFrom.TargetWeight;
            _Easing = copyFrom._Easing;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a clone of this <see cref="FadeGroup"/> for a single target node (`copyTo`).</summary>
        public FadeGroup CloneForSingleTarget(AnimancerNode copyFrom, AnimancerNode copyTo)
        {
            if (!IsValid)
                return null;

            var clone = Pool.Instance.Acquire();

            if (copyFrom == FadeIn.Node)
            {
                clone.FadeIn = new(copyTo, FadeIn.StartingWeight);
            }
            else
            {
                for (int i = 0; i < FadeOutInternal.Count; i++)
                {
                    var fadeOut = FadeOutInternal[i];
                    if (fadeOut.Node == copyFrom)
                    {
                        clone.FadeOutInternal.Add(new(copyTo, fadeOut.StartingWeight));
                        goto CopyDetails;
                    }
                }

                return null;
            }

            CopyDetails:
            clone.ChangeParent(copyTo);
            clone.CopyDetailsFrom(this);
            clone.StartFade();

            return clone;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Pooling
        /************************************************************************************************************************/

        /// <summary>An <see cref="ObjectPool{T}"/> for <see cref="FadeGroup"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/Pool
        public class Pool : ObjectPool<FadeGroup>
        {
            /************************************************************************************************************************/

            /// <summary>Singleton.</summary>
            public static Pool Instance = new();

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override FadeGroup New()
                => new();

            /************************************************************************************************************************/
#if UNITY_ASSERTIONS
            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override FadeGroup Acquire()
            {
                var fade = base.Acquire();
                Debug.Assert(fade.FadeIn.Node == null, $"{nameof(fade.FadeIn)} is not null");
                Debug.Assert(fade.FadeOutInternal.Count == 0, $"{nameof(fade.FadeOutInternal)} is not empty");
                Debug.Assert(fade.Easing == null, $"{nameof(fade.Easing)} is not null");
                return fade;
            }

            /// <inheritdoc/>
            public override void Release(FadeGroup item)
            {
                Debug.Assert(((IUpdatable)item).UpdatableIndex < 0,
                    $"Releasing {nameof(FadeGroup)} which is still registered for updates.",
                    item.Graph?.Component as Object);

                base.Release(item);
            }

            /************************************************************************************************************************/
#endif
            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

