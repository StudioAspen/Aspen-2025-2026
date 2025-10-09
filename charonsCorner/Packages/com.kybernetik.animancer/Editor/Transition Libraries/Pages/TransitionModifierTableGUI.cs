// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;
using static Animancer.Editor.TransitionLibraries.TransitionLibrarySelection;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only]
    /// A <see cref="TableGUI"/> for editing
    /// <see cref="Animancer.TransitionLibraries.TransitionLibraryDefinition.Modifiers"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionModifierTableGUI
    [Serializable]
    public class TransitionModifierTableGUI : TableGUI
    {
        /************************************************************************************************************************/

        [NonSerialized] private TransitionLibraryWindow _Window;
        [NonSerialized] private Vector2Int _SelectedCell;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TransitionModifierTableGUI"/>.</summary>
        public TransitionModifierTableGUI()
        {
            base.DoCellGUI = DoCellGUI;
            CalculateWidestLabel = CalculateWidestTransitionLabel;
            MinCellSize = new(LineHeight * 2, LineHeight);
            MaxCellSize = new(LineHeight * 4, LineHeight);
        }

        /************************************************************************************************************************/

        /// <summary>Draws the table GUI.</summary>
        public void DoGUI(
            Rect area,
            TransitionLibraryWindow window)
        {
            _Window = window;
            _SelectedCell = RecalculateSelectedCell(window.Selection);

            var transitions = window.Data.Transitions;
            DoTableGUI(area, transitions.Length, transitions.Length);
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the table coordinates of the `selection`.</summary>
        private Vector2Int RecalculateSelectedCell(TransitionLibrarySelection selection)
        {
            if (selection.Validate())
            {
                switch (selection.Type)
                {
                    case SelectionType.FromTransition:
                    case SelectionType.ToTransition:
                    case SelectionType.Modifier:
                        var cell = new Vector2Int(selection.ToIndex, selection.FromIndex);

                        if (cell.x < 0)
                            cell.x = int.MinValue;

                        if (cell.y < 0)
                            cell.y = int.MinValue;

                        return cell;
                }
            }

            return new(int.MinValue, int.MinValue);
        }

        /************************************************************************************************************************/

        /// <summary>Draws a table cell.</summary>
        private new void DoCellGUI(Rect area, int column, int row)
        {
            var invertHover = false;

            if (column < 0)
            {
                if (row < 0)
                    DoCornerGUI(area);
                else
                    DoLabelGUI(
                        area,
                        row,
                        RightLabelStyle,
                        SelectionType.FromTransition);
            }
            else if (row < 0)
            {
                DoLabelGUI(
                    area,
                    column,
                    EditorStyles.label,
                    SelectionType.ToTransition);

                invertHover = true;
            }
            else
            {
                DoFadeDurationGUI(area, _Window, row, column, "");

            }

            DrawHighlightGUI(area, column, row, invertHover);
        }

        /************************************************************************************************************************/

        /// <summary>Draws the header corner.</summary>
        private void DoCornerGUI(Rect area)
        {
            area.xMin += StandardSpacing;

            var fromArea = area;
            fromArea.y += area.height - LineHeight;
            fromArea.height = LineHeight;

            var toArea = fromArea;
            toArea.y -= toArea.height - Padding;

            var removeArea = toArea;
            removeArea.y -= removeArea.height - Padding;

            var createArea = removeArea;
            createArea.y -= createArea.height - Padding;

            fromArea.width -= VerticalScrollBar.fixedWidth + Padding;

            var style = RightLabelStyle;
            var fontStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;

            GUI.Label(fromArea, "From", style);
            GUI.Label(toArea, "To", style);

            style.fontStyle = fontStyle;

            DoCreateButtonGUI(createArea);
            DoDeleteButtonGUI(removeArea);
        }

        /************************************************************************************************************************/

        /// <summary>Draws a button to create a new transition.</summary>
        private void DoCreateButtonGUI(Rect area)
        {
            if (GUI.Button(area, "Create Transition"))
                TransitionLibraryOperations.CreateTransition(_Window);
        }

        /************************************************************************************************************************/

        /// <summary>Draws a button to remove the selected transition.</summary>
        private void DoDeleteButtonGUI(Rect area)
        {
            TransitionAssetBase transition = null;
            int index = -1;

            var selection = _Window.Selection;
            switch (selection.Type)
            {
                case SelectionType.FromTransition:
                    transition = selection.FromTransition;
                    index = selection.FromIndex;
                    break;

                case SelectionType.ToTransition:
                    transition = selection.ToTransition;
                    index = selection.ToIndex;
                    break;
            }

            using (new EditorGUI.DisabledScope(index < 0 || index >= _Window.Data.Transitions.Length))
                if (GUI.Button(area, "Remove Transition"))
                    TransitionLibraryOperations.AskHowToDeleteTransition(transition, index, _Window);
        }

        /************************************************************************************************************************/

        /// <summary>Draws a row or column label.</summary>
        private void DoLabelGUI(
            Rect area,
            int index,
            GUIStyle style,
            SelectionType selectionType)
        {
            if (!_Window.Data.Transitions.TryGet(index, out var transition))
                return;

            HandleTransitionLabelInput(
                ref area,
                _Window,
                transition,
                index,
                selectionType,
                CalculateTargetTransitionIndex);

            GUI.Label(area, GetTransitionName(transition), style);
        }

        /************************************************************************************************************************/

        /// <summary>Returns the name of the `transition` with a special message for <c>null</c>.</summary>
        public static string GetTransitionName(TransitionAssetBase transition)
            => transition != null
            ? transition.GetCachedName()
            : "<Missing Transition>";

        /************************************************************************************************************************/

        private static readonly int LabelHint = "Label".GetHashCode();

        [NonSerialized] private static bool _IsLabelDrag;

        /// <summary>Handles input events on transition labels.</summary>
        public static void HandleTransitionLabelInput(
            ref Rect area,
            TransitionLibraryWindow window,
            TransitionAssetBase transition,
            int index,
            SelectionType selectionType,
            Func<Rect, int, Event, int> calculateTargetTransitionIndex)
        {
            var control = new GUIControl(area, LabelHint);

            switch (control.EventType)
            {
                case EventType.MouseDown:
                    if (control.Event.button == 0 &&
                        control.TryUseMouseDown())
                    {
                        if (control.Event.clickCount == 2)
                            EditorGUIUtility.PingObject(transition);
                        else
                            window.Selection.Select(window, transition, index, selectionType);

                        _IsLabelDrag = false;
                    }

                    break;

                case EventType.MouseUp:
                    if (control.TryUseMouseUp() && _IsLabelDrag)
                    {
                        var target = calculateTargetTransitionIndex(area, index, control.Event);
                        TransitionLibrarySort.MoveTransition(window, index, target);
                        window.Selection.Select(window, transition, index, selectionType);
                    }

                    break;

                case EventType.MouseDrag:
                    if (control.TryUseHotControl())
                        _IsLabelDrag = true;
                    break;
            }

            if (GUIUtility.hotControl == control.ID && _IsLabelDrag)
            {
                RepaintEverything();
                area.y = control.Event.mousePosition.y - area.height * 0.5f;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the transition index for a drag and drop operation.</summary>
        private static int CalculateTargetTransitionIndex(
            Rect area,
            int index,
            Event currentEvent)
        {
            var distance = currentEvent.mousePosition.y - area.y;
            var offset = Mathf.FloorToInt(distance / area.height);
            return index + offset;
        }

        /************************************************************************************************************************/

        private static GUIStyle _FadeDurationStyle;

        /// <summary>Draws the fade duration for a particular transition combination.</summary>
        public static void DoFadeDurationGUI(
            Rect area,
            TransitionLibraryWindow window,
            int from,
            int to,
            string label)
        {
            _FadeDurationStyle ??= new(EditorStyles.numberField)
            {
                alignment = TextAnchor.MiddleLeft,
            };

            var previousHotControl = GUIUtility.hotControl;

            var hasModifier = window.Data.TryGetModifier(from, to, out var modifier);

            var labelStyle = EditorStyles.label.fontStyle;
            try
            {
                if (hasModifier)
                {
                    EditorStyles.label.fontStyle = FontStyle.Bold;
                    _FadeDurationStyle.fontStyle = FontStyle.Bold;
                    _FadeDurationStyle.fontSize = EditorStyles.numberField.fontSize;
                }
                else
                {
                    _FadeDurationStyle.fontStyle = FontStyle.Normal;
                    _FadeDurationStyle.fontSize = EditorStyles.numberField.fontSize * 4 / 5;
                }

                EditorGUI.BeginChangeCheck();

                // This is basically a float field,
                // but anything that fails to parse will clear the field instead of setting it to 0.

                var text = modifier.FadeDuration.ToStringCached();
                text = EditorGUI.TextField(area, label, text, _FadeDurationStyle);

                if (EditorGUI.EndChangeCheck())
                {
                    if (!float.TryParse(text, out var fadeDuration))
                        fadeDuration = float.NaN;

                    window.RecordUndo()
                        .SetModifier(modifier.WithFadeDuration(fadeDuration));

                    hasModifier = true;

                    RepaintEverything();
                }
            }
            finally
            {
                EditorStyles.label.fontStyle = labelStyle;
            }

            if (previousHotControl != GUIUtility.hotControl)
            {
                window.Selection.Select(
                    window,
                    modifier,
                    modifier.FromIndex,
                    SelectionType.Modifier);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws the selection and hover highlights for a particular cell.</summary>
        private void DrawHighlightGUI(Rect area, int column, int row, bool invertHover)
        {
            if (_Window.Highlighter.EventType != EventType.Repaint)
                return;

            var selected =
                _SelectedCell.x == column ||
                _SelectedCell.y == row;

            var hover = false;

            if (_Window.Highlighter.IsMouseOver)
            {
                if (invertHover)
                    (row, column) = (column, row);

                var mousePosition = Event.current.mousePosition;
                if ((column >= 0 && IsInlineWithX(area, mousePosition.x)) ||
                    (row >= 0 && IsInlineWithY(area, mousePosition.y)))
                {
                    hover = true;
                }
            }

            _Window.Highlighter.DrawHighlightGUI(area, selected, hover);
        }

        /************************************************************************************************************************/

        /// <summary>Is `x` inside the `area`.</summary>
        private static bool IsInlineWithX(Rect area, float x)
            => area.xMin <= x
            && area.xMax > x;

        /// <summary>Is `y` inside the `area`.</summary>
        private static bool IsInlineWithY(Rect area, float y)
            => area.yMin <= y
            && area.yMax > y;

        /************************************************************************************************************************/

        /// <summary>Calculates the largest width of all transition labels.</summary>
        private float CalculateWidestTransitionLabel()
        {
            var widest = LineHeight * 2;

            var transitions = _Window.Data.Transitions;
            for (int i = 0; i < transitions.Length; i++)
            {
                var transition = transitions[i];
                if (transition == null)
                    continue;

                var label = transition.GetCachedName();
                var width = CalculateLabelWidth(label);
                if (widest < width)
                    widest = width;
            }

            return widest;
        }

        /************************************************************************************************************************/
    }
}

#endif

