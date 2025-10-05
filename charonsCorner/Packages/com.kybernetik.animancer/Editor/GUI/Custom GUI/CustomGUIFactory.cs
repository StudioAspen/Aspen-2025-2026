// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

//#define LOG_CUSTOM_GUI_FACTORY

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws a custom GUI for an object.</summary>
    /// <remarks>
    /// Every non-abstract type implementing this interface must have at least one <see cref="CustomGUIAttribute"/>.
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/CustomGUIFactory
    /// 
    public static class CustomGUIFactory
    {
        /************************************************************************************************************************/

        private static readonly Dictionary<Type, Type>
            TargetTypeToGUIType = new();

        static CustomGUIFactory()
        {
            foreach (var guiType in TypeCache.GetTypesWithAttribute(typeof(CustomGUIAttribute)))
            {
                if (guiType.IsAbstract ||
                    guiType.IsInterface)
                    continue;

                if (!typeof(ICustomGUI).IsAssignableFrom(guiType))
                {
                    Debug.LogWarning(
                        $"{guiType.FullName} has a {nameof(CustomGUIAttribute)}" +
                        $" but doesn't implement {nameof(ICustomGUI)}.");
                    continue;
                }

                var attribute = guiType.GetCustomAttribute<CustomGUIAttribute>();
                if (attribute.TargetType != null)
                {

                    TargetTypeToGUIType.Add(attribute.TargetType, guiType);
                }
            }
        }

        /************************************************************************************************************************/

        private static readonly ConditionalWeakTable<object, ICustomGUI>
            TargetToGUI = new();

        /// <summary>Returns an existing <see cref="ICustomGUI"/> for the `targetType` or creates one if necessary.</summary>
        /// <remarks>Returns null if the `targetType` is null or no valid <see cref="ICustomGUI"/> type is found.</remarks>
        public static ICustomGUI GetOrCreateForType(Type targetType)
        {
            if (targetType == null)
                return null;

            if (TargetToGUI.TryGetValue(targetType, out var gui))
                return gui;

            gui = Create(targetType);

            TargetToGUI.Add(targetType, gui);

            return gui;
        }

        /// <summary>Returns an existing <see cref="ICustomGUI"/> for the `value` or creates one if necessary.</summary>
        /// <remarks>Returns null if the `value` is null or no valid <see cref="ICustomGUI"/> type is found.</remarks>
        public static ICustomGUI GetOrCreateForObject(object value)
        {
            if (value == null)
                return null;

            if (TargetToGUI.TryGetValue(value, out var gui))
                return gui;

            gui = Create(value.GetType());
            if (gui != null)
                gui.Value = value;

            TargetToGUI.Add(value, gui);
            return gui;
        }

        /************************************************************************************************************************/

        /// <summary>Creates an <see cref="ICustomGUI"/> for the `targetType`.</summary>
        /// <remarks>Returns null if the `value` is null or no valid <see cref="ICustomGUI"/> type is found.</remarks>
        public static ICustomGUI Create(Type targetType)
        {
            if (!TryGetGUIType(targetType, out var guiType))
                return null;

            try
            {
                if (guiType.IsGenericTypeDefinition)
                    guiType = guiType.MakeGenericType(targetType);

                var gui = (ICustomGUI)Activator.CreateInstance(guiType);

                return gui;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return null;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Tries to determine the valid <see cref="ICustomGUI"/> type for drawing the `target`.</summary>
        public static bool TryGetGUIType(Type target, out Type gui)
        {
            // Try the target and its base types.

            var type = target;
            while (type != null && type != typeof(object))
            {
                if (TargetTypeToGUIType.TryGetValue(type, out gui))
                    return true;

                type = type.BaseType;
            }

            // Try any interfaces.

            var interfaces = target.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
                if (TargetTypeToGUIType.TryGetValue(interfaces[i], out gui))
                    return true;

            // Try base object.

            return TargetTypeToGUIType.TryGetValue(typeof(object), out gui);
        }

        /************************************************************************************************************************/
    }
}

#endif

