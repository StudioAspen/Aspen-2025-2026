// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using Animancer.Editor;
using System;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;

namespace Animancer.Units.Editor
{
    /// <summary>[Editor-Only] A <see cref="PropertyDrawer"/> for fields with a <see cref="UnitsAttribute"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units.Editor/UnitsAttributeDrawer
    [CustomPropertyDrawer(typeof(UnitsAttribute), true)]
    public class UnitsAttributeDrawer : PropertyDrawer
    {
        /************************************************************************************************************************/

        /// <summary>The attribute on the field being drawn.</summary>
        public UnitsAttribute Attribute { get; private set; }

        /// <summary>The converters used to generate display strings for each of the fields.</summary>
        public CompactUnitConversionCache[] DisplayConverters { get; private set; }

        /************************************************************************************************************************/

        /// <summary>Gathers the <see cref="Attribute"/> and sets up the <see cref="DisplayConverters"/>.</summary>
        public void Initialize()
            => Initialize(attribute);

        /// <summary>Gathers the <see cref="Attribute"/> and sets up the <see cref="DisplayConverters"/>.</summary>
        public void Initialize(Attribute attribute)
        {
            if (Attribute != null)
                return;

            Attribute = (UnitsAttribute)attribute;

            var suffixes = Attribute.Suffixes;
            DisplayConverters = new CompactUnitConversionCache[suffixes.Length];
            for (int i = 0; i < suffixes.Length; i++)
                DisplayConverters[i] = new(suffixes[i]);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var lineCount = GetLineCount(property, label);
            return LineHeight * lineCount + StandardSpacing * (lineCount - 1);
        }

        /// <summary>Determines how many lines tall the `property` should be.</summary>
        protected virtual int GetLineCount(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.wideMode
            ? 1
            : 2;

        /************************************************************************************************************************/

        /// <summary>Begins a GUI property block to be ended by <see cref="EndProperty"/>.</summary>
        protected static void BeginProperty(
            Rect area,
            SerializedProperty property,
            ref GUIContent label,
            out float value)
        {
            label = EditorGUI.BeginProperty(area, label, property);

            EditorGUI.BeginChangeCheck();

            value = property.floatValue;
        }

        /// <summary>Ends a GUI property block started by <see cref="BeginProperty"/>.</summary>
        protected static void EndProperty(
            Rect area,
            SerializedProperty property,
            ref float value)
        {
            if (TryUseClickEvent(area, 2))
                DefaultValues.SetToDefault(ref value, property);

            if (EditorGUI.EndChangeCheck())
                property.floatValue = value;

            EditorGUI.EndProperty();
        }

        /************************************************************************************************************************/

        /// <summary>Draws this attribute's fields for the `property`.</summary>
        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            Initialize();
            BeginProperty(area, property, ref label, out var value);
            DoFieldGUI(area, label, ref value);
            EndProperty(area, property, ref value);
        }

        /************************************************************************************************************************/

        private static readonly int TextFieldHash = "EditorTextField".GetHashCode();

        /// <summary>Draws this attribute's fields.</summary>
        public void DoFieldGUI(Rect area, GUIContent label, ref float value)
        {
            var isMultiLine = area.height >= LineHeight * 2;
            area.height = LineHeight;

            DoOptionalBeforeGUI(
                Attribute.IsOptional,
                area,
                out var toggleArea,
                out var guiWasEnabled,
                out var previousLabelWidth);

            var hasLabel = label != null && !string.IsNullOrEmpty(label.text);
            Rect allFieldArea;

            if (isMultiLine)
            {
                EditorGUI.LabelField(area, label);
                label = null;
                NextVerticalArea(ref area);

                EditorGUI.indentLevel++;
                allFieldArea = EditorGUI.IndentedRect(area);
                EditorGUI.indentLevel--;
            }
            else if (hasLabel)
            {
                var labelXMax = area.x + EditorGUIUtility.labelWidth;
                allFieldArea = new(labelXMax, area.y, area.xMax - labelXMax, area.height);
            }
            else
            {
                allFieldArea = area;
            }

            CountActiveFields(out var count, out var last);

            var currentEvent = Event.current;

            var beforeControlID = GUIUtility.GetControlID(TextFieldHash, FocusType.Passive, area);

            if (float.IsNaN(value) &&
                Attribute.DisabledText is not null &&
                currentEvent.type == EventType.Repaint &&
                !area.Contains(currentEvent.mousePosition) &&
                !HasKeyboardControl(beforeControlID, beforeControlID + count))
            {
                var dragArea = area;
                dragArea.width = EditorGUIUtility.labelWidth;
                EditorGUIUtility.AddCursorRect(dragArea, MouseCursor.SlideArrow);

                label ??= GUIContent.none;

                EditorGUI.TextField(area, label, Attribute.DisabledText);

                for (int i = 1; i < count; i++)
                    GUIUtility.GetControlID(TextFieldHash, FocusType.Keyboard, area);
            }
            else
            {
                var width = (allFieldArea.width - StandardSpacing * (count - 1)) / count;
                var fieldArea = new Rect(allFieldArea.x, allFieldArea.y, width, allFieldArea.height);

                var displayValue = GetDisplayValue(value, Attribute.DefaultValue);

                // Draw the active fields.
                for (int i = 0; i < Attribute.Multipliers.Length; i++)
                {
                    var multiplier = Attribute.Multipliers[i];
                    if (float.IsNaN(multiplier))
                        continue;

                    if (hasLabel)
                    {
                        fieldArea.xMin = area.xMin;
                    }
                    else if (i < last)
                    {
                        fieldArea.width = width;
                        fieldArea.xMax = AnimancerUtilities.Round(fieldArea.xMax);
                    }
                    else
                    {
                        fieldArea.xMax = area.xMax;
                    }

                    EditorGUI.BeginChangeCheck();

                    var fieldValue = displayValue * multiplier;
                    fieldValue = DoSpecialFloatField(fieldArea, label, fieldValue, DisplayConverters[i]);
                    label = null;
                    hasLabel = false;

                    if (EditorGUI.EndChangeCheck())
                        value = fieldValue / multiplier;

                    fieldArea.x += fieldArea.width + StandardSpacing;
                }
            }

            DoOptionalAfterGUI(
                Attribute.IsOptional,
                toggleArea,
                ref value,
                Attribute.DefaultValue,
                guiWasEnabled,
                previousLabelWidth);

            Validate.ValueRule(ref value, Attribute.Rule);
        }

        /************************************************************************************************************************/

        /// <summary>Counts the number of active <see cref="UnitsAttribute.Multipliers"/>.</summary>
        private void CountActiveFields(out int count, out int last)
        {
            count = 0;
            last = 0;

            for (int i = 0; i < Attribute.Multipliers.Length; i++)
            {
                if (!float.IsNaN(Attribute.Multipliers[i]))
                {
                    count++;
                    last = i;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Is the <see cref="GUIUtility.keyboardControl"/> in the specified range (inclusive)?</summary>
        private static bool HasKeyboardControl(int minControlID, int maxControlID)
        {
            var keyboardControl = GUIUtility.keyboardControl;
            return keyboardControl >= minControlID && keyboardControl <= maxControlID;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Draws a <see cref="EditorGUI.FloatField(Rect, GUIContent, float)"/> with an alternate string
        /// when it's not selected (for example, "1" might display as "1s" to indicate "seconds").
        /// </summary>
        /// <remarks>
        /// This method treats most <see cref="EventType"/>s normally,
        /// but for <see cref="EventType.Repaint"/> it instead draws a text field with the converted string.
        /// </remarks>
        public static float DoSpecialFloatField(
            Rect area,
            GUIContent label,
            float value,
            CompactUnitConversionCache toString)
        {
            if (label != null && !string.IsNullOrEmpty(label.text))
            {
                if (Event.current.type != EventType.Repaint)
                    return EditorGUI.FloatField(area, label, value);

                var dragArea = new Rect(area.x, area.y, EditorGUIUtility.labelWidth, area.height);
                EditorGUIUtility.AddCursorRect(dragArea, MouseCursor.SlideArrow);

                var text = toString.Convert(value, area.width - EditorGUIUtility.labelWidth);
                EditorGUI.TextField(area, label, text);
            }
            else
            {
                var indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                if (Event.current.type != EventType.Repaint)
                    value = EditorGUI.FloatField(area, value);
                else
                    EditorGUI.TextField(area, toString.Convert(value, area.width));

                EditorGUI.indentLevel = indentLevel;
            }

            return value;
        }

        /************************************************************************************************************************/

        /// <summary>Prepares the details for drawing a toggle to set the field to <see cref="float.NaN"/>.</summary>
        /// <remarks>Call this before drawing the field then call <see cref="DoOptionalAfterGUI"/> after it.</remarks>
        public void DoOptionalBeforeGUI(
            bool isOptional,
            Rect area,
            out Rect toggleArea,
            out bool guiWasEnabled,
            out float previousLabelWidth)
        {
            toggleArea = area;
            guiWasEnabled = GUI.enabled;
            previousLabelWidth = EditorGUIUtility.labelWidth;
            if (!isOptional)
                return;

            toggleArea.x += previousLabelWidth;

            toggleArea.width = ToggleWidth;
            EditorGUIUtility.labelWidth += toggleArea.width;

            EditorGUIUtility.AddCursorRect(toggleArea, MouseCursor.Arrow);

            // We need to draw the toggle after everything else to it goes on top of the label. But we want it to
            // get priority for input events, so we disable the other controls during those events in its area.
            var currentEvent = Event.current;
            if (guiWasEnabled && toggleArea.Contains(currentEvent.mousePosition))
            {
                switch (currentEvent.type)
                {
                    case EventType.Repaint:
                    case EventType.Layout:
                        break;

                    default:
                        GUI.enabled = false;
                        break;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws a toggle to set the `value` to <see cref="float.NaN"/> when disabled.</summary>
        public void DoOptionalAfterGUI(
            bool isOptional,
            Rect area,
            ref float value,
            float defaultValue,
            bool guiWasEnabled,
            float previousLabelWidth)
        {
            GUI.enabled = guiWasEnabled;
            EditorGUIUtility.labelWidth = previousLabelWidth;

            if (!isOptional)
                return;

            area.x += StandardSpacing;

            var wasEnabled = !float.IsNaN(value);

            // Use the EditorGUI method instead to properly handle EditorGUI.showMixedValue.
            //var isEnabled = GUI.Toggle(area, wasEnabled, GUIContent.none);

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var isEnabled = EditorGUI.Toggle(area, wasEnabled);

            EditorGUI.indentLevel = indentLevel;

            if (isEnabled != wasEnabled)
            {
                value = isEnabled ? defaultValue : float.NaN;
                Deselect();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns the value that should be displayed for a given field.</summary>
        public static float GetDisplayValue(float value, float defaultValue)
            => float.IsNaN(value) ? defaultValue : value;

        /************************************************************************************************************************/
    }
}

#endif

