// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="AnimancerState"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ParametizedAnimancerStateDrawer_1
    [CustomGUI(typeof(ManualMixerState))]
    public class ParametizedAnimancerStateDrawer<T> : AnimancerStateDrawer<T>
        where T : AnimancerState, IParametizedState
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void DoDetailsGUI()
        {
            base.DoDetailsGUI();

            if (!IsExpanded)
                return;

            EditorGUI.indentLevel++;

            var parameters = ListPool.Acquire<StateParameterDetails>();
            Value.GetParameters(parameters);
            DoParametersGUI(parameters);
            ListPool.Release(parameters);

            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <summary>Draws fields for all `parameters`.</summary>
        private void DoParametersGUI(List<StateParameterDetails> parameters)
        {
            if (parameters.Count == 0)
                return;

            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth -= AnimancerGUI.IndentSize;

            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < parameters.Count; i++)
                parameters[i] = DoParameterGUI(i, parameters[i]);

            EditorGUIUtility.labelWidth = labelWidth;

            if (EditorGUI.EndChangeCheck())
                Value.SetParameters(parameters);
        }

        /************************************************************************************************************************/

        /// <summary>Draws fields for the `parameter`.</summary>
        private StateParameterDetails DoParameterGUI(int index, StateParameterDetails parameter)
        {
            var area = AnimancerGUI.LayoutSingleLineRect(AnimancerGUI.SpacingMode.Before);

            var indentLevel = EditorGUI.indentLevel;
            var labelWidth = EditorGUIUtility.labelWidth;

            var label = parameter.label;

            if (parameter.SupportsBinding && Value.Graph.HasParameters)
            {
                area = EditorGUI.IndentedRect(area);
                EditorGUI.indentLevel = 0;

                parameter = DoBindingGUI(ref area, index, parameter, ref label);
            }
            else
            {
                EditorGUIUtility.labelWidth += AnimancerGUI.IndentSize;
            }

            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    parameter.value = EditorGUI.FloatField(area, label, (float)parameter.value);
                    break;

                case AnimatorControllerParameterType.Int:
                    parameter.value = EditorGUI.IntField(area, label, (int)parameter.value);
                    break;

                case AnimatorControllerParameterType.Bool:
                    parameter.value = EditorGUI.Toggle(area, label, (bool)parameter.value);
                    break;

                case AnimatorControllerParameterType.Trigger:
                    parameter.value = EditorGUI.Toggle(area, label, (bool)parameter.value, EditorStyles.radioButton);
                    break;

                default:
                    EditorGUI.LabelField(area, label, "Unsupported Type: " + parameter.type);
                    break;
            }

            EditorGUI.indentLevel = indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;

            return parameter;
        }

        /************************************************************************************************************************/

        /// <summary>Draws a dropdown for the `parameter`'s name binding.</summary>
        private StateParameterDetails DoBindingGUI(
            ref Rect area,
            int index,
            StateParameterDetails parameter,
            ref string fieldLabel)
        {
            if (!parameter.SupportsBinding)
                return parameter;

            var spacing = AnimancerGUI.StandardSpacing;

            float width;
            if (string.IsNullOrEmpty(parameter.name))
            {
                width = area.height + spacing;
                EditorGUIUtility.labelWidth -= width + AnimancerGUI.IndentSize + spacing;
            }
            else
            {
                width = EditorGUIUtility.labelWidth - AnimancerGUI.IndentSize;
                fieldLabel = "";
            }

            var labelArea = AnimancerGUI.StealFromLeft(
                ref area,
                width,
                spacing);

            using (var label = PooledGUIContent.Acquire(parameter.name))
            {
                if (EditorGUI.DropdownButton(labelArea, label, FocusType.Passive))
                    ShowBindingSelectionMenu(labelArea, index, parameter.name);
            }

            return parameter;
        }

        /************************************************************************************************************************/

        /// <summary>Shows a context menu for selecting the parameter binding.</summary>
        private void ShowBindingSelectionMenu(Rect area, int index, string currentName)
        {
            var menu = new GenericMenu();

            menu.AddItem(
                new("None"),
                string.IsNullOrEmpty(currentName),
                () => SetParameterName(index, null));

            menu.AddSeparator("");

            foreach (var parameter in Value.Graph.Parameters)
            {
                if (parameter.ValueType != typeof(float))
                    continue;

                var name = parameter.Key;

                menu.AddItem(
                    new(name),
                    name == currentName,
                    () => SetParameterName(index, name));
            }

            menu.DropDown(area);
        }

        /************************************************************************************************************************/

        /// <summary>Sets the name binding of the specified parameter.</summary>
        private void SetParameterName(int index, string name)
        {
            var parameters = ListPool.Acquire<StateParameterDetails>();
            Value.GetParameters(parameters);

            var modify = parameters[index];
            modify.name = name;
            parameters[index] = modify;

            Value.SetParameters(parameters);
            ListPool.Release(parameters);
        }

        /************************************************************************************************************************/
    }
}

#endif

