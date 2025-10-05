// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>
    /// Enforces various rules throughout the system, most of which are compiled out if UNITY_ASSERTIONS is not defined
    /// (by default, it is only defined in the Unity Editor and in Development Builds).
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/Validate
    /// 
    public static partial class Validate
    {
        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional]
        /// Throws if the `clip` is <c>null</c>, not an asset, or marked as <see cref="AnimationClip.legacy"/>.
        /// </summary>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="ArgumentException"/>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertAnimationClip(AnimationClip clip, bool throwIfNull, string operation)
        {
#if UNITY_ASSERTIONS
            if (clip == null)
            {
                if (!throwIfNull)
                    return;

#pragma warning disable IDE0041 // Use 'is null' check (that would suggest changing to == which is wrong).
                var error = ReferenceEquals(clip, null)
                    ? $"Unable to {operation} because the {nameof(AnimationClip)} is null."
                    : $"Unable to {operation} because the {nameof(AnimationClip)} has been destroyed.";

                throw new NullReferenceException(error);
#pragma warning restore IDE0041 // Use 'is null' check.
            }

#if UNITY_EDITOR
            if (OptionalWarning.DynamicAnimation.IsEnabled() &&
                !UnityEditor.EditorUtility.IsPersistent(clip))
                OptionalWarning.DynamicAnimation.Log(
                    $"Attempted to {operation} using an {nameof(AnimationClip)} '{clip.name}' which is not an asset." +
                    " Unity doesn't support dynamically creating animations for Animancer in runtime builds." +
                    " This warning should be disabled if you're loading animations from Asset Bundles or Addressables" +
                    " or if you only intend to use the animation in the Unity Editor.",
                    clip);
#endif

            if (clip.legacy)
                throw new ArgumentException(
                    $"Unable to {operation} because the {nameof(AnimationClip)} '{clip.name}' is a lagacy animation" +
                    " and therefore cannot be used by Animancer" +
                    " If it was imported as part of a model then the model's Rig type must be Humanoid or Generic." +
                    " Otherwise you can use the 'Toggle Legacy' function in the clip's context menu" +
                    " (via the cog icon in the top right of its Inspector).");
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Throws if the <see cref="AnimancerNodeBase.Graph"/> is not the `graph`.</summary>
        /// <exception cref="ArgumentException"/>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertGraph(AnimancerNode node, AnimancerGraph graph)
        {
#if UNITY_ASSERTIONS
            if (node.Graph != graph)
            {
                AnimancerNodeBase.MarkAsUsed(node);

                throw new ArgumentException(
                    $"{nameof(AnimancerNode)}.{nameof(AnimancerNode.Graph)} mismatch:" +
                    $" cannot use a node in an {nameof(AnimancerGraph)} that is not its {nameof(AnimancerNode.Graph)}: " +
                    node.GetDescription());
            }
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Throws if the `node`'s <see cref="Playable"/> is invalid.</summary>
        /// <exception cref="InvalidOperationException"/>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertPlayable(AnimancerNode node)
        {
#if UNITY_ASSERTIONS
            if (node._Playable.IsValid() &&
                node.Graph._PlayableGraph.IsValid())
                return;

            var description = node.ToString();

            var stackTrace = AnimancerNode.GetConstructorStackTrace(node);
            if (stackTrace != null)
                description += "\n\n" + stackTrace;

            AnimancerNodeBase.MarkAsUsed(node);

            if (node is AnimancerState state)
                state.Destroy();

            if (node.Graph == null)
                throw new InvalidOperationException(
                    $"{nameof(AnimancerNode)}.{nameof(AnimancerNode.Graph)} hasn't been set so its" +
                    $" {nameof(Playable)} hasn't been created. It can be set by playing the state" +
                    $" or calling {nameof(AnimancerState.SetGraph)} on it directly." +
                    $" {nameof(AnimancerState.SetParent)} would also work if the parent has a" +
                    $" {nameof(AnimancerNode.Graph)}." +
                    $"\n• Node: {description}");
            else if (!node.Graph._PlayableGraph.IsValid())
                throw new InvalidOperationException(
                    $"{nameof(AnimancerGraph)}.{nameof(AnimancerGraph.PlayableGraph)} has already been destroyed." +
                    $" This is often caused by a character attempting to access a state on a different character," +
                    $" such as if they share a Transition and are both accessing its State without realising it" +
                    $" only holds the most recently played state." +
                    $"\n• Graph: {node.Graph}" +
                    $"\n• Node: {description}");
            else
                throw new InvalidOperationException(
                    $"{nameof(AnimancerNode)}.{nameof(AnimancerNodeBase.Playable)}" +
                    $" has either been destroyed or was never created." +
                    $"\n• Graph: {node.Graph}" +
                    $"\n• Node: {description}");
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional]
        /// Throws if the `state` was not actually assigned to its specified <see cref="AnimancerNode.Index"/> in
        /// the `states`.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="IndexOutOfRangeException">
        /// The <see cref="AnimancerNode.Index"/> is larger than the number of `states`.
        /// </exception>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertCanRemoveChild(AnimancerState state, IList<AnimancerState> childStates, int childCount)
        {
#if UNITY_ASSERTIONS
            var index = state.Index;

            if (index < 0)
                throw new InvalidOperationException(
                    $"Cannot remove a child state that did not have an {nameof(state.Index)} assigned");

            if ((uint)index >= (uint)childCount)
                throw new IndexOutOfRangeException(
                    $"{nameof(AnimancerState)}.{nameof(state.Index)} ({index})" +
                    $" is outside the collection of states (Count {childCount})");

            if (childStates[index] != state)
                throw new InvalidOperationException(
                    $"Cannot remove a child state that was not actually connected to its port on {state.Parent}:" +
                    $"\n• Port: {index}" +
                    $"\n• Connected Child: {AnimancerUtilities.ToStringOrNull(childStates[index])}" +
                    $"\n• Disconnecting Child: {AnimancerUtilities.ToStringOrNull(state)}");
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Throws if the `weight` is negative, infinity, or NaN.</summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertSetWeight(AnimancerNode node, float weight)
        {
#if UNITY_ASSERTIONS
            if (!(weight >= 0) || weight == float.PositiveInfinity)// Reversed comparison includes NaN.
            {
                AnimancerNodeBase.MarkAsUsed(node);
                throw new ArgumentOutOfRangeException(
                    nameof(weight),
                    weight,
                    $"{nameof(AnimancerNode.Weight)} must be a finite positive value");
            }
#endif
        }

        /************************************************************************************************************************/
    }
}

