// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// An <see cref="AnimancerState"/> which blends an array of other states together
    /// using linear interpolation between the specified thresholds.
    /// </summary>
    /// <remarks>
    /// This mixer type is similar to the 1D Blend Type in Mecanim Blend Trees.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">
    /// Mixers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/LinearMixerState
    public class LinearMixerState : MixerState<float>,
        ICopyable<LinearMixerState>
    {
        /************************************************************************************************************************/

        private bool _ExtrapolateSpeed = true;

        /// <summary>
        /// Should setting the <see cref="MixerState{TParameter}.Parameter"/> above the highest threshold
        /// increase the <see cref="AnimancerNodeBase.Speed"/> of this mixer proportionally?
        /// </summary>
        public bool ExtrapolateSpeed
        {
            get => _ExtrapolateSpeed;
            set
            {
                if (_ExtrapolateSpeed == value)
                    return;

                _ExtrapolateSpeed = value;

                if (!_Playable.IsValid())
                    return;

                var speed = Speed;

                var childCount = ChildCount;
                if (value && childCount > 0)
                {
                    var threshold = GetThreshold(childCount - 1);
                    if (Parameter > threshold)
                        speed *= Parameter / threshold;
                }

                _Playable.SetSpeed(speed);
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string GetParameterError(float value)
            => value.IsFinite() ? null : Strings.MustBeFinite;

        /************************************************************************************************************************/

        /// <summary>The lowest threshold (which is for the first child since they must be sorted).</summary>
        public float MinThreshold
            => GetThreshold(0);

        /// <summary>The highest threshold (which is for the last child since they must be sorted).</summary>
        public float MaxThreshold
            => GetThreshold(ChildCount - 1);

        /// <inheritdoc/>
        public override float NormalizedParameter
        {
            get
            {
                var min = MinThreshold;
                var max = MaxThreshold;

                if (min < 0 && max > 0)// Interpolate -1 to 1.
                {
                    var value = Parameter;

                    if (value > 0)
                        value = AnimancerUtilities.InverseLerpUnclamped(0, max, value);
                    else if (value < 0)
                        value = AnimancerUtilities.InverseLerpUnclamped(0, min, -value);

                    return value;
                }
                else// Interpolate 0 to 1.
                {
                    return AnimancerUtilities.InverseLerpUnclamped(MinThreshold, MaxThreshold, Parameter);
                }
            }

            set => Parameter = Mathf.LerpUnclamped(MinThreshold, MaxThreshold, value);
        }

        /************************************************************************************************************************/
        #region Parameter Binding
        /************************************************************************************************************************/

        private NodeParameter<float> _ParameterBinding;

        /// <summary>
        /// If set, this will be used as a key in the <see cref="ParameterDictionary"/> so any
        /// changes to that parameter will automatically set the <see cref="MixerState{TParameter}.Parameter"/>.
        /// </summary>
        public StringReference ParameterName
        {
            get => _ParameterBinding.Key;
            set
            {
                if (_ParameterBinding.SetKeyCheckNeedsInitialize(value))
                    _ParameterBinding.Initialize(this, parameter => Parameter = parameter);
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void SetGraph(AnimancerGraph graph)
        {
            if (Graph == graph)
                return;

            _ParameterBinding.UnBindIfInitialized();

            base.SetGraph(graph);

            _ParameterBinding.BindIfInitialized();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Destroy()
        {
            base.Destroy();

            _ParameterBinding.UnBindIfInitialized();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerState Clone(CloneContext context)
        {
            var clone = new LinearMixerState();
            clone.CopyFrom(this, context);
            return clone;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public sealed override void CopyFrom(MixerState<float> copyFrom, CloneContext context)
            => this.CopyFromBase(copyFrom, context);

        /// <inheritdoc/>
        public virtual void CopyFrom(LinearMixerState copyFrom, CloneContext context)
        {
            _ExtrapolateSpeed = copyFrom._ExtrapolateSpeed;

            base.CopyFrom(copyFrom, context);

            ParameterName = copyFrom.ParameterName;
        }

        /************************************************************************************************************************/
#if UNITY_ASSERTIONS
        /************************************************************************************************************************/

        private bool _ShouldCheckThresholdSorting;

        /// <summary>
        /// Called whenever the thresholds are changed. Indicates that <see cref="AssertThresholdsSorted"/> needs to
        /// be called by the next <see cref="ForceRecalculateWeights"/> if <c>UNITY_ASSERTIONS</c> is defined, then
        /// calls <see cref="MixerState{TParameter}.OnThresholdsChanged"/>.
        /// </summary>
        public override void OnThresholdsChanged()
        {
            _ShouldCheckThresholdSorting = true;

            base.OnThresholdsChanged();
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the thresholds are not sorted from lowest to highest without
        /// any duplicates.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="InvalidOperationException">The thresholds have not been initialized.</exception>
        public void AssertThresholdsSorted()
        {
#if UNITY_ASSERTIONS
            _ShouldCheckThresholdSorting = false;
#endif

            if (!HasThresholds)
            {
                MarkAsUsed(this);
                throw new InvalidOperationException("Thresholds have not been initialized");
            }

            var previous = float.NegativeInfinity;

            var childCount = ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                var state = ChildStates[i];
                if (state == null)
                    continue;

                var next = GetThreshold(i);
                if (next > previous)
                {
                    previous = next;
                }
                else
                {
                    MarkAsUsed(this);
                    var reason = next == previous
                        ? "Mixer has multiple identical thresholds."
                        : "Mixer has thresholds out of order.";
                    throw new ArgumentException(
                        $"{reason} They must be sorted from lowest to highest with no equal values." +
                        $"\n{this.GetDescription()}");
                }
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void ForceRecalculateWeights()
        {
#if UNITY_ASSERTIONS
            if (_ShouldCheckThresholdSorting)
                AssertThresholdsSorted();
#endif

            // Go through all states, figure out how much weight to give those with thresholds adjacent to the
            // current parameter value using linear interpolation, and set all others to 0 weight.

            var childCount = ChildCount;
            if (childCount == 0)
                goto ResetExtrapolatedSpeed;

            var index = 0;
            var previousState = ChildStates[index];

            var parameter = Parameter;
            var previousThreshold = GetThreshold(index);

            if (parameter <= previousThreshold)
            {
                DisableRemainingStates(index);

                if (previousThreshold >= 0)
                {
                    Playable.SetChildWeight(previousState, 1);
                    goto ResetExtrapolatedSpeed;
                }
            }
            else
            {
                while (++index < childCount)
                {
                    var nextState = ChildStates[index];
                    var nextThreshold = GetThreshold(index);

                    if (parameter > previousThreshold && parameter <= nextThreshold)
                    {
                        var t = (parameter - previousThreshold) / (nextThreshold - previousThreshold);
                        Playable.SetChildWeight(previousState, 1 - t);
                        Playable.SetChildWeight(nextState, t);
                        DisableRemainingStates(index);
                        goto ResetExtrapolatedSpeed;
                    }
                    else
                    {
                        Playable.SetChildWeight(previousState, 0);
                    }

                    previousState = nextState;
                    previousThreshold = nextThreshold;
                }
            }

            Playable.SetChildWeight(previousState, 1);

            if (ExtrapolateSpeed)
                _Playable.SetSpeed(Speed * (parameter / previousThreshold));

            return;

            ResetExtrapolatedSpeed:
            if (ExtrapolateSpeed && _Playable.IsValid())
                _Playable.SetSpeed(Speed);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Assigns the thresholds to be evenly spaced between the specified min and max (inclusive).
        /// </summary>
        public LinearMixerState AssignLinearThresholds(float min = 0, float max = 1)
        {
#if UNITY_ASSERTIONS
            if (min >= max)
            {
                MarkAsUsed(this);
                throw new ArgumentException($"{nameof(min)} must be less than {nameof(max)}");
            }
#endif
            var childCount = ChildCount;

            var thresholds = new float[childCount];

            var increment = (max - min) / (childCount - 1);

            for (int i = 0; i < childCount; i++)
            {
                thresholds[i] =
                    i < childCount - 1 ?
                    min + i * increment :// Assign each threshold linearly spaced between the min and max.
                    max;// and ensure that the last one is exactly at the max (to avoid floating-point error).
            }

            SetThresholds(thresholds);

            return this;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string separator)
        {
            text.AppendField(separator, nameof(ExtrapolateSpeed), ExtrapolateSpeed);

            base.AppendDetails(text, separator);
        }

        /************************************************************************************************************************/
        #region Inspector
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void GetParameters(List<StateParameterDetails> parameters)
        {
            parameters.Add(new(
                "Parameter",
                ParameterName,
                AnimatorControllerParameterType.Float,
                Parameter));
        }

        /// <inheritdoc/>
        public override void SetParameters(List<StateParameterDetails> parameters)
        {
            var parameter = parameters[0];
            ParameterName = parameter.name;
            Parameter = (float)parameter.value;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

