// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System.Reflection;
using UnityEditor;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Utilities for using <see cref="DefaultValueAttribute"/>s.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/DefaultValues
    public static class DefaultValues
    {
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// If the field represented by the `property` has a <see cref="DefaultValueAttribute"/>,
        /// this method sets the `value` to its <see cref="DefaultValueAttribute.Primary"/> value.
        /// If it was already at the value, it sets it to the <see cref="DefaultValueAttribute.Secondary"/>
        /// value instead. And if the field has no attribute, it uses the default for the type.
        /// </summary>
        public static void SetToDefault<T>(ref T value, SerializedProperty property)
        {
            var accessor = property.GetAccessor();
            var field = accessor.GetField(property);
            if (field == null)
                accessor.SetValue(property, null);
            else
                SetToDefault(ref value, field);
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// If the field represented by the `property` has a <see cref="DefaultValueAttribute"/>,
        /// this method sets the `value` to its <see cref="DefaultValueAttribute.Primary"/> value.
        /// If it was already at the value, it sets it to the <see cref="DefaultValueAttribute.Secondary"/>
        /// value instead. And if the field has no attribute, it uses the default for the type.
        /// </summary>
        public static void SetToDefault<T>(ref T value, FieldInfo field)
        {
            var defaults = field.GetAttribute<DefaultValueAttribute>();
            if (defaults != null)
                defaults.SetToDefault(ref value);
            else
                value = default;
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Sets the `value` equal to the <see cref="DefaultValueAttribute.Primary"/> value.
        /// If it was already at the value, it sets it equal to the <see cref="DefaultValueAttribute.Secondary"/>
        /// value instead.
        /// </summary>
        public static void SetToDefault<T>(this DefaultValueAttribute attribute, ref T value)
        {
            var primary = attribute.Primary;
            if (!Equals(value, primary))
            {
                value = (T)primary;
                return;
            }

            var secondary = attribute.Secondary;
            if (secondary != null || !typeof(T).IsValueType)
            {
                value = (T)secondary;
                return;
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Sets the `value` equal to the `primary` value.
        /// If it was already at the value, it sets it equal to the `secondary` value instead.
        /// </summary>
        public static void SetToDefault<T>(ref T value, T primary, T secondary)
        {
            if (!Equals(value, primary))
                value = primary;
            else
                value = secondary;
        }

        /************************************************************************************************************************/
    }
}

#endif

