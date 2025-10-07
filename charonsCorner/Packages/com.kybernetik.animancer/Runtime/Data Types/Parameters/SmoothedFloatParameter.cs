// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_ASSERTIONS
#define ANIMANCER_DEBUG_PARAMETERS
#endif

using System;
using UnityEngine;

namespace Animancer
{
    /// <summary>
    /// A wrapper around a <see cref="Parameter{T}"/> containing a <see cref="float"/>
    /// which uses <see cref="Mathf.SmoothDamp(float, float, ref float, float, float, float)"/>
    /// to smoothly update its value.
    /// </summary>
    /// 
    /// <remarks>
    /// <strong>Sample:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/samples/mixers/directional">
    /// Directional Mixers</see> (uses <see cref="SmoothedVector2Parameter"/> which is similar).
    /// <para></para>
    /// <strong>Example:</strong>
    /// <code>
    /// [SerializeField] private AnimancerComponent _Animancer;
    /// [SerializeField] private StringAsset _Parameter;
    /// [SerializeField, Seconds] private float _ParameterSmoothTime;
    /// 
    /// private SmoothedFloatParameter _SmoothedParameter;
    /// 
    /// protected virtual void Awake()
    /// {
    ///     _SmoothedParameter = new SmoothedFloatParameter(
    ///         _Animancer,
    ///         _Parameter,
    ///         _ParameterSmoothTime);
    /// }
    /// 
    /// protected virtual void Update()
    /// {
    ///     _SmoothedParameter.TargetValue = ...;
    /// }
    /// 
    /// protected virtual void OnDestroy()
    /// {
    ///     _SmoothedParameter.Dispose();
    /// }
    /// </code>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/SmoothedFloatParameter
    public class SmoothedFloatParameter : Updatable, IDisposable
    {
        /************************************************************************************************************************/

        private bool _IsChangingValue;
        private float _CurrentValue;
        private float _TargetValue;
        private float _Velocity;

        /************************************************************************************************************************/

        /// <summary>The graph containing the <see cref="Parameter"/>.</summary>
        public readonly AnimancerGraph Graph;

        /// <summary>The target parameter being damped.</summary>
        public readonly Parameter<float> Parameter;

        /// <summary>The amount of time allowed to smooth out a value change.</summary>
        public float SmoothTime { get; set; }

        /// <summary>The maximum speed that the current value can move towards the target.</summary>
        public float MaxSpeed { get; set; }

        /************************************************************************************************************************/

        /// <summary>The value that the parameter is moving towards.</summary>
        public float CurrentValue
        {
            get => _CurrentValue;
            set
            {
                _CurrentValue = _TargetValue = value;
                _Velocity = 0;
                Graph.CancelPreUpdate(this);

                _IsChangingValue = true;
                Parameter.Value = value;
                _IsChangingValue = false;
            }
        }

        /// <summary>The value that the parameter is moving towards.</summary>
        public float TargetValue
        {
            get => _TargetValue;
            set
            {
                _TargetValue = value;

                if (value != _CurrentValue)
                    Graph.RequirePreUpdate(this);
            }
        }

        /// <summary>The speed at which the value is currently moving.</summary>
        public float Velocity
        {
            get => _Velocity;
            set
            {
                _Velocity = value;

                if (value != 0)
                    Graph.RequirePreUpdate(this);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="SmoothedFloatParameter"/>.</summary>
        public SmoothedFloatParameter(
            AnimancerGraph graph,
            Parameter<float> parameter,
            float smoothTime,
            float maxSpeed = float.PositiveInfinity)
        {
#if UNITY_ASSERTIONS
            AnimancerUtilities.Assert(graph != null, $"{nameof(graph)} is null.");
            AnimancerUtilities.Assert(parameter != null, $"{nameof(parameter)} is null.");
#endif

            Graph = graph;
            Parameter = parameter;
            SmoothTime = smoothTime;
            MaxSpeed = maxSpeed;

            _CurrentValue = TargetValue = parameter.Value;

            parameter.OnValueChanged += OnValueChanged;
        }

        /// <summary>Creates a new <see cref="SmoothedFloatParameter"/>.</summary>
        public SmoothedFloatParameter(
            AnimancerGraph graph,
            StringReference key,
            float smoothTime,
            float maxSpeed = float.PositiveInfinity)
            : this(graph, graph.Parameters.GetOrCreate<float>(key), smoothTime, maxSpeed)
        {
        }

        /************************************************************************************************************************/

        /// <summary>Unbinds this smoother from the <see cref="Parameter"/>.</summary>
        public void Dispose()
        {
            Parameter.OnValueChanged -= OnValueChanged;
            Graph.CancelPreUpdate(this);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Ignores changes made by this system,
        /// but uses any others to set the <see cref="TargetValue"/>.
        /// </summary>
        private void OnValueChanged(float value)
        {
            if (_IsChangingValue)
                return;

            TargetValue = value;

            _IsChangingValue = true;
            Parameter.Value = _CurrentValue;
            _IsChangingValue = false;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Update()
        {
            var deltaTime = AnimancerGraph.DeltaTime;

            _CurrentValue = Mathf.SmoothDamp(
                _CurrentValue,
                TargetValue,
                ref _Velocity,
                SmoothTime,
                MaxSpeed,
                deltaTime);

            _IsChangingValue = true;

            const float StopThreshold = 0.01f;

            if (Math.Abs(_CurrentValue - TargetValue) < StopThreshold &&
                Math.Abs(_Velocity) < StopThreshold)
            {
                Graph.CancelPreUpdate(this);
                _CurrentValue = TargetValue;
            }

            Parameter.Value = _CurrentValue;

            _IsChangingValue = false;
        }

        /************************************************************************************************************************/
    }
}

