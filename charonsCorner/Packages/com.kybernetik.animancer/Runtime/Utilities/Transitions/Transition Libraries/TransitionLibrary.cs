// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer.TransitionLibraries
{
    /// <summary>[Pro-Only]
    /// A library of <see cref="ITransition"/>s which allows specific
    /// transition combinations to be overridden without needing to be hard coded.
    /// </summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/libraries">
    /// Transition Libraries</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.TransitionLibraries/TransitionLibrary
    public class TransitionLibrary :
        IAnimationClipCollection,
        ICopyable<TransitionLibrary>
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        /// <summary>[Pro-Only] Modifiers in the order they are created.</summary>
        /// <remarks>The <see cref="TransitionModifierGroup.Index"/> of each item corresponds to its position in this list.</remarks>
        private readonly List<TransitionModifierGroup>
            TransitionModifiers = new();

        /// <summary>[Pro-Only] Modifiers registered by their <see cref="IHasKey.Key"/> as well as any custom aliases.</summary>
        private readonly Dictionary<object, TransitionModifierGroup>
            KeyedTransitionModifiers = new();

        /************************************************************************************************************************/

        /// <summary>[Pro-Only] The number of transitions in this library.</summary>
        public int Count
            => TransitionModifiers.Count;

        /// <summary>[Pro-Only] The number of transitions in this library plus any additional aliases.</summary>
        public int AliasCount
            => KeyedTransitionModifiers.Count;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Queries
        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Does this library contain a transition registered with the `key`?
        /// </summary>
        public bool ContainsKey(object key)
            => KeyedTransitionModifiers.ContainsKey(key);

        /// <summary>[Pro-Only]
        /// Does this library contain a transition registered with the <see cref="IHasKey.Key"/>?
        /// </summary>
        public bool ContainsKey(IHasKey hasKey)
            => ContainsKey(hasKey.Key);

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Tries to find a <see cref="TransitionModifierGroup"/> registered with the `key`.
        /// </summary>
        public bool TryGetTransition(object key, out TransitionModifierGroup transition)
        {
            if (KeyedTransitionModifiers.TryGetValue(key, out transition))
                return true;

            if (key is AnimancerState state)
            {
                key = AnimancerUtilities.GetRootKey(state.Key);

                return
                    key != null &&
                    KeyedTransitionModifiers.TryGetValue(key, out transition);
            }

            AssertStringReference(key);
            return false;
        }

        /// <summary>[Pro-Only]
        /// Tries to find a <see cref="TransitionModifierGroup"/> registered with the <see cref="IHasKey.Key"/>.
        /// </summary>
        public bool TryGetTransition(IHasKey hasKey, out TransitionModifierGroup transition)
            => TryGetTransition(hasKey.Key, out transition);

        /// <summary>[Pro-Only]
        /// Tries to find a <see cref="TransitionModifierGroup"/>
        /// via its <see cref="TransitionModifierGroup.Index"/>.
        /// </summary>
        public bool TryGetTransition(int index, out TransitionModifierGroup transition)
            => TransitionModifiers.TryGet(index, out transition)
            && transition != null;

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Finds the <see cref="TransitionModifierGroup.Index"/> of the group registered with the `key`
        /// or returns <c>-1</c>.
        /// </summary>
        public int IndexOf(object key)
            => TryGetTransition(key, out var group)
            ? group.Index
            : -1;

        /// <summary>[Pro-Only]
        /// Finds the <see cref="TransitionModifierGroup.Index"/> of the group registered with the `key`
        /// or returns <c>-1</c>.
        /// </summary>
        public int IndexOf(IHasKey hasKey)
            => IndexOf(hasKey.Key);

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Returns the fade duration to use when transitioning from `from` to the `transition`.
        /// </summary>
        public float GetFadeDuration(
            object from,
            ITransition to)
        {
            if (from != null &&
                TryGetTransition(to.Key, out var group))
                return group.GetFadeDuration(from);

            return to.FadeDuration;
        }

        /// <summary>[Pro-Only]
        /// Returns the fade duration to use when transitioning from `from` to the `transition`.
        /// </summary>
        public float GetFadeDuration(
            IHasKey from,
            ITransition to)
            => GetFadeDuration(from?.Key, to);

        /// <summary>[Pro-Only]
        /// Returns the fade duration to use when transitioning from the
        /// <see cref="AnimancerLayer.CurrentState"/> to the `transition`.
        /// </summary>
        public float GetFadeDuration(
            AnimancerLayer layer,
            ITransition transition)
            => GetFadeDuration(layer.CurrentState?.Key, transition);

        /// <summary>[Pro-Only]
        /// Returns the fade duration to use when transitioning from the
        /// <see cref="AnimancerLayer.CurrentState"/> to the `key`.
        /// </summary>
        public float GetFadeDuration(
            AnimancerLayer layer,
            object key,
            float fadeDuration)
        {
            AssertStringReference(key);

            var from = layer.CurrentState?.Key;
            if (from != null &&
                TryGetTransition(key, out var group))
                return group.GetFadeDuration(from);

            return fadeDuration;
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only] Gathers all the animations in this library.</summary>
        public virtual void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            for (int i = TransitionModifiers.Count - 1; i >= 0; i--)
                clips.GatherFromSource(TransitionModifiers[i].Transition);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Add
        /************************************************************************************************************************/

        /// <summary>[Pro-Only] Adds the contents of the `definition` to this library.</summary>
        /// <remarks>Existing values will be completely replaced.</remarks>
        public void Initialize(TransitionLibraryDefinition definition)
        {
            Clear();

            if (definition == null)
                return;

            var count = definition.Transitions.Length;

            if (TransitionModifiers.Capacity < count)
            {
                var capacity = Math.Max(count, 16);
                TransitionModifiers.Capacity = capacity;
                KeyedTransitionModifiers.EnsureCapacity(capacity);
            }

            for (int i = 0; i < count; i++)
            {
                var transition = definition.Transitions[i];
                if (transition != null)
                    SetTransition(transition);
            }

            for (int i = 0; i < definition.Modifiers.Length; i++)
                SetFadeDuration(definition.Modifiers[i]);

            if (definition.AliasAllTransitions)
            {
                for (int i = 0; i < count; i++)
                {
                    var transition = definition.Transitions[i];
                    var modifier = TransitionModifiers[i];
                    KeyedTransitionModifiers[StringReference.Get(transition.name)] = modifier;
                }
            }

            for (int i = 0; i < definition.Aliases.Length; i++)
            {
                var alias = definition.Aliases[i];
                if (alias.Name != null &&
                    TransitionModifiers.TryGet(alias.Index, out var group))
                    KeyedTransitionModifiers[alias.Name.Name] = group;
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only] Adds the `transition` to this library.</summary>
        /// <exception cref="ArgumentException">A transition is already registered with the `key`.</exception>
        public TransitionModifierGroup AddTransition(
            object key,
            ITransition transition)
        {
            AssertStringReference(key);

            var modifier = new TransitionModifierGroup(TransitionModifiers.Count, transition);
            KeyedTransitionModifiers.Add(key, modifier);
            TransitionModifiers.Add(modifier);
            return modifier;
        }

        /// <summary>[Pro-Only] Adds the `transition` to this library.</summary>
        /// <exception cref="ArgumentException">A transition is already registered with the `key`.</exception>
        public TransitionModifierGroup AddTransition(
            IHasKey hasKey,
            ITransition transition)
            => AddTransition(hasKey.Key, transition);

        /// <summary>[Pro-Only] Adds the `transition` to this library.</summary>
        /// <exception cref="ArgumentException">A transition is already registered with the `key`.</exception>
        public TransitionModifierGroup AddTransition(
            ITransition transition)
            => AddTransition(transition, transition);

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Adds the `transition` to this library or replaces the existing one registered with the `key`.
        /// </summary>
        public TransitionModifierGroup SetTransition(
            object key,
            ITransition transition)
        {
            if (TryGetTransition(key, out var oldModifier))
            {
                oldModifier.Transition = transition;
                return oldModifier;
            }

            return AddTransition(key, transition);
        }

        /// <summary>[Pro-Only]
        /// Adds the `transition` to this library or replaces the existing one registered with the `key`.
        /// </summary>
        public TransitionModifierGroup SetTransition(
            IHasKey hasKey,
            ITransition transition)
            => SetTransition(hasKey.Key, transition);

        /// <summary>[Pro-Only]
        /// Adds the `transition` to this library or replaces the existing one registered with the `key`.
        /// </summary>
        public TransitionModifierGroup SetTransition(
            ITransition transition)
            => SetTransition(transition, transition);

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Sets the <see cref="ITransition.FadeDuration"/> to use when transitioning from `from` to `to`.
        /// </summary>
        public void SetFadeDuration(
            object from,
            ITransition to,
            float fadeDuration)
        {
            var group = SetTransition(to.Key, to);
            group.SetFadeDuration(
                from,
                fadeDuration);
        }

        /// <summary>[Pro-Only]
        /// Sets the <see cref="ITransition.FadeDuration"/> to use when transitioning from `from` to `to`.
        /// </summary>
        public void SetFadeDuration(
            IHasKey from,
            ITransition to,
            float fadeDuration)
            => SetFadeDuration(from.Key, to, fadeDuration);

        /// <summary>[Pro-Only]
        /// Sets the <see cref="ITransition.FadeDuration"/> to use when transitioning from
        /// <see cref="TransitionModifierDefinition.FromIndex"/> to <see cref="TransitionModifierDefinition.ToIndex"/>.
        /// </summary>
        public bool SetFadeDuration(
            TransitionModifierDefinition modifier)
        {
            if (!TransitionModifiers.TryGet(modifier.FromIndex, out var from) ||
                !TransitionModifiers.TryGet(modifier.ToIndex, out var to))
                return false;

            to.SetFadeDuration(
                from.Transition.Key,
                modifier.FadeDuration);
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only] Registers the `group` with another `key`.</summary>
        public void AddAlias(
            object key,
            TransitionModifierGroup group)
        {
            AssertStringReference(key);
            AssertGroup(group);
            KeyedTransitionModifiers.Add(key, group);
        }

        /// <summary>[Pro-Only] Registers the `transition` with the `key`.</summary>
        /// <remarks>Also registers it with its <see cref="IHasKey.Key"/> if it wasn't already.</remarks>
        public TransitionModifierGroup AddAlias(
            object key,
            ITransition transition)
        {
            var group = SetTransition(transition);
            AddAlias(key, group);
            return group;
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only] Adds the contents of `copyFrom` into this library.</summary>
        /// <remarks>
        /// This method adds and replaces values, but does not remove any
        /// (unlike <see cref="CopyFrom(TransitionLibrary, CloneContext)"/>.
        /// </remarks>
        public void AddLibrary(TransitionLibrary library, CloneContext context)
        {
            if (library == null)
                return;

            for (int i = 0; i < TransitionModifiers.Count; i++)
            {
                var group = TransitionModifiers[i];
                context[group.Transition] = group;
            }

            foreach (var group in library.KeyedTransitionModifiers)
            {
                var transition = group.Value.Transition;

                if (context.TryGetClone(transition, out var clone) &&
                    clone is TransitionModifierGroup cloneGroup)
                {
                    AssertGroup(cloneGroup);
                    KeyedTransitionModifiers[group.Key] = cloneGroup;
                }
                else
                {
                    cloneGroup = SetTransition(group.Key, group.Value.Transition);
                    cloneGroup.CopyFrom(group.Value);
                    context[transition] = cloneGroup;
                }
            }
        }

        /// <summary>[Pro-Only] Adds the contents of `copyFrom` into this library.</summary>
        /// <remarks>
        /// This method adds and replaces values, but does not remove any
        /// (unlike <see cref="CopyFrom(TransitionLibrary, CloneContext)"/>.
        /// </remarks>
        public void AddLibrary(TransitionLibrary library)
        {
            var context = CloneContext.Pool.Instance.Acquire();
            AddLibrary(library, context);
            CloneContext.Pool.Instance.Release(context);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        /// <remarks>See also <see cref="AddLibrary(TransitionLibrary, CloneContext)"/>.</remarks>
        public void CopyFrom(TransitionLibrary copyFrom, CloneContext context)
        {
            Clear();

            if (copyFrom == null)
                return;

            var count = copyFrom.TransitionModifiers.Count;
            for (int i = 0; i < count; i++)
                TransitionModifiers.Add(copyFrom.TransitionModifiers[i].Clone(context));

            foreach (var group in copyFrom.KeyedTransitionModifiers)
            {
                var clone = TransitionModifiers[group.Value.Index];
                KeyedTransitionModifiers.Add(group.Key, clone);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Remove
        /************************************************************************************************************************/

        // Remove from the dictionary but not the list because there might be multiple aliases for that index.

        /// <summary>[Pro-Only] Removes the transition registered with the `key`.</summary>
        public bool RemoveTransition(object key)
            => KeyedTransitionModifiers.Remove(key);

        /// <summary>[Pro-Only] Removes the transition registered with the <see cref="IHasKey.Key"/>.</summary>
        public bool RemoveTransition(IHasKey hasKey)
            => RemoveTransition(hasKey.Key);

        /************************************************************************************************************************/

        /// <summary>[Pro-Only] Removes a modified fade duration for transitioning from `from` to `to`.</summary>
        public bool RemoveFadeDuration(object from, object to)
            => TryGetTransition(to, out var group)
            && group.FromKeyToFadeDuration != null
            && group.FromKeyToFadeDuration.Remove(from);

        /// <summary>[Pro-Only] Removes a modified fade duration for transitioning from `from` to `to`.</summary>
        public bool RemoveFadeDuration(IHasKey from, IHasKey to)
            => RemoveFadeDuration(from.Key, to.Key);

        /************************************************************************************************************************/

        /// <summary>[Pro-Only] Removes everything from this library, leaving it empty.</summary>
        public void Clear()
        {
            TransitionModifiers.Clear();
            KeyedTransitionModifiers.Clear();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Play
        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="AnimancerLayer.Play(ITransition, float, FadeMode)"/>
        /// with the fade duration potentially modified by this library.
        /// </summary>
        public AnimancerState Play(
            AnimancerLayer layer,
            ITransition transition)
            => layer.Play(
                transition,
                GetFadeDuration(layer, transition),
                transition.FadeMode);

        /// <summary>
        /// Calls <see cref="AnimancerLayer.Play(ITransition, float, FadeMode)"/>
        /// with the fade duration potentially modified by this library.
        /// </summary>
        public AnimancerState Play(
            AnimancerLayer layer,
            TransitionModifierGroup transition)
        {
            var from = layer.CurrentState?.Key;
            var to = transition.Transition;

            var fadeDuration = from != null
                ? transition.GetFadeDuration(from)
                : to.FadeDuration;

            return layer.Play(
                to,
                fadeDuration,
                to.FadeMode);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Plays the transition registered with the specified `key` if there is one.
        /// Otherwise, returns <c>null</c>.
        /// </summary>
        public AnimancerState TryPlay(
            AnimancerLayer layer,
            object key)
            => TryGetTransition(key, out var transition)
            ? Play(layer, transition)
            : null;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Assertions
        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional]
        /// Logs <see cref="OptionalWarning.StringReference"/> if the `key` is a <see cref="string"/>.
        /// </summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        private void AssertStringReference(object key)
        {
#if UNITY_ASSERTIONS
            if (key is string keyString)
            {
                if (StringReference.TryGet(keyString, out var keyReference) &&
                    KeyedTransitionModifiers.ContainsKey(keyReference))
                    Debug.LogError(
                        $"{nameof(TransitionLibrary)} key type mismatch:" +
                        $" attempted to use string '{keyString}'," +
                        $" but that value is registered as a {nameof(StringReference)}." +
                        $" Use a {nameof(StringReference)} to ensure the correct lookup.");
                else
                    OptionalWarning.StringReference.Log(
                        $"A string '{keyString}' is being used as a key in a {nameof(TransitionLibrary)}." +
                        $" {nameof(StringReference)}s should be used instead of strings because they are more efficient" +
                        $" and to avoid mismatches with aliases in a {nameof(TransitionLibraryDefinition)}.");
            }
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional]
        /// Asserts that the <see cref="TransitionModifierGroup.Index"/>
        /// corresponds to the <see cref="TransitionModifiers"/>.
        /// </summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        [HideInCallstack]
        internal void AssertGroup(TransitionModifierGroup group)
        {
#if UNITY_ASSERTIONS
            if (!TransitionModifiers.TryGet(group.Index, out var registered) ||
                registered != group)
                Debug.LogError(
                    $"{nameof(CloneContext)} contains an {nameof(TransitionModifierGroup)}" +
                    $" which isn't part of this {nameof(TransitionLibrary)}." +
                    $" It must have been added to the context manually.");
#endif
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

