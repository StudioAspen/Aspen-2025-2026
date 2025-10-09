// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    partial struct AnimancerEvent
    {
        /// <summary>A non-generic interface for <see cref="Parameter{T}"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/IParameter
        public interface IParameter
        {
            /************************************************************************************************************************/

            /// <summary>The parameter value.</summary>
            object Value { get; set; }

            /************************************************************************************************************************/
        }

        /// <summary>
        /// Base class for <see cref="IInvokable"/>s which assign the <see cref="CurrentParameter"/>.
        /// </summary>
        /// <remarks>
        /// Inherit from <see cref="ParameterBoxed{T}"/>
        /// instead of this if <typeparamref name="T"/> is a value type to avoid repeated boxing costs.
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Parameter_1
        public abstract class Parameter<T> :
            IParameter,
            IInvokable
        {
            /************************************************************************************************************************/

            [SerializeField]
            private T _Value;

            /// <summary>[<see cref="SerializeField"/>] The serialized <typeparamref name="T"/>.</summary>
            public virtual T Value
            {
                get => _Value;
                set => _Value = value;
            }

            /// <inheritdoc/>
            object IParameter.Value
            {
                get => _Value;
                set => _Value = (T)value;
            }

            /// <inheritdoc/>
            public virtual void Invoke()
            {
                CurrentParameter = _Value;
                Current.InvokeBoundCallback();
            }

            /************************************************************************************************************************/
        }
    }
}

