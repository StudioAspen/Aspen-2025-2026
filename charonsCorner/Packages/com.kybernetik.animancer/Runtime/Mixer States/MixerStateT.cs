// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// Base class for mixers which blend an array of child states together based on a <see cref="Parameter"/>.
    /// </summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers">
    /// Mixers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/MixerState_1
    /// 
    public abstract class MixerState<TParameter> : ManualMixerState,
        ICopyable<MixerState<TParameter>>
    {
        /************************************************************************************************************************/
        #region Thresholds
        /************************************************************************************************************************/

        /// <summary>The parameter values at which each of the child states are used and blended.</summary>
        private TParameter[] _Thresholds = Array.Empty<TParameter>();

        /************************************************************************************************************************/

        /// <summary>
        /// Has the array of thresholds been initialized with a size at least equal to the
        /// <see cref="ManualMixerState.ChildCount"/>.
        /// </summary>
        public bool HasThresholds
            => _Thresholds.Length >= ChildCount;

        /************************************************************************************************************************/

        /// <summary>Returns the value of the threshold associated with the specified `index`.</summary>
        public TParameter GetThreshold(int index)
            => _Thresholds[index];

        /************************************************************************************************************************/

        /// <summary>Sets the value of the threshold associated with the specified `index`.</summary>
        public void SetThreshold(int index, TParameter threshold)
        {
            _Thresholds[index] = threshold;
            OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Assigns the specified array as the thresholds to use for blending.
        /// <para></para>
        /// WARNING: if you keep a reference to the `thresholds` array you must call <see cref="OnThresholdsChanged"/>
        /// whenever any changes are made to it, otherwise this mixer may not blend correctly.
        /// </summary>
        public void SetThresholds(params TParameter[] thresholds)
        {
            if (thresholds.Length < ChildCount)
            {
                MarkAsUsed(this);
                throw new ArgumentOutOfRangeException(nameof(thresholds),
                    $"Threshold count ({thresholds.Length}) must not be less than child count ({ChildCount}).");
            }

            _Thresholds = thresholds;
            OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="Array.Length"/> of the <see cref="_Thresholds"/> is below the
        /// <see cref="AnimancerNodeBase.ChildCount"/>, this method assigns a new array with size equal to the
        /// <see cref="ManualMixerState.ChildCapacity"/> and returns true.
        /// </summary>
        public bool ValidateThresholdCount()
        {
            if (_Thresholds.Length >= ChildCount)
                return false;

            _Thresholds = new TParameter[ChildCapacity];
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Called whenever the thresholds are changed. By default this method simply indicates that the blend weights
        /// need recalculating but it can be overridden by child classes to perform validation checks or optimisations.
        /// </summary>
        public virtual void OnThresholdsChanged()
        {
            SetWeightsDirty();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls `calculate` for each of the <see cref="ManualMixerState.ChildStates"/> and stores the returned value
        /// as the threshold for that state.
        /// </summary>
        public void CalculateThresholds(Func<AnimancerState, TParameter> calculate)
        {
            ValidateThresholdCount();

            for (int i = ChildCount - 1; i >= 0; i--)
                _Thresholds[i] = calculate(GetChild(i));

            OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Stores the values of all parameters, calls <see cref="AnimancerNode.DestroyPlayable"/>, then restores the
        /// parameter values.
        /// </summary>
        public override void RecreatePlayable()
        {
            base.RecreatePlayable();
            SetWeightsDirty();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Parameter
        /************************************************************************************************************************/

        private TParameter _Parameter;

        /// <summary>The value used to calculate the weights of the child states.</summary>
        /// <remarks>
        /// Setting this value takes effect immediately (during the next animation update) without any
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/mixers#smoothing">Smoothing</see>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The value is NaN or Infinity.</exception>
        public TParameter Parameter
        {
            get => _Parameter;
            set
            {
#if UNITY_ASSERTIONS
                if (Graph != null)
                    Validate.AssertPlayable(this);

                var error = GetParameterError(value);
                if (error != null)
                {
                    MarkAsUsed(this);
                    throw new ArgumentOutOfRangeException(nameof(value), error);
                }
#endif

                _Parameter = value;
                SetWeightsDirty();
            }
        }

        /// <summary>
        /// Returns an error message if the given `parameter` value can't be assigned to the <see cref="Parameter"/>.
        /// Otherwise returns null.
        /// </summary>
        public abstract string GetParameterError(TParameter parameter);

        /************************************************************************************************************************/

        /// <summary>The <see cref="Parameter"/> normalized across all thresholds.</summary>
        /// <remarks>
        /// If the threshold range includes both negative and positive values, the normalized range is -1 to 1.
        /// Otherwise, the normalized range is 0 to 1.
        /// </remarks>
        public abstract TParameter NormalizedParameter { get; set; }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Weight Calculation
        /************************************************************************************************************************/

        /// <summary>Should the weights of all child states be recalculated?</summary>
        public bool WeightsAreDirty { get; private set; }

        /// <summary>Registers this mixer to recalculate its weights during the next animation update.</summary>
        public void SetWeightsDirty()
        {
            if (!WeightsAreDirty)
            {
                WeightsAreDirty = true;
                Graph?.RequirePreUpdate(this);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If <see cref="WeightsAreDirty"/> this method recalculates the weights of all child states and returns true.
        /// </summary>
        public bool RecalculateWeights()
        {
            if (!WeightsAreDirty)
                return false;

            ForceRecalculateWeights();
            WeightsAreDirty = false;
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Recalculates the weights of all child states based on the current value of the
        /// <see cref="MixerState{TParameter}.Parameter"/> and the thresholds.
        /// </summary>
        protected virtual void ForceRecalculateWeights() { }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnSetIsPlaying()
        {
            base.OnSetIsPlaying();

            if (WeightsAreDirty || SynchronizedChildCount > 0)
                Graph?.RequirePreUpdate(this);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override double RawTime
        {
            get
            {
                if (_Playable.IsValid())
                    RecalculateWeights();

                return base.RawTime;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float Length
        {
            get
            {
                if (_Playable.IsValid())
                    RecalculateWeights();

                return base.Length;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Vector3 AverageVelocity
        {
            get
            {
                if (_Playable.IsValid())
                    RecalculateWeights();

                return base.AverageVelocity;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void CreatePlayable(out Playable playable)
        {
            base.CreatePlayable(out playable);
            RecalculateWeights();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Update()
        {
            RecalculateWeights();
            base.Update();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Initialization
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnChildCapacityChanged()
        {
            Array.Resize(ref _Thresholds, ChildCapacity);
            OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <summary>Assigns the `state` as a child of this mixer and assigns the `threshold` for it.</summary>
        public void Add(AnimancerState state, TParameter threshold)
        {
            Add(state);
            SetThreshold(state.Index, threshold);
        }

        /// <summary>
        /// Creates and returns a new <see cref="ClipState"/> to play the `clip` as a child of this mixer, and assigns
        /// the `threshold` for it.
        /// </summary>
        public ClipState Add(AnimationClip clip, TParameter threshold)
        {
            var state = Add(clip);
            SetThreshold(state.Index, threshold);
            return state;
        }

        /// <summary>
        /// Calls <see cref="AnimancerUtilities.CreateStateAndApply"/> then 
        /// <see cref="Add(AnimancerState, TParameter)"/>.
        /// </summary>
        public AnimancerState Add(Animancer.ITransition transition, TParameter threshold)
        {
            var state = Add(transition);
            SetThreshold(state.Index, threshold);
            return state;
        }

        /// <summary>Calls one of the other <see cref="Add(object, TParameter)"/> overloads as appropriate.</summary>
        public AnimancerState Add(object child, TParameter threshold)
        {
            if (child is AnimationClip clip)
                return Add(clip, threshold);

            if (child is ITransition transition)
                return Add(transition, threshold);

            if (child is AnimancerState state)
            {
                Add(state, threshold);
                return state;
            }

            MarkAsUsed(this);
            throw new ArgumentException(
                $"Unable to add '{AnimancerUtilities.ToStringOrNull(child)}' as child of '{this}'.");
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public sealed override void CopyFrom(ManualMixerState copyFrom, CloneContext context)
            => this.CopyFromBase(copyFrom, context);

        /// <inheritdoc/>
        public virtual void CopyFrom(MixerState<TParameter> copyFrom, CloneContext context)
        {
            base.CopyFrom(copyFrom, context);

            var childCount = copyFrom.ChildCount;
            if (copyFrom._Thresholds != null)
            {
                if (_Thresholds == null || _Thresholds.Length != childCount)
                    _Thresholds = new TParameter[childCount];

                var count = Math.Min(childCount, copyFrom._Thresholds.Length);
                Array.Copy(copyFrom._Thresholds, _Thresholds, count);
            }

            Parameter = copyFrom.Parameter;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Descriptions
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string GetDisplayKey(AnimancerState state)
            => $"[{state.Index}] {_Thresholds[state.Index]}";

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string separator)
        {
            text.Append(separator)
                .Append($"{nameof(Parameter)}: ");
            AppendParameter(text, Parameter);

            text.Append(separator)
                .Append("Thresholds: ");

            var thresholdCount = Math.Min(ChildCapacity, _Thresholds.Length);
            for (int i = 0; i < thresholdCount; i++)
            {
                if (i > 0)
                    text.Append(", ");

                AppendParameter(text, _Thresholds[i]);
            }

            base.AppendDetails(text, separator);
        }

        /************************************************************************************************************************/

        /// <summary>Appends the `parameter` in a viewer-friendly format.</summary>
        public virtual void AppendParameter(StringBuilder description, TParameter parameter)
        {
            description.Append(parameter);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

