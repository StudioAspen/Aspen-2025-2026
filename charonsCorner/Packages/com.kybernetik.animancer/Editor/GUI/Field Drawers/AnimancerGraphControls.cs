// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using Animancer.Units;
using System;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws manual controls for the <see cref="AnimancerGraph.PlayableGraph"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerGraphControls
    [Serializable, InternalSerializableType]
    public class AnimancerGraphControls : AnimancerSettingsGroup
    {
        /************************************************************************************************************************/

        /// <summary>Draws manual controls for the <see cref="AnimancerGraph.PlayableGraph"/>.</summary>
        public static void DoGraphGUI(AnimancerGraph graph, out Rect area)
        {
            GUILayout.BeginVertical();

            DoSpeedSliderGUI(graph);

            DoAddAnimationGUI(graph);

            GUILayout.EndVertical();

            area = GUILayoutUtility.GetLastRect();
        }

        /************************************************************************************************************************/

        /// <summary>Draws the <see cref="AnimancerGraphSpeedSlider"/>.</summary>
        private static void DoSpeedSliderGUI(AnimancerGraph graph)
        {
            if (!AnimancerGraphSpeedSlider.Instance.IsOn)
                return;

            var area = LayoutSingleLineRect();
            area = area.Expand(StandardSpacing, 0);

            AnimancerGraphSpeedSlider.Instance.Graph = graph;
            AnimancerGraphSpeedSlider.Instance.DoSpeedSlider(ref area, null);
        }

        /************************************************************************************************************************/

        /// <summary>Draws a toggle to play and pause the graph.</summary>
        public static void DoPlayPauseToggle(Rect area, AnimancerGraph graph, GUIStyle style = null)
        {
            EditorGUI.BeginChangeCheck();

            var isGraphPlaying = AnimancerGUI.DoPlayPauseToggle(
                area,
                graph.IsGraphPlaying,
                style);

            if (EditorGUI.EndChangeCheck())
                graph.IsGraphPlaying = isGraphPlaying;
        }

        /************************************************************************************************************************/

        /// <summary>Draws a button to step time forward.</summary>
        public static void DoFrameStepButton(Rect area, AnimancerGraph graph, GUIStyle style)
        {
            if (GUI.Button(area, AnimancerIcons.StepForwardIcon, style))
                graph.Evaluate(FrameStep);
        }

        /************************************************************************************************************************/
        #region Add Animation
        /************************************************************************************************************************/

        private static void DoAddAnimationGUI(AnimancerGraph graph)
        {
            if (!AnimancerGraphDrawer.ShowAddAnimation)
                return;

            var label = "Add Animation";

            var area = LayoutSingleLineRect(SpacingMode.Before);

            var labelArea = StealFromLeft(ref area, EditorStyles.label.CalculateWidth(label), StandardSpacing);
            var objectArea = StealFromRight(ref area, area.width * 0.35f, StandardSpacing);
            var clipArea = area;

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            GUI.Label(labelArea, label);

            var sourceClip = EditorGUI.ObjectField(clipArea, null, typeof(AnimationClip), false);
            var source = EditorGUI.ObjectField(objectArea, null, typeof(Object), false);

            EditorGUI.indentLevel = indentLevel;

            if (sourceClip is AnimationClip sourceClipTyped)
            {
                graph.Layers[0].Play(sourceClipTyped);
                return;
            }

            if (source == null)
                return;

            if (source is ITransition transition)
            {
                graph.Layers[0].Play(transition);
                return;
            }

            var transitionAsset = TryCreateTransitionAttribute.TryCreateTransitionAsset(source);
            if (transitionAsset != null)
            {
                var state = graph.Layers[0].Play(transitionAsset);

                if (!EditorUtility.IsPersistent(transitionAsset))
                {
                    state.SetDebugName(source);
                    Object.DestroyImmediate(transitionAsset);
                }

                return;
            }

            using (SetPool<AnimationClip>.Instance.Acquire(out var clips))
            {
                clips.GatherFromSource(source);
                foreach (var clip in clips)
                    graph.Layers[0].Play(clip);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Settings
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string DisplayName
            => "Graph Controls";

        /// <inheritdoc/>
        public override int Index
            => 2;

        /************************************************************************************************************************/

        [SerializeField]
        [Seconds(Rule = Validate.Value.IsNotNegative)]
        [DefaultValue(0.02f)]
        [Tooltip("The amount of time that will be added by a single frame step")]
        private float _FrameStep = 0.02f;

        /// <summary>The amount of time that will be added by a single frame step (in seconds).</summary>
        public static float FrameStep
            => AnimancerSettingsGroup<AnimancerGraphControls>.Instance._FrameStep;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

