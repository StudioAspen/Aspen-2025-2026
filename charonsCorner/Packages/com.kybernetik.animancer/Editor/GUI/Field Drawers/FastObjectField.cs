// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A GUI wrapper for drawing any object as a label with an icon.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/FastObjectField
    /// 
    public struct FastObjectField
    {
        /************************************************************************************************************************/

        /// <summary>A <see cref="FastObjectField"/> representing <c>null</c>.</summary>
        public static FastObjectField Null => new()
        {
            Text = "Null"
        };

        /************************************************************************************************************************/

        /// <summary>The object passed into the last <c>Draw</c> call.</summary>
        public object Value { get; private set; }

        /// <summary>The text used for the label in the last <c>Draw</c> call.</summary>
        public string Text { get; private set; }

        /// <summary>The icon drawn in the last <c>Draw</c> call.</summary>
        public Texture Icon { get; private set; }

        /************************************************************************************************************************/

        /// <summary>Sets the current details directly.</summary>
        public void Set(object value, string text, Texture icon)
        {
            Value = value;
            Text = text;
            Icon = icon;
        }

        /************************************************************************************************************************/

        /// <summary>Draws a field for the `value`.</summary>
        public Rect Draw(Rect area, string label, object value, bool drawPing = true)
        {
            if (drawPing)
                ObjectHighlightGUI.Draw(area, value);

            if (!string.IsNullOrEmpty(label))
            {
                var labelWidth = EditorGUIUtility.labelWidth - area.x + AnimancerGUI.StandardSpacing + 1;
                var labelArea = AnimancerGUI.StealFromLeft(ref area, labelWidth);
                EditorGUI.LabelField(labelArea, label);
            }

            Draw(area, value, false);
            return area;
        }

        /************************************************************************************************************************/

        /// <summary>Draws a field for the `value`.</summary>
        public void Draw(Rect area, object value, bool drawPing = true)
        {
            if (Value != value && Event.current.type == EventType.Layout)
                SetValue(value);

            Draw(area, drawPing);
        }

        /************************************************************************************************************************/

        /// <summary>Draws a field for the <see cref="Value"/>.</summary>
        public readonly void Draw(Rect area, bool drawPing = true)
        {
            HandleClick(area, drawPing);

            if (Icon != null)
            {
                var iconArea = AnimancerGUI.StealFromLeft(ref area, area.height);
                GUI.DrawTexture(iconArea, Icon);
            }

            GUI.Label(area, Text);
        }

        /************************************************************************************************************************/

        private readonly void HandleClick(Rect area, bool drawPing)
        {
            var currentEvent = Event.current;

            switch (currentEvent.rawType)
            {
                case EventType.MouseUp:
                    if (currentEvent.button == 0 &&
                        area.Contains(currentEvent.mousePosition))
                    {
                        ObjectHighlightGUI.Highlight(Value);
                    }
                    break;

                case EventType.Repaint:
                    if (drawPing)
                        ObjectHighlightGUI.Draw(area, Value);
                    break;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sets the cached details.</summary>
        public void SetValue(object value, string text, Texture icon = null)
        {
            Value = value;
            Text = text;
            Icon = icon;
        }

        /************************************************************************************************************************/

        /// <summary>Sets the cached details based on the `value`.</summary>
        public void SetValue(object value)
        {
            Value = value;
            Text = GetText();
            Icon = GetIcon();
        }

        /************************************************************************************************************************/

        /// <summary>Returns a string based on the <see cref="Value"/>.</summary>
        private readonly string GetText()
        {
            if (Value == null)
                return "Null";

            if (Value is Object obj)
            {
                if (obj == null)
                    return $"Null({obj.GetType().Name})";

                return obj.GetCachedName();
            }

            if (Value is string str)
                return $"\"{str}\"";

            return Value.ToString();
        }

        /************************************************************************************************************************/

        /// <summary>Returns an icon based on the type of the <see cref="Value"/>.</summary>
        private readonly Texture GetIcon()
            => GetIcon(Value);

        /// <summary>Returns an icon based on the type of the `value`.</summary>
        public static Texture GetIcon(object value)
        {
            if (value == null)
                return null;

            if (value is Object obj)
                return AssetPreview.GetMiniThumbnail(obj);

            var type = value is AnimancerState
                ? typeof(AnimatorState)
                : value.GetType();

            return AssetPreview.GetMiniTypeThumbnail(type);
        }

        /************************************************************************************************************************/

        /// <summary>Clears all cached details.</summary>
        public void Clear()
        {
            Value = null;
            Text = null;
            Icon = null;
        }

        /************************************************************************************************************************/
    }

}

#endif

