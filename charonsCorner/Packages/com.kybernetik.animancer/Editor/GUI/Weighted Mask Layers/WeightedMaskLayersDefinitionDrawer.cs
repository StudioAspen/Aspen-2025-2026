// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A <see cref="PropertyDrawer"/> for <see cref="WeightedMaskLayersDefinition"/> fields.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/WeightedMaskLayersDefinitionDrawer
    [CustomPropertyDrawer(typeof(WeightedMaskLayersDefinition), true)]
    public class WeightedMaskLayersDefinitionDrawer : EditableFieldDrawer
    {
        /************************************************************************************************************************/

        private static readonly Action<SerializedProperty> OnEditTarget = property =>
            WeightedMaskLayersDefinitionWindow.Open<WeightedMaskLayersDefinitionWindow>(
                (WeightedMaskLayers)property.serializedObject.targetObject, false);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            try
            {
                if (property.serializedObject.targetObject is WeightedMaskLayers)
                    OnEdit += OnEditTarget;

                base.OnGUI(area, property, label);
            }
            finally
            {
                OnEdit -= OnEditTarget;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void GetEditButtonLabel(SerializedProperty property, GUIContent label)
        {
            var transforms = property.FindPropertyRelative(WeightedMaskLayersDefinition.TransformsField);
            var weights = property.FindPropertyRelative(WeightedMaskLayersDefinition.WeightsField);

            var transformCount = transforms.arraySize;
            var groupCount = transformCount > 0
                ? weights.arraySize / transformCount
                : 0;

            label.text = $"Edit [{transformCount} Transforms] x [{groupCount} Groups]";
        }

        /************************************************************************************************************************/
    }
}

#endif

