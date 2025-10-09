// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //
// FlexiMotion // https://kybernetik.com.au/flexi-motion // Copyright 2023 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;

namespace Animancer.Editor
// namespace FlexiMotion.Editor
{
    /// <summary>[Editor-Only] A <see cref="PropertyDrawer"/> which adds an "Edit" button to a field.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/EditableFieldDrawer
    /// https://kybernetik.com.au/flexi-motion/api/FlexiMotion.Editor/EditableFieldDrawer
    public abstract class EditableFieldDrawer : PropertyDrawer
    {
        /************************************************************************************************************************/

        /// <summary>The method to call when the "Edit" button is clicked.</summary>
        /// <remarks>Set this in a custom editor before drawing the attributed field then clear it afterwards.</remarks>
        public static event Action<SerializedProperty> OnEdit;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            DrawEditableArea(area, property);
            EditorGUI.PropertyField(area, property, label, true);
        }

        /************************************************************************************************************************/

        private static GUIStyle
            _LeftAlignedButtonStyle;

        private static readonly GUIContent
            EditContent = new();

        private void DrawEditableArea(Rect area, SerializedProperty property)
        {
            if (property.hasMultipleDifferentValues)
                return;

            var label = EditContent;
            label.text = null;
            label.tooltip = null;

            GetEditButtonLabel(property, label);
            if (!string.IsNullOrEmpty(label.text))
            {
                area.xMin += EditorGUIUtility.labelWidth + StandardSpacing;
                area.height = LineHeight;

                if (OnEdit != null)
                {
                    _LeftAlignedButtonStyle ??= new GUIStyle(EditorStyles.miniButton)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        padding = EditorStyles.miniPullDown.padding,
                    };

                    if (GUI.Button(area, label, _LeftAlignedButtonStyle))
                        OnEdit(property);
                }
                else
                {
                    GUI.Label(area, label);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sets the `label` for the "Edit" button.</summary>
        public abstract void GetEditButtonLabel(SerializedProperty property, GUIContent label);

        /************************************************************************************************************************/
    }
}

#endif

