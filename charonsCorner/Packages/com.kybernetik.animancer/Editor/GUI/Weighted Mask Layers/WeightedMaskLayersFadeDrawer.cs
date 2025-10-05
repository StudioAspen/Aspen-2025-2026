// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A custom GUI for <see cref="WeightedMaskLayers.Fade"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/WeightedMaskLayersFadeDrawer
    [CustomGUI(typeof(WeightedMaskLayers.Fade))]
    public class WeightedMaskLayersFadeDrawer : CustomGUI<WeightedMaskLayers.Fade>
    {
        /************************************************************************************************************************/

        private bool _IsExpanded;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI()
        {
            _IsExpanded = EditorGUILayout.Foldout(_IsExpanded, "Weighted Mask Layers Fade", true);

            if (_IsExpanded)
                DoDetailsGUI();
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI for the target's fields.</summary>
        protected virtual void DoDetailsGUI()
        {
            EditorGUI.indentLevel++;

            Value.ElapsedTime = EditorGUILayout.Slider("Elapsed", Value.ElapsedTime, 0, Value.Duration);

            Value.Duration = EditorGUILayout.FloatField("Duration", Value.Duration);

            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/
    }
}

#endif

