// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Runtime.CompilerServices;
using UnityEngine;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    partial struct AnimancerEvent
    {
        /// <summary>Executes <see cref="Invoker.InvokeAllAndClear"/> after animations in the Dynamic Update cycle.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/InvokerDynamic
        [AnimancerHelpUrl(typeof(InvokerDynamic))]
        [AddComponentMenu("")]// Singleton creates itself.
        public class InvokerDynamic : Invoker
        {
            /************************************************************************************************************************/

            private static InvokerDynamic _Instance;

            /// <summary>Creates the singleton instance.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static InvokerDynamic Initialize()
                => AnimancerUtilities.InitializeSingleton(ref _Instance);

            /************************************************************************************************************************/

            /// <summary>Should this system execute events?</summary>
            /// <remarks>If disabled, this system will not be re-enabled automatically.</remarks>
            public static bool Enabled
            {
                get => _Instance != null && _Instance.enabled;
                set
                {
                    if (value)
                    {
                        Initialize();
                        _Instance.enabled = true;
                    }
                    else if (_Instance != null)
                    {
                        _Instance.enabled = false;
                    }
                }
            }

            /************************************************************************************************************************/

            /// <summary>After animation update with dynamic timestep.</summary>
            protected virtual void LateUpdate()
            {
                InvokeAllAndClear();
            }

            /************************************************************************************************************************/
        }
    }
}

