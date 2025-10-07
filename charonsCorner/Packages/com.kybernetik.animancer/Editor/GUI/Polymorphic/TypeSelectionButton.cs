// Animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A button that allows the user to select an object type for a [<see cref="SerializeReference"/>] field.
    /// </summary>
    /// 
    /// <remarks>
    /// <strong>Example:</strong>
    /// <code>
    /// public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
    /// {
    ///     using (new TypeSelectionButton(area, property, label, true))
    ///     {
    ///         EditorGUI.PropertyField(area, property, label, true);
    ///     }
    /// }
    /// </code></remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TypeSelectionButton
    /// 
    public readonly struct TypeSelectionButton : IDisposable
    {
        /************************************************************************************************************************/

        /// <summary>The pixel area occupied by the button.</summary>
        public readonly Rect Area;

        /// <summary>The <see cref="SerializedProperty"/> representing the attributed field.</summary>
        public readonly SerializedProperty Property;

        /// <summary>The original <see cref="Event.type"/> from when this button was initialized.</summary>
        public readonly EventType EventType;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TypeSelectionButton"/>.</summary>
        public TypeSelectionButton(
            Rect area,
            SerializedProperty property,
            bool hasLabel)
        {
            area.height = AnimancerGUI.LineHeight;

            if (hasLabel)
                area.xMin += EditorGUIUtility.labelWidth + AnimancerGUI.StandardSpacing;

            var currentEvent = Event.current;

            Area = area;
            Property = property;
            EventType = currentEvent.type;

            if (Property.propertyType != SerializedPropertyType.ManagedReference)
                return;

            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                case EventType.MouseUp:
                    if (area.Contains(currentEvent.mousePosition))
                        currentEvent.type = EventType.Ignore;
                    break;
            }
        }

        /************************************************************************************************************************/

        void IDisposable.Dispose()
            => DoGUI();

        /// <summary>Draws this button's GUI.</summary>
        /// <remarks>Run this method after drawing the target property so the button draws on top of its label.</remarks>
        public void DoGUI()
        {
            if (Property.propertyType != SerializedPropertyType.ManagedReference)
                return;

            var currentEvent = Event.current;
            var eventType = currentEvent.type;
            var area = Area;

            PrepareSharedReferenceArea(ref area, out var sharedButtonArea, out var value, out var references);

            using (var label = PooledGUIContent.Acquire())
            {
                switch (EventType)
                {
                    case EventType.MouseDown:
                    case EventType.MouseUp:
                        currentEvent.type = EventType;
                        break;

                    case EventType.Layout:
                        break;

                    // Only Repaint events actually care what the label is.
                    case EventType.Repaint:
                        var valueType = Property.managedReferenceValue?.GetType();
                        if (valueType == null)
                        {
                            label.text = "Null";
                            label.tooltip = "Nothing is assigned";
                        }
                        else
                        {
                            label.text = valueType.GetNameCS(false);
                            label.tooltip = valueType.GetNameCS(true);
                        }
                        break;

                    default:
                        return;
                }

                if (GUI.Button(area, label, EditorStyles.popup))
                    TypeSelectionMenu.Show(Property);
            }

            DoSharedReferenceGUI(sharedButtonArea, value, references, currentEvent.type);

            if (currentEvent.type == EventType)
                currentEvent.type = eventType;
        }

        /************************************************************************************************************************/

        /// <summary>Allocates an area for <see cref="DoSharedReferenceGUI"/> if the `value` is shared.</summary>
        private void PrepareSharedReferenceArea(
            ref Rect remainingArea,
            out Rect sharedButtonArea,
            out object value,
            out List<SharedReferenceCache.Field> references)
        {
            sharedButtonArea = default;
            value = default;
            references = default;

            if (!TypeSelectionMenu.VisualiseSharedReferences)
                return;

            value = Property.managedReferenceValue;
            if (value == null)
                return;

            var referenceCache = SharedReferenceCache.Get(Property.serializedObject);
            if (!referenceCache.TryGetInfo(value, out references) ||
                references.Count <= 1)
                return;

            sharedButtonArea = AnimancerGUI.StealFromRight(
                ref remainingArea,
                remainingArea.height + AnimancerGUI.StandardSpacing * 2,
                AnimancerGUI.StandardSpacing);
        }

        /************************************************************************************************************************/

        private static ConditionalWeakTable<object, object>
            _VisualiseLinks;
        private static GUIStyle
            _SharedReferenceStyle;
        private static Texture
            _SharedReferenceIcon;

        /// <summary>Draws a toggle to enable/disable visualisation of the `value`'s shared references.</summary>
        private void DoSharedReferenceGUI(
            Rect area,
            object value,
            List<SharedReferenceCache.Field> references,
            EventType eventType)
        {
            if (area.width == 0)
                return;

            var wasVisualising = _VisualiseLinks != null && _VisualiseLinks.TryGetValue(value, out _);
            var color = eventType == EventType.Repaint
                ? AnimancerGUI.GetHashColor(value.GetHashCode(), 0.5f, 1, 0.7f)
                : Color.white;

            if (wasVisualising)
                new LinkLine(area, Property.GetFriendlyPath(), references, color);

            using (var label = PooledGUIContent.Acquire(null, GetTooltip(references, eventType)))
            {
                if (_SharedReferenceStyle == null)
                {
                    _SharedReferenceStyle ??= new(EditorStyles.miniButton)
                    {
                        padding = new RectOffset(0, 0, -2, 0),
                        overflow = new RectOffset(),
                    };

                    _SharedReferenceIcon = AnimancerIcons.Load(EditorGUIUtility.isProSkin
                        ? "d_Linked@2x"
                        : "Linked@2x");
                }

                label.image = _SharedReferenceIcon;

                var oldColor = GUI.color;
                GUI.color = color;

                var isVisualising = GUI.Toggle(area, wasVisualising, label, _SharedReferenceStyle);

                GUI.color = oldColor;

                if (isVisualising != wasVisualising)
                {
                    if (isVisualising)
                    {
                        _VisualiseLinks ??= new();
                        _VisualiseLinks.Add(value, null);
                    }
                    else
                    {
                        _VisualiseLinks.Remove(value);
                    }
                }

                label.image = null;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Builds a tooltip describing the `references`.</summary>
        private static string GetTooltip(
            List<SharedReferenceCache.Field> references,
            EventType eventType)
        {
            if (eventType != EventType.Repaint)
                return null;

            var text = StringBuilderPool.Instance.Acquire();

            text.Append("This reference is shared by:");

            for (int i = 0; i < references.Count; i++)
                text.Append("\n• ").Append(ObjectNames.NicifyVariableName(references[i].path));

            text.Append("\nClick to visualise");

            return text.ReleaseToString();
        }

        /************************************************************************************************************************/
        #region Link Lines
        /************************************************************************************************************************/

        private static readonly List<LinkLine>
            LinkLines = new();

        private static int _DelayLinkLines;

        /************************************************************************************************************************/

        /// <summary>
        /// Any shared reference link lines which would be drawn after this call are instead
        /// delayed until the corresponding <see cref="EndDelayingLinkLines"/> call.
        /// </summary>
        public static void BeginDelayingLinkLines()
        {
            _DelayLinkLines++;
        }

        /// <summary>
        /// Ends a block started by <see cref="BeginDelayingLinkLines"/>.
        /// When all such blocks are cancelled, this method draws all delayed links between shared reference fields.
        /// </summary>
        public static void EndDelayingLinkLines()
        {
            _DelayLinkLines--;

            if (_DelayLinkLines <= 0)
            {
                for (int i = LinkLines.Count - 1; i >= 0; i--)
                    LinkLines[i].Draw();

                LinkLines.Clear();
            }
        }

        /************************************************************************************************************************/

        /// <summary>The details needed to draw a line between fields which share the same reference.</summary>
        private readonly struct LinkLine
        {
            /************************************************************************************************************************/

            /// <summary>The area of the button which toggles visibility of link lines.</summary>
            public readonly Rect SharedButtonArea;

            /// <summary>The property path of the target field.</summary>
            public readonly string Path;

            /// <summary>The shared reference cache for the target field.</summary>
            public readonly List<SharedReferenceCache.Field> References;

            /// <summary>The color of the link line.</summary>
            public readonly Color Color;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="LinkLine"/>.</summary>
            public LinkLine(
                Rect sharedButtonArea,
                string path,
                List<SharedReferenceCache.Field> references,
                Color color)
            {
                sharedButtonArea.position += AnimancerGUI.GuiOffset;

                SharedButtonArea = sharedButtonArea;
                Path = path;
                References = references;
                Color = color;

                if (_DelayLinkLines <= 0)
                    Draw();
                else
                    LinkLines.Add(this);
            }

            /************************************************************************************************************************/

            /// <summary>Draws a line between the current field and the the previous field referencing the same value.</summary>
            public void Draw()
            {
                var currentEvent = Event.current;
                if (currentEvent.type != EventType.Repaint)
                    return;

                var index = SetArea(References, Path, SharedButtonArea);

                Handles.DrawLine(default, default);// Necessary for DrawCurve to work.

                AnimancerGUI.BeginTriangles(Color);

                var position = GetLeftCenter(SharedButtonArea);

                for (int i = index - 1; i >= 0; i--)
                {
                    var otherArea = References[i].area;
                    if (otherArea == default)
                        continue;

                    var otherPosition = GetLeftCenter(otherArea);
                    DrawCurve(position, otherPosition);
                    break;
                }

                AnimancerGUI.EndTriangles();
            }

            /************************************************************************************************************************/

            /// <summary>Sets the <see cref="SharedReferenceCache.Field.area"/> of the current field.</summary>
            private static int SetArea(
                List<SharedReferenceCache.Field> references,
                string path,
                Rect area)
            {
                for (int i = 0; i < references.Count; i++)
                {
                    var reference = references[i];
                    if (reference.path != path)
                        continue;

                    reference.area = area;
                    references[i] = reference;
                    return i;
                }

                return -1;
            }

            /************************************************************************************************************************/

            /// <summary>Returns the center point of the left edge of the `rect`.</summary>
            private static Vector2 GetLeftCenter(Rect rect)
                => new(rect.x, rect.y + rect.height * 0.5f);

            /************************************************************************************************************************/

            /// <summary>Draws a line between `a` and `b` curved towards x = 0.</summary>
            private static void DrawCurve(
                Vector2 a,
                Vector2 b)
            {
                const int Segments = 16;
                const float Increment = 1f / (Segments - 1);

                var width = CalculateCurveWidth(Math.Abs(a.y - b.y));

                var previous = a;

                for (int i = 0; i < Segments; i++)
                {
                    var t = i * Increment;
                    var next = Vector2.LerpUnclamped(a, b, t);

                    var curve = 0.5f - t;
                    curve *= 2;
                    curve *= curve;
                    curve = 1 - curve;
                    next.x *= 1 - curve * width;

                    AnimancerGUI.DrawLineBatched(previous, next, 2);

                    previous = next;
                }
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Calculates the desired width for a curve with the given `height`
            /// as a portion of the total available width.
            /// </summary>
            private static float CalculateCurveWidth(float height)
            {
                const float
                    MinWidth = 0.05f,
                    MaxWidth = 0.8f;

                var maxHeight = AnimancerGUI.LineHeight * 100;

                if (height > maxHeight)
                    return MaxWidth;

                var t = height / maxHeight;
                t = 1 - t;
                t *= t;

                return Mathf.Lerp(MaxWidth, MinWidth, t);
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif
