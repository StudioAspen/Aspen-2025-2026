// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="ParameterDictionary"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ParameterDictionaryDrawer
    /// 
    public static class ParameterDictionaryDrawer
    {
        /************************************************************************************************************************/

        private const string
            KeyPrefix = AnimancerGraphDrawer.KeyPrefix;

        private static readonly BoolPref
            AreParametersExpanded = new(KeyPrefix + nameof(AreParametersExpanded), false);

        /************************************************************************************************************************/

        /// <summary>Draws the <see cref="AnimancerGraph.Parameters"/>.</summary>
        public static void DoParametersGUI(AnimancerGraph graph)
        {
            if (!graph.HasParameters)
                return;

            EditorGUI.indentLevel++;

            var parameters = graph.Parameters;

            AreParametersExpanded.Value = AnimancerGUI.DoLabelFoldoutFieldGUI(
                "Parameters",
                parameters.Count.ToStringCached(),
                AreParametersExpanded);

            if (AreParametersExpanded)
            {
                EditorGUI.indentLevel++;

                var sortedParameters = ListPool.Acquire<IParameter>();
                sortedParameters.AddRange(parameters);
                sortedParameters.Sort();

                foreach (var item in sortedParameters)
                    DoParameterGUI(item);
                ListPool.Release(sortedParameters);

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the `parameter`.</summary>
        private static void DoParameterGUI(IParameter parameter)
        {
            var gui = CustomGUIFactory.GetOrCreateForObject(parameter);
            if (gui == null)
            {
                EditorGUILayout.LabelField(parameter.Key, parameter.Value.ToString());
                return;
            }

            gui.SetLabel(parameter.Key);
            gui.DoGUI();
        }

        /************************************************************************************************************************/
    }
}

#endif

