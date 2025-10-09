// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A custom GUI for <see cref="FadeGroup"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/FadeGroupDrawer
    [CustomGUI(typeof(FadeGroup))]
    public class FadeGroupDrawer : CustomGUI<FadeGroup>
    {
        /************************************************************************************************************************/

        private bool _IsExpanded;
        private AnimationCurve _DisplayCurve;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI()
        {
            _IsExpanded = EditorGUILayout.Foldout(_IsExpanded, "", true);

            var area = GUILayoutUtility.GetLastRect();

            InitializeDisplayCurve(ref _DisplayCurve);

            _DisplayCurve = EditorGUI.CurveField(area, TargetName, _DisplayCurve);

            if (_IsExpanded)
                DoDetailsGUI();
        }

        /************************************************************************************************************************/

        /// <summary>The display name of the target.</summary>
        protected virtual string TargetName
        {
            get
            {
                var name = Value.GetType().GetNameCS(false);
                if (!Value.IsValid)
                    name += " (Cancelled)";
                return name;
            }
        }

        /************************************************************************************************************************/

        private static readonly Keyframe[] DisplayCurveKeyframes = new Keyframe[16];

        /// <summary>Initializes the `curve` to represent the target's fade values over normalized time.</summary>
        protected virtual void InitializeDisplayCurve(ref AnimationCurve curve)
        {
            curve ??= new();

            try
            {
                var increment = 1f / (DisplayCurveKeyframes.Length - 1);
                for (int i = 0; i < DisplayCurveKeyframes.Length; i++)
                {
                    var progress = increment * i;

                    var weight = Value.Easing != null
                        ? Value.Easing(progress)
                        : progress;

                    DisplayCurveKeyframes[i] = new(progress, weight);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Array.Clear(DisplayCurveKeyframes, 0, DisplayCurveKeyframes.Length);
            }

            curve.keys = DisplayCurveKeyframes;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI for the target's fields.</summary>
        protected virtual void DoDetailsGUI()
        {
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            Value.NormalizedTime = EditorGUILayout.Slider("Normalized Time", Value.NormalizedTime, 0, 1);
            if (EditorGUI.EndChangeCheck())
            {
                Value.NormalizedTime = Mathf.Clamp(Value.NormalizedTime, 0, 0.99f);
                Value.ApplyWeights();
            }

            EditorGUI.BeginChangeCheck();
            var fadeDuration = EditorGUILayout.FloatField("Fade Duration", Value.FadeDuration);
            if (EditorGUI.EndChangeCheck())
                Value.FadeDuration = fadeDuration;

            EditorGUILayout.LabelField(
                Value.TargetWeight > 0 ? "Fade In" : "Fade Out",
                "To " + Value.TargetWeight);

            EditorGUI.indentLevel++;
            DoNodeWeightGUI(Value.FadeIn);
            EditorGUI.indentLevel--;

            var fadeOutCount = Value.FadeOut.Count;
            if (fadeOutCount > 0)
            {
                EditorGUILayout.LabelField("Fade Out", fadeOutCount.ToStringCached());

                EditorGUI.indentLevel++;
                for (int i = 0; i < fadeOutCount; i++)
                    DoNodeWeightGUI(Value.FadeOut[i]);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI for the given `nodeWeight`.</summary>
        private void DoNodeWeightGUI(NodeWeight nodeWeight)
        {
            EditorGUILayout.LabelField(nodeWeight.Node?.GetPath());
        }

        /************************************************************************************************************************/
    }
}

#endif

