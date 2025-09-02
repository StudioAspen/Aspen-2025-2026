// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_ASSERTIONS
#define ANIMANCER_DEBUG_PARAMETERS
#endif

using System;
using UnityEngine;

namespace Animancer
{
    /// <summary>Manages two <see cref="SmoothedFloatParameter"/>s as a <see cref="Vector2"/>.</summary>
    /// 
    /// <remarks>
    /// <strong>Sample:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/samples/mixers/directional">
    /// Directional Mixers</see>
    /// <para></para>
    /// <strong>Example:</strong>
    /// <code>
    /// [SerializeField] private AnimancerComponent _Animancer;
    /// [SerializeField] private StringAsset _ParameterX;
    /// [SerializeField] private StringAsset _ParameterY;
    /// [SerializeField, Seconds] private float _ParameterSmoothTime;
    /// 
    /// private SmoothedVector2Parameter _SmoothedParameters;
    /// 
    /// protected virtual void Awake()
    /// {
    ///     _SmoothedParameters = new SmoothedVector2Parameter(
    ///         _Animancer,
    ///         _ParameterX,
    ///         _ParameterY,
    ///         _ParameterSmoothTime);
    /// }
    /// 
    /// protected virtual void Update()
    /// {
    ///     _SmoothedParameters.TargetValue = new Vector2(...);
    /// }
    /// 
    /// protected virtual void OnDestroy()
    /// {
    ///     _SmoothedParameters.Dispose();
    /// }
    /// </code>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/SmoothedVector2Parameter
    public class SmoothedVector2Parameter : IDisposable
    {
        /************************************************************************************************************************/

        /// <summary>The <see cref="Vector2.x"/> parameter.</summary>
        public readonly SmoothedFloatParameter X;

        /// <summary>The <see cref="Vector2.y"/> parameter.</summary>
        public readonly SmoothedFloatParameter Y;

        /************************************************************************************************************************/

        /// <summary>The amount of time allowed to smooth out a value change.</summary>
        /// <remarks>The getter returns the value from <see cref="X"/> but the setter sets both parameters.</remarks>
        public float SmoothTime
        {
            get => X.SmoothTime;
            set
            {
                X.CurrentValue = value;
                Y.CurrentValue = value;
            }
        }

        /// <summary>The maximum speed that the current value can move towards the target.</summary>
        /// <remarks>The getter returns the value from <see cref="X"/> but the setter sets both parameters.</remarks>
        public float MaxSpeed
        {
            get => X.MaxSpeed;
            set
            {
                X.MaxSpeed = value;
                Y.MaxSpeed = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The value that the parameters are moving towards.</summary>
        public Vector2 CurrentValue
        {
            get => new(X.CurrentValue, Y.CurrentValue);
            set
            {
                X.CurrentValue = value.x;
                Y.CurrentValue = value.y;
            }
        }

        /// <summary>The value that the parameters are moving towards.</summary>
        public Vector2 TargetValue
        {
            get => new(X.TargetValue, Y.TargetValue);
            set
            {
                X.TargetValue = value.x;
                Y.TargetValue = value.y;
            }
        }

        /// <summary>The speed at which the parameters are currently moving.</summary>
        public Vector2 Velocity
        {
            get => new(X.Velocity, Y.Velocity);
            set
            {
                X.Velocity = value.x;
                Y.Velocity = value.y;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="SmoothedVector2Parameter"/>.</summary>
        public SmoothedVector2Parameter(
            SmoothedFloatParameter x,
            SmoothedFloatParameter y)
        {
#if UNITY_ASSERTIONS
            AnimancerUtilities.Assert(x != null, $"{nameof(x)} is null.");
            AnimancerUtilities.Assert(y != null, $"{nameof(y)} is null.");
#endif

            X = x;
            Y = y;
        }

        /// <summary>Creates a new <see cref="SmoothedVector2Parameter"/>.</summary>
        public SmoothedVector2Parameter(
            AnimancerGraph graph,
            StringReference keyX,
            StringReference keyY,
            float smoothTime,
            float maxSpeed = float.PositiveInfinity)
        {
            X = new(graph, keyX, smoothTime, maxSpeed);
            Y = new(graph, keyY, smoothTime, maxSpeed);
        }

        /************************************************************************************************************************/

        /// <summary>Disposes the <see cref="X"/> and <see cref="Y"/>.</summary>
        public void Dispose()
        {
            X.Dispose();
            Y.Dispose();
        }

        /************************************************************************************************************************/
    }
}

