// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Various GUI utilities used throughout Animancer.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerGUI
    public static partial class AnimancerGUI
    {
        /************************************************************************************************************************/
        #region Standard Values
        /************************************************************************************************************************/

        /// <summary>The highlight color used for fields showing a warning.</summary>
        public static readonly Color
            WarningFieldColor = new(1, 0.9f, 0.6f);

        /// <summary>The highlight color used for fields showing an error.</summary>
        public static readonly Color
            ErrorFieldColor = new(1, 0.6f, 0.6f);

        /// <summary>Returns a color with uniform Red, Green, and Blue values.</summary>
        public static Color Grey(float rgb, float alpha = 1)
            => new(rgb, rgb, rgb, alpha);

        /************************************************************************************************************************/

        /// <summary><see cref="GUILayout.ExpandWidth"/> set to false.</summary>
        public static readonly GUILayoutOption[]
            DontExpandWidth = { GUILayout.ExpandWidth(false) };

        /************************************************************************************************************************/

        /// <summary>Returns <see cref="EditorGUIUtility.singleLineHeight"/>.</summary>
        public static float LineHeight => EditorGUIUtility.singleLineHeight;

        /// <summary>
        /// Calculates the number of vertical pixels required to draw the specified `lineCount` using the
        /// <see cref="LineHeight"/> and <see cref="StandardSpacing"/>.
        /// </summary>
        public static float CalculateHeight(int lineCount)
            => lineCount <= 0
            ? 0
            : LineHeight * lineCount + StandardSpacing * (lineCount - 1);

        /************************************************************************************************************************/

        /// <summary>Returns <see cref="EditorGUIUtility.standardVerticalSpacing"/>.</summary>
        public static float StandardSpacing => EditorGUIUtility.standardVerticalSpacing;

        /************************************************************************************************************************/

        private static float _IndentSize = float.NaN;

        /// <summary>
        /// The number of pixels of indentation for each <see cref="EditorGUI.indentLevel"/> increment.
        /// </summary>
        public static float IndentSize
        {
            get
            {
                if (float.IsNaN(_IndentSize))
                {
                    var indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;
                    _IndentSize = EditorGUI.IndentedRect(default).x;
                    EditorGUI.indentLevel = indentLevel;
                }

                return _IndentSize;
            }
        }

        /************************************************************************************************************************/

        private static float _ToggleWidth = -1;

        /// <summary>The width of a standard <see cref="GUISkin.toggle"/> with no label.</summary>
        public static float ToggleWidth
        {
            get
            {
                if (_ToggleWidth == -1)
                    _ToggleWidth = GUI.skin.toggle.CalculateWidth(GUIContent.none);
                return _ToggleWidth;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The color of the standard label text.</summary>
        public static Color TextColor
            => GUI.skin.label.normal.textColor;

        /************************************************************************************************************************/

        private static GUIStyle _MiniButtonStyle;

        /// <summary>A more compact <see cref="EditorStyles.miniButton"/> with a fixed size as a tiny box.</summary>
        public static GUIStyle MiniButtonStyle
            => _MiniButtonStyle ??= new(EditorStyles.miniButton)
            {
                margin = new(0, 0, 2, 0),
                padding = new(2, 3, 2, 2),
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = LineHeight,
                fixedWidth = LineHeight - 1,
            };

        private static GUIStyle _NoPaddingButtonStyle;

        /// <summary><see cref="MiniButtonStyle"/> with no <see cref="GUIStyle.padding"/>.</summary>
        public static GUIStyle NoPaddingButtonStyle
            => _NoPaddingButtonStyle ??= new(MiniButtonStyle)
            {
                padding = new(),
                fixedWidth = LineHeight,
            };

        /************************************************************************************************************************/

        private static GUIStyle _RightLabelStyle;

        /// <summary><see cref="EditorStyles.label"/> using <see cref="TextAnchor.MiddleRight"/>.</summary>
        public static GUIStyle RightLabelStyle
            => _RightLabelStyle ??= new(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight,
            };

        /************************************************************************************************************************/

        private static GUIStyle _MiniButtonNoPadding;

        /// <summary>A more compact <see cref="EditorStyles.miniButton"/> with no padding for its content.</summary>
        public static GUIStyle MiniButtonNoPadding
        {
            get
            {
                _MiniButtonNoPadding ??= new(EditorStyles.miniButton)
                {
                    padding = new(),
                    overflow = new(),
                };

                return _MiniButtonNoPadding;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Constants used by <see cref="Event.commandName"/>.</summary>
        /// <remarks>Key combinations are listed for Windows. Other platforms may differ.</remarks>
        public static class Commands
        {
            /************************************************************************************************************************/

            /// <summary><see cref="KeyCode.Delete"/></summary>
            public const string SoftDelete = "SoftDelete";

            /// <summary><see cref="KeyCode.LeftControl"/> + <see cref="KeyCode.Delete"/></summary>
            public const string Delete = "Delete";

            /// <summary><see cref="KeyCode.LeftControl"/> + <see cref="KeyCode.C"/></summary>
            public const string Copy = "Copy";

            /// <summary><see cref="KeyCode.LeftControl"/> + <see cref="KeyCode.X"/></summary>
            public const string Cut = "Cut";

            /// <summary><see cref="KeyCode.LeftControl"/> + <see cref="KeyCode.V"/></summary>
            public const string Paste = "Paste";

            /// <summary><see cref="KeyCode.LeftControl"/> + <see cref="KeyCode.D"/></summary>
            public const string Duplicate = "Duplicate";

            /// <summary><see cref="KeyCode.LeftControl"/> + <see cref="KeyCode.A"/></summary>
            public const string SelectAll = "SelectAll";

            /// <summary><see cref="KeyCode.F"/></summary>
            public const string FrameSelected = "FrameSelected";

            /// <summary><see cref="KeyCode.LeftShift"/> + <see cref="KeyCode.F"/></summary>
            public const string FrameSelectedWithLock = "FrameSelectedWithLock";

            /// <summary><see cref="KeyCode.LeftControl"/> + <see cref="KeyCode.F"/></summary>
            public const string Find = "Find";

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Layout
        /************************************************************************************************************************/

        /// <summary>The offset currently applied to the GUI by <see cref="GUI.BeginGroup(Rect)"/>.</summary>
        public static Vector2 GuiOffset { get; set; }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="UnityEditorInternal.InternalEditorUtility.RepaintAllViews"/>.</summary>
        public static void RepaintEverything()
            => UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

        /************************************************************************************************************************/

        /// <summary><see cref="GUILayoutUtility.GetRect(float, float)"/></summary>
        public static Rect LayoutRect(float height)
            => GUILayoutUtility.GetRect(0, height);

        /// <summary><see cref="GUILayoutUtility.GetRect(float, float, GUIStyle)"/></summary>
        public static Rect LayoutRect(float height, GUIStyle style)
            => GUILayoutUtility.GetRect(0, height, style);

        /************************************************************************************************************************/

        /// <summary>Indicates where <see cref="LayoutSingleLineRect"/> should add the <see cref="StandardSpacing"/>.</summary>
        public enum SpacingMode
        {
            /// <summary>No extra space.</summary>
            None,

            /// <summary>Add extra space before the new area.</summary>
            Before,

            /// <summary>Add extra space after the new area.</summary>
            After,

            /// <summary>Add extra space before and after the new area.</summary>
            BeforeAndAfter
        }

        /// <summary>
        /// Uses <see cref="GUILayoutUtility.GetRect(float, float)"/> to get a <see cref="Rect"/> with the specified
        /// `height` and the <see cref="StandardSpacing"/> added according to the specified `spacing`.
        /// </summary>
        public static Rect LayoutRect(float height, SpacingMode spacing)
        {
            Rect rect;
            switch (spacing)
            {
                case SpacingMode.None:
                    return LayoutRect(height);

                case SpacingMode.Before:
                    rect = LayoutRect(height + StandardSpacing);
                    rect.yMin += StandardSpacing;
                    return rect;

                case SpacingMode.After:
                    rect = LayoutRect(height + StandardSpacing);
                    rect.height -= StandardSpacing;
                    return rect;

                case SpacingMode.BeforeAndAfter:
                    rect = LayoutRect(height + StandardSpacing * 2);
                    rect.yMin += StandardSpacing;
                    rect.height -= StandardSpacing;
                    return rect;

                default:
                    throw new ArgumentException($"Unsupported {nameof(StandardSpacing)}: " + spacing, nameof(spacing));
            }
        }

        /// <summary>
        /// Uses <see cref="GUILayoutUtility.GetRect(float, float)"/> to get a <see cref="Rect"/> occupying a single
        /// standard line with the <see cref="StandardSpacing"/> added according to the specified `spacing`.
        /// </summary>
        public static Rect LayoutSingleLineRect(SpacingMode spacing = SpacingMode.None)
            => LayoutRect(LineHeight, spacing);

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="Rect.height"/> is positive, this method moves the <see cref="Rect.y"/> by that amount and
        /// adds the <see cref="StandardSpacing"/>.
        /// </summary>
        public static void NextVerticalArea(ref Rect area)
        {
            if (area.height > 0)
                area.y += area.height + StandardSpacing;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Subtracts the `width` from the left side of the `area`
        /// and returns a new <see cref="Rect"/> occupying the removed section.
        /// </summary>
        public static Rect StealFromLeft(ref Rect area, float width, float padding = 0)
        {
            var newRect = new Rect(area.x, area.y, width, area.height);
            area.xMin += width + padding;
            return newRect;
        }

        /// <summary>
        /// Subtracts the `width` from the right side of the `area`
        /// and returns a new <see cref="Rect"/> occupying the removed section.
        /// </summary>
        public static Rect StealFromRight(ref Rect area, float width, float padding = 0)
        {
            area.width -= width + padding;
            return new(area.xMax + padding, area.y, width, area.height);
        }

        /// <summary>
        /// Subtracts the `height` from the top side of the `area`
        /// and returns a new <see cref="Rect"/> occupying the removed section.
        /// </summary>
        public static Rect StealFromTop(ref Rect area, float height, float padding = 0)
        {
            var newRect = new Rect(area.x, area.y, area.width, height);
            area.yMin += height + padding;
            return newRect;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Subtracts the <see cref="LineHeight"/> from the top side of the `area`
        /// and returns a new <see cref="Rect"/> occupying the removed section.
        /// </summary>
        public static Rect StealLineFromTop(ref Rect area)
            => StealFromTop(ref area, LineHeight, StandardSpacing);

        /************************************************************************************************************************/

        /// <summary>
        /// Returns a copy of the `rect` expanded by the specified `amount`
        /// (or contracted if negative).
        /// </summary>
        public static Rect Expand(this Rect rect, float amount)
            => new(
                rect.x - amount,
                rect.y - amount,
                rect.width + amount * 2,
                rect.height + amount * 2);

        /// <summary>
        /// Returns a copy of the `rect` expanded by the specified amounts
        /// on each axis (or contracted if negative).
        /// </summary>
        public static Rect Expand(this Rect rect, float x, float y)
            => new(
                rect.x - x,
                rect.y - y,
                rect.width + x * 2,
                rect.height + y * 2);

        /************************************************************************************************************************/

        /// <summary>Returns a copy of the `rect` expanded to include the `other`.</summary>
        public static Rect Encapsulate(this Rect rect, Rect other)
            => Rect.MinMaxRect(
                Math.Min(rect.xMin, other.xMin),
                Math.Min(rect.yMin, other.yMin),
                Math.Max(rect.xMax, other.xMax),
                Math.Max(rect.yMax, other.yMax));

        /************************************************************************************************************************/

        /// <summary>
        /// Divides the given `area` such that the fields associated with both labels will have equal space
        /// remaining after the labels themselves.
        /// </summary>
        public static void SplitHorizontally(
            Rect area,
            string label0,
            string label1,
            out float width0,
            out float width1,
            out Rect rect0,
            out Rect rect1)
        {
            width0 = CalculateLabelWidth(label0);
            width1 = CalculateLabelWidth(label1);

            const float Padding = 1;

            rect0 = rect1 = area;

            var remainingWidth = area.width - width0 - width1 - Padding;
            rect0.width = width0 + remainingWidth * 0.5f;
            rect1.xMin = rect0.xMax + Padding;
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] Calls <see cref="GUIStyle.CalcMinMaxWidth"/> and returns the max width.</summary>
        public static float CalculateWidth(this GUIStyle style, GUIContent content)
        {
            style.CalcMinMaxWidth(content, out _, out var width);
            return Mathf.Ceil(width);
        }

        /// <summary>[Animancer Extension] Calls <see cref="GUIStyle.CalcMinMaxWidth"/> and returns the max width.</summary>
        public static float CalculateWidth(this GUIStyle style, string text)
        {
            using (var content = PooledGUIContent.Acquire(text))
                return style.CalculateWidth(content);
        }

        /************************************************************************************************************************/

        private static ConversionCache<string, float> _LabelWidthCache;

        /// <summary>
        /// Calls <see cref="GUIStyle.CalcMinMaxWidth"/> using <see cref="GUISkin.label"/> and returns the max
        /// width. The result is cached for efficient reuse.
        /// </summary>
        public static float CalculateLabelWidth(string text)
        {
            _LabelWidthCache ??= ConversionCache.CreateWidthCache(GUI.skin.label);

            return _LabelWidthCache.Convert(text);
        }

        /************************************************************************************************************************/

        private static string[] _IntToStringCache;

        /// <summary>Caches and returns <see cref="int.ToString()"/> if <c>0 &lt;= value &lt; 100</c>.</summary>
        public static string ToStringCached(this int value)
        {
            const int CacheSize = 100;

            if (value < 0 || value >= CacheSize)
                return value.ToString();

            if (_IntToStringCache == null)
            {
                _IntToStringCache = new string[CacheSize];
                for (int i = 0; i < _IntToStringCache.Length; i++)
                    _IntToStringCache[i] = i.ToString();
            }

            return _IntToStringCache[value];
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Begins a vertical layout group using the given style and decreases the
        /// <see cref="EditorGUIUtility.labelWidth"/> to compensate for the indentation.
        /// </summary>
        public static void BeginVerticalBox(GUIStyle style)
        {
            if (style == null)
            {
                GUILayout.BeginVertical();
                return;
            }

            GUILayout.BeginVertical(style);
            EditorGUIUtility.labelWidth -= style.padding.left;
        }

        /// <summary>
        /// Ends a layout group started by <see cref="BeginVerticalBox"/> and restores the
        /// <see cref="EditorGUIUtility.labelWidth"/>.
        /// </summary>
        public static void EndVerticalBox(GUIStyle style)
        {
            if (style != null)
                EditorGUIUtility.labelWidth += style.padding.left;

            GUILayout.EndVertical();
        }

        /************************************************************************************************************************/

        private static Func<Rect> _GetGUIClipRect;

        /// <summary>Returns the <see cref="Rect"/> of the current <see cref="GUI.BeginClip(Rect)"/>.</summary>
        public static Rect GetGUIClipRect()
        {
            if (_GetGUIClipRect != null)
                return _GetGUIClipRect();

            var type = typeof(GUI).Assembly.GetType("UnityEngine.GUIClip");
            var method = type?.GetMethod("GetTopRect", AnimancerReflection.AnyAccessBindings);

            if (method != null &&
                method.ReturnType != null &&
                method.GetParameters().Length == 0)
            {
                _GetGUIClipRect = (Func<Rect>)Delegate.CreateDelegate(typeof(Func<Rect>), method);
            }
            else
            {
                _GetGUIClipRect = () => new(0, 0, Screen.width, Screen.height);
            }

            return _GetGUIClipRect();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Labels
        /************************************************************************************************************************/

        private static GUIStyle _WeightLabelStyle;
        private static float _WeightLabelWidth = -1;

        /// <summary>
        /// Draws a label showing the `weight` aligned to the right side of the `area` and reduces its
        /// <see cref="Rect.width"/> to remove that label from its area.
        /// </summary>
        public static void DoWeightLabel(ref Rect area, float weight, float effectiveWeight)
        {
            var label = WeightToShortString(weight, out var isExact);

            _WeightLabelStyle ??= new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
            };

            if (_WeightLabelWidth < 0)
            {
                _WeightLabelStyle.fontStyle = FontStyle.Italic;
                _WeightLabelWidth = _WeightLabelStyle.CalculateWidth("0.0");
            }

            _WeightLabelStyle.normal.textColor = Color.Lerp(Color.grey, TextColor, 0.2f + effectiveWeight * 0.8f);
            _WeightLabelStyle.fontStyle = isExact ? FontStyle.Normal : FontStyle.Italic;

            var weightArea = StealFromRight(ref area, _WeightLabelWidth);

            GUI.Label(weightArea, label, _WeightLabelStyle);
        }

        /************************************************************************************************************************/

        private static ConversionCache<float, string> _ShortWeightCache;

        /// <summary>Returns a string which approximates the `weight` into no more than 3 digits.</summary>
        private static string WeightToShortString(float weight, out bool isExact)
        {
            isExact = true;

            if (weight == 0)
                return "0.0";
            if (weight == 1)
                return "1.0";

            isExact = false;

            if (weight >= -0.5f && weight < 0.05f)
                return "~0.";
            if (weight >= 0.95f && weight < 1.05f)
                return "~1.";

            if (weight <= -99.5f)
                return "-??";
            if (weight >= 999.5f)
                return "???";

            _ShortWeightCache ??= new(value =>
            {
                if (value < -9.5f) return $"{value:F0}";
                if (value < -0.5f) return $"{value:F0}.";
                if (value < 9.5f) return $"{value:F1}";
                if (value < 99.5f) return $"{value:F0}.";
                return $"{value:F0}";
            });

            var rounded = weight > 0 ? Mathf.Floor(weight * 10) : Mathf.Ceil(weight * 10);
            isExact = Mathf.Approximately(weight * 10, rounded);

            return _ShortWeightCache.Convert(weight);
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="EditorGUIUtility.labelWidth"/> from before <see cref="BeginTightLabel"/>.</summary>
        private static float _TightLabelWidth;

        /// <summary>
        /// Stores the <see cref="EditorGUIUtility.labelWidth"/> and changes it to the exact width of the `label`.
        /// </summary>
        public static string BeginTightLabel(string label)
        {
            _TightLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = CalculateLabelWidth(label) + EditorGUI.indentLevel * IndentSize;
            return label;
        }

        /// <summary>Reverts <see cref="EditorGUIUtility.labelWidth"/> to its previous value.</summary>
        public static void EndTightLabel()
        {
            EditorGUIUtility.labelWidth = _TightLabelWidth;
        }

        /************************************************************************************************************************/

        /// <summary>Draws a button using <see cref="EditorStyles.miniButton"/> and <see cref="DontExpandWidth"/>.</summary>
        public static bool CompactMiniButton(GUIContent content)
            => GUILayout.Button(content, EditorStyles.miniButton, DontExpandWidth);

        /// <summary>Draws a button using <see cref="EditorStyles.miniButton"/>.</summary>
        public static bool CompactMiniButton(Rect area, GUIContent content)
            => GUI.Button(area, content, EditorStyles.miniButton);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Fields
        /************************************************************************************************************************/

        /// <summary>Draws a label field with a foldout.</summary>
        public static bool DoLabelFoldoutFieldGUI(string label, string value, bool isExpanded)
        {
            using (var labelContent = PooledGUIContent.Acquire(label))
            using (var valueContent = PooledGUIContent.Acquire(value))
                return DoLabelFoldoutFieldGUI(labelContent, valueContent, isExpanded);
        }

        /// <summary>Draws a label field with a foldout.</summary>
        public static bool DoLabelFoldoutFieldGUI(GUIContent label, GUIContent value, bool isExpanded)
        {
            var area = LayoutSingleLineRect();

            EditorGUI.LabelField(area, label, value);

            return EditorGUI.Foldout(area, isExpanded, "", true);
        }

        /// <summary>Draws a foldout which stores its state in a hash set.</summary>
        public static bool DoHashedFoldoutGUI<T>(Rect area, HashSet<T> expandedItems, T item)
        {
            var wasExpanded = expandedItems.Contains(item);
            var isExpanded = EditorGUI.Foldout(area, wasExpanded, "", true);

            if (isExpanded != wasExpanded)
                if (isExpanded)
                    expandedItems.Add(item);
                else
                    expandedItems.Remove(item);

            return isExpanded;
        }

        /************************************************************************************************************************/

        /// <summary>Draws an object reference field.</summary>
        public static T DoObjectFieldGUI<T>(
            Rect area,
            GUIContent label,
            T value,
            bool allowSceneObjects)
            where T : Object
            => EditorGUI.ObjectField(area, label, value, typeof(T), allowSceneObjects) as T;

        /// <summary>Draws an object reference field.</summary>
        public static T DoObjectFieldGUI<T>(
            Rect area,
            string label,
            T value,
            bool allowSceneObjects)
            where T : Object
        {
            using var content = PooledGUIContent.Acquire(label);
            return DoObjectFieldGUI(area, content, value, allowSceneObjects);
        }

        /************************************************************************************************************************/

        /// <summary>Draws an object reference field.</summary>
        public static T DoObjectFieldGUI<T>(
            GUIContent label,
            T value,
            bool allowSceneObjects)
            where T : Object
        {
            var height = EditorGUIUtility.HasObjectThumbnail(typeof(T)) ? 64f : LineHeight;
            var area = EditorGUILayout.GetControlRect(label != null, height);
            return DoObjectFieldGUI(area, label, value, allowSceneObjects);
        }

        /// <summary>Draws an object reference field.</summary>
        public static T DoObjectFieldGUI<T>(
            string label,
            T value,
            bool allowSceneObjects)
            where T : Object
        {
            using var content = PooledGUIContent.Acquire(label);
            return DoObjectFieldGUI(content, value, allowSceneObjects);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Draws an object reference field with a dropdown button as its label
        /// and returns true if clicked.
        /// </summary>
        public static bool DoDropdownObjectFieldGUI<T>(
            Rect area,
            GUIContent label,
            bool showDropdown,
            ref T value)
            where T : Object
        {
            var labelWidth = EditorGUIUtility.labelWidth;

            labelWidth += 2;
            area.xMin -= 1;

            var spacing = StandardSpacing;
            var labelArea = StealFromLeft(ref area, labelWidth - spacing, spacing);

            value = DoObjectFieldGUI(area, "", value, true);

            if (showDropdown)
            {
                return EditorGUI.DropdownButton(labelArea, label, FocusType.Passive);
            }
            else
            {
                GUI.Label(labelArea, label);
                return false;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Events
        /************************************************************************************************************************/

        /// <summary>Sets <see cref="GUI.changed"/> if `guiChanged` is <c>true</c>.</summary>
        public static void SetGuiChanged(bool guiChanged)
        {
            if (guiChanged)
                GUI.changed = true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="Event.Use"/> and sets the
        /// <see cref="GUI.changed"/> and <see cref="GUIUtility.hotControl"/>.
        /// </summary>
        public static void Use(this Event guiEvent, int controlId, bool guiChanged = false)
        {
            SetGuiChanged(guiChanged);
            GUIUtility.hotControl = controlId;
            guiEvent.Use();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the <see cref="GUIUtility.hotControl"/> and uses the `currentEvent`
        /// if the mouse position is inside the `area`.
        /// </summary>
        /// <remarks>This method is useful for handling <see cref="EventType.MouseDown"/>.</remarks>
        public static bool TryUseMouseDown(Rect area, Event currentEvent, int controlID)
        {
            if (!area.Contains(currentEvent.mousePosition))
                return false;

            GUIUtility.keyboardControl = 0;
            GUIUtility.hotControl = controlID;
            currentEvent.Use();
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Releases the <see cref="GUIUtility.hotControl"/> and uses the `currentEvent` if it was the active control.
        /// </summary>
        /// <remarks>This method is useful for handling <see cref="EventType.MouseUp"/>.</remarks>
        public static bool TryUseMouseUp(Event currentEvent, int controlID, bool guiChanged = false)
        {
            if (GUIUtility.hotControl != controlID)
                return false;

            GUIUtility.hotControl = 0;
            currentEvent.Use();
            SetGuiChanged(guiChanged);
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Uses the `currentEvent` and sets <see cref="GUI.changed"/>
        /// if the `controlID` matches the <see cref="GUIUtility.hotControl"/>.
        /// </summary>
        /// <remarks>This method is useful for handling <see cref="EventType.MouseDrag"/>.</remarks>
        public static bool TryUseHotControl(Event currentEvent, int controlID, bool guiChanged = true)
        {
            if (GUIUtility.hotControl != controlID)
                return false;

            SetGuiChanged(guiChanged);
            currentEvent.Use();
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Uses the `currentEvent` if the `controlID` has <see cref="GUIUtility.keyboardControl"/>.
        /// If a `key` is specified, other keys will be ignored.
        /// </summary>
        /// <remarks>
        /// This method is useful for handling
        /// <see cref="EventType.KeyDown"/> and <see cref="EventType.KeyUp"/>.
        /// </remarks>
        public static bool TryUseKey(Event currentEvent, int controlID, KeyCode key = KeyCode.None)
        {
            if (GUIUtility.keyboardControl != controlID)
                return false;

            if (key != KeyCode.None && currentEvent.keyCode != key)
                return false;

            currentEvent.Use();
            GUI.changed = true;
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns true and uses the current event if it is
        /// <see cref="EventType.MouseUp"/> inside the specified `area`.
        /// </summary>
        /// <remarks>Uses <see cref="EventType.MouseDown"/> and <see cref="EventType.MouseUp"/> events.</remarks>
        public static bool TryUseClickEvent(Rect area, int button = -1, int controlID = 0)
        {
            if (controlID == 0)
                controlID = GUIUtility.GetControlID(FocusType.Passive);

            var currentEvent = Event.current;

            if (button >= 0 && currentEvent.button != button)
                return false;

            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    TryUseMouseDown(area, currentEvent, controlID);
                    break;

                case EventType.MouseUp:
                    return
                        TryUseMouseUp(currentEvent, controlID, true) &&
                        area.Contains(currentEvent.mousePosition);
            }

            return false;
        }

        /// <summary>
        /// Returns true and uses the current event if it is <see cref="EventType.MouseUp"/> inside the last GUI Layout
        /// <see cref="Rect"/> that was drawn.
        /// </summary>
        public static bool TryUseClickEventInLastRect(int button = -1)
            => TryUseClickEvent(GUILayoutUtility.GetLastRect(), button);

        /************************************************************************************************************************/

        /// <summary>Is the `currentEvent` a Middle Click or Alt + Left Click? </summary>
        public static bool IsMiddleClick(this Event currentEvent)
            => currentEvent.button == 2
            || (currentEvent.button == 0 && currentEvent.modifiers == EventModifiers.Alt);

        /************************************************************************************************************************/

        /// <summary>Deselects any selected IMGUI control.</summary>
        public static void Deselect()
        {
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Other
        /************************************************************************************************************************/

        /// <summary>Draws a line.</summary>
        /// <remarks>
        /// Use <see cref="BeginTriangles"/>, <see cref="DrawLineBatched"/>, and <see cref="EndTriangles"/>
        /// if you want to draw multiple lines more efficiently.
        /// </remarks>
        public static void DrawLine(
            Vector2 a,
            Vector2 b,
            float width,
            Color color)
        {
            BeginTriangles(color);
            DrawLineBatched(a, b, width);
            EndTriangles();
        }

        /************************************************************************************************************************/

        /// <summary>Sets up the rendering details for <see cref="DrawLineBatched"/>.</summary>
        /// <remarks>
        /// If the color doesn't work correctly, you may need to call
        /// <see cref="Handles.DrawLine(Vector3, Vector3)"/> before this.
        /// </remarks>
        public static void BeginTriangles(Color color)
        {
            GL.Begin(GL.TRIANGLES);

            GL.Color(color);
        }

        /// <summary>Cleans up the rendering details for <see cref="DrawLineBatched"/>.</summary>
        public static void EndTriangles()
        {
            GL.End();
        }

        /************************************************************************************************************************/

        /// <summary>Draws a line.</summary>
        /// <remarks>Must be called after <see cref="BeginTriangles"/> and before <see cref="EndTriangles"/>.</remarks>
        public static void DrawLineBatched(
            Vector2 a,
            Vector2 b,
            float width)
        {
            var perpendicular = 0.5f * width * (a - b).GetPerpendicular().normalized;

            var a0 = a - perpendicular;
            var a1 = a + perpendicular;
            var b0 = b - perpendicular;
            var b1 = b + perpendicular;

            GL.Vertex(a0);
            GL.Vertex(a1);
            GL.Vertex(b0);

            GL.Vertex(a1);
            GL.Vertex(b0);
            GL.Vertex(b1);
        }

        /************************************************************************************************************************/

        /// <summary>Draws triangular arrow.</summary>
        /// <remarks>Must be called after <see cref="BeginTriangles"/> and before <see cref="EndTriangles"/>.</remarks>
        public static void DrawArrowTriangleBatched(
            Vector2 point,
            Vector2 direction,
            float width,
            float length)
        {
            direction.Normalize();

            var perpendicular = 0.5f * width * direction.GetPerpendicular();

            // These commented out bits would use the point as the center of the triangle instead.

            direction *= length;// * 0.5f;

            var back = point - direction;

            GL.Vertex(point);// + direction);
            GL.Vertex(back + perpendicular);
            GL.Vertex(back - perpendicular);
        }

        /************************************************************************************************************************/

        /// <summary>Returns a vector perpendicular to the given value with the same magnitude.</summary>
        public static Vector2 GetPerpendicular(this Vector2 vector)
            => new(vector.y, -vector.x);

        /************************************************************************************************************************/

        /// <summary>Draws a `sprite` in the given `area`.</summary>
        public static void DrawSprite(Rect area, Sprite sprite)
        {
            var texture = sprite.texture;
            var textureWidth = texture.width;
            var textureHeight = texture.height;
            var spriteRect = sprite.rect;
            spriteRect.x /= textureWidth;
            spriteRect.y /= textureHeight;
            spriteRect.width /= textureWidth;
            spriteRect.height /= textureHeight;

            GUI.DrawTextureWithTexCoords(
                area,
                texture,
                spriteRect);
        }

        /************************************************************************************************************************/

        /// <summary>Returns a colour with its hue based on the `hash`.</summary>
        public static Color GetHashColor(int hash, float s = 1, float v = 1, float a = 1)
        {
            uint uHash = (uint)hash;
            double dHash = (double)uHash / uint.MaxValue;
            float h = (float)dHash;
            var color = Color.HSVToRGB(h, s, v);
            color.a = a;
            return color;
        }

        /************************************************************************************************************************/

        /// <summary>Clears the <see cref="Selection.objects"/> then returns it to its current state.</summary>
        /// <remarks>
        /// This forces the <see cref="UnityEditorInternal.ReorderableList"/> drawer to adjust to height changes which
        /// it unfortunately doesn't do on its own..
        /// </remarks>
        public static void ReSelectCurrentObjects()
        {
            var selection = Selection.objects;
            Selection.objects = Array.Empty<Object>();
            EditorApplication.delayCall += () =>
                EditorApplication.delayCall += () =>
                    Selection.objects = selection;
        }

        /************************************************************************************************************************/

        /// <summary>Draws a button which toggles between play and pause icons.</summary>
        public static bool DoPlayPauseToggle(
            Rect area,
            bool isPlaying,
            GUIStyle style = null,
            string tooltip = null)
        {
            var content = isPlaying
                ? AnimancerIcons.PauseIcon
                : AnimancerIcons.PlayIcon;

            var oldTooltip = content.tooltip;
            content.tooltip = tooltip;

            style ??= MiniButtonNoPadding;

            if (GUI.Button(area, content, style))
                isPlaying = !isPlaying;

            content.tooltip = oldTooltip;

            return isPlaying;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

