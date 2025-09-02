// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A <see cref="PropertyDrawer"/> for <see cref="IPolymorphic"/> and <see cref="PolymorphicAttribute"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/PolymorphicDrawer
    [CustomPropertyDrawer(typeof(IPolymorphic), true)]
    [CustomPropertyDrawer(typeof(PolymorphicAttribute), true)]
    public sealed class PolymorphicDrawer : PropertyDrawer
    {
        /************************************************************************************************************************/

        private bool _DrawerThrewException;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 0f;

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                GetDetails(property, out var value, out var drawer, out var details);

                if (value == null)
                    return AnimancerGUI.LineHeight;

                if (drawer != null)
                {
                    if (details.SeparateHeader)
                    {
                        if (!property.isExpanded)
                            return AnimancerGUI.LineHeight;

                        height += AnimancerGUI.LineHeight + AnimancerGUI.StandardSpacing;
                    }

                    try
                    {
                        height += drawer.GetPropertyHeight(property, label);
                        return height;
                    }
                    catch (Exception exception)
                    {
                        _DrawerThrewException = true;
                        Debug.LogException(exception, property.serializedObject.targetObject);
                        // Continue to the regular calculation.
                    }
                }
            }

            height += EditorGUI.GetPropertyHeight(property, label, true);

            return height;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(area, property, label, true);
                return;
            }

            GetDetails(property, out _, out var drawer, out var details);

            var drawTypeSelectionButton = drawer == null || drawer is not IPolymorphic;

            var button = drawTypeSelectionButton
                ? new TypeSelectionButton(area, property, true)
                : default;

            if (drawer != null)
            {
                if (details.SeparateHeader)
                {
                    var foldoutArea = area;
                    foldoutArea.width = EditorGUIUtility.labelWidth;
                    foldoutArea.height = AnimancerGUI.LineHeight;

                    label = EditorGUI.BeginProperty(foldoutArea, label, property);

                    property.isExpanded = EditorGUI.Foldout(foldoutArea, property.isExpanded, label, true);

                    EditorGUI.EndProperty();

                    area.yMin += AnimancerGUI.LineHeight + AnimancerGUI.StandardSpacing;
                }

                // If drawing a separate header, don't draw the body if it's collapsed.
                if (!details.SeparateHeader || property.isExpanded)
                {
                    try
                    {
#pragma warning disable UNT0027 // Do not call PropertyDrawer.OnGUI(). Should only apply to calling base.OnGUI.
                        drawer.OnGUI(area, property, label);
#pragma warning restore UNT0027 // Do not call PropertyDrawer.OnGUI().
                    }
                    catch (Exception exception)
                    {
                        _DrawerThrewException = true;
                        Debug.LogException(exception, property.serializedObject.targetObject);
                        // Continue to PropertyField.
                    }
                }
            }
            else
            {
                EditorGUI.PropertyField(area, property, label, true);
            }

            if (drawTypeSelectionButton)
                button.DoGUI();
        }

        /************************************************************************************************************************/

        private void GetDetails(
            SerializedProperty property,
            out object value,
            out PropertyDrawer drawer,
            out PolymorphicDrawerDetails details)
        {
            value = property.managedReferenceValue;

            if (_DrawerThrewException || value == null)
            {
                drawer = null;
                details = PolymorphicDrawerDetails.Default;
                return;
            }

            if (PropertyDrawers.TryGetDrawer(value.GetType(), fieldInfo, attribute, out drawer) &&
                drawer is PolymorphicDrawer)
                drawer = null;

            details = PolymorphicDrawerDetails.Get(value);
        }

        /************************************************************************************************************************/
    }
}

#endif
