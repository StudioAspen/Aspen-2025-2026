// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using UnityEngine;

namespace Animancer.TransitionLibraries
{
    /// <summary>[<see cref="SerializableAttribute"/>]
    /// Details about how to modify a transition when it comes from a specific source.
    /// </summary>
    /// <remarks>
    /// Multiple of these can be used to build a <see cref="TransitionModifierGroup"/> at runtime.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/libraries">
    /// Transition Libraries</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.TransitionLibraries/TransitionModifierDefinition
    [Serializable]
    public struct TransitionModifierDefinition :
        IEquatable<TransitionModifierDefinition>
    {
        /************************************************************************************************************************/

        [SerializeField]
        private int _From;

        /// <summary>The index of the source transition in the <see cref="TransitionLibraryDefinition"/>.</summary>
        public readonly int FromIndex
            => _From;

        /************************************************************************************************************************/

        [SerializeField]
        private int _To;

        /// <summary>The index of the destination transition in the <see cref="TransitionLibraryDefinition"/>.</summary>
        public readonly int ToIndex
            => _To;

        /************************************************************************************************************************/

        [SerializeField]
        private float _Fade;

        /// <summary>The fade duration for this override to use.</summary>
        public readonly float FadeDuration
            => _Fade;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TransitionModifierDefinition"/>.</summary>
        public TransitionModifierDefinition(
            int fromIndex,
            int toIndex,
            float fadeDuration)
        {
            _From = fromIndex;
            _To = toIndex;
            _Fade = fadeDuration;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a copy of this override with the specified <see cref="FadeDuration"/>.</summary>
        public readonly TransitionModifierDefinition WithFadeDuration(float fadeDuration)
            => new(_From, _To, fadeDuration);

        /// <summary>Creates a copy of this override with the specified <see cref="FromIndex"/> and <see cref="ToIndex"/>.</summary>
        public readonly TransitionModifierDefinition WithIndices(int fromIndex, int toIndex)
            => new(fromIndex, toIndex, _Fade);

        /************************************************************************************************************************/

        /// <summary>Creates a new string describing this override.</summary>
        public override readonly string ToString()
            => $"{nameof(TransitionModifierDefinition)}({_From}->{_To}={_Fade})";

        /************************************************************************************************************************/
        #region Equality
        /************************************************************************************************************************/

        /// <summary>Are all fields in this object equal to the equivalent in `obj`?</summary>
        public override readonly bool Equals(object obj)
            => obj is TransitionModifierDefinition value
            && Equals(value);

        /// <summary>Are all fields in this object equal to the equivalent fields in `other`?</summary>
        public readonly bool Equals(TransitionModifierDefinition other)
            => _From == other._From
            && _To == other._To
            && _Fade.IsEqualOrBothNaN(other._Fade);

        /// <summary>Are all fields in `a` equal to the equivalent fields in `b`?</summary>
        public static bool operator ==(TransitionModifierDefinition a, TransitionModifierDefinition b)
            => a.Equals(b);

        /// <summary>Are any fields in `a` not equal to the equivalent fields in `b`?</summary>
        public static bool operator !=(TransitionModifierDefinition a, TransitionModifierDefinition b)
            => !(a == b);

        /************************************************************************************************************************/

        /// <summary>Returns a hash code based on the values of this object's fields.</summary>
        public override readonly int GetHashCode()
            => AnimancerUtilities.Hash(-871379578,
                _From.SafeGetHashCode(),
                _To.SafeGetHashCode(),
                _Fade.SafeGetHashCode());

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

