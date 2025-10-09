// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using Animancer.Editor;
using Animancer.Editor.Previews;
using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Units.Editor
{
    /// <summary>[Editor-Only]
    /// A <see cref="PropertyDrawer"/> for <see cref="float"/> fields with a <see cref="UnitsAttribute"/>
    /// which displays them using 3 fields: Normalized, Seconds, and Frames.
    /// </summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#time-fields">
    /// Time Fields</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units.Editor/AnimationTimeAttributeDrawer
    [CustomPropertyDrawer(typeof(AnimationTimeAttribute), true)]
    public class AnimationTimeAttributeDrawer : UnitsAttributeDrawer
    {
        /************************************************************************************************************************/

        /// <summary>The default value to be used for the next field drawn by this attribute.</summary>
        public static float NextDefaultValue { get; set; } = float.NaN;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override int GetLineCount(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.wideMode || TransitionDrawer.Context.Property == null
            ? 1
            : 2;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();

            var nextDefaultValue = NextDefaultValue;

            BeginProperty(area, property, ref label, out var value);
            OnGUI(area, label, ref value);
            EndProperty(area, property, ref value);

            if (EditorGUI.EndChangeCheck())
            {
                var index = (int)AnimationTimeAttribute.Units.Normalized;
                TransitionPreviewWindow.PreviewNormalizedTime =
                    GetDisplayValue(value, nextDefaultValue) * Attribute.Multipliers[index];
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI for this attribute.</summary>
        public void OnGUI(Rect area, GUIContent label, ref float value)
        {
            Initialize();

            var context = TransitionDrawer.Context;
            if (context.Property == null)
            {
                value = DoSpecialFloatField(area, label, value, DisplayConverters[Attribute.UnitIndex]);
                goto Return;
            }

            var length = context.MaximumDuration;
            if (length <= 0)
                length = float.NaN;

            AnimancerUtilities.TryGetFrameRate(context.Transition, out var frameRate);

            var multipliers = CalculateMultipliers(length, frameRate);
            if (multipliers == null)
            {
                EditorGUI.LabelField(area, label.text, $"Invalid {nameof(Validate)}.{nameof(Validate.Value)}");
                goto Return;
            }

            DoPreviewTimeButton(ref area, ref value, multipliers);

            Attribute.IsOptional = !float.IsNaN(NextDefaultValue);
            Attribute.DefaultValue = NextDefaultValue;
            DoFieldGUI(area, label, ref value);

            Return:
            NextDefaultValue = float.NaN;
        }

        /************************************************************************************************************************/

        private float[] CalculateMultipliers(float length, float frameRate)
        {
            switch ((AnimationTimeAttribute.Units)Attribute.UnitIndex)
            {
                case AnimationTimeAttribute.Units.Normalized:
                    Attribute.Multipliers[(int)AnimationTimeAttribute.Units.Normalized] = 1;
                    Attribute.Multipliers[(int)AnimationTimeAttribute.Units.Seconds] = length;
                    Attribute.Multipliers[(int)AnimationTimeAttribute.Units.Frames] = length * frameRate;
                    break;

                case AnimationTimeAttribute.Units.Seconds:
                    Attribute.Multipliers[(int)AnimationTimeAttribute.Units.Normalized] = 1f / length;
                    Attribute.Multipliers[(int)AnimationTimeAttribute.Units.Seconds] = 1;
                    Attribute.Multipliers[(int)AnimationTimeAttribute.Units.Frames] = frameRate;
                    break;

                case AnimationTimeAttribute.Units.Frames:
                    Attribute.Multipliers[(int)AnimationTimeAttribute.Units.Normalized] = 1f / length / frameRate;
                    Attribute.Multipliers[(int)AnimationTimeAttribute.Units.Seconds] = 1f / frameRate;
                    Attribute.Multipliers[(int)AnimationTimeAttribute.Units.Frames] = 1;
                    break;

                default:
                    return null;
            }

            var settings = AnimancerSettingsGroup<AnimationTimeAttributeSettings>.Instance;
            ApplyVisibilitySetting(settings.showNormalized, AnimationTimeAttribute.Units.Normalized);
            ApplyVisibilitySetting(settings.showSeconds, AnimationTimeAttribute.Units.Seconds);
            ApplyVisibilitySetting(settings.showFrames, AnimationTimeAttribute.Units.Frames);

            void ApplyVisibilitySetting(bool show, AnimationTimeAttribute.Units setting)
            {
                if (show)
                    return;

                var index = (int)setting;
                if (Attribute.UnitIndex != index)
                    Attribute.Multipliers[index] = float.NaN;
            }

            return Attribute.Multipliers;
        }

        /************************************************************************************************************************/

        private void DoPreviewTimeButton(
            ref Rect area,
            ref float value,
            float[] multipliers)
        {
            if (!TransitionPreviewWindow.IsPreviewingCurrentProperty())
                return;

            var previewTime = TransitionPreviewWindow.PreviewNormalizedTime;

            const string Tooltip =
                "� Left Click = preview the current value of this field." +
                "\n� Right Click = set this field to use the current preview time.";

            var displayValue = GetDisplayValue(value, NextDefaultValue);

            var multiplier = multipliers[(int)AnimationTimeAttribute.Units.Normalized];
            displayValue *= multiplier;

            var isCurrent = Mathf.Approximately(displayValue, previewTime);

            var buttonArea = area;
            if (TransitionDrawer.DoPreviewButtonGUI(ref buttonArea, isCurrent, Tooltip))
            {
                if (Event.current.button != 1)
                    TransitionPreviewWindow.PreviewNormalizedTime = displayValue;
                else
                    value = previewTime / multiplier;
            }

            // Only steal the button area for single line fields.
            if (area.height <= AnimancerGUI.LineHeight)
                area = buttonArea;
        }

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/
    #region Settings
    /************************************************************************************************************************/

    /// <summary>[Editor-Only] Options to determine how <see cref="AnimationTimeAttribute"/> displays.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units.Editor/AnimationTimeAttributeSettings
    [Serializable, InternalSerializableType]
    public class AnimationTimeAttributeSettings : AnimancerSettingsGroup
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string DisplayName
            => "Time Fields";

        /// <inheritdoc/>
        public override int Index
            => 5;

        /************************************************************************************************************************/

        /// <summary>Should time fields show approximations if the value is too long for the GUI?</summary>
        /// <remarks>This setting is used by <see cref="CompactUnitConversionCache"/>.</remarks>
        [Tooltip("Should time fields show approximations if the value is too long for the GUI?" +
            " For example, '1.111111' could instead show '1.111~'.")]
        public bool showApproximations = true;

        /// <summary>Should the <see cref="AnimationTimeAttribute.Units.Normalized"/> field be shown?</summary>
        /// <remarks>This setting is ignored for fields which directly store the normalized value.</remarks>
        [Tooltip("Should the " + nameof(AnimationTimeAttribute.Units.Normalized) + " field be shown?")]
        public bool showNormalized = true;

        /// <summary>Should the <see cref="AnimationTimeAttribute.Units.Seconds"/> field be shown?</summary>
        /// <remarks>This setting is ignored for fields which directly store the seconds value.</remarks>
        [Tooltip("Should the " + nameof(AnimationTimeAttribute.Units.Seconds) + " field be shown?")]
        public bool showSeconds = true;

        /// <summary>Should the <see cref="AnimationTimeAttribute.Units.Frames"/> field be shown?</summary>
        /// <remarks>This setting is ignored for fields which directly store the frame value.</remarks>
        [Tooltip("Should the " + nameof(AnimationTimeAttribute.Units.Frames) + " field be shown?")]
        public bool showFrames = true;

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/
    #endregion
    /************************************************************************************************************************/
}

#endif

