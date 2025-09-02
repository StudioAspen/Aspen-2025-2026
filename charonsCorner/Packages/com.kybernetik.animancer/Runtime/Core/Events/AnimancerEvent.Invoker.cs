// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    partial struct AnimancerEvent
    {
        /************************************************************************************************************************/

        /// <summary>Events ready to be invoked by the next <see cref="Invoker.InvokeAllAndClear"/>.</summary>
        /// <remarks>
        /// This field should be inside the Invoker class.
        /// But that can potentially cause a TypeLoadException if Invoker initializes before AnimancerEvent.
        /// Having it out in AnimancerEvent avoids that possibility.
        /// </remarks>
        private static readonly List<Invocation>
            InvocationQueue = new();

        /************************************************************************************************************************/

        /// <summary>Gathers delegates in a static list to be invoked at a later time by any child class.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/Invoker
        [DefaultExecutionOrder(-30000)]// Run as soon as possible in whatever update cycle is being executed.
        [ExecuteAlways]
        public abstract class Invoker : MonoBehaviour
        {
            /************************************************************************************************************************/

            /// <summary>Ensures that an appropriate <see cref="Invoker"/> has been created.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Invoker Initialize(bool fixedUpdate)
                => fixedUpdate
                ? InvokerFixed.Initialize()
                : InvokerDynamic.Initialize();

            /// <summary>Ensures that an appropriate <see cref="Invoker"/> has been created.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Invoker Initialize(AnimatorUpdateMode updateMode)
            {
                const AnimatorUpdateMode FixedUpdateMode =
#if UNITY_2023_1_OR_NEWER
                    AnimatorUpdateMode.Fixed;
#else
                    AnimatorUpdateMode.AnimatePhysics;
#endif

                return Initialize(updateMode == FixedUpdateMode);

            }

            /************************************************************************************************************************/

            /// <summary>[Internal] Adds an event to the queue to be invoked by the next update.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Add(Invocation invocation)
            {
#if UNITY_ASSERTIONS
                if (!HasEnabledInstance)
                    Debug.LogWarning(
                        $"There is no currently enabled {nameof(AnimancerEvent)}.{nameof(Invoker)}" +
                        $" so events will not be invoked.");
#endif

                InvocationQueue.Add(invocation);
            }

            /************************************************************************************************************************/

            /// <summary>
            /// In case <see cref="InvokeAllAndClear"/> gets called recursively,
            /// we need to avoid invoking the same event multiple times
            /// without the performance cost of immediately removing them each from the queue.
            /// </summary>
            private static int _CurrentInvocation;

            /// <summary>Invokes all queued events and clears the queue.</summary>
            public static void InvokeAllAndClear()
            {
                while (_CurrentInvocation < InvocationQueue.Count)
                    InvocationQueue[_CurrentInvocation++].Invoke();

                InvocationQueue.Clear();
                _CurrentInvocation = 0;
            }

            /************************************************************************************************************************/

            /// <summary>Returns an enumerator for all invocations currently in the queue.</summary>
            public static List<Invocation>.Enumerator EnumerateInvocationQueue()
                => InvocationQueue.GetEnumerator();

            /************************************************************************************************************************/
#if UNITY_ASSERTIONS
            /************************************************************************************************************************/

            private static readonly List<Invoker>
                Instances = new();

            /************************************************************************************************************************/

            /// <summary>[Assert-Only] Registers this instance.</summary>
            protected virtual void Awake()
                => Instances.Add(this);

            /************************************************************************************************************************/

            /// <summary>[Assert-Only] Un-registers this instance.</summary>
            protected virtual void OnDestroy()
                => Instances.Remove(this);

            /************************************************************************************************************************/

            /// <summary>[Assert-Only] Is there any <see cref="Behaviour.enabled"/> instance?</summary>
            private static bool HasEnabledInstance
            {
                get
                {
                    for (int i = 0; i < Instances.Count; i++)
                        if (Instances[i].enabled)
                            return true;

                    return false;
                }
            }

            /************************************************************************************************************************/
#endif
            /************************************************************************************************************************/
        }
    }
}

