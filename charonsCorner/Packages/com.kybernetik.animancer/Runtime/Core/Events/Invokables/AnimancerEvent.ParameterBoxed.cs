// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    partial struct AnimancerEvent
    {
        /// <summary>
        /// An <see cref="Parameter{T}"/>s which internally boxes value types
        /// to avoid re-boxing them every <see cref="Invoke"/>.
        /// </summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/ParameterBoxed_1
        public abstract class ParameterBoxed<T> : Parameter<T>,
        IParameter
#if UNITY_EDITOR
            , ISerializationCallbackReceiver
#endif
            where T : struct
        {
            /************************************************************************************************************************/

            private object _Boxed;

            /// <inheritdoc/>
            public override T Value
            {
                get => base.Value;
                set
                {
                    base.Value = value;
                    _Boxed = null;
                }
            }

            /// <inheritdoc/>
            object IParameter.Value
            {
                get => Value;
                set => Value = (T)value;
            }

            /// <inheritdoc/>
            public override void Invoke()
            {
                CurrentParameter = _Boxed ??= base.Value;
                Current.InvokeBoundCallback();
            }

            /************************************************************************************************************************/
#if UNITY_EDITOR
            /************************************************************************************************************************/

            /// <inheritdoc/>
            void ISerializationCallbackReceiver.OnBeforeSerialize() { }

            /// <inheritdoc/>
            void ISerializationCallbackReceiver.OnAfterDeserialize()
                => _Boxed = null;

            /************************************************************************************************************************/
#endif
            /************************************************************************************************************************/
        }
    }
}

