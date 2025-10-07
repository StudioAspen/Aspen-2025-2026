// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A custom GUI for <see cref="IParameter"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ParameterGUI_1
    /// 
    [CustomGUI(typeof(IParameter))]
    public class ParameterGUI<TParameter> : CustomGUI<TParameter>
        where TParameter : IParameter
    {
        /************************************************************************************************************************/

        private static readonly HashSet<string>
            ExpandedNames = new();

        /************************************************************************************************************************/

        private static ICustomGUI _ValueGUI;
        private static ICustomGUI _DelegateGUI;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI()
        {
            ParameterDictionary.IsDrawingInspector = true;

            var isExpanded = DoFoldoutGUI(out var startArea);

            DoValueGUI();

            if (isExpanded)
            {
                EditorGUI.indentLevel++;

                DoDetailsGUI();

                EditorGUI.indentLevel--;
            }

            ParameterDictionary.IsDrawingInspector = false;

            var endArea = GUILayoutUtility.GetLastRect();

            var totalArea = startArea;
            totalArea.yMax = endArea.yMax;

            if (AnimancerGUI.TryUseClickEvent(totalArea, 1))
                ShowContextMenu();
        }

        /************************************************************************************************************************/

        /// <summary>Draws the <see cref="CustomGUI{T}.Value"/> and returns the area used.</summary>
        private Rect DoValueGUI()
        {
            _ValueGUI ??= CustomGUIFactory.GetOrCreateForType(Value.ValueType);

            _ValueGUI.Label = Label;
            _ValueGUI.Value = Value.Value;
            _ValueGUI.DoGUI();
            Value.Value = _ValueGUI.Value;

            return GUILayoutUtility.GetLastRect();
        }

        /************************************************************************************************************************/

        /// <summary>Draws a foldout for the parameter details and returns true if expanded.</summary>
        private bool DoFoldoutGUI(out Rect totalArea)
        {
            var area = AnimancerGUI.LayoutSingleLineRect();
            totalArea = area;

            GUILayout.Space(-AnimancerGUI.LineHeight - AnimancerGUI.StandardSpacing);

            var indented = EditorGUI.IndentedRect(area);
            area.xMax = indented.xMin;

            EditorGUIUtility.AddCursorRect(area, MouseCursor.Arrow);

            return AnimancerGUI.DoHashedFoldoutGUI(area, ExpandedNames, Label.text);
        }

        /************************************************************************************************************************/

        /// <summary>Draws the details of the parameter.</summary>
        private void DoDetailsGUI()
        {
            EditorGUILayout.LabelField("Type", Value.ValueType.GetNameCS(false));

            _DelegateGUI ??= CustomGUIFactory.GetOrCreateForType(typeof(MulticastDelegate));

            _DelegateGUI.SetLabel("On Value Changed");
            _DelegateGUI.Value = Value.GetOnValueChanged();
            _DelegateGUI.DoGUI();
        }

        /************************************************************************************************************************/

        /// <summary>Shows a context menu for the parameter.</summary>
        private void ShowContextMenu()
        {
            var menu = new GenericMenu();

            menu.AddItem(new("Log Interactions"), Value.LogContext != null, () =>
            {
                Value.LogContext = Value.LogContext is null
                    ? ""
                    : null;
            });

            menu.AddItem(new("Inspector Control Only"), Value.InspectorControlOnly, () =>
            {
                Value.InspectorControlOnly = !Value.InspectorControlOnly;
            });

            AnimancerEditorUtilities.AddDocumentationLink(
                menu,
                "Parameters Documentation",
                Strings.DocsURLs.Parameters);

            menu.ShowAsContext();
        }

        /************************************************************************************************************************/
    }
}

#endif

