// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    /// <summary>An object which can be converted to another type.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/IConvertable_1
    public interface IConvertable<out T>
    {
        /************************************************************************************************************************/

        /// <summary>Returns the equivalent of this object as <typeparamref name="T"/>.</summary>
        T Convert();

        /************************************************************************************************************************/
    }

    /// <summary>Utility methods for <see cref="IConvertable{T}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/ConvertableUtilities
    public static partial class ConvertableUtilities
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Custom conversion functions used as a fallback
        /// for types that can't implement <see cref="IConvertable{T}"/>.
        /// </summary>
        public static readonly Dictionary<Type, Func<object, Type, object>>
            CustomConverters = new();

        /************************************************************************************************************************/

        /// <summary>Tries to convert the `original` to <typeparamref name="T"/>.</summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>If the `original` is already a <typeparamref name="T"/> then it's returned directly.</item>
        /// <item>Or if it's an <see cref="IConvertable{T}"/> then <see cref="IConvertable{T}.Convert"/> is used.</item>
        /// <item>Otherwise, this method throws an <see cref="ArgumentException"/>.</item>
        /// </list>
        /// </remarks>
        public static T ConvertOrThrow<T>(object original)
        {
            if (TryConvert<T>(original, out var converted))
                return converted;

            throw new ArgumentException(
                $"Unable to convert '{AnimancerUtilities.ToStringOrNull(original)}'" +
                $" to '{typeof(T).GetNameCS()}'.");
        }

        /************************************************************************************************************************/

        /// <summary>Tries to convert the `original` to <typeparamref name="T"/>.</summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>If the `original` is already a <typeparamref name="T"/> then it's returned directly.</item>
        /// <item>Or if it's an <see cref="IConvertable{T}"/> then <see cref="IConvertable{T}.Convert"/> is used.</item>
        /// <item>Otherwise, this method returns the <c>default(T)</c>.</item>
        /// </list>
        /// </remarks>
        public static T ConvertOrDefault<T>(object original)
        {
            TryConvert<T>(original, out var converted);
            return converted;
        }

        /************************************************************************************************************************/

        /// <summary>Tries to convert the `original` to <typeparamref name="T"/>.</summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>If the `original` is already a <typeparamref name="T"/> then it's returned directly.</item>
        /// <item>Or if it's an <see cref="IConvertable{T}"/> then <see cref="IConvertable{T}.Convert"/> is used.</item>
        /// <item>Otherwise, this method returns <c>false</c>.</item>
        /// </list>
        /// </remarks>
        public static bool TryConvert<T>(object original, out T converted)
        {
            if (original is null)
            {
                converted = default;
                return converted == null;// True for value type, false for reference type.
            }

            if (original is T t)
            {
                converted = t;
                return true;
            }

            if (original is IConvertable<T> convertable)
            {
                converted = convertable.Convert();
                return true;
            }

            if (CustomConverters.TryGetValue(original.GetType(), out var converter))
            {
                converted = (T)converter(original, typeof(T));
                return converted != null;
            }

            converted = default;
            return false;
        }

        /************************************************************************************************************************/

        /// <summary>Initializes the inbuilt custom converters.</summary>
        static ConvertableUtilities()
        {
            CustomConverters.Add(typeof(GameObject), TryGetComponent);
        }

        /************************************************************************************************************************/

        /// <summary>Tries to get a component if the `original` is a <see cref="GameObject"/>.</summary>
        private static object TryGetComponent(object original, Type type)
        {
            if (original is GameObject gameObject &&
                gameObject.TryGetComponent(type, out var component))
                return component;

            return null;
        }

        /************************************************************************************************************************/
    }
}

