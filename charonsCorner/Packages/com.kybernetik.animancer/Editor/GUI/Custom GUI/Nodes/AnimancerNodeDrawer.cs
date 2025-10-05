// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using static Animancer.Editor.AnimancerGUI;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="AnimancerNode"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerNodeDrawer_1
    /// 
    public abstract class AnimancerNodeDrawer<T> : CustomGUI<T>
        where T : AnimancerNode
    {
        /************************************************************************************************************************/

        /// <summary>Extra padding for the left side of the labels.</summary>
        public const float ExtraLeftPadding = 3;

        /************************************************************************************************************************/

        /// <summary>Should the target node's details be expanded in the Inspector?</summary>
        public ref bool IsExpanded
            => ref Value._IsInspectorExpanded;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI()
        {
            if (!Value.IsValid())
                return;

            GUILayout.BeginVertical();
            {
                DoHeaderGUI();
                DoDetailsGUI();
            }
            GUILayout.EndVertical();

            if (TryUseClickEvent(GUILayoutUtility.GetLastRect(), 1))
                OpenContextMenu();

        }

        /************************************************************************************************************************/

        /// <summary>Draws the name and other details of the <see cref="CustomGUI{T}.Value"/> in the GUI.</summary>
        protected virtual void DoHeaderGUI()
        {
            var area = LayoutSingleLineRect(SpacingMode.Before);
            DoLabelGUI(area);
            DoFoldoutGUI(area);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Draws a field for the <see cref="AnimancerState.MainObject"/> if it has one, otherwise just a simple text
        /// label.
        /// </summary>
        protected abstract void DoLabelGUI(Rect area);

        /// <summary>Draws a foldout arrow to expand/collapse the node details.</summary>
        protected abstract void DoFoldoutGUI(Rect area);

        /************************************************************************************************************************/

        private FastObjectField _DebugNameField;

        /// <summary>Draws the details of the <see cref="CustomGUI{T}.Value"/>.</summary>
        protected virtual void DoDetailsGUI()
        {
            if (!IsExpanded)
                return;

            var debugName = Value.DebugName;
            if (debugName == null)
                return;

            var area = LayoutSingleLineRect(SpacingMode.Before);
            area = EditorGUI.IndentedRect(area);

            _DebugNameField.Draw(area, "Debug Name", debugName);
        }

        /************************************************************************************************************************/

        private static readonly int FloatFieldHash = "EditorTextField".GetHashCode();

        /// <summary>
        /// Draws controls for <see cref="AnimancerState.IsPlaying"/>, <see cref="AnimancerNodeBase.Speed"/>, and
        /// <see cref="AnimancerNode.Weight"/>.
        /// </summary>
        protected void DoNodeDetailsGUI()
        {
            var area = LayoutSingleLineRect(SpacingMode.Before);
            area.xMin += EditorGUI.indentLevel * IndentSize + ExtraLeftPadding;
            var xMin = area.xMin;

            var labelWidth = EditorGUIUtility.labelWidth;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Is Playing.
            if (Value is AnimancerState state)
            {
                var buttonArea = StealFromLeft(ref area, LineHeight, StandardSpacing);
                state.IsPlaying = DoPlayPauseToggle(buttonArea, state.IsPlaying);
            }

            SplitHorizontally(area, "Speed", "Weight",
                out var speedWidth,
                out var weightWidth,
                out var speedRect,
                out var weightRect);

            // Speed.
            EditorGUIUtility.labelWidth = speedWidth;
            EditorGUI.BeginChangeCheck();
            var speed = EditorGUI.FloatField(speedRect, "Speed", Value.Speed);
            if (EditorGUI.EndChangeCheck())
                Value.Speed = speed;
            if (TryUseClickEvent(speedRect, 2))
                Value.Speed = Value.Speed != 1 ? 1 : 0;

            // Weight.
            EditorGUIUtility.labelWidth = weightWidth;
            EditorGUI.BeginChangeCheck();
            var weight = EditorGUI.FloatField(weightRect, "Weight", Value.Weight);
            if (EditorGUI.EndChangeCheck())
                SetWeight(Mathf.Max(weight, 0));
            if (TryUseClickEvent(weightRect, 2))
                SetWeight(Value.Weight != 1 ? 1 : 0);

            // Real Speed.
            // Mixer Synchronization changes the internal Playable Speed without setting the State Speed.

            speed = (float)Value._Playable.GetSpeed();
            if (Value.Speed != speed)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    area = LayoutSingleLineRect(SpacingMode.Before);
                    area.xMin = xMin;

                    var label = BeginTightLabel("Real Speed");
                    EditorGUIUtility.labelWidth = CalculateLabelWidth(label);
                    EditorGUI.FloatField(area, label, speed);
                    EndTightLabel();
                }
            }
            else// Add a dummy ID so that subsequent IDs don't change when the Real Speed appears or disappears.
            {
                GUIUtility.GetControlID(FloatFieldHash, FocusType.Keyboard);
            }

            EditorGUI.indentLevel = indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;

            DoFadeDetailsGUI();
        }

        /************************************************************************************************************************/

        /// <summary>Indicates whether changing the <see cref="AnimancerNode.Weight"/> should normalize its siblings.</summary>
        protected virtual bool AutoNormalizeSiblingWeights
            => false;

        private void SetWeight(float weight)
        {
            if (weight < 0 ||
                weight > 1 ||
                Mathf.Approximately(Value.Weight, 1) ||
                !AutoNormalizeSiblingWeights)
                goto JustSetWeight;

            var parent = Value.Parent;
            if (parent == null)
                goto JustSetWeight;

            var totalWeight = 0f;
            var siblingCount = parent.ChildCount;
            for (int i = 0; i < siblingCount; i++)
            {
                var sibling = parent.GetChildNode(i);
                if (sibling.IsValid())
                    totalWeight += sibling.Weight;
            }

            // If the weights weren't previously normalized, don't normalize them now.
            if (!Mathf.Approximately(totalWeight, 1))
                goto JustSetWeight;

            var siblingWeightMultiplier = (totalWeight - weight) / (totalWeight - Value.Weight);

            for (int i = 0; i < siblingCount; i++)
            {
                var sibling = parent.GetChildNode(i);
                if (sibling != Value && sibling.IsValid())
                    sibling.Weight *= siblingWeightMultiplier;
            }

            JustSetWeight:
            Value.Weight = weight;
        }

        /************************************************************************************************************************/

        private float
            _FadeDuration = float.NaN,
            _TargetWeight = float.NaN;

        /// <summary>
        /// Draws the <see cref="AnimancerNode.FadeSpeed"/>
        /// and <see cref="AnimancerNode.TargetWeight"/>.
        /// </summary>
        private void DoFadeDetailsGUI()
        {
            var area = LayoutSingleLineRect(SpacingMode.Before);
            area = EditorGUI.IndentedRect(area);
            area.xMin += ExtraLeftPadding;

            var durationLabel = "Fade Duration";
            var targetLabel = "Target Weight";

            SplitHorizontally(
                area,
                durationLabel,
                targetLabel,
                out var durationWidth,
                out var weightWidth,
                out var durationRect,
                out var weightRect);

            var labelWidth = EditorGUIUtility.labelWidth;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();

            var fade = Value.FadeGroup;

            var fadeDuration = DoFadeDurationGUI(durationWidth, durationRect, durationLabel, fade);
            var targetWeight = DoTargetWeightGUI(weightWidth, weightRect, targetLabel, fade);

            if (EditorGUI.EndChangeCheck())
                SetFade(targetWeight, fadeDuration);

            EditorGUI.indentLevel = indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;
        }

        /************************************************************************************************************************/

        private float DoFadeDurationGUI(
            float labelWidth,
            Rect area,
            string label,
            FadeGroup fade)
        {
            EditorGUIUtility.labelWidth = labelWidth;

            var fadeDuration = fade != null ? fade.FadeDuration : _FadeDuration;
            fadeDuration = EditorGUI.DelayedFloatField(area, label, fadeDuration);
            if (fadeDuration > 0)
            {
            }
            else// NaN or Negative.
            {
                fadeDuration = _FadeDuration = float.NaN;
            }

            if (TryUseClickEvent(area, 2))
            {
                var defaultFadeDuration = AnimancerGraph.DefaultFadeDuration;
                if (fadeDuration != 0 || defaultFadeDuration == 0)
                {
                    fadeDuration = 0;
                }
                else
                {
                    var fadeDistance = Math.Abs(Value.Weight - Value.TargetWeight);
                    if (fadeDistance != 0)
                    {
                        fadeDuration = fadeDistance / defaultFadeDuration;
                    }
                    else
                    {
                        fadeDuration = defaultFadeDuration;
                    }
                }
            }

            return fadeDuration;
        }

        /************************************************************************************************************************/

        private float DoTargetWeightGUI(
            float labelWidth,
            Rect area,
            string label,
            FadeGroup fade)
        {
            EditorGUIUtility.labelWidth = labelWidth;

            var targetWeight = fade != null
                ? fade.TargetWeight
                : _TargetWeight.IsFinite()
                ? _TargetWeight
                : Value.Weight;

            targetWeight = EditorGUI.DelayedFloatField(area, label, targetWeight);
            if (targetWeight >= 0)
            {
            }
            else// NaN or Negative.
            {
                targetWeight = _TargetWeight = float.NaN;
            }

            if (TryUseClickEvent(area, 2))
            {
                if (targetWeight != Value.Weight)
                    targetWeight = Value.Weight;
                else if (targetWeight != 1)
                    targetWeight = 1;
                else
                    targetWeight = 0;
            }

            return targetWeight;
        }

        /************************************************************************************************************************/

        /// <summary>Starts a fade or changes the details of an existing one.</summary>
        private void SetFade(float targetWeight, float fadeDuration)
        {
            _TargetWeight = targetWeight;
            _FadeDuration = fadeDuration;

            if (!targetWeight.IsFinite() ||
                !fadeDuration.IsFinite() ||
                targetWeight == Value.Weight ||
                fadeDuration <= 0)
                return;

            // If it's a state attached to a layer, start a proper cross fade.
            if (Value is AnimancerState state &&
                state.Parent is AnimancerLayer layer)
            {
                layer.Play(state, fadeDuration, FadeMode.FixedDuration);
                // That might not have started a fade if the state was already playing,
                // So just continue to verify its details.
            }

            var fade = Value.FadeGroup;
            if (fade != null && fade.FadeIn.Node == Value)
            {
                fade.TargetWeight = targetWeight;
                fade.FadeDuration = fadeDuration;
                return;
            }

            Value.StartFade(targetWeight, fadeDuration);
        }

        /************************************************************************************************************************/
        #region Context Menu
        /************************************************************************************************************************/

        /// <summary>
        /// The menu label prefix used for details about the <see cref="CustomGUI{T}.Value"/>.
        /// </summary>
        protected const string DetailsPrefix = "Details/";

        /// <summary>
        /// Checks if the current event is a context menu click within the `clickArea` and opens a context menu with various
        /// functions for the <see cref="CustomGUI{T}.Value"/>.
        /// </summary>
        protected void OpenContextMenu()
        {
            var menu = new GenericMenu();

            menu.AddDisabledItem(new(Value.ToString()));

            PopulateContextMenu(menu);

            menu.AddItem(new(DetailsPrefix + "Log Details"), false,
                () => Debug.Log(Value.GetDescription(), Value.Graph?.Component as Object));

            menu.AddItem(new(DetailsPrefix + "Log Details Of Everything"), false,
                () => Debug.Log(Value.Graph.GetDescription(), Value.Graph?.Component as Object));
            AnimancerGraphDrawer.AddPlayableGraphVisualizerFunction(menu, DetailsPrefix, Value.Graph._PlayableGraph);

            menu.ShowAsContext();
        }

        /// <summary>Adds functions relevant to the <see cref="CustomGUI{T}.Value"/>.</summary>
        protected abstract void PopulateContextMenu(GenericMenu menu);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

