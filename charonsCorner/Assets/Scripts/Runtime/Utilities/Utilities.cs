using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public static class Utilities 
    {
        /// <summary>
        /// Null propogation check for Unity object methods.
        /// Equivalent to 'obj?.Method()' but Unity safe.
        /// </summary>
        public static T IfNotNullThenCall<T>(this T obj, Action<T> action) where T : UnityEngine.Object
        {
            if (obj != null)
                action(obj);
            return obj;
        }

        /// <summary>
        /// Null propogation check for Unity object methods/properties that return a value.
        /// Equivalent to 'obj?.Method()' or 'obj?.Property' but Unity safe.
        /// </summary>
        public static TResult IfNotNullThenGet<T, TResult>(this T obj, Func<T, TResult> func) where T : UnityEngine.Object
        {
            return obj != null ? func(obj) : default;
        }
    }
}
