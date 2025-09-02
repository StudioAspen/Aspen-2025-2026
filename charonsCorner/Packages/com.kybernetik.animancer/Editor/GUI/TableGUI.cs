// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Utility for drawing tables.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TableGUI
    [Serializable]
    public class TableGUI
    {
        /************************************************************************************************************************/

        /// <summary>The pixel spacing between cells.</summary>
        public static float Padding
            => 0;// StandardSpacing;

        /// <summary>The style for the horizontal scroll bar.</summary>
        public static GUIStyle HorizontalScrollBar
            => GUI.skin.horizontalScrollbar;

        /// <summary>The style for the vertical scroll bar.</summary>
        public static GUIStyle VerticalScrollBar
            => GUI.skin.verticalScrollbar;

        /************************************************************************************************************************/

        /// <summary>Draws the GUI for a specified cell.</summary>
        /// <remarks>`column` and `row` are given <c>-1</c> for the labels.</remarks>
        public delegate void CellGUIDelegate(Rect area, int column, int row);

        /// <summary>Draws the GUI for a specified cell.</summary>
        /// <remarks>`column` and `row` are given <c>-1</c> for the labels.</remarks>
        public CellGUIDelegate DoCellGUI;

        [NonSerialized] private Vector2 _MinCellSize;// Pixels.
        [NonSerialized] private Vector2 _MaxCellSize;// Pixels.

        [SerializeField] private Vector2 _LabelSize = new(0.25f, 0.25f);// Normalized.
        [SerializeField] private Vector2 _ScrollPosition;

        /// <summary>The minimum pixel size of each cell.</summary>
        public ref Vector2 MinCellSize
            => ref _MinCellSize;

        /// <summary>The maximum pixel size of each cell.</summary>
        public ref Vector2 MaxCellSize
            => ref _MaxCellSize;

        /// <summary>[<see cref="SerializeField"/>] The normalized size of the header labels.</summary>
        public ref Vector2 LabelSize
            => ref _LabelSize;

        /// <summary>[<see cref="SerializeField"/>] The position the table is currently scrolled to.</summary>
        public ref Vector2 ScrollPosition
            => ref _ScrollPosition;

        /************************************************************************************************************************/

        /// <summary>Draws this table.</summary>
        public void DoTableGUI(
            Rect area,
            int columns,
            int rows)
        {
            HandleInput(area);

            var scrollBarSize = new Vector2(
                VerticalScrollBar.fixedWidth + Padding,
                HorizontalScrollBar.fixedHeight + Padding);

            area.size -= scrollBarSize;

            CalculateSizes(area, columns, rows, out var labelSize, out var cellSize);

            var indexBounds = new RectInt(
                (int)(_ScrollPosition.x / cellSize.x),
                (int)(_ScrollPosition.y / cellSize.y),
                Mathf.CeilToInt((area.width - labelSize.x) / cellSize.x) + 1,
                Mathf.CeilToInt((area.height - labelSize.y) / cellSize.y) + 1);

            area.size += scrollBarSize;

            var cornerArea = new Rect(area.position, labelSize + scrollBarSize);
            DoCellGUI(cornerArea, -1, -1);

            var labelResizerArea = cornerArea;
            labelResizerArea.xMin = labelResizerArea.xMax - scrollBarSize.x;
            labelResizerArea.yMin = labelResizerArea.yMax - scrollBarSize.y;
            DoLabelResizerGUI(labelResizerArea, area);

            var columnLabelArea = new Rect(
                area.x + scrollBarSize.x + labelSize.x + Padding,
                area.y,
                cellSize.x,
                labelSize.y);
            var rowLabelArea = new Rect(
                area.x,
                area.y + scrollBarSize.y + labelSize.y + Padding,
                labelSize.x,
                cellSize.y);

            area.xMin += labelSize.x + Padding + scrollBarSize.x;
            area.yMin += labelSize.y + Padding + scrollBarSize.y;

            GUI.BeginClip(area);

            for (int x = indexBounds.xMin; x < indexBounds.xMax; x++)
            {
                for (int y = indexBounds.yMin; y < indexBounds.yMax; y++)
                {
                    var cellArea = new Rect(
                        (cellSize.x + Padding) * x - _ScrollPosition.x,
                        (cellSize.y + Padding) * y - _ScrollPosition.y,
                        cellSize.x,
                        cellSize.y);

                    DoCellGUI(cellArea, x, y);
                }
            }

            GUI.EndClip();

            DrawColumnLabels(columnLabelArea, columns, area.width, scrollBarSize.y);
            DrawRowLabels(rowLabelArea, rows, area.height, scrollBarSize.x);
        }

        /************************************************************************************************************************/

        private static readonly int LabelResizerHint = "LabelResizer".GetHashCode();

        private static GUIContent _LabelResizerIcon;

        private void DoLabelResizerGUI(
            Rect resizerArea,
            Rect tableArea)
        {
            var control = new GUIControl(resizerArea, LabelResizerHint);

            switch (control.EventType)
            {
                case EventType.MouseDown:
                    if (control.Event.button == 0 &&
                        control.TryUseMouseDown())
                    {
                        if (control.Event.clickCount == 2)
                        {
                            AutoSizeLabels(tableArea);
                            GUIUtility.hotControl = 0;
                        }
                    }

                    break;

                case EventType.MouseUp:
                    control.TryUseMouseUp();
                    break;

                case EventType.MouseDrag:
                    if (control.TryUseHotControl())
                    {
                        var offset = control.Event.mousePosition - tableArea.position;
                        LabelSize = new(
                            offset.x / tableArea.width,
                            offset.y / tableArea.height);
                    }

                    break;

                case EventType.KeyDown:
                    if (control.TryUseKey(KeyCode.Escape))
                        Deselect();
                    break;

                case EventType.Repaint:
                    EditorGUIUtility.AddCursorRect(resizerArea, MouseCursor.ResizeUpLeft);

                    AnimancerIcons.IconContent(ref _LabelResizerIcon, "MoveTool");
                    if (_LabelResizerIcon != null)
                        GUI.DrawTexture(resizerArea, _LabelResizerIcon.image);
                    break;
            }
        }

        /************************************************************************************************************************/

        private static readonly Matrix4x4
            Rotate90LeftMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, -90));

        private void DrawColumnLabels(Rect area, int count, float availableSize, float scrollSize)
        {
            var previousClip = GetGUIClipRect();
            GUI.EndClip();

            var totalSize =
                area.width * count +
                Padding * (count - 1);

            var totalArea = new Rect(
                area.x,
                area.y + area.height + Padding,
                availableSize,
                scrollSize);

            var translation = previousClip.position + area.position;
            translation = -translation.GetPerpendicular();
            translation.x -= area.height;

            area.x = 0;
            area.y = -_ScrollPosition.x;
            (area.width, area.height) = (area.height, area.width);

            GUI.BeginClip(new(0, 0, area.width, availableSize));

            var matrix = GUI.matrix;
            GUI.matrix =
                Rotate90LeftMatrix *
                Matrix4x4.Translate(translation);

            for (int i = 0; i < count; i++)
            {
                DoCellGUI(area, i, -1);

                area.y += area.height + Padding;
            }

            GUI.matrix = matrix;

            GUI.EndClip();
            GUI.BeginClip(previousClip);

            var enabled = GUI.enabled;
            GUI.enabled = availableSize < totalSize;

            _ScrollPosition.x = GUI.HorizontalScrollbar(
                totalArea,
                _ScrollPosition.x,
                availableSize,
                0,
                totalSize);

            GUI.enabled = enabled;
        }

        /************************************************************************************************************************/

        private void DrawRowLabels(Rect area, int count, float availableSize, float scrollSize)
        {
            var totalArea = area;
            totalArea.height = availableSize;

            GUI.BeginClip(totalArea);

            area.x = 0;
            area.y = -_ScrollPosition.y;

            for (int i = 0; i < count; i++)
            {
                DoCellGUI(area, -1, i);

                area.y += area.height + Padding;
            }

            GUI.EndClip();

            var totalSize =
                area.height * count +
                Padding * (count - 1);

            totalArea.x += totalArea.width + Padding;
            totalArea.width = scrollSize;

            var enabled = GUI.enabled;
            GUI.enabled = availableSize < totalSize;

            _ScrollPosition.y = GUI.VerticalScrollbar(
                totalArea,
                _ScrollPosition.y,
                availableSize,
                0,
                totalSize);

            GUI.enabled = enabled;
        }

        /************************************************************************************************************************/

        private static readonly int ControlHint = nameof(TableGUI).GetHashCode();

        private void HandleInput(Rect area)
        {
            var control = new GUIControl(area, ControlHint);

            switch (control.EventType)
            {
                case EventType.ScrollWheel:
                    if (control.ContainsMousePosition)
                    {
                        var delta = control.Event.delta * 5;
                        if (control.Event.shift)
                            delta = delta.GetPerpendicular();

                        _ScrollPosition += delta;

                        control.Event.Use();
                    }
                    break;

                case EventType.MouseDown:
                    if (control.Event.IsMiddleClick())
                        control.TryUseMouseDown();
                    break;

                case EventType.MouseUp:
                    control.TryUseMouseUp();
                    break;

                case EventType.MouseDrag:
                    if (control.TryUseHotControl())
                        _ScrollPosition -= control.Event.delta;
                    break;
            }
        }

        /************************************************************************************************************************/
        #region Size Calculation
        /************************************************************************************************************************/

        /// <summary>Calculates the current label and cell sizes for the given `area`.</summary>
        public void CalculateSizes(
            Rect area,
            int columns,
            int rows,
            out Vector2 labelSize,
            out Vector2 cellSize)
        {
            // Min cell size.
            cellSize = _MinCellSize;
            if (cellSize.x < 1)
                cellSize.x = LineHeight;
            if (cellSize.y < 1)
                cellSize.y = LineHeight;

            // Min label size.
            labelSize.x = Mathf.Clamp(_LabelSize.x, 0, 0.9f);
            labelSize.y = Mathf.Clamp(_LabelSize.y, 0, 0.9f);

            labelSize = Vector2.Scale(area.size, labelSize);
            if (labelSize == default)
                labelSize = cellSize;

            // Expand cells if there is more area available, up to the max.
            var availableSize = area.size - labelSize;
            cellSize.x = StretchCellSize(availableSize.x, cellSize.x, _MaxCellSize.x, columns);
            cellSize.y = StretchCellSize(availableSize.y, cellSize.y, _MaxCellSize.y, rows);

            // Expand labels if there is more area available.
            labelSize.x = StretchLabelSize(area.width, labelSize.x, cellSize.x, columns);
            labelSize.y = StretchLabelSize(area.height, labelSize.y, cellSize.y, rows);
        }

        /************************************************************************************************************************/

        private static float StretchCellSize(
            float availableSize,
            float cellSize,
            float maxCellSize,
            int cellCount)
        {
            if (cellSize < maxCellSize)
            {
                availableSize -= Padding * (cellCount - 1);
                if (availableSize > cellSize * cellCount)
                    cellSize = Math.Min(availableSize / cellCount, maxCellSize);
            }

            return cellSize;
        }

        /************************************************************************************************************************/

        private static float StretchLabelSize(
            float availableSize,
            float labelSize,
            float cellSize,
            int cellCount)
        {
            labelSize = Math.Max(labelSize, availableSize - (cellSize + Padding) * cellCount);
            labelSize = Math.Max(labelSize, cellSize);
            return labelSize;
        }

        /************************************************************************************************************************/

        /// <summary>A delegate to calculate the largest pixel width of the header labels.</summary>
        public Func<float> CalculateWidestLabel { get; set; }

        private void AutoSizeLabels(Rect tableArea)
        {
            if (CalculateWidestLabel == null)
                return;

            var targetLabelSize = CalculateWidestLabel();

            _LabelSize.x = targetLabelSize / tableArea.width;
            _LabelSize.y = targetLabelSize / tableArea.height;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

