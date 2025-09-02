// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using UnityEngine;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// An <see cref="AnimancerState"/> which blends an array of other states together based on a two dimensional
    /// parameter and thresholds using Gradient Band Interpolation.
    /// </summary>
    /// <remarks>
    /// This mixer type is similar to the 2D Freeform Cartesian Blend Type in Mecanim Blend Trees.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">
    /// Mixers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/CartesianMixerState
    /// 
    public class CartesianMixerState : Vector2MixerState,
        ICopyable<CartesianMixerState>
    {
        /************************************************************************************************************************/

        /// <summary>Precalculated values to speed up the recalculation of weights.</summary>
        private Vector2[][] _BlendFactors;

        /// <summary>Indicates whether the <see cref="_BlendFactors"/> need to be recalculated.</summary>
        private bool _BlendFactorsAreDirty = true;

        /************************************************************************************************************************/

        /// <summary>
        /// Called whenever the thresholds are changed. Indicates that the internal blend factors need to be
        /// recalculated and triggers weight recalculation.
        /// </summary>
        public override void OnThresholdsChanged()
        {
            _BlendFactorsAreDirty = true;
            base.OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void ForceRecalculateWeights()
        {
            var childCount = ChildCount;
            if (childCount == 0)
            {
                return;
            }
            else if (childCount == 1)
            {
                Playable.SetChildWeight(ChildStates[0], 1);
                return;
            }

            CalculateBlendFactors(childCount);

            float totalWeight = 0;

            var weights = GetTemporaryFloatArray(childCount);

            for (int i = 0; i < childCount; i++)
            {
                var state = ChildStates[i];

                var blendFactors = _BlendFactors[i];

                var threshold = GetThreshold(i);
                var thresholdToParameter = Parameter - threshold;

                float weight = 1;

                for (int j = 0; j < childCount; j++)
                {
                    if (j == i)
                        continue;

                    var newWeight = 1 - Vector2.Dot(thresholdToParameter, blendFactors[j]);

                    if (weight > newWeight)
                        weight = newWeight;
                }

                if (weight < 0.01f)
                    weight = 0;

                weights[i] = weight;
                totalWeight += weight;
            }

            NormalizeAndApplyWeights(totalWeight, weights);
        }

        /************************************************************************************************************************/

        private void CalculateBlendFactors(int childCount)
        {
            if (!_BlendFactorsAreDirty)
                return;

            _BlendFactorsAreDirty = false;

            // Resize the precalculated values.
            if (AnimancerUtilities.SetLength(ref _BlendFactors, childCount))
            {
                for (int i = 0; i < childCount; i++)
                    _BlendFactors[i] = new Vector2[childCount];
            }

            // Calculate the blend factors between each combination of thresholds.
            for (int i = 0; i < childCount; i++)
            {
                var blendFactors = _BlendFactors[i];

                var thresholdI = GetThreshold(i);

                var j = i + 1;
                for (; j < childCount; j++)
                {
                    var thresholdIToJ = GetThreshold(j) - thresholdI;

#if UNITY_ASSERTIONS
                    if (thresholdIToJ == default)
                    {
                        MarkAsUsed(this);
                        throw new ArgumentException(
                            $"Mixer has multiple identical thresholds.\n{this.GetDescription()}");
                    }
#endif

                    thresholdIToJ /= thresholdIToJ.sqrMagnitude;

                    // Each factor is used in [i][j] with it's opposite in [j][i].
                    blendFactors[j] = thresholdIToJ;
                    _BlendFactors[j][i] = -thresholdIToJ;
                }
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerState Clone(CloneContext context)
        {
            var clone = new CartesianMixerState();
            clone.CopyFrom(this, context);
            return clone;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public sealed override void CopyFrom(Vector2MixerState copyFrom, CloneContext context)
            => this.CopyFromBase(copyFrom, context);

        /// <inheritdoc/>
        public virtual void CopyFrom(CartesianMixerState copyFrom, CloneContext context)
        {
            _BlendFactorsAreDirty = copyFrom._BlendFactorsAreDirty;
            if (!_BlendFactorsAreDirty)
                _BlendFactors = copyFrom._BlendFactors;

            base.CopyFrom(copyFrom, context);
        }

        /************************************************************************************************************************/
    }
}

