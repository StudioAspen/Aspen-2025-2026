// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using UnityEngine;

namespace Animancer
{
    /// <summary>An object which wraps a <see cref="WrappedObject"/> object.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/IWrapper
    public interface IWrapper
    {
        /************************************************************************************************************************/

        /// <summary>The wrapped object.</summary>
        /// <remarks>
        /// Use <see cref="AnimancerUtilities.TryGetWrappedObject"/>
        /// in case the <see cref="WrappedObject"/> is also an <see cref="IWrapper"/>.
        /// </remarks>
        object WrappedObject { get; }

        /************************************************************************************************************************/
    }

    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerUtilities
    public partial class AnimancerUtilities
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Returns the last <see cref="IWrapper.WrappedObject"/>
        /// which is a <typeparamref name="T"/>, including the `wrapper` itself.
        /// </summary>
        public static bool TryGetWrappedObject<T>(
            object wrapper,
            out T wrapped,
            bool logException = false)
            where T : class
        {
            wrapped = default;

            while (true)
            {
                if (wrapper is T t)
                    wrapped = t;

                if (wrapper is IWrapper targetWrapper)
                {
                    try
                    {
                        wrapper = targetWrapper.WrappedObject;
                    }
                    catch (Exception exception)
                    {
                        if (logException)
                            Debug.LogException(exception);

                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return wrapped != null;
        }

        /************************************************************************************************************************/
    }
}

