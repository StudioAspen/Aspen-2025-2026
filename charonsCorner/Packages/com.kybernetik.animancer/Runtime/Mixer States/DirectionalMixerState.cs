// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using UnityEngine;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// An <see cref="AnimancerState"/> which blends an array of other states together based on a two dimensional
    /// parameter and thresholds using Polar Gradient Band Interpolation.
    /// </summary>
    /// <remarks>
    /// This mixer type is similar to the 2D Freeform Directional Blend Type in Mecanim Blend Trees.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">
    /// Mixers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/DirectionalMixerState
    /// 
    public class DirectionalMixerState : Vector2MixerState,
        ICopyable<DirectionalMixerState>
    {
        /************************************************************************************************************************/

        /// <summary>Precalculated magnitudes of all thresholds to speed up the recalculation of weights.</summary>
        private float[] _ThresholdMagnitudes;

        /// <summary>Precalculated values to speed up the recalculation of weights.</summary>
        private Vector2[][] _BlendFactors;

        /// <summary>Indicates whether the <see cref="_BlendFactors"/> need to be recalculated.</summary>
        private bool _BlendFactorsAreDirty = true;

        /// <summary>The multiplier that controls how much an angle (in radians) is worth compared to normalized distance.</summary>
        private const float AngleFactor = 2;

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
                var state = ChildStates[0];
                Playable.SetChildWeight(state, 1);
                return;
            }

            CalculateBlendFactors(childCount);

            var parameterMagnitude = Parameter.magnitude;
            float totalWeight = 0;

            var weights = GetTemporaryFloatArray(childCount);

            for (int i = 0; i < childCount; i++)
            {
                var state = ChildStates[i];
                var blendFactors = _BlendFactors[i];

                var thresholdI = GetThreshold(i);
                var magnitudeI = _ThresholdMagnitudes[i];

                // Convert the threshold to polar coordinates (distance, angle)
                // and interpolate the weight based on those.
                var differenceIToParameter = parameterMagnitude - magnitudeI;
                var angleIToParameter = SignedAngle(thresholdI, Parameter) * AngleFactor;

                float weight = 1;

                for (int j = 0; j < childCount; j++)
                {
                    if (j == i)
                        continue;

                    var magnitudeJ = _ThresholdMagnitudes[j];
                    var averageMagnitude = (magnitudeJ + magnitudeI) * 0.5f;

                    var polarIToParameter = new Vector2(
                        differenceIToParameter / averageMagnitude,
                        angleIToParameter);

                    var newWeight = 1 - Vector2.Dot(polarIToParameter, blendFactors[j]);

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
            if (_BlendFactors == null || _BlendFactors.Length != childCount)
            {
                _ThresholdMagnitudes = new float[childCount];

                _BlendFactors = new Vector2[childCount][];
                for (int i = 0; i < childCount; i++)
                    _BlendFactors[i] = new Vector2[childCount];
            }

            // Calculate the magnitude of each threshold.
            for (int i = 0; i < childCount; i++)
            {
                _ThresholdMagnitudes[i] = GetThreshold(i).magnitude;
            }

            // Calculate the blend factors between each combination of thresholds.
            for (int i = 0; i < childCount; i++)
            {
                var blendFactors = _BlendFactors[i];

                var thresholdI = GetThreshold(i);
                var magnitudeI = _ThresholdMagnitudes[i];

                var j = 0;// i + 1;
                for (; j < childCount; j++)
                {
                    if (i == j)
                        continue;

                    var thresholdJ = GetThreshold(j);
                    var magnitudeJ = _ThresholdMagnitudes[j];

#if UNITY_ASSERTIONS
                    if (thresholdI == thresholdJ)
                    {
                        MarkAsUsed(this);
                        throw new ArgumentException(
                            $"Mixer has multiple identical thresholds.\n{this.GetDescription()}");
                    }
#endif

                    var averageMagnitude = (magnitudeI + magnitudeJ) * 0.5f;

                    // Convert the thresholds to polar coordinates (distance, angle) and interpolate the weight based on those.

                    var differenceIToJ = magnitudeJ - magnitudeI;

                    var angleIToJ = SignedAngle(thresholdI, thresholdJ);

                    var polarIToJ = new Vector2(
                        differenceIToJ / averageMagnitude,
                        angleIToJ * AngleFactor);

                    polarIToJ /= polarIToJ.sqrMagnitude;

                    // Each factor is used in [i][j] with it's opposite in [j][i].
                    blendFactors[j] = polarIToJ;
                    _BlendFactors[j][i] = -polarIToJ;
                }
            }
        }

        /************************************************************************************************************************/

        private static float SignedAngle(Vector2 a, Vector2 b)
        {
            // If either vector is exactly at the origin, the angle is 0.
            if ((a.x == 0 && a.y == 0) || (b.x == 0 && b.y == 0))
            {
                // Due to floating point error the formula below usually gives 0 but sometimes Pi,
                // which screws up our other calculations so we need it to always be 0 properly.
                return 0;
            }

            return Mathf.Atan2(
                a.x * b.y - a.y * b.x,
                a.x * b.x + a.y * b.y);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerState Clone(CloneContext context)
        {
            var clone = new DirectionalMixerState();
            clone.CopyFrom(this, context);
            return clone;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public sealed override void CopyFrom(Vector2MixerState copyFrom, CloneContext context)
            => this.CopyFromBase(copyFrom, context);

        /// <inheritdoc/>
        public virtual void CopyFrom(DirectionalMixerState copyFrom, CloneContext context)
        {
            _ThresholdMagnitudes = copyFrom._ThresholdMagnitudes;
            _BlendFactorsAreDirty = copyFrom._BlendFactorsAreDirty;
            if (!_BlendFactorsAreDirty)
                _BlendFactors = copyFrom._BlendFactors;

            base.CopyFrom(copyFrom, context);
        }

        /************************************************************************************************************************/
    }
}

