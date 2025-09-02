// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Collections.Generic;

namespace Animancer.TransitionLibraries
{
    /// <summary>
    /// An <see cref="ITransition"/> and a dictionary to modify it based on the previous state.
    /// </summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/libraries">
    /// Transition Libraries</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.TransitionLibraries/TransitionModifierGroup
    public class TransitionModifierGroup :
        ICloneable<TransitionModifierGroup>,
        ICopyable<TransitionModifierGroup>
    {
        /************************************************************************************************************************/

        /// <summary>The index at which this group was added to its <see cref="TransitionLibrary"/>.</summary>
        public readonly int Index;

        /************************************************************************************************************************/

        private ITransition _Transition;

        /// <summary>The target transition of this group.</summary>
        /// <remarks>Can't be <c>null</c>.</remarks>
        public ITransition Transition
        {
            get => _Transition;
            set
            {
                AnimancerUtilities.Assert(
                    value != null,
                    $"{nameof(TransitionModifierGroup)}.{nameof(Transition)} can't be null.");

                _Transition = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Custom <see cref="ITransition.FadeDuration"/>s to use when playing the <see cref="Transition"/>
        /// depending on the <see cref="IHasKey.Key"/> of the source state it is coming from.
        /// </summary>
        /// <remarks>This is <c>null</c> by default until <see cref="SetFadeDuration"/> adds something.</remarks>
        public Dictionary<object, float> FromKeyToFadeDuration { get; set; }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TransitionModifierGroup"/>.</summary>
        public TransitionModifierGroup(
            int index,
            ITransition transition)
        {
            Index = index;
            Transition = transition;
        }

        /************************************************************************************************************************/

        /// <summary>Sets the `fadeDuration` to use when transitioning from `from` to the <see cref="Transition"/>.</summary>
        public void SetFadeDuration(object from, float fadeDuration)
        {
            FromKeyToFadeDuration ??= new();
            FromKeyToFadeDuration[from] = fadeDuration;
        }

        /// <summary>Removes the fade duration modifier set for transitioning from `from` to the <see cref="Transition"/>.</summary>
        public void ResetFadeDuration(object from)
            => FromKeyToFadeDuration?.Remove(from);

        /************************************************************************************************************************/

        /// <summary>Returns the fade duration to use when transitioning from `from` to the <see cref="Transition"/>.</summary>
        public float GetFadeDuration(object from)
            => FromKeyToFadeDuration != null
            && FromKeyToFadeDuration.TryGetValue(AnimancerUtilities.GetRootKey(from), out var fadeDuration)
            ? fadeDuration
            : Transition.FadeDuration;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public TransitionModifierGroup Clone(CloneContext context)
        {
            var clone = new TransitionModifierGroup(Index, null);
            clone.CopyFrom(this);
            return clone;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public void CopyFrom(TransitionModifierGroup copyFrom, CloneContext context)
        {
            Transition = copyFrom.Transition;

            if (copyFrom.FromKeyToFadeDuration == null)
            {
                FromKeyToFadeDuration?.Clear();
            }
            else
            {
                FromKeyToFadeDuration ??= new();
                foreach (var item in copyFrom.FromKeyToFadeDuration)
                    FromKeyToFadeDuration[item.Key] = item.Value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Describes this object.</summary>
        public override string ToString()
            => $"{nameof(TransitionModifierGroup)}([{Index}] {AnimancerUtilities.ToStringOrNull(Transition)})";

        /************************************************************************************************************************/
    }
}

