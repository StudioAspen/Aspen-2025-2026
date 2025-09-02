// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>
    /// Holds an instance of <typeparamref name="T"/> which is automatically created
    /// using its parameterless constructor when first accessed.
    /// </summary>
    /// <remarks>
    /// Don't use classes that inherit from <see cref="Object"/> as <typeparamref name="T"/>.
    /// <para></para>
    /// This is close to the "Singleton Programming Pattern", except it can't prevent additional instances of
    /// <typeparamref name="T"/> from being created elsewhere.
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/Static_1
    public static class Static<T>
        where T : class, new()
    {
        /************************************************************************************************************************/

        /// <summary>
        /// An instance of <typeparamref name="T"/> which is automatically created
        /// using its parameterless constructor when first accessed.
        /// </summary>
        public static readonly T Instance = new();

        /************************************************************************************************************************/

        /// <summary>Ensures that the <see cref="Instance"/> has been created without immediately using it.</summary>
        public static void Initialize() { }

        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        static Static()
        {
            if (Instance is Object)
            {
                Debug.LogError(
                    $"{typeof(Static<T>).GetNameCS()} type is invalid:" +
                    $" {nameof(UnityEngine)}.{nameof(Object)} types require special memory management by Unity" +
                    $" which may cause problems when used with this system.",
                    Instance as Object);
            }
        }
#endif

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional]
        /// Call this in the constructor of <typeparamref name="T"/> to make sure there is only one instance.
        /// </summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertSingularity()
        {
#if UNITY_ASSERTIONS
            // If the static instance has already been set, then this is not the first one to be created.
            if (Instance != null)
            {
                var name = typeof(T).GetNameCS();
                throw new InvalidOperationException(
                    $"Multiple {name} objects have been created." +
                    $"\nUse Static<{name}>.Instance instead of creating your own instances.");
            }
#endif
        }

        /************************************************************************************************************************/
    }
}

