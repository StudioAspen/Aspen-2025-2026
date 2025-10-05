// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using Animancer.TransitionLibraries;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;
using static Animancer.Editor.TransitionLibraries.TransitionLibrarySelection;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only]
    /// A <see cref="TransitionLibraryWindowPage"/> for editing transition aliases.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibraryAliasesPage
    [Serializable]
    public class TransitionLibraryAliasesPage : TransitionLibraryWindowPage
    {
        /************************************************************************************************************************/

        [SerializeField]
        private Vector2 _ScrollPosition;

        [NonSerialized]
        private bool _HasSorted;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string DisplayName
            => "Transition Aliases";

        /// <inheritdoc/>
        public override string HelpTooltip
            => "Aliases are custom names which can be used to refer to transitions instead of direct references.";

        /// <inheritdoc/>
        public override int Index
            => 1;

        /************************************************************************************************************************/

        private static readonly List<Rect>
            TransitionAreas = new();

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnGUI(Rect area)
        {
            var definition = Window.Data;

            if (!_HasSorted)
            {
                _HasSorted = true;
                definition.SortAliases();
            }

            var currentEvent = Event.current;
            var isRepaint = currentEvent.type == EventType.Repaint;
            if (isRepaint)
                TransitionAreas.Clear();

            area.yMin += StandardSpacing;
            area.xMin += StandardSpacing;
            area.xMax -= StandardSpacing;

            var transitions = definition.Transitions;
            var aliases = definition.Aliases;

            var viewArea = new Rect(
                0,
                0,
                area.width,
                CalculateHeight(1 + transitions.Length + aliases.Length) + StandardSpacing);

            if (viewArea.height > area.height)
                viewArea.width -= GUI.skin.verticalScrollbar.fixedWidth;

            _ScrollPosition = GUI.BeginScrollView(area, _ScrollPosition, viewArea);

            viewArea.height = LineHeight;

            DoAliasAllGUI(viewArea);

            NextVerticalArea(ref viewArea);

            var aliasIndex = 0;

            for (int i = 0; i < transitions.Length; i++)
            {
                var totalTransitionArea = viewArea;

                if (isRepaint)
                    TransitionAreas.Add(viewArea);

                DoTransitionGUI(viewArea, transitions[i], i);

                NextVerticalArea(ref viewArea);

                while (aliasIndex < aliases.Length)
                {
                    var alias = aliases[aliasIndex];

                    if (alias.Index != i)
                    {
                        if (alias.Index < i && currentEvent.type != EventType.Used)
                        {
                            Debug.LogError("Aliases aren't properly sorted.");
                            definition.SortAliases();
                            GUIUtility.ExitGUI();
                        }

                        break;
                    }

                    DoAliasGUI(viewArea, alias, aliasIndex++);

                    NextVerticalArea(ref viewArea);
                }

                // Highlights.

                totalTransitionArea.yMax = viewArea.yMin - StandardSpacing;

                var selected = Window.Selection.FromIndex == i || Window.Selection.ToIndex == i;
                var hover = totalTransitionArea.Contains(currentEvent.mousePosition);

                Window.Highlighter.DrawHighlightGUI(totalTransitionArea, selected, hover);
            }

            GUI.EndScrollView();
        }

        /************************************************************************************************************************/

        /// <summary>Draws <see cref="TransitionLibraryDefinition.AliasAllTransitions"/>.</summary>
        private void DoAliasAllGUI(Rect area)
        {
            var definition = Window.Data;

            using (var label = PooledGUIContent.Acquire(
                "Alias All Transitions",
                TransitionLibraryDefinition.AliasAllTransitionsTooltip))
                definition.AliasAllTransitions = EditorGUI.Toggle(area, label, definition.AliasAllTransitions);

            if (TryUseClickEvent(area, 0))
                definition.AliasAllTransitions = !definition.AliasAllTransitions;
        }

        /************************************************************************************************************************/

        /// <summary>Draws a `transition`.</summary>
        private void DoTransitionGUI(Rect area, TransitionAssetBase transition, int index)
        {
            var addArea = StealFromLeft(ref area, LineHeight * 5, StandardSpacing);

            TransitionModifierTableGUI.HandleTransitionLabelInput(
                ref area,
                Window,
                transition,
                index,
                SelectionType.ToTransition,
                CalculateTargetTransitionIndex);

            var typeArea = StealFromRight(ref area, area.width * 0.5f, StandardSpacing);

            var label = transition.GetCachedName();
            GUI.Label(area, label);

            var wrappedTransition = transition.GetTransition();
            var type = wrappedTransition != null
                ? wrappedTransition.GetType().GetNameCS(false)
                : "Null";
            GUI.Label(typeArea, type);

            if (GUI.Button(addArea, "Add"))
            {
                var alias = new NamedIndex(null, index);
                Window.RecordUndo().AddAlias(alias);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the transition index for a drag and drop operation.</summary>
        private static int CalculateTargetTransitionIndex(
            Rect area,
            int index,
            Event currentEvent)
        {
            var y = currentEvent.mousePosition.y;

            for (int i = 0; i < TransitionAreas.Count; i++)
                if (y <= TransitionAreas[i].yMax)
                    return i;

            return TransitionAreas.Count;
        }

        /************************************************************************************************************************/

        /// <summary>Draws an `alias`.</summary>
        private void DoAliasGUI(Rect area, NamedIndex alias, int index)
        {
            var removeArea = StealFromLeft(ref area, LineHeight * 5, StandardSpacing);

            EditorGUI.BeginChangeCheck();

            var name = StringAssetDrawer.DrawGUI(area, GUIContent.none, alias.Name, Window.SourceObject, out _);

            if (EditorGUI.EndChangeCheck())
            {
                Window.RecordUndo().Aliases[index] = alias.With(name as StringAsset);
            }

            if (GUI.Button(removeArea, "Remove"))
            {
                Window.RecordUndo().RemoveAlias(index);
            }
        }

        /************************************************************************************************************************/
    }
}

#endif

