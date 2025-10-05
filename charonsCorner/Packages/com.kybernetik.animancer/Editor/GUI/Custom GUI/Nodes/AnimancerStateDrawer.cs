// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using System;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGraphDrawer;
using static Animancer.Editor.AnimancerGUI;
using static Animancer.Editor.AnimancerStateDrawerColors;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="AnimancerState"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerStateDrawer_1
    [CustomGUI(typeof(AnimancerState))]
    public class AnimancerStateDrawer<T> : AnimancerNodeDrawer<T>
        where T : AnimancerState
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override bool AutoNormalizeSiblingWeights
            => AutoNormalizeWeights
            && typeof(T) != typeof(ManualMixerState);

        /************************************************************************************************************************/

        private FastObjectField _NameField;
        private FastObjectField _MainObjectField;

        /// <summary>Draws the state's main label with a bar to indicate its current time.</summary>
        protected override void DoLabelGUI(Rect area)
        {
            area = area.Expand(StandardSpacing, 0);

            var wholeArea = area;

            var effectiveWeight = Value.EffectiveWeight;

            var highlightArea = default(Rect);
            var isRepaint = Event.current.type == EventType.Repaint;
            if (isRepaint)
            {
                EditorGUI.DrawRect(wholeArea, HeaderBackgroundColor);

                highlightArea = DoTimeHighlightBarGUI(wholeArea, effectiveWeight);

                DoEventsGUI(wholeArea);

                ObjectHighlightGUI.Draw(wholeArea, Value);
            }

            DoWeightLabel(ref area, Value.Weight, effectiveWeight);

            AnimationBindings.DoBindingMatchGUI(ref area, Value);

            HandleLabelClick(wholeArea);

            area = EditorGUI.IndentedRect(area);

            var name = Value.DebugName ?? Value.Key;
            var mainObject = Value.MainObject;

            if (mainObject == null)
            {
                var value = name ?? Value;
                var drawPing = value != Value;
                _NameField.Draw(area, value, drawPing);
            }
            else if (ReferenceEquals(name, mainObject) ||
               (name is Object nameObject && nameObject == mainObject) ||
               (name is ITransition && Current != null && !Current.IsMainObjectUsedMultipleTimes(mainObject)))
            {
                _MainObjectField.Draw(area, mainObject, false);
            }
            else
            {
                if (name != null)
                {
                    var nameArea = StealFromLeft(ref area, EditorGUIUtility.labelWidth - IndentSize);
                    _NameField.Draw(nameArea, name, true);
                }

                _MainObjectField.Draw(area, mainObject, false);
            }

            if (isRepaint)
                DoDetailLinesGUI(wholeArea, highlightArea, effectiveWeight);
        }

        /************************************************************************************************************************/

        /// <summary>Draws a progress bar to show the animation time.</summary>
        public Rect DoTimeHighlightBarGUI(Rect area, float effectiveWeight)
            => DoTimeHighlightBarGUI(
                area,
                Value.IsPlaying,
                effectiveWeight,
                Value.Time,
                Value.EffectiveSpeed,
                Value.Length,
                Value.IsLooping);

        /// <summary>Draws a progress bar to show the animation time.</summary>
        public static Rect DoTimeHighlightBarGUI(
            Rect area,
            bool isPlaying,
            float effectiveWeight,
            float time,
            float speed,
            float length,
            bool isLooping)
        {
            if (ScaleTimeBarByWeight)
            {
                var height = area.height;
                area.height *= Mathf.Clamp01(effectiveWeight);
                area.y += height - area.height;
            }

            var color = isPlaying ? PlayingBarColor : PausedBarColor;

            var wrappedTime = GetWrappedTime(time, length, isLooping);

            if (length == 0)
            {
                if (time == 0)
                    return area;
            }
            else
            {
                if (speed >= 0 || time == 0)
                {
                    area.width *= Mathf.Clamp01(wrappedTime / length);
                }
                else
                {
                    var xMax = area.xMax;
                    area.x += area.width * Mathf.Clamp01(wrappedTime / length);
                    area.x = Mathf.Floor(area.x);
                    area.xMax = xMax;
                }
            }

            EditorGUI.DrawRect(area, color);

            return area;
        }

        /************************************************************************************************************************/

        /// <summary>Draws lines for the current weight, time, and fade destination.</summary>
        public void DoDetailLinesGUI(
            Rect totalArea,
            Rect highlightArea,
            float effectiveWeight)
        {
            var length = Value.Length;

            var speed = Value.Speed;
            var speedSign = speed >= 0 ? 1 : -1;
            var currentX = speed >= 0 ? highlightArea.xMax : highlightArea.xMin - 1;
            var forwardEdge = speed >= 0 ? totalArea.xMax : totalArea.xMin - 1;

            var color = FadeLineColor;
            color.a = color.a * effectiveWeight * 0.75f + 0.25f;

            if (Value.Time != 0 || Value.IsPlaying || Value.Weight != 0)
            {
                EditorGUI.DrawRect(
                    new(highlightArea.x, highlightArea.yMin, highlightArea.width, 1),
                    color);

                if (length == 0)
                    return;

                EditorGUI.DrawRect(
                    new(currentX - speedSign, totalArea.y, 1, totalArea.height),
                    color);
            }
            else if (length == 0)
            {
                return;
            }

            if (!Value.IsPlaying)
                return;

            var fade = Value.FadeGroup;
            if (fade == null || !fade.IsValid)
                return;

            var currentCorner = new Vector2(currentX, highlightArea.yMin);

            var targetWeight = Value.TargetWeight;
            var remainingFadeDuration = fade.RemainingFadeDuration;

            var targetCorner = new Vector2(
                currentCorner.x + speed * remainingFadeDuration / Value.Length * totalArea.width,
                Mathf.Lerp(totalArea.yMax, totalArea.yMin, targetWeight));

            var intersect = Mathf.InverseLerp(currentCorner.x, targetCorner.x, forwardEdge);
            var end = Vector2.LerpUnclamped(currentCorner, targetCorner, intersect);

            BeginTriangles(color);

            DrawLineBatched(
                currentCorner,
                end,
                1);

            if (intersect < 1 && Value.IsLooping)
            {
                end.x -= speedSign * totalArea.width;
                targetCorner.x -= speedSign * totalArea.width;

                DrawLineBatched(
                    end,
                    targetCorner,
                    1);
            }

            EndTriangles();
        }

        /************************************************************************************************************************/

        /// <summary>Draws marks on the timeline for each event.</summary>
        private void DoEventsGUI(Rect area)
        {
            if (!ShowEvents)
                return;

            DoAnimancerEventsGUI(area);
            DoAnimationEventsGUI(area);
        }

        /// <summary>Draws marks on the timeline for each Animancer Event.</summary>
        private void DoAnimancerEventsGUI(Rect area)
        {
            var events = Value.SharedEvents;
            if (events == null)
                return;

            for (int i = 0; i < events.Count; i++)
                DoEventTick(area, events[i].normalizedTime);

            if (events.OnEnd != null)
                DoEventTick(area, events.GetRealNormalizedEndTime(Value.Speed));
        }

        /// <summary>Draws marks on the timeline for each Animation Event.</summary>
        private void DoAnimationEventsGUI(Rect area)
        {
            var clip = Value.MainObject as AnimationClip;
            if (clip == null)
                return;

            var inverseLength = 1f / Value.Length;

            var events = clip.GetCachedEvents();
            for (int i = 0; i < events.Length; i++)
                DoEventTick(area, events[i].time * inverseLength);
        }

        /// <summary>Draws a mark on the timeline for an event.</summary>
        private static void DoEventTick(Rect area, float normalizedTime)
        {
            if (normalizedTime >= 0 && normalizedTime <= 1)
            {
                var x = area.x + area.width * normalizedTime;
                var eventArea = new Rect(x - 1, area.y, 2, area.height * 0.3f);
                EditorGUI.DrawRect(eventArea, EventTickColor);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Handles clicks on the label area.</summary>
        private void HandleLabelClick(Rect area)
        {
            var currentEvent = Event.current;
            if (currentEvent.type != EventType.MouseUp ||
                currentEvent.button != 0 ||
                !area.Contains(currentEvent.mousePosition))
                return;

            currentEvent.Use(0);

            if (currentEvent.control)
                FadeInTarget();
            else
                ToggleExpanded(currentEvent.alt);
        }

        /// <summary>Fades in the target state (or its parent state if not directly attached to a layer).</summary>
        private void FadeInTarget()
        {
            Value.Graph.UnpauseGraph();

            AnimancerState target = Value;
            while (target != null)
            {
                var parent = target.Parent;
                if (parent is AnimancerLayer layer)
                {
                    var fadeDuration = target.CalculateEditorFadeDuration(
                        AnimancerGraph.DefaultFadeDuration);
                    layer.Play(target, fadeDuration);
                    return;
                }

                target = parent as AnimancerState;
            }
        }

        /// <summary>Toggles the target's details between expanded and collapsed.</summary>
        private void ToggleExpanded(bool toggleSiblings)
        {
            IsExpanded = !IsExpanded;

            if (toggleSiblings)
            {
                var parent = Value.Parent;
                var childCount = parent.ChildCount;
                for (int i = 0; i < childCount; i++)
                    parent.GetChildNode(i)._IsInspectorExpanded = IsExpanded;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void DoFoldoutGUI(Rect area)
        {
            var hierarchyMode = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = true;

            IsExpanded = EditorGUI.Foldout(area, IsExpanded, GUIContent.none, true);

            EditorGUIUtility.hierarchyMode = hierarchyMode;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Gets the current <see cref="AnimancerState.Time"/>.
        /// If the state is looping, the value is modulo by the <see cref="AnimancerState.Length"/>.
        /// </summary>
        private float GetWrappedTime(out float length)
            => GetWrappedTime(Value.Time, length = Value.Length, Value.IsLooping);

        /// <summary>
        /// Gets the current <see cref="AnimancerState.Time"/>.
        /// If the state is looping, the value is modulo by the <see cref="AnimancerState.Length"/>.
        /// </summary>
        private static float GetWrappedTime(float time, float length, bool isLooping)
        {
            var wrappedTime = time;

            if (isLooping)
            {
                wrappedTime = AnimancerUtilities.Wrap(wrappedTime, length);
                if (wrappedTime == 0 && time != 0)
                    wrappedTime = length;
            }

            return wrappedTime;
        }

        /************************************************************************************************************************/

        private FastObjectField _KeyField;
        private FastObjectField _OwnerField;

        /************************************************************************************************************************/

        /// <summary>The display name of the <see cref="AnimancerState.MainObject"/> field.</summary>
        public virtual string MainObjectName
            => "Main Object";

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void DoDetailsGUI()
        {
            base.DoDetailsGUI();

            if (!IsExpanded)
                return;

            EditorGUI.indentLevel++;

            DoOptionalReferenceGUI(ref _KeyField, "Key", Value.Key);
            DoOptionalReferenceGUI(ref _OwnerField, "Owner", Value.Owner);

            var mainObject = Value.MainObject;
            if (mainObject != null)
            {
                var mainObjectType = Value.MainObjectType ?? typeof(Object);

                EditorGUI.BeginChangeCheck();

                var area = LayoutSingleLineRect(SpacingMode.Before);

                mainObject = EditorGUI.ObjectField(
                    area,
                    MainObjectName,
                    mainObject,
                    mainObjectType,
                    true);

                if (EditorGUI.EndChangeCheck())
                    Value.MainObject = mainObject;
            }

            DoTimeSliderGUI();
            DoNodeDetailsGUI();
            DoOnEndGUI();
            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <summary>Draws a `reference` if it isn't <c>null</c>.</summary>
        private static void DoOptionalReferenceGUI(ref FastObjectField field, string label, object reference)
        {
            if (reference != null)
                field.Draw(LayoutSingleLineRect(SpacingMode.Before), label, reference);
        }

        /************************************************************************************************************************/

        /// <summary>Draws a slider for controlling the current <see cref="AnimancerState.Time"/>.</summary>
        private void DoTimeSliderGUI()
        {
            if (Value.Length <= 0)
                return;

            var time = GetWrappedTime(out var length);

            if (length == 0)
                return;

            var area = LayoutSingleLineRect(SpacingMode.Before);

            var normalized = DoNormalizedTimeToggle(ref area);

            string label;
            float max;
            if (normalized)
            {
                label = "Normalized Time";
                time /= length;
                max = 1;
            }
            else
            {
                label = "Time";
                max = length;
            }

            DoLoopCounterGUI(ref area, length);

            EditorGUI.BeginChangeCheck();

            label = BeginTightLabel(label);
            time = EditorGUI.Slider(area, label, time, 0, max);
            EndTightLabel();

            if (TryUseClickEvent(area, 2))
                time = 0;

            if (EditorGUI.EndChangeCheck())
            {
                if (normalized)
                    Value.NormalizedTime = time;
                else
                    Value.Time = time;
            }
        }

        /************************************************************************************************************************/

        private static bool DoNormalizedTimeToggle(ref Rect area)
        {
            using (var label = PooledGUIContent.Acquire("N"))
            {
                var style = MiniButtonStyle;

                var width = style.CalculateWidth(label);
                var toggleArea = StealFromRight(ref area, width);

                UseNormalizedTimeSliders.Value = GUI.Toggle(toggleArea, UseNormalizedTimeSliders, label, style);
            }

            return UseNormalizedTimeSliders;
        }

        /************************************************************************************************************************/

        private static ConversionCache<int, string> _LoopCounterCache;

        private void DoLoopCounterGUI(ref Rect area, float length)
        {
            _LoopCounterCache ??= new(x => $"x{x}");

            string label;
            var normalizedTime = Value.Time / length;
            if (float.IsNaN(normalizedTime))
            {
                label = "NaN";
            }
            else
            {
                var loops = Mathf.FloorToInt(Value.Time / length);
                label = _LoopCounterCache.Convert(loops);
            }

            var width = CalculateLabelWidth(label);

            var labelArea = StealFromRight(ref area, width);

            GUI.Label(labelArea, label);
        }

        /************************************************************************************************************************/

        private void DoOnEndGUI()
        {
            var events = Value.SharedEvents;
            if (events == null)
                return;

            var drawer = EventSequenceDrawer.Get(events);
            var area = LayoutRect(drawer.CalculateHeight(events), SpacingMode.Before);

            using (var label = PooledGUIContent.Acquire("Events"))
                drawer.DoGUI(ref area, events, label);
        }

        /************************************************************************************************************************/
        #region Context Menu
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void PopulateContextMenu(GenericMenu menu)
        {
            AddContextMenuFunctions(menu);

            menu.AddFunction("Play",
                !Value.IsPlaying || Value.Weight != 1,
                () =>
                {
                    AnimancerState.SkipNextExpectFade();
                    Value.Graph.UnpauseGraph();
                    Value.Graph.Layers[0].Play(Value);
                });

            AnimancerEditorUtilities.AddFadeFunction(menu, "Cross Fade (Ctrl + Click)",
                Value.Weight != 1,
                Value,
                duration =>
                {
                    AnimancerState.SkipNextExpectFade();
                    Value.Graph.UnpauseGraph();
                    Value.Graph.Layers[0].Play(Value, duration);
                });

            menu.AddSeparator("");
            menu.AddItem(new("Destroy State"),
                false,
                () => Value.Destroy());

            menu.AddSeparator("");

            AddDisplayOptions(menu);

            AnimancerEditorUtilities.AddDocumentationLink(
                menu,
                "State Documentation",
                Strings.DocsURLs.States);
        }

        /************************************************************************************************************************/

        /// <summary>Adds the details of this state to the `menu`.</summary>
        protected virtual void AddContextMenuFunctions(GenericMenu menu)
        {
            menu.AddDisabledItem(new($"{DetailsPrefix}{nameof(Value.Key)}: {AnimancerUtilities.ToStringOrNull(Value.Key)}"));
            menu.AddDisabledItem(new($"{DetailsPrefix}{nameof(Value.Owner)}: {AnimancerUtilities.ToStringOrNull(Value.Owner)}"));

            var length = Value.Length;
            if (!float.IsNaN(length))
                menu.AddDisabledItem(new($"{DetailsPrefix}{nameof(Value.Length)}: {length}"));

            menu.AddDisabledItem(new($"{DetailsPrefix}Playable Path: {Value.GetPath()}"));

            var mainAsset = Value.MainObject;
            if (mainAsset != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(mainAsset);
                if (assetPath != null)
                    menu.AddDisabledItem(new($"{DetailsPrefix}Asset Path: {assetPath.Replace("/", "->")}"));
            }

            var events = Value.SharedEvents;
            if (events != null)
            {
                for (int i = 0; i < events.Count; i++)
                {
                    var index = i;
                    var name = events.GetName(i);
                    AddEventFunctions(
                        menu,
                        name.IsNullOrEmpty() ? "Event " + index : name,
                        name,
                        events[index],
                        () => events.SetCallback(index, AnimancerEvent.InvokeBoundCallback),
                        () => events.Remove(index));
                }

                AddEventFunctions(
                    menu,
                    "End Event",
                    default,
                    events.EndEvent,
                    () => events.EndEvent = new(float.NaN, null), null);
            }
        }

        /************************************************************************************************************************/

        private void AddEventFunctions(
            GenericMenu menu,
            string displayName,
            StringReference name,
            AnimancerEvent animancerEvent,
            GenericMenu.MenuFunction clearEvent,
            GenericMenu.MenuFunction removeEvent)
        {
            displayName = $"Events/{displayName}/";

            menu.AddDisabledItem(new($"{displayName}{nameof(AnimancerState.NormalizedTime)}: {animancerEvent.normalizedTime}"));

            bool canInvoke;
            if (animancerEvent.callback == null)
            {
                menu.AddDisabledItem(new(displayName + "Callback: null"));
                canInvoke = false;
            }
            else if (animancerEvent.callback == AnimancerEvent.DummyCallback)
            {
                menu.AddDisabledItem(new(displayName + "Callback: Dummy"));
                canInvoke = false;
            }
            else
            {
                var label = displayName +
                    (animancerEvent.callback.Target != null
                    ? ("Target: " + animancerEvent.callback.Target)
                    : "Target: null");

                var targetObject = animancerEvent.callback.Target as Object;
                menu.AddFunction(label,
                    targetObject != null,
                    () => Selection.activeObject = targetObject);

                menu.AddDisabledItem(new(
                    $"{displayName}Declaring Type: {animancerEvent.callback.Method.DeclaringType.GetNameCS()}"));

                menu.AddDisabledItem(new(
                    $"{displayName}Method: {animancerEvent.callback.Method}"));

                canInvoke = true;
            }

            if (clearEvent != null)
                menu.AddFunction(displayName + "Clear", canInvoke || !float.IsNaN(animancerEvent.normalizedTime), clearEvent);

            if (removeEvent != null)
                menu.AddFunction(displayName + "Remove", true, removeEvent);

            menu.AddFunction(displayName + "Invoke", canInvoke, () => animancerEvent.DelayInvoke(name, Value));
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }

    /// <summary>[Editor-Only] Colors used by <see cref="AnimancerStateDrawer{T}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerStateDrawerColors
    public static class AnimancerStateDrawerColors
    {
        /************************************************************************************************************************/

        /// <summary>Colors used by this system.</summary>
        public static readonly Color
            HeaderBackgroundColor = Grey(0.35f, 0.35f),
            PlayingBarColor = new(0.15f, 0.7f, 0.15f, 0.4f),// Green = Playing.
            PausedBarColor = new(0.7f, 0.7f, 0.15f, 0.4f),// Yelow = Paused.
            FadeLineColor = new(0.3f, 1, 0.3f, 1);

        /// <summary>Colors used by this system.</summary>
        public static Color EventTickColor
            => Grey(EditorGUIUtility.isProSkin ? 0.8f : 0.2f, 0.8f);

        /************************************************************************************************************************/
    }
}

#endif

