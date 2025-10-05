// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Utility for a toggle which can show and hide a speed slider.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ToggledSpeedSlider
    public class ToggledSpeedSlider
    {
        /************************************************************************************************************************/

        /// <summary>The content displayed on the toggle.</summary>
        /// <remarks>The <see cref="GUIContent.text"/> is set by the <see cref="Speed"/>.</remarks>
        public readonly GUIContent GUIContent = new();

        /// <summary>Is the toggle currently on?</summary>
        public readonly BoolPref IsOn;

        /************************************************************************************************************************/

        private float _Speed = float.NaN;

        /// <summary>The current speed value.</summary>
        public float Speed
        {
            get => _Speed;
            set
            {
                if (_Speed == value)
                    return;

                _Speed = value;
                GUIContent.text = _Speed.ToString("0.0x");
                OnSetSpeed(_Speed);
            }
        }

        /// <summary>Called when the <see cref="Speed"/> is changed.</summary>
        protected virtual void OnSetSpeed(float speed) { }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="ToggledSpeedSlider"/>.</summary>
        public ToggledSpeedSlider(string prefKey)
        {
            IsOn = new(prefKey);
        }

        /************************************************************************************************************************/

        /// <summary>Draws a toggle to show or hide the speed slider.</summary>
        public virtual bool DoToggleGUI(Rect area, GUIStyle style)
        {
            HandleResetClick(area);

            style ??= GUI.skin.toggle;

            IsOn.Value = GUI.Toggle(area, IsOn, GUIContent, style);

            return IsOn;
        }

        /************************************************************************************************************************/

        private static float _SpeedLabelWidth;

        /// <summary>Draws a slider to control the <see cref="Speed"/>.</summary>
        public float DoSpeedSlider(ref Rect area, GUIStyle backgroundStyle = null)
        {
            if (!IsOn)
                return Speed;

            var sliderArea = StealLineFromTop(ref area);
            sliderArea = sliderArea.Expand(-StandardSpacing, 0);

            if (backgroundStyle != null)
                GUI.Label(sliderArea, GUIContent.none, backgroundStyle);

            HandleResetClick(sliderArea);

            var label = "Speed";

            if (_SpeedLabelWidth == 0)
                _SpeedLabelWidth = CalculateLabelWidth(label);

            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = _SpeedLabelWidth;

            EditorGUI.BeginChangeCheck();

            var speed = EditorGUI.Slider(sliderArea, label, Speed, 0.1f, 2);

            if (EditorGUI.EndChangeCheck())
                Speed = speed;

            EditorGUIUtility.labelWidth = labelWidth;

            return Speed;
        }

        /************************************************************************************************************************/

        /// <summary>Handles Right or Middle Clicking in the `area` to reset the <see cref="Speed"/>.</summary>
        public void HandleResetClick(Rect area)
        {
            if (!TryUseClickEvent(area, 1) && !TryUseClickEvent(area, 2))
                return;

            if (Speed != 1)
                Speed = 1;
            else
                Speed = 0.5f;
        }

        /************************************************************************************************************************/
    }

}

#endif

