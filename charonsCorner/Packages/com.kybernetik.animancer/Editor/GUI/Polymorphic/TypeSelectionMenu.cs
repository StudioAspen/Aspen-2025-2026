// Animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A context menu for selecting a <see cref="Type"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TypeSelectionMenu
    public static class TypeSelectionMenu
    {
        /************************************************************************************************************************/

        private const string
            PrefKeyPrefix = nameof(TypeSelectionMenu) + ".",
            PrefMenuPrefix = "Display Options/";

        /// <summary>Should shared references be shown in the GUI?</summary>
        public static readonly BoolPref
            VisualiseSharedReferences = new(
                PrefKeyPrefix + nameof(VisualiseSharedReferences),
                PrefMenuPrefix + "Visualise Shared References",
                true);

        /// <summary>Should full type names be displayed?</summary>
        public static readonly BoolPref
            UseFullNames = new(
                PrefKeyPrefix + nameof(UseFullNames),
                PrefMenuPrefix + "Show Full Names");

        /// <summary>Should options be grouped in sub menus based on their inheritance hierarchy?</summary>
        public static readonly BoolPref
            UseTypeHierarchy = new(
                PrefKeyPrefix + nameof(UseTypeHierarchy),
                PrefMenuPrefix + "Show Type Hierarchy");

        /************************************************************************************************************************/

        /// <summary>Shows a menu to select which type of object to assign to the `property`.</summary>
        public static void Show(SerializedProperty property)
        {
            var value = property.managedReferenceValue;
            var accessor = property.GetAccessor();
            var fieldType = accessor.GetFieldElementType(property);
            var selectedType = value?.GetType();

            var menu = new GenericMenu();

            AddPrefs(menu);
            AddDocumentation(menu, fieldType);

            menu.AddSeparator("");
            menu.AddDisabledItem(new(ObjectNames.NicifyVariableName(property.GetFriendlyPath())));
            menu.AddSeparator("");

            AddTypeSelector(menu, property, fieldType, selectedType, null);

            AddSharedReferences(menu, property, fieldType, value);

            var inheritors = GetDerivedTypes(fieldType);
            for (int i = 0; i < inheritors.Count; i++)
                AddTypeSelector(menu, property, fieldType, selectedType, inheritors[i]);

            menu.ShowAsContext();
        }

        /************************************************************************************************************************/

        /// <summary>Adds items for toggling the display options.</summary>
        private static void AddPrefs(GenericMenu menu)
        {
            VisualiseSharedReferences.AddToggleFunction(menu);
            UseFullNames.AddToggleFunction(menu);
            UseTypeHierarchy.AddToggleFunction(menu);
        }

        /************************************************************************************************************************/

        /// <summary>Adds an itme for opening the documentation if a <see cref="HelpURLAttribute"/> is present.</summary>
        private static void AddDocumentation(
            GenericMenu menu,
            Type fieldType)
        {
            var help = fieldType.GetCustomAttribute<HelpURLAttribute>();
            if (help == null ||
                string.IsNullOrWhiteSpace(help.URL))
                return;

            var label = $"Documentation: {help.URL.Replace('/', '\\')}";
            menu.AddItem(new(label), false, () => Application.OpenURL(help.URL));
        }

        /************************************************************************************************************************/

        /// <summary>Adds items for selecting shared references.</summary>
        private static void AddSharedReferences(
            GenericMenu menu,
            SerializedProperty property,
            Type fieldType,
            object currentValue)
        {
            foreach (var item in GetObjectsAndPaths(property.serializedObject, fieldType))
            {
                var label = $"Shared Reference/{ObjectNames.NicifyVariableName(item.Value)}";
                var state = item.Key == currentValue ? MenuFunctionState.Selected : MenuFunctionState.Normal;
                menu.AddPropertyModifierFunction(property, label, state, targetProperty =>
                {
                    targetProperty.managedReferenceValue = item.Key;
                });
            }
        }

        /************************************************************************************************************************/

        /// <summary>Gathers all potential references that could be shared.</summary>
        private static List<KeyValuePair<object, string>> GetObjectsAndPaths(
            SerializedObject serializedObject,
            Type fieldType)
        {
            var objectsAndPaths = new List<KeyValuePair<object, string>>();

            var referenceCache = SharedReferenceCache.Get(serializedObject);
            referenceCache.GatherReferences();

            foreach (var item in referenceCache)
            {
                if (!fieldType.IsAssignableFrom(item.Key.GetType()))
                    continue;

                foreach (var info in item.Value)
                {
                    objectsAndPaths.Add(new(item.Key, info.path));
                }
            }

            objectsAndPaths.Sort(
                (a, b) => Comparer<string>.Default.Compare(a.Value, b.Value));

            return objectsAndPaths;
        }

        /************************************************************************************************************************/

        /// <summary>Adds a menu function to assign a new instance of the `newType` to the `property`.</summary>
        private static void AddTypeSelector(
            GenericMenu menu,
            SerializedProperty property,
            Type fieldType,
            Type selectedType,
            Type newType)
        {
            var label = GetSelectorLabel(fieldType, newType);
            var state = selectedType == newType ? MenuFunctionState.Selected : MenuFunctionState.Normal;
            menu.AddPropertyModifierFunction(property, label, state, targetProperty =>
            {
                var oldValue = property.GetValue();
                var newValue = AnimancerReflection.CreateDefaultInstance(newType);

                CopyCommonFields(oldValue, newValue);

                if (newValue is IPolymorphicReset reset)
                    reset.Reset(oldValue);

                targetProperty.managedReferenceValue = newValue;
                targetProperty.isExpanded = true;
            });
        }

        /************************************************************************************************************************/

        private static string GetSelectorLabel(Type fieldType, Type newType)
        {
            if (newType == null)
                return "Null";

            if (!UseTypeHierarchy)
                return newType.GetNameCS(UseFullNames);

            var label = StringBuilderPool.Instance.Acquire();

            if (fieldType.IsInterface)// Interface.
            {
                while (true)
                {
                    if (label.Length > 0)
                        label.Insert(0, '/');

                    var displayType = newType.IsGenericType ?
                        newType.GetGenericTypeDefinition() :
                        newType;
                    label.Insert(0, displayType.GetNameCS(UseFullNames));

                    newType = newType.BaseType;

                    if (newType == null ||
                        !fieldType.IsAssignableFrom(newType))
                        break;
                }
            }
            else// Base Class.
            {
                while (true)
                {
                    if (label.Length > 0)
                        label.Insert(0, '/');

                    label.Insert(0, newType.GetNameCS(UseFullNames));

                    newType = newType.BaseType;

                    if (newType == null)
                        break;

                    if (fieldType.IsAbstract)
                    {
                        if (newType == fieldType)
                            break;
                    }
                    else
                    {
                        if (newType == fieldType.BaseType)
                            break;
                    }
                }
            }

            return label.ReleaseToString();
        }

        /************************************************************************************************************************/

        private static readonly List<Type>
            AllTypes = new(1024);
        private static readonly Dictionary<Type, List<Type>>
            TypeToDerived = new();

        /// <summary>Returns a list of all types that inherit from the `baseType`.</summary>
        public static List<Type> GetDerivedTypes(Type baseType)
        {
            if (!TypeToDerived.TryGetValue(baseType, out var derivedTypes))
            {
                if (AllTypes.Count == 0)
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (int iAssembly = 0; iAssembly < assemblies.Length; iAssembly++)
                    {
                        var assembly = assemblies[iAssembly];
                        if (assembly.IsDynamic)
                            continue;

                        var types = assembly.GetExportedTypes();
                        for (int iType = 0; iType < types.Length; iType++)
                        {
                            var type = types[iType];
                            if (IsViableType(type))
                                AllTypes.Add(type);
                        }
                    }

                    AllTypes.Sort((a, b) => a.FullName.CompareTo(b.FullName));
                }

                derivedTypes = new();
                for (int i = 0; i < AllTypes.Count; i++)
                {
                    var type = AllTypes[i];
                    if (baseType.IsAssignableFrom(type))
                        derivedTypes.Add(type);
                }
                TypeToDerived.Add(baseType, derivedTypes);
            }

            return derivedTypes;
        }

        /************************************************************************************************************************/

        /// <summary>Is the `type` supported by <see cref="SerializeReference"/> fields?</summary>
        public static bool IsViableType(Type type)
            => !type.IsAbstract
            && !type.IsEnum
            && !type.IsGenericTypeDefinition
            && !type.IsInterface
            && !type.IsPrimitive
            && !type.IsSpecialName
            && type.Name[0] != '<'
            && type.IsDefined(typeof(SerializableAttribute), false)
            && !type.IsDefined(typeof(ObsoleteAttribute), true)
            && !typeof(Object).IsAssignableFrom(type)
            && type.GetConstructor(AnimancerReflection.InstanceBindings, null, Type.EmptyTypes, null) != null;

        /************************************************************************************************************************/

        /// <summary>
        /// Copies the values of all fields in `from` into corresponding fields in `to` as long as they have the same
        /// name and compatible types.
        /// </summary>
        public static void CopyCommonFields(object from, object to)
        {
            if (from == null ||
                to == null)
                return;

            var nameToFromField = new Dictionary<string, FieldInfo>();
            var fromType = from.GetType();
            do
            {
                var fromFields = fromType.GetFields(AnimancerReflection.InstanceBindings | BindingFlags.DeclaredOnly);

                for (int i = 0; i < fromFields.Length; i++)
                {
                    var field = fromFields[i];
                    nameToFromField[field.Name] = field;
                }

                fromType = fromType.BaseType;
            }
            while (fromType != null);

            var toType = to.GetType();
            do
            {
                var toFields = toType.GetFields(AnimancerReflection.InstanceBindings | BindingFlags.DeclaredOnly);

                for (int i = 0; i < toFields.Length; i++)
                {
                    var toField = toFields[i];
                    if (nameToFromField.TryGetValue(toField.Name, out var fromField))
                    {
                        var fromValue = fromField.GetValue(from);
                        if (fromValue == null || toField.FieldType.IsAssignableFrom(fromValue.GetType()))
                            toField.SetValue(to, fromValue);
                    }
                }

                toType = toType.BaseType;
            }
            while (toType != null);
        }

        /************************************************************************************************************************/
    }
}

#endif
