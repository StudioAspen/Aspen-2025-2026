// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if UNITY_6000_0_OR_NEWER
using GetDrawerTypeForTypeDelegate = System.Func<System.Type, System.Type[], bool, System.Type>;
#elif UNITY_2022_3_OR_NEWER
using GetDrawerTypeForTypeDelegate = System.Func<System.Type, bool, System.Type>;
#else
using GetDrawerTypeForTypeDelegate = System.Func<System.Type, System.Type>;
#endif

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A cache of <see cref="PropertyDrawer"/>s mapped to their target type.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/PropertyDrawers
    public static class PropertyDrawers
    {
        /************************************************************************************************************************/

        private const string
            ScriptAttributeUtility = "UnityEditor.ScriptAttributeUtility",
            FieldFieldName = "m_FieldInfo",
            AttributeFieldName = "m_Attribute";

        private static readonly GetDrawerTypeForTypeDelegate
            GetDrawerTypeForType;

        private static readonly FieldInfo
            FieldField,
            AttributeField;

        /************************************************************************************************************************/

        static PropertyDrawers()
        {
            var editorAssembly = typeof(CustomPropertyDrawer).Assembly;
            var scriptAttributeUtility = editorAssembly.GetType(ScriptAttributeUtility);
            if (scriptAttributeUtility == null)
                return;

            var getDrawerTypeForType = scriptAttributeUtility.GetMethod(
                nameof(GetDrawerTypeForType),
                AnimancerReflection.StaticBindings,
                null,
#if UNITY_6000_0_OR_NEWER
                new Type[] { typeof(Type), typeof(Type[]), typeof(bool) },
#elif UNITY_2022_3_OR_NEWER
                new Type[] { typeof(Type), typeof(bool) },
#else
                new Type[] { typeof(Type) },
#endif
                null);
            if (getDrawerTypeForType == null)
                return;

            GetDrawerTypeForType = (GetDrawerTypeForTypeDelegate)Delegate.CreateDelegate(
                typeof(GetDrawerTypeForTypeDelegate),
                getDrawerTypeForType);

            var propertyDrawer = typeof(PropertyDrawer);
            FieldField = propertyDrawer.GetField(FieldFieldName, AnimancerReflection.InstanceBindings);
            AttributeField = propertyDrawer.GetField(AttributeFieldName, AnimancerReflection.InstanceBindings);

            Selection.selectionChanged += OnSelectionChanged;
        }

        /************************************************************************************************************************/

        private static readonly Dictionary<Type, PropertyDrawer>
            ObjectTypeToDrawer = new();

        private static readonly Dictionary<Type, PropertyDrawer>
            DrawerTypeToInstance = new();

        /// <summary>Tries to get a <see cref="PropertyDrawer"/> for the given `objectType`.</summary>
        public static bool TryGetDrawer(
            Type objectType,
            FieldInfo field,
            Attribute attribute,
            out PropertyDrawer drawer)
        {
            if (GetDrawerTypeForType == null)
            {
                drawer = null;
                return false;
            }

            if (ObjectTypeToDrawer.TryGetValue(objectType, out drawer))
                return true;

            Type drawerType;
            try
            {
#if UNITY_6000_0_OR_NEWER
                drawerType = GetDrawerTypeForType(objectType, Type.EmptyTypes, true);
#elif UNITY_2022_3_OR_NEWER
                drawerType = GetDrawerTypeForType(objectType, true);
#else
                drawerType = GetDrawerTypeForType(objectType);
#endif
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ObjectTypeToDrawer.Add(objectType, null);
                return false;
            }

            if (DrawerTypeToInstance.TryGetValue(drawerType, out drawer))
            {
                ObjectTypeToDrawer.Add(objectType, drawer);
                return true;
            }

            try
            {
                drawer = (PropertyDrawer)Activator.CreateInstance(drawerType);

                FieldField?.SetValue(drawer, field);
                AttributeField?.SetValue(drawer, attribute);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            ObjectTypeToDrawer.Add(objectType, drawer);
            DrawerTypeToInstance.Add(drawerType, drawer);
            return drawer != null;
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Indicates that a cached <see cref="PropertyDrawer"/>
        /// should not be kept in the cache after the selection changes.
        /// </summary>
        /// https://kybernetik.com.au/animancer/api/Animancer.Editor/IDiscardOnSelectionChange
        public interface IDiscardOnSelectionChange { }

        /************************************************************************************************************************/

        /// <summary>Discards any cached <see cref="IDiscardOnSelectionChange"/> drawers.</summary>
        private static void OnSelectionChanged()
        {
            DiscardOnSelectionChanged(ObjectTypeToDrawer);
            DiscardOnSelectionChanged(DrawerTypeToInstance);
        }

        /************************************************************************************************************************/

        /// <summary>Discards any cached <see cref="IDiscardOnSelectionChange"/> drawers.</summary>
        private static void DiscardOnSelectionChanged(Dictionary<Type, PropertyDrawer> drawers)
        {
            var discard = ListPool<Type>.Instance.Acquire();

            foreach (var drawer in drawers)
                if (drawer.Value is IDiscardOnSelectionChange)
                    discard.Add(drawer.Key);

            for (int i = discard.Count - 1; i >= 0; i--)
                drawers.Remove(discard[i]);

            ListPool<Type>.Instance.Release(discard);
        }

        /************************************************************************************************************************/
    }
}

#endif

