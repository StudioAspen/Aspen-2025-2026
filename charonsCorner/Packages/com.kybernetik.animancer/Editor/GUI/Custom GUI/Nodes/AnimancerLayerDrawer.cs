// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A custom Inspector for an <see cref="AnimancerLayer"/> which sorts and exposes some of its internal values.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerLayerDrawer
    /// 
    [CustomGUI(typeof(AnimancerLayer))]
    public class AnimancerLayerDrawer : AnimancerNodeDrawer<AnimancerLayer>
    {
        /************************************************************************************************************************/

        /// <summary>The states in the target layer which have non-zero <see cref="AnimancerNode.Weight"/>.</summary>
        public readonly List<AnimancerState> ActiveStates = new();

        /// <summary>The states in the target layer which have zero <see cref="AnimancerNode.Weight"/>.</summary>
        public readonly List<AnimancerState> InactiveStates = new();

        /************************************************************************************************************************/
        #region Gathering
        /************************************************************************************************************************/

        /// <summary>Initializes an editor in the list for each layer in the `graph`.</summary>
        /// <remarks>
        /// The `count` indicates the number of elements actually being used.
        /// Spare elements are kept in the list in case they need to be used again later.
        /// </remarks>
        internal static void GatherLayerEditors(
            AnimancerGraph graph,
            List<AnimancerLayerDrawer> editors,
            out int count)
        {
            count = graph.Layers.Count;
            for (int i = 0; i < count; i++)
            {
                AnimancerLayerDrawer editor;
                if (editors.Count <= i)
                {
                    editor = new();
                    editors.Add(editor);
                }
                else
                {
                    editor = editors[i];
                }

                editor.GatherStates(graph.Layers[i]);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the target `layer` and sorts its states and their keys into the active/inactive lists.
        /// </summary>
        private void GatherStates(AnimancerLayer layer)
        {
            Value = layer;

            ActiveStates.Clear();
            InactiveStates.Clear();

            foreach (var state in layer)
            {
                if (state.IsActive ||
                    (!AnimancerGraphDrawer.SeparateActiveFromInactiveStates && AnimancerGraphDrawer.ShowInactiveStates))
                {
                    ActiveStates.Add(state);
                    continue;
                }

                if (AnimancerGraphDrawer.ShowInactiveStates)
                    InactiveStates.Add(state);
            }

            SortAndGatherKeys(ActiveStates);
            SortAndGatherKeys(InactiveStates);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sorts any entries that use another state as their key to come right after that state.
        /// See <see cref="AnimancerLayer.Play(AnimancerState, float, FadeMode)"/>.
        /// </summary>
        private static void SortAndGatherKeys(List<AnimancerState> states)
        {
            var count = states.Count;
            if (count == 0)
                return;

            AnimancerGraphDrawer.ApplySortStatesByName(states);

            // Sort any states that use another state as their key to be right after the key.
            for (int i = 0; i < count; i++)
            {
                var state = states[i];
                var key = state.Key;

                if (key is not AnimancerState keyState)
                    continue;

                var keyStateIndex = states.IndexOf(keyState);
                if (keyStateIndex < 0 || keyStateIndex + 1 == i)
                    continue;

                states.RemoveAt(i);

                if (keyStateIndex < i)
                    keyStateIndex++;

                states.Insert(keyStateIndex, state);

                i--;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <summary>Draws the layer's name and weight.</summary>
        protected override void DoLabelGUI(Rect area)
        {
            var label = Value.IsAdditive ? "Additive" : "Override";
            if (Value._Mask != null)
                label = $"{label} ({Value._Mask.GetCachedName()})";

            area.xMin += FoldoutIndent;

            DoWeightLabel(ref area, Value.Weight, Value.EffectiveWeight);

            EditorGUIUtility.labelWidth -= FoldoutIndent;
            EditorGUI.LabelField(area, Value.ToString(), label);
            EditorGUIUtility.labelWidth += FoldoutIndent;
        }

        /************************************************************************************************************************/

        /// <summary>The number of pixels of indentation required to fit the foldout arrow.</summary>
        const float FoldoutIndent = 12;

        /// <inheritdoc/>
        protected override void DoFoldoutGUI(Rect area)
        {
            var hierarchyMode = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = true;

            area.xMin += FoldoutIndent;
            IsExpanded = EditorGUI.Foldout(area, IsExpanded, GUIContent.none, true);

            EditorGUIUtility.hierarchyMode = hierarchyMode;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void DoDetailsGUI()
        {
            EditorGUI.indentLevel++;

            base.DoDetailsGUI();

            if (IsExpanded)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(FoldoutIndent);
                GUILayout.BeginVertical();

                DoLayerDetailsGUI();
                DoNodeDetailsGUI();

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;

            DoStatesGUI();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Draws controls for <see cref="AnimancerLayer.IsAdditive"/> and <see cref="AnimancerLayer._Mask"/>.
        /// </summary>
        private void DoLayerDetailsGUI()
        {
            var area = LayoutSingleLineRect(SpacingMode.Before);
            area = EditorGUI.IndentedRect(area);
            area.xMin += ExtraLeftPadding;

            var labelWidth = EditorGUIUtility.labelWidth;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var additiveLabel = "Is Additive";

            var additiveWidth = GUI.skin.toggle.CalculateWidth(additiveLabel) + StandardSpacing * 2;
            var additiveArea = StealFromLeft(ref area, additiveWidth, StandardSpacing);
            var maskArea = area;

            // Additive.
            EditorGUIUtility.labelWidth = CalculateLabelWidth(additiveLabel);
            EditorGUI.BeginChangeCheck();
            var isAdditive = EditorGUI.Toggle(additiveArea, additiveLabel, Value.IsAdditive);
            if (EditorGUI.EndChangeCheck())
                Value.IsAdditive = isAdditive;

            // Mask.
            using (var label = PooledGUIContent.Acquire("Mask"))
            {
                EditorGUIUtility.labelWidth = CalculateLabelWidth(label.text);
                EditorGUI.BeginChangeCheck();
                var mask = DoObjectFieldGUI(maskArea, label, Value.Mask, false);
                if (EditorGUI.EndChangeCheck())
                    Value.Mask = mask;
            }

            EditorGUI.indentLevel = indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;
        }

        /************************************************************************************************************************/

        private void DoStatesGUI()
        {
            if (!AnimancerGraphDrawer.ShowInactiveStates)
            {
                DoStatesGUI("Active States", ActiveStates);
            }
            else if (AnimancerGraphDrawer.SeparateActiveFromInactiveStates)
            {
                DoStatesGUI("Active States", ActiveStates);
                DoStatesGUI("Inactive States", InactiveStates);
            }
            else
            {
                DoStatesGUI("States", ActiveStates);
            }

            if (Value.Weight != 0 &&
                !Value.IsAdditive &&
                !Mathf.Approximately(Value.GetTotalChildWeight(), 1))
            {
                var message =
                    "The total Weight of all states in this layer does not equal 1" +
                    " which will likely give undesirable results.";

                if (AreAllStatesFadingOut())
                    message +=
                        " If you no longer want anything playing on a layer," +
                        " you should fade out that layer instead of fading out its states.";

                message += " Click here for more information.";

                EditorGUILayout.HelpBox(message, MessageType.Warning);

                if (TryUseClickEventInLastRect())
                    EditorUtility.OpenWithDefaultApp(Strings.DocsURLs.Layers);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Are all the target's states fading out to 0?</summary>
        private bool AreAllStatesFadingOut()
        {
            var count = Value.ActiveStates.Count;
            if (count == 0)
                return false;

            for (int i = 0; i < count; i++)
            {
                var state = Value.ActiveStates[i];
                if (state.TargetWeight != 0)
                    return false;
            }

            return true;
        }

        /************************************************************************************************************************/

        /// <summary>Draws all `states` in the given list.</summary>
        public void DoStatesGUI(string label, List<AnimancerState> states)
        {
            var area = LayoutSingleLineRect();

            const string Label = "Weight";
            var width = CalculateLabelWidth(Label);
            GUI.Label(StealFromRight(ref area, width), Label);

            EditorGUI.LabelField(area, label, states.Count.ToStringCached());

            EditorGUI.indentLevel++;
            for (int i = 0; i < states.Count; i++)
            {
                DoStateGUI(states[i]);
            }
            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <summary>Cached Inspectors that have already been created for states.</summary>
        private readonly Dictionary<AnimancerState, ICustomGUI>
            StateInspectors = new();

        /// <summary>Draws the Inspector for the given `state`.</summary>
        private void DoStateGUI(AnimancerState state)
        {
            if (!StateInspectors.TryGetValue(state, out var inspector))
            {
                inspector = CustomGUIFactory.GetOrCreateForObject(state);
                StateInspectors.Add(state, inspector);
            }

            inspector?.DoGUI();
            DoChildStatesGUI(state);
        }

        /************************************************************************************************************************/

        /// <summary>Draws all child states of the `state`.</summary>
        private void DoChildStatesGUI(AnimancerState state)
        {
            if (!state._IsInspectorExpanded)
                return;

            EditorGUI.indentLevel++;

            foreach (var child in state)
                if (child != null)
                    DoStateGUI(child);

            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void DoHeaderGUI()
        {
            if (!AnimancerGraphDrawer.ShowSingleLayerHeader &&
                Value.Graph.Layers.Count == 1 &&
                Value.Weight == 1 &&
                Value.TargetWeight == 1 &&
                Value.Speed == 1 &&
                !Value.IsAdditive &&
                Value._Mask == null &&
                Value.Graph.Component != null &&
                Value.Graph.Component.Animator != null &&
                Value.Graph.Component.Animator.runtimeAnimatorController == null)
                return;

            base.DoHeaderGUI();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI()
        {
            if (!Value.IsValid())
                return;

            base.DoGUI();

            var area = GUILayoutUtility.GetLastRect();
            HandleDragAndDropToPlay(area, Value);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If <see cref="AnimationClip"/>s or <see cref="IAnimationClipSource"/>s are dropped inside the `dropArea`,
        /// this method creates a new state in the `target` for each animation.
        /// </summary>
        public static void HandleDragAndDropToPlay(Rect area, object layerOrGraph)
        {
            if (layerOrGraph == null)
                return;

            _DragAndDropPlayTarget = layerOrGraph;

            _DragAndDropPlayHandler ??= HandleDragAndDropToPlay;
            _DragAndDropPlayHandler.Handle(area);

            _DragAndDropPlayTarget = null;
        }

        private static DragAndDropHandler<Object> _DragAndDropPlayHandler;
        private static object _DragAndDropPlayTarget;

        private static AnimancerLayer DragAndDropPlayTargetLayer
            => _DragAndDropPlayTarget as AnimancerLayer
            ?? (_DragAndDropPlayTarget is AnimancerGraph graph ? graph.Layers[0] : null);

        /// <summary>Handles drag and drop events to play animations and transitions.</summary>
        public static bool HandleDragAndDropToPlay(Object obj, bool isDrop)
        {
            if (_DragAndDropPlayTarget == null)
                return false;

            if (obj is AnimationClip clip)
            {
                if (clip.legacy)
                    return false;

                if (isDrop)
                    DragAndDropPlayTargetLayer.Play(clip);

                return true;
            }

            if (obj is ITransition transition)
            {
                if (isDrop)
                    DragAndDropPlayTargetLayer.Play(transition);

                return true;
            }

            var transitionAsset = TryCreateTransitionAttribute.TryCreateTransitionAsset(obj);
            if (transitionAsset != null)
            {
                if (isDrop)
                    DragAndDropPlayTargetLayer.Play(transitionAsset);

                if (!EditorUtility.IsPersistent(transitionAsset))
                    Object.DestroyImmediate(transitionAsset);

                return true;
            }

            using (ListPool<AnimationClip>.Instance.Acquire(out var clips))
            {
                clips.GatherFromSource(obj);

                var anyValid = false;

                for (int i = 0; i < clips.Count; i++)
                {
                    clip = clips[i];
                    if (clip.legacy)
                        continue;

                    if (!isDrop)
                        return true;

                    anyValid = true;
                    DragAndDropPlayTargetLayer.Play(clip);

                }

                if (anyValid)
                    return true;
            }

            return false;
        }

        /************************************************************************************************************************/
        #region Context Menu
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void PopulateContextMenu(GenericMenu menu)
        {
            menu.AddDisabledItem(new($"{DetailsPrefix}{nameof(Value.CurrentState)}: {Value.CurrentState}"));
            menu.AddDisabledItem(new($"{DetailsPrefix}{nameof(Value.CommandCount)}: {Value.CommandCount}"));

            menu.AddFunction("Stop",
                HasAnyStates((state) => state.IsPlaying || state.Weight != 0),
                () => Value.Stop());

            AnimancerEditorUtilities.AddFadeFunction(menu, "Fade In",
                Value.Index > 0 && Value.Weight != 1, Value,
                (duration) => Value.StartFade(1, duration));
            AnimancerEditorUtilities.AddFadeFunction(menu, "Fade Out",
                Value.Index > 0 && Value.Weight != 0, Value,
                (duration) => Value.StartFade(0, duration));

            AnimancerNodeBase.AddContextMenuIK(menu, Value);

            menu.AddSeparator("");

            menu.AddFunction("Destroy States",
                ActiveStates.Count > 0 || InactiveStates.Count > 0,
                () => Value.DestroyStates());

            AnimancerGraphDrawer.AddRootFunctions(menu, Value.Graph);

            menu.AddSeparator("");

            AnimancerGraphDrawer.AddDisplayOptions(menu);

            AnimancerEditorUtilities.AddDocumentationLink(menu, "Layer Documentation", Strings.DocsURLs.Layers);

            menu.ShowAsContext();
        }

        /************************************************************************************************************************/

        private bool HasAnyStates(Func<AnimancerState, bool> condition)
        {
            foreach (var state in Value)
            {
                if (condition(state))
                    return true;
            }

            return false;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

