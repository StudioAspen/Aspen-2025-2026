// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    partial struct AnimancerEvent
    {
        /// <summary>Executes <see cref="Invoker.InvokeAllAndClear"/> after animations in the Fixed Update cycle.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/InvokerFixed
        [AnimancerHelpUrl(typeof(InvokerFixed))]
        [AddComponentMenu("")]// Singleton creates itself.
        public class InvokerFixed : Invoker
        {
            /************************************************************************************************************************/

            private static InvokerFixed _Instance;

            /// <summary>Creates the singleton instance.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static InvokerFixed Initialize()
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

            /// <summary>A cached instance of <see cref="UnityEngine.WaitForFixedUpdate"/>.</summary>
            public static readonly WaitForFixedUpdate
                WaitForFixedUpdate = new();

            /************************************************************************************************************************/

            /// <summary>Starts the <see cref="LateFixedUpdate"/> coroutine.</summary>
            protected virtual void OnEnable()
                => StartCoroutine(LateFixedUpdate());

            /************************************************************************************************************************/

            /// <summary>After animation update with fixed timestep.</summary>
            private IEnumerator LateFixedUpdate()
            {
                while (true)
                {
                    yield return WaitForFixedUpdate;
                    InvokeAllAndClear();
                }
            }

            /************************************************************************************************************************/
        }
    }
}

