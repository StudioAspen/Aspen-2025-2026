// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Animancer.TransitionLibraries
{
    /// <summary>[<see cref="SerializableAttribute"/>]
    /// A library of transitions and other details which can create a <see cref="TransitionLibrary"/>.
    /// </summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/libraries">
    /// Transition Libraries</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.TransitionLibraries/TransitionLibraryDefinition
    [Serializable]
    public class TransitionLibraryDefinition :
        IAnimationClipCollection,
        ICopyable<TransitionLibraryDefinition>,
        IEquatable<TransitionLibraryDefinition>,
        IHasDescription
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        [SerializeField]
        private TransitionAssetBase[]
            _Transitions = Array.Empty<TransitionAssetBase>();

        /// <summary>[<see cref="SerializeField"/>] The transitions in this library.</summary>
        /// <remarks>This property uses an empty array instead of <c>null</c>.</remarks>
        public TransitionAssetBase[] Transitions
        {
            get => _Transitions;
            set => _Transitions = value.NullIsEmpty();
        }

        /************************************************************************************************************************/

        [SerializeField]
        private TransitionModifierDefinition[]
            _Modifiers = Array.Empty<TransitionModifierDefinition>();

        /// <summary>[<see cref="SerializeField"/>] Modified fade durations for specific transition combinations.</summary>
        /// <remarks>This property uses an empty array instead of <c>null</c>.</remarks>
        public TransitionModifierDefinition[] Modifiers
        {
            get => _Modifiers;
            set => _Modifiers = value.NullIsEmpty();
        }

        /************************************************************************************************************************/

        [SerializeField]
        private NamedIndex[]
            _Aliases = Array.Empty<NamedIndex>();

        /// <summary>[<see cref="SerializeField"/>] Alternate names that can be used to look up transitions.</summary>
        /// <remarks>
        /// This array should always be sorted, use <see cref="SortAliases"/> if necessary.
        /// <para></para>
        /// This property uses an empty array instead of <c>null</c>.
        /// </remarks>
        public NamedIndex[] Aliases
        {
            get => _Aliases;
            set => _Aliases = value.NullIsEmpty();
        }

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip(AliasAllTransitionsTooltip)]
        private bool _AliasAllTransitions;

        /// <summary>[<see cref="SerializeField"/>]
        /// Should all Transitions automatically be registered using their name as an Alias?
        /// </summary>
        public ref bool AliasAllTransitions
            => ref _AliasAllTransitions;

#if UNITY_EDITOR
        /// <summary>[Editor-Only] [Internal]
        /// The name of the field which stores the <see cref="AliasAllTransitions"/>.
        /// </summary>
        internal const string AliasAllTransitionsField = nameof(_AliasAllTransitions);
#endif

        /// <summary>Tooltip for the <see cref="AliasAllTransitions"/> field.</summary>
        public const string AliasAllTransitionsTooltip =
            "Should all Transitions automatically be registered using their name as an Alias?";

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Transitions
        /************************************************************************************************************************/

        /// <summary>
        /// <see cref="AnimancerUtilities.TryGet{T}(IList{T}, int, out T)"/> for the <see cref="Transitions"/>.
        /// </summary>
        public bool TryGetTransition(
            int index,
            out TransitionAssetBase transition)
            => _Transitions.TryGet(index, out transition)
            && transition != null;

        /************************************************************************************************************************/

        /// <summary>Adds an item to the end of the <see cref="Transitions"/>.</summary>
        public void AddTransition(
            TransitionAssetBase transition)
            => AnimancerUtilities.InsertAt(
                ref _Transitions,
                _Transitions.Length,
                transition);

        /************************************************************************************************************************/

        /// <summary>
        /// Removes an item from the <see cref="Transitions"/>
        /// and adjusts the other fields to account for the moved indices.
        /// </summary>
        public void RemoveTransition(int index)
        {
            if ((uint)index >= _Transitions.Length)
                return;

            AnimancerUtilities.RemoveAt(ref _Transitions, index);

            for (int i = _Modifiers.Length - 1; i >= 0; i--)
            {
                var modifier = _Modifiers[i];

                // Remove any modifiers targeting that transition.
                if (modifier.FromIndex == index ||
                    modifier.ToIndex == index)
                {
                    AnimancerUtilities.RemoveAt(ref _Modifiers, i);
                }
                else// Adjust the indices of any modifiers after it.
                {
                    var fromIndex = modifier.FromIndex;
                    if (fromIndex > index)
                        fromIndex--;

                    var toIndex = modifier.ToIndex;
                    if (toIndex > index)
                        toIndex--;

                    _Modifiers[i] = modifier.WithIndices(fromIndex, toIndex);
                }
            }

            for (int i = _Aliases.Length - 1; i >= 0; i--)
            {
                var alias = _Aliases[i];

                // Remove any aliases targeting that transition.
                if (alias.Index == index)
                {
                    AnimancerUtilities.RemoveAt(ref _Aliases, i);
                }
                else// Adjust the indices of any aliases after it.
                {
                    if (alias.Index > index)
                        _Aliases[i] = alias.With(alias.Index - 1);
                }
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Modifiers
        /************************************************************************************************************************/

        /// <summary>Tries to find an item in the <see cref="Modifiers"/> with the specified indices.</summary>
        /// <remarks>
        /// If unsuccessful, the `modifier` is given the <see cref="ITransition.FadeDuration"/>
        /// from the <see cref="Transitions"/> at the `toIndex`. and this method returns false.
        /// </remarks>
        public bool TryGetModifier(
            int fromIndex,
            int toIndex,
            out TransitionModifierDefinition modifier)
        {
            var index = IndexOfModifier(fromIndex, toIndex);
            if (index >= 0)
            {
                modifier = _Modifiers[index];
                return true;
            }

            var fadeDuration = TryGetTransition(toIndex, out var transition)
                ? transition.TryGetFadeDuration()
                : float.NaN;
            modifier = new(fromIndex, toIndex, fadeDuration);
            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the index in the <see cref="Modifiers"/> which matches the given
        /// <see cref="TransitionModifierDefinition.FromIndex"/> and
        /// <see cref="TransitionModifierDefinition.ToIndex"/> or -1 if no such item exists.
        /// </summary>
        public int IndexOfModifier(int fromIndex, int toIndex)
        {
            for (int i = _Modifiers.Length - 1; i >= 0; i--)
            {
                var modifier = _Modifiers[i];
                if (modifier.FromIndex == fromIndex &&
                    modifier.ToIndex == toIndex)
                    return i;
            }

            return -1;
        }

        /************************************************************************************************************************/

        /// <summary>Adds or replaces an item in the <see cref="Modifiers"/>.</summary>
        public void SetModifier(
            TransitionModifierDefinition modifier)
        {
            if (float.IsNaN(modifier.FadeDuration))
            {
                RemoveModifier(modifier);
                return;
            }

            if (modifier.FadeDuration < 0)
                modifier = modifier.WithFadeDuration(0);

            var index = IndexOfModifier(modifier.FromIndex, modifier.ToIndex);
            if (index >= 0)
            {
                _Modifiers[index] = modifier;
            }
            else
            {
                AnimancerUtilities.InsertAt(ref _Modifiers, _Modifiers.Length, modifier);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Removes an item from the <see cref="Modifiers"/>.</summary>
        public bool RemoveModifier(
            TransitionModifierDefinition modifier)
            => RemoveModifier(modifier.FromIndex, modifier.ToIndex);

        /// <summary>Removes an item from the <see cref="Modifiers"/>.</summary>
        public bool RemoveModifier(int fromIndex, int toIndex)
        {
            var index = IndexOfModifier(fromIndex, toIndex);
            if (index < 0)
                return false;

            AnimancerUtilities.RemoveAt(ref _Modifiers, index);
            return true;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Aliases
        /************************************************************************************************************************/

        /// <summary>Adds an item to the <see cref="Aliases"/>, sorted by its values.</summary>
        public int AddAlias(NamedIndex alias)
        {
            int i = 0;
            for (; i < _Aliases.Length; i++)
                if (alias.CompareTo(_Aliases[i]) <= 0)
                    break;

            AnimancerUtilities.InsertAt(ref _Aliases, i, alias);
            return i;
        }

        /************************************************************************************************************************/

        /// <summary>Removes an item from the <see cref="Aliases"/>.</summary>
        public bool RemoveAlias(NamedIndex alias)
        {
            var index = Array.IndexOf(_Aliases, alias);
            if (index < 0)
                return false;

            RemoveAlias(index);
            return true;
        }

        /// <summary>Removes an item from the <see cref="Aliases"/>.</summary>
        public void RemoveAlias(int index)
            => AnimancerUtilities.RemoveAt(ref _Aliases, index);

        /************************************************************************************************************************/

        /// <summary>Ensures that the <see cref="Aliases"/> are sorted.</summary>
        /// <remarks>This method shouldn't need to be called manually since aliases are always added in order.</remarks>
        public void SortAliases()
            => Array.Sort(_Aliases, (a, b) => a.CompareTo(b));

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Equality
        /************************************************************************************************************************/

        /// <summary>Are all fields in this object equal to the equivalent in `obj`?</summary>
        public override bool Equals(object obj)
            => Equals(obj as TransitionLibraryDefinition);

        /// <summary>Are all fields in this object equal to the equivalent fields in `other`?</summary>
        public bool Equals(TransitionLibraryDefinition other)
            => other != null
            && AnimancerUtilities.ContentsAreEqual(_Transitions, other._Transitions)
            && AnimancerUtilities.ContentsAreEqual(_Modifiers, other._Modifiers)
            && AnimancerUtilities.ContentsAreEqual(_Aliases, other._Aliases)
            && _AliasAllTransitions == other._AliasAllTransitions;

        /// <summary>Are all fields in `a` equal to the equivalent fields in `b`?</summary>
        public static bool operator ==(TransitionLibraryDefinition a, TransitionLibraryDefinition b)
            => a is null
                ? b is null
                : a.Equals(b);

        /// <summary>Are any fields in `a` not equal to the equivalent fields in `b`?</summary>
        public static bool operator !=(TransitionLibraryDefinition a, TransitionLibraryDefinition b)
            => !(a == b);

        /************************************************************************************************************************/

        /// <summary>Returns a hash code based on the values of this object's fields.</summary>
        public override int GetHashCode()
            => AnimancerUtilities.Hash(-871379578,
                _Transitions.SafeGetHashCode(),
                _Modifiers.SafeGetHashCode(),
                _Aliases.SafeGetHashCode(),
                _AliasAllTransitions.SafeGetHashCode());

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Other
        /************************************************************************************************************************/

        /// <summary>Gathers all the animations in this definition.</summary>
        public virtual void GatherAnimationClips(ICollection<AnimationClip> clips)
            => clips.GatherFromSource(_Transitions);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public void CopyFrom(TransitionLibraryDefinition copyFrom, CloneContext context)
        {
            AnimancerUtilities.CopyExactArray(copyFrom._Transitions, ref _Transitions);
            AnimancerUtilities.CopyExactArray(copyFrom._Modifiers, ref _Modifiers);
            AnimancerUtilities.CopyExactArray(copyFrom._Aliases, ref _Aliases);
            _AliasAllTransitions = copyFrom._AliasAllTransitions;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public void AppendDescription(StringBuilder text, string separator = "\n")
        {
            text.Append(GetType().Name);

            if (!separator.StartsWithNewLine())
                separator = "\n" + separator;

            var indentedSeparator = separator + Strings.Indent;

            text.AppendField(separator, nameof(Transitions), Transitions.Length);
            for (int i = 0; i < Transitions.Length; i++)
                text.AppendField(indentedSeparator, i.ToString(), Transitions[i]);

            text.AppendField(separator, nameof(Modifiers), Modifiers.Length);
            for (int i = 0; i < Modifiers.Length; i++)
                text.AppendField(indentedSeparator, i.ToString(), Modifiers[i]);

            text.AppendField(separator, nameof(Aliases), Aliases.Length);
            for (int i = 0; i < Aliases.Length; i++)
                text.AppendField(indentedSeparator, i.ToString(), Aliases[i]);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

