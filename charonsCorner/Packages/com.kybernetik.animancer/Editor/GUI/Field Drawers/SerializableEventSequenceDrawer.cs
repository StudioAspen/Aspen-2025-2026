// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Editor.Previews;
using Animancer.Units;
using Animancer.Units.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;
using static Animancer.Editor.AnimancerGUI;
using Object = UnityEngine.Object;
using SerializableSequence = Animancer.AnimancerEvent.Sequence.Serializable;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="SerializableSequence"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/SerializableEventSequenceDrawer
    [CustomPropertyDrawer(typeof(SerializableSequence), true)]
    public class SerializableEventSequenceDrawer : PropertyDrawer
    {
        /************************************************************************************************************************/

        /// <summary><see cref="RepaintEverything"/></summary>
        public static UnityAction Repaint = RepaintEverything;

        private const float
            EventSeparatorThickness = 1,
            EventSeparatorPadding = 1;

        private static readonly Color
            SelectedEventHighlightColor = new(0.3f, 0.55f, 0.95f, 0.25f);

        private readonly Dictionary<string, List<AnimBool>>
            EventVisibility = new();

        private AnimBool GetVisibility(Context context, int index)
        {
            var path = context.Property.propertyPath;
            if (!EventVisibility.TryGetValue(path, out var list))
                EventVisibility.Add(path, list = new());

            while (list.Count <= index)
            {
                var visible = context.Property.isExpanded || context.SelectedEvent == index;
                list.Add(new(visible, Repaint));
            }

            return list[index];
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calculates the number of vertical pixels the `property` will occupy when it is drawn.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.hasMultipleDifferentValues)
                return LineHeight;

            using var context = Context.Get(property);

            var height = LineHeight;

            var count = Math.Max(1, context.Times.Count);
            for (int i = 0; i < count; i++)
            {
                height += CalculateEventHeight(context, i) * GetVisibility(context, i).faded;
            }

            var events = context.Sequence?.InitializedEvents;
            if (events != null)
                height += EventSequenceDrawer.Get(events).CalculateHeight(events) + StandardSpacing;

            return height;
        }

        /************************************************************************************************************************/

        private float CalculateEventHeight(Context context, int index)
        {
            // Name.
            var height = index < context.Times.Count - 1
                ? LineHeight + StandardSpacing
                : 0;// End Events don't have a Name.

            // Time.
            height += AnimationTimeAttributeDrawer.GetPropertyHeight(null, null) + StandardSpacing;

            // Callback.
            if (!SerializableEventSequenceDrawerSettings.HideEventCallbacks || context.Callbacks.Count > 0)
            {
                height += index < context.Callbacks.Count
                    ? EditorGUI.GetPropertyHeight(context.Callbacks.GetElement(index), null, false)
                    : DummyInvokableDrawer.Height;
                height += StandardSpacing;
            }

            // Separator.
            if (index > 0)
                height += EventSeparatorPadding + EventSeparatorThickness + EventSeparatorPadding;

            return height;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI for the `property`.</summary>
        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            var warnings = OptionalWarning.ProOnly.DisableTemporarily();

            using var context = Context.Get(property);

            DoHeaderGUI(ref area, label, context);

            if (property.hasMultipleDifferentValues)
                return;

            EditorGUI.indentLevel++;
            DoAllEventsGUI(ref area, context);
            EditorGUI.indentLevel--;

            var sequence = context.Sequence?.InitializedEvents;
            if (sequence != null)
            {
                using (var content = PooledGUIContent.Acquire("Runtime Events",
                    $"The runtime {nameof(AnimancerEvent)}.{nameof(AnimancerEvent.Sequence)}" +
                    $" created from the serialized data above"))
                {
                    EventSequenceDrawer.Get(sequence).DoGUI(ref area, sequence, content);
                }
            }

            warnings.Enable();
        }

        /************************************************************************************************************************/

        private void DoHeaderGUI(ref Rect area, GUIContent label, Context context)
        {
            if (!EditorGUIUtility.hierarchyMode)
                EditorGUI.indentLevel--;

            area.height = LineHeight;
            var headerArea = area;
            NextVerticalArea(ref area);

            label = EditorGUI.BeginProperty(headerArea, label, context.Property);

            if (!context.Property.hasMultipleDifferentValues)
            {
                var addEventArea = StealFromRight(ref headerArea, headerArea.height, StandardSpacing);
                DoAddRemoveEventButtonGUI(addEventArea, context);
            }

            if (context.TransitionContext.Transition != null)
            {
                EditorGUI.EndProperty();

                TimelineGUI.DoGUI(headerArea, context, out var addEventNormalizedTime);

                if (!float.IsNaN(addEventNormalizedTime))
                {
                    AddEvent(context, addEventNormalizedTime, null);
                }
            }
            else
            {
                string summary;
                if (context.Times.Count == 0)
                {
                    summary = "[0] End Time 1";
                }
                else
                {
                    var index = context.Times.Count - 1;
                    var endTime = context.Times.GetElement(index).floatValue;
                    summary = $"[{index}] End Time {endTime:G3}";
                }

                using (var content = PooledGUIContent.Acquire(summary))
                    EditorGUI.LabelField(headerArea, label, content);

                EditorGUI.EndProperty();
            }

            EditorGUI.BeginChangeCheck();
            context.Property.isExpanded =
                EditorGUI.Foldout(headerArea, context.Property.isExpanded, GUIContent.none, true);
            if (EditorGUI.EndChangeCheck())
                context.SelectedEvent = -1;

            if (!EditorGUIUtility.hierarchyMode)
                EditorGUI.indentLevel++;
        }

        /************************************************************************************************************************/

        private static readonly int EventTimeHash = "EventTime".GetHashCode();

        private static int _HotControlAdjustRoot;
        private static int _SelectedEventToHotControl;

        private void DoAllEventsGUI(ref Rect area, Context context)
        {
            var currentEvent = Event.current;
            var originalEventType = currentEvent.type;
            if (originalEventType == EventType.Used)
                return;

            var rootControlID = GUIUtility.GetControlID(EventTimeHash - 1, FocusType.Passive);

            var eventCount = Mathf.Max(1, context.Times.Count);
            for (int i = 0; i < eventCount; i++)
            {
                if (i > 0)
                {
                    area.y += EventSeparatorPadding;

                    var separatorArea = EditorGUI.IndentedRect(area);
                    separatorArea.xMin -= LineHeight;
                    separatorArea.height = EventSeparatorThickness;
                    EditorGUI.DrawRect(separatorArea, Grey(0.5f, 0.5f));

                    area.y += EventSeparatorThickness + EventSeparatorPadding;
                }

                var controlID = GUIUtility.GetControlID(EventTimeHash + i, FocusType.Passive);

                if (rootControlID == _HotControlAdjustRoot &&
                    _SelectedEventToHotControl > 0 &&
                    i == context.SelectedEvent)
                {
                    GUIUtility.hotControl = GUIUtility.keyboardControl = controlID + _SelectedEventToHotControl;
                    _SelectedEventToHotControl = 0;
                    _HotControlAdjustRoot = -1;
                }

                if (i == context.SelectedEvent)
                {
                    var highlightArea = EditorGUI.IndentedRect(area);
                    highlightArea.xMin -= LineHeight;
                    highlightArea.height = CalculateEventHeight(context, i);
                    if (i > 0)
                    {
                        highlightArea.y -= EventSeparatorPadding;
                        highlightArea.height -= EventSeparatorPadding * 2;
                    }
                    EditorGUI.DrawRect(highlightArea, SelectedEventHighlightColor);
                }

                DoEventGUI(ref area, context, i, false);

                if (currentEvent.type == EventType.Used && originalEventType == EventType.MouseUp)
                {
                    context.SelectedEvent = i;

                    if (SortEvents(context))
                    {
                        _SelectedEventToHotControl = GUIUtility.keyboardControl - controlID;
                        _HotControlAdjustRoot = rootControlID;
                        Deselect();
                    }

                    GUIUtility.ExitGUI();
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI fields for the event at the specified `index`.</summary>
        public void DoEventGUI(ref Rect area, Context context, int index, bool autoSort)
        {
            GetEventLabels(
                index,
                context,
                out var nameLabel,
                out var timeLabel,
                out var callbackLabel,
                out var defaultTime,
                out var isEndEvent);

            var y = area.y;

            var visibility = GetVisibility(context, index);
            visibility.target = context.Property.isExpanded || context.SelectedEvent == index;

            var x = area.xMin;
            area.xMin = 0;

            area.height = CalculateEventHeight(context, index) * visibility.faded;

            var offset = GuiOffset;
            GuiOffset += area.position;

            TypeSelectionButton.BeginDelayingLinkLines();
            try
            {
                GUI.BeginGroup(area, GUIStyle.none);

                if (visibility.faded > 0)
                {
                    area.xMin = x;
                    area.y = 0;

                    DoNameGUI(ref area, context, index, nameLabel);
                    DoTimeGUI(ref area, context, index, autoSort, timeLabel, defaultTime, isEndEvent);
                    DoCallbackGUI(ref area, context, index, callbackLabel);

                    area.y = area.y * visibility.faded + y;
                    area.height *= visibility.faded;
                }

                GUI.EndGroup();
            }
            finally
            {
                GuiOffset = offset;

                TypeSelectionButton.EndDelayingLinkLines();
            }

            area.xMin = x;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the time field for the event at the specified `index`.</summary>
        public static void DoNameGUI(
            ref Rect area,
            Context context,
            int index,
            string nameLabel)
        {
            if (nameLabel == null)
                return;

            EditorGUI.BeginChangeCheck();

            area.height = LineHeight;
            var fieldArea = area;
            NextVerticalArea(ref area);

            using (var label = PooledGUIContent.Acquire(nameLabel,
                "An optional name which can be used to identify the event in code." +
                " Leaving all names blank is recommended if you aren't using them."))
            {
                fieldArea = EditorGUI.PrefixLabel(fieldArea, label);
            }

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var nameProperty = index < context.Names.Count
                ? context.Names.GetElement(index)
                : null;

            var name = nameProperty?.objectReferenceValue;

            DoNameWarningGUI(ref fieldArea, context, name);

            var exitGUI = false;

            if (nameProperty != null)
            {
                EditorGUI.PropertyField(fieldArea, nameProperty, GUIContent.none);
            }
            else
            {
                EditorGUI.BeginProperty(fieldArea, GUIContent.none, context.Names.Property);

                EditorGUI.BeginChangeCheck();

                name = StringAssetDrawer.DrawGUI(fieldArea, GUIContent.none, null, out exitGUI);

                if (EditorGUI.EndChangeCheck() && name != null)
                {
                    // Expand up to the new name.
                    // If we need to expand more than one slot, make sure all the new ones are null.
                    context.Names.Count++;
                    if (context.Names.Count < index + 1)
                    {
                        var nextProperty = context.Names.GetElement(context.Names.Count - 1);
                        nextProperty.objectReferenceValue = null;
                        context.Names.Count = index + 1;
                    }

                    // Get and assign the new property.
                    nameProperty = context.Names.GetElement(index);
                    nameProperty.objectReferenceValue = name;
                }

                if (!exitGUI)
                    EditorGUI.EndProperty();

            }

            EditorGUI.indentLevel = indentLevel;

            if (EditorGUI.EndChangeCheck())
            {
                var events = context.Sequence?.InitializedEvents;
                events?.SetName(index, name as StringAsset);
            }

            if (exitGUI)
            {
                context.Names.Property.serializedObject.ApplyModifiedProperties();
                GUIUtility.ExitGUI();
            }
        }

        /************************************************************************************************************************/

        private static void DoNameWarningGUI(ref Rect area, Context context, Object name)
        {
            var property = context.TransitionContext.Property;
            var attribute = AttributeCache<EventNamesAttribute>.FindAttribute(property);
            if (attribute == null || !attribute.HasNames)
                return;

            var icon = name == null || Array.IndexOf(attribute.Names, (StringReference)name.name) >= 0
                ? AnimancerIcons.Info
                : AnimancerIcons.Warning;

            var warningArea = StealFromLeft(ref area, area.height, StandardSpacing);

            var tooltip = attribute.NamesToString("Expected Names:");
            using (var content = PooledGUIContent.Acquire("", tooltip))
            {
                content.image = icon;
                GUI.Label(warningArea, content);
                content.image = null;
            }
        }

        /************************************************************************************************************************/

        private static readonly AnimationTimeAttributeDrawer
            AnimationTimeAttributeDrawer = new();

        static SerializableEventSequenceDrawer()
            => AnimationTimeAttributeDrawer.Initialize(
                new AnimationTimeAttribute(AnimationTimeAttribute.Units.Normalized));

        private static float _PreviousTime = float.NaN;

        /// <summary>Draws the time field for the event at the specified `index`.</summary>
        public static void DoTimeGUI(
            ref Rect area,
            Context context,
            int index,
            bool autoSort,
            string timeLabel,
            float defaultTime,
            bool isEndEvent)
        {
            EditorGUI.BeginChangeCheck();

            area.height = AnimationTimeAttributeDrawer.GetPropertyHeight(null, null);
            var timeArea = area;
            NextVerticalArea(ref area);

            var middleClickedTimeArea = TryUseClickEvent(timeArea, 2);

            float normalizedTime;

            using (var label = PooledGUIContent.Acquire(timeLabel,
                isEndEvent ? Strings.Tooltips.EndTime : Strings.Tooltips.CallbackTime))
            {
                var length = context.TransitionContext.Transition != null
                    ? context.TransitionContext.MaximumDuration
                    : float.NaN;

                if (index < context.Times.Count)
                {
                    var timeProperty = context.Times.GetElement(index);
                    if (timeProperty == null)// Multi-selection screwed up the property retrieval.
                    {
                        EditorGUI.BeginChangeCheck();

                        var propertyLabel = EditorGUI.BeginProperty(timeArea, label, context.Times.Property);
                        if (isEndEvent)
                            AnimationTimeAttributeDrawer.NextDefaultValue = defaultTime;
                        normalizedTime = float.NaN;
                        AnimationTimeAttributeDrawer.OnGUI(timeArea, propertyLabel, ref normalizedTime);

                        EditorGUI.EndProperty();

                        if (EditorGUI.EndChangeCheck())
                        {
                            context.Times.Count = context.Times.Count;
                            timeProperty = context.Times.GetElement(index);
                            timeProperty.floatValue = normalizedTime;
                            SyncEventTimeChange(context, index, normalizedTime);
                        }
                    }
                    else// Event time property was correctly retrieved.
                    {
                        var wasEditingTextField = EditorGUIUtility.editingTextField;
                        if (!wasEditingTextField)
                            _PreviousTime = float.NaN;

                        EditorGUI.BeginChangeCheck();

                        var propertyLabel = EditorGUI.BeginProperty(timeArea, label, timeProperty);

                        if (isEndEvent)
                            AnimationTimeAttributeDrawer.NextDefaultValue = defaultTime;
                        normalizedTime = timeProperty.floatValue;
                        AnimationTimeAttributeDrawer.OnGUI(timeArea, propertyLabel, ref normalizedTime);

                        EditorGUI.EndProperty();

                        if (middleClickedTimeArea)
                            normalizedTime = float.NaN;

                        var isEditingTextField = EditorGUIUtility.editingTextField;
                        if (EditorGUI.EndChangeCheck() ||
                            middleClickedTimeArea ||
                            (wasEditingTextField && !isEditingTextField))
                        {
                            if (float.IsNaN(normalizedTime))
                            {
                                RemoveEvent(context, index);
                                Deselect();
                            }
                            else if (isEndEvent)
                            {
                                timeProperty.floatValue = normalizedTime;
                                SyncEventTimeChange(context, index, normalizedTime);
                            }
                            else if (!autoSort && isEditingTextField)
                            {
                                _PreviousTime = normalizedTime;
                            }
                            else
                            {
                                if (!float.IsNaN(_PreviousTime))
                                {
                                    if (Event.current.keyCode != KeyCode.Escape)
                                    {
                                        normalizedTime = _PreviousTime;
                                        Deselect();
                                    }

                                    _PreviousTime = float.NaN;
                                }

                                WrapEventTime(context, ref normalizedTime);

                                timeProperty.floatValue = normalizedTime;
                                SyncEventTimeChange(context, index, normalizedTime);

                                if (autoSort)
                                    SortEvents(context);
                            }

                            GUI.changed = true;
                        }
                    }
                }
                else// Dummy End Event (when there are no event times).
                {
                    AnimancerUtilities.Assert(index == 0, "Dummy end event index != 0");
                    EditorGUI.BeginChangeCheck();

                    EditorGUI.BeginProperty(timeArea, GUIContent.none, context.Times.Property);

                    AnimationTimeAttributeDrawer.NextDefaultValue = defaultTime;
                    normalizedTime = float.NaN;
                    AnimationTimeAttributeDrawer.OnGUI(timeArea, label, ref normalizedTime);

                    EditorGUI.EndProperty();

                    if (EditorGUI.EndChangeCheck() && !float.IsNaN(normalizedTime))
                    {
                        context.Times.Count = 1;
                        var timeProperty = context.Times.GetElement(0);
                        timeProperty.floatValue = normalizedTime;
                        SyncEventTimeChange(context, 0, normalizedTime);
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                var eventType = Event.current.type;
                if (eventType == EventType.Layout)
                    return;

                if (eventType == EventType.Used)
                {
                    normalizedTime = UnitsAttributeDrawer.GetDisplayValue(normalizedTime, defaultTime);
                    TransitionPreviewWindow.PreviewNormalizedTime = normalizedTime;
                }

                GUIUtility.ExitGUI();
            }
        }

        /// <summary>Draws the time field for the event at the specified `index`.</summary>
        public static void DoTimeGUI(ref Rect area, Context context, int index, bool autoSort)
        {
            GetEventLabels(
                index,
                context,
                out var _,
                out var timeLabel,
                out var _,
                out var defaultTime,
                out var isEndEvent);

            DoTimeGUI(
                ref area,
                context,
                index,
                autoSort,
                timeLabel,
                defaultTime,
                isEndEvent);
        }

        /************************************************************************************************************************/

        /// <summary>Updates the <see cref="SerializableSequence.Events"/> to accomodate a changed event time.</summary>
        public static void SyncEventTimeChange(Context context, int index, float normalizedTime)
        {
            var events = context.Sequence?.InitializedEvents;
            if (events == null)
                return;

            if (index == events.Count)// End Event.
            {
                events.NormalizedEndTime = normalizedTime;
            }
            else// Regular Event.
            {
                events.SetNormalizedTime(index, normalizedTime);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI fields for the event at the specified `index`.</summary>
        public static void DoCallbackGUI(
            ref Rect area,
            Context context,
            int index,
            string callbackLabel)
        {
            if (SerializableEventSequenceDrawerSettings.HideEventCallbacks && context.Callbacks.Count == 0)
                return;

            EditorGUI.BeginChangeCheck();

            using (var label = PooledGUIContent.Acquire(callbackLabel))
            {
                if (index < context.Callbacks.Count)
                {
                    var callback = context.Callbacks.GetElement(index);
                    area.height = EditorGUI.GetPropertyHeight(callback, false);

                    EditorGUI.PropertyField(area, callback, label, false);
                }
                else if (DummyInvokableDrawer.DoGUI(ref area, label, context.Callbacks.Property, out var callback))
                {
                    try
                    {
                        SerializableSequence.DisableCompactArrays = true;

                        if (index >= context.Times.Count)
                        {
                            context.Times.Property.InsertArrayElementAtIndex(index);
                            context.Times.Count++;
                            context.Times.GetElement(index).floatValue = float.NaN;
                            context.Times.Property.serializedObject.ApplyModifiedProperties();
                        }

                        context.Callbacks.Property.ForEachTarget(callbacksProperty =>
                        {
                            var accessor = callbacksProperty.GetAccessor();
                            var oldCallbacks = (Array)accessor.GetValue(callbacksProperty.serializedObject.targetObject);

                            Array newCallbacks;
                            if (oldCallbacks == null)
                            {
                                var elementType = accessor.GetFieldElementType(callbacksProperty);
                                newCallbacks = Array.CreateInstance(elementType, 1);
                            }
                            else
                            {
                                var elementType = oldCallbacks.GetType().GetElementType();
                                newCallbacks = Array.CreateInstance(elementType, index + 1);
                                Array.Copy(oldCallbacks, newCallbacks, oldCallbacks.Length);
                            }

                            newCallbacks.SetValue(callback, index);
                            accessor.SetValue(callbacksProperty, newCallbacks);
                        });

                        context.Callbacks.Property.OnPropertyChanged();
                        context.Callbacks.Property.GetArrayElementAtIndex(index).isExpanded = true;
                        context.Callbacks.Refresh();
                    }
                    finally
                    {
                        SerializableSequence.DisableCompactArrays = false;
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (index < context.Callbacks.Count)
                {
                    var events = context.Sequence?.InitializedEvents;
                    if (events != null)
                    {
                        var animancerEvent = index < events.Count
                            ? events[index]
                            : events.EndEvent;

                        if (AnimancerEvent.IsNullOrDummy(animancerEvent.callback))
                        {
                            context.Callbacks.Property.serializedObject.ApplyModifiedProperties();
                            var property = context.Callbacks.GetElement(index);
                            var callback = property.GetValue();
                            var invoke = SerializableSequence.GetInvoke(callback as IInvokable);
                            if (index < events.Count)
                                events.SetCallback(index, invoke);
                            else
                                events.OnEnd = invoke;
                        }
                    }
                }
            }

            NextVerticalArea(ref area);
        }

        /************************************************************************************************************************/

        private static ConversionCache<int, string>
            _NameLabelCache,
            _TimeLabelCache,
            _CallbackLabelCache;

        private static void GetEventLabels(
            int index,
            Context context,
            out string nameLabel,
            out string timeLabel,
            out string callbackLabel,
            out float defaultTime,
            out bool isEndEvent)
        {
            if (index >= context.Times.Count - 1)
            {
                nameLabel = null;
                timeLabel = "End Time";
                callbackLabel = "End Callback";

                defaultTime = AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(
                    context.TransitionContext.Transition?.Speed ?? 1);
                isEndEvent = true;
            }
            else
            {
                if (_NameLabelCache == null)
                {
                    _NameLabelCache = new((i) => $"Event {i} Name");
                    _TimeLabelCache = new((i) => $"Event {i} Time");
                    _CallbackLabelCache = new((i) => $"Event {i} Callback");
                }

                nameLabel = _NameLabelCache.Convert(index);
                timeLabel = _TimeLabelCache.Convert(index);
                callbackLabel = _CallbackLabelCache.Convert(index);

                defaultTime = 0;
                isEndEvent = false;
            }
        }

        /************************************************************************************************************************/

        private static void WrapEventTime(Context context, ref float normalizedTime)
        {
            var transition = context.TransitionContext.Transition;
            if (transition != null && transition.IsLooping)
            {
                if (normalizedTime == 0)
                    return;
                else if (normalizedTime % 1 == 0)
                    normalizedTime = AnimancerEvent.AlmostOne;
                else
                    normalizedTime = AnimancerUtilities.Wrap01(normalizedTime);
            }
        }

        /************************************************************************************************************************/
        #region Event Modification
        /************************************************************************************************************************/

        private static GUIStyle _AddEventStyle;
        private static GUIContent _AddEventContent;

        /// <summary>Draws a button to add a new event or remove the selected one.</summary>
        public void DoAddRemoveEventButtonGUI(Rect area, Context context)
        {
            if (ShowAddButton(context))
            {
                AnimancerIcons.IconContent(ref _AddEventContent, "Animation.AddEvent", Strings.ProOnlyTag + "Add event");

                _AddEventStyle ??= new(EditorStyles.miniButton)
                {
                    fixedHeight = 0,
                    padding = new(-1, 1, 0, 0),
                };

                if (GUI.Button(area, _AddEventContent, _AddEventStyle))
                {
                    // If the target is currently being previewed, add the event at the currently selected time.
                    var state = TransitionPreviewWindow.GetCurrentState();
                    var normalizedTime = state != null ? state.NormalizedTime : float.NaN;
                    AddEvent(context, normalizedTime, null);
                }
            }
            else
            {
                if (GUI.Button(area, AnimancerIcons.ClearIcon("Remove selected event"), NoPaddingButtonStyle))
                {
                    RemoveEvent(context, context.SelectedEvent);
                }
            }
        }

        /************************************************************************************************************************/

        private static bool ShowAddButton(Context context)
        {
            // Nothing selected = Add.
            if (context.SelectedEvent < 0)
                return true;

            // No times means no events exist = Add.
            if (context.Times.Count == 0)
                return true;

            // Regular event selected = Remove.
            if (context.SelectedEvent < context.Times.Count - 1)
                return false;

            // End has non-default time = Remove.
            if (!float.IsNaN(context.Times.GetElement(context.SelectedEvent).floatValue))
                return false;

            // End has non-empty callback = Remove.
            // If the end callback was empty, the array would have been compacted.
            if (context.Callbacks.Count == context.Times.Count)
                return false;

            // End has empty callback = Add.
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>Adds an event to the sequence represented by the given `context`.</summary>
        public static void AddEvent(
            Context context,
            float normalizedTime,
            StringAsset name)
        {
            // If the time is NaN, add it halfway between the last event and the end.

            if (context.Times.Count == 0)
            {
                // Having any events means we need the end time too.
                context.Times.Count = 2;
                context.Times.GetElement(1).floatValue = float.NaN;
                if (float.IsNaN(normalizedTime))
                    normalizedTime = 0.5f;
            }
            else
            {
                context.Times.Property.InsertArrayElementAtIndex(context.Times.Count - 1);
                context.Times.Count++;

                if (float.IsNaN(normalizedTime))
                {
                    var transition = context.TransitionContext.Transition;

                    var previousTime = context.Times.Count >= 3
                        ? context.Times.GetElement(context.Times.Count - 3).floatValue
                        : AnimancerEvent.Sequence.GetDefaultNormalizedStartTime(transition.Speed);

                    var endTime = context.Times.GetElement(context.Times.Count - 1).floatValue;
                    if (float.IsNaN(endTime))
                        endTime = AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(transition.Speed);

                    normalizedTime = previousTime < endTime
                        ? (previousTime + endTime) * 0.5f
                        : previousTime;
                }
            }

            WrapEventTime(context, ref normalizedTime);

            var newEventIndex = context.Times.Count - 2;
            context.Times.GetElement(newEventIndex).floatValue = normalizedTime;
            context.SelectedEvent = newEventIndex;

            if (context.Callbacks.Count > newEventIndex)
            {
                context.Callbacks.Property.InsertArrayElementAtIndex(newEventIndex);
                context.Callbacks.Property.serializedObject.ApplyModifiedProperties();

                // Make sure the callback starts empty rather than copying an existing value.
                var callback = context.Callbacks.GetElement(newEventIndex);
                callback.SetValue(null);
                context.Callbacks.Property.OnPropertyChanged();
            }

            if (context.Names.Count > newEventIndex || name != null)
            {
                while (context.Names.Count <= newEventIndex)
                {
                    var index = context.Names.Count++;

                    var nameProperty = context.Names.GetElement(index);
                    nameProperty.SetValue(index != newEventIndex
                        ? null
                        : name);
                }

                context.Names.Property.serializedObject.ApplyModifiedProperties();
            }

            // Update the runtime sequence accordingly.
            var events = context.Sequence?.InitializedEvents;
            events?.Add(normalizedTime, AnimancerEvent.DummyCallback);

            OptionalWarning.UselessEvent.Disable();

            if (Event.current != null)
            {
                GUI.changed = true;
                GUIUtility.ExitGUI();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Removes the event at the specified `index`.</summary>
        public static void RemoveEvent(Context context, int index)
        {
            if (context.Times.Count == 0)
                return;

            // If it's an End Event, set it to NaN.
            if (index >= context.Times.Count - 1)
            {
                context.Times.GetElement(index).floatValue = float.NaN;

                if (context.Callbacks.Count > index)
                    context.Callbacks.Count--;

                Deselect();

                // Update the runtime sequence accordingly.
                var events = context.Sequence?.InitializedEvents;
                if (events != null)
                {
                    events.EndEvent = new(float.NaN, null);
                }
            }
            else// Otherwise remove it.
            {
                context.Times.Property.DeleteArrayElementAtIndex(index);
                context.Times.Count--;

                // Update the runtime sequence accordingly.
                var events = context.Sequence?.InitializedEvents;
                events?.Remove(index);

                if (index < context.Names.Count)
                {
                    context.Names.Property.DeleteArrayElementAtIndex(index);
                    context.Names.Count--;
                }

                if (index < context.Callbacks.Count)
                {
                    context.Callbacks.Property.DeleteArrayElementAtIndex(index);
                    context.Callbacks.Count--;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sorts the events in the `context` according to their times.</summary>
        private static bool SortEvents(Context context)
        {
            if (context.Times.Count <= 2)
                return false;

            // The serializable sequence sorts itself in ISerializationCallbackReceiver.OnBeforeSerialize.
            var selectedEvent = context.SelectedEvent;
            var sorted = context.Property.serializedObject.ApplyModifiedProperties();
            if (!sorted)
                return false;

            context.Property.serializedObject.Update();
            context.Times.Refresh();
            context.Names.Refresh();
            context.Callbacks.Refresh();
            return context.SelectedEvent != selectedEvent;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region OnBeforeSerialize
        /************************************************************************************************************************/

        [InitializeOnLoadMethod]
        private static void InitializeOnBeforeSerialize()
            => SerializableSequence.OnBeforeSerialize += OnBeforeSerialize;

        private static void OnBeforeSerialize(SerializableSequence sequence)
        {
            var warnings = OptionalWarning.ProOnly.DisableTemporarily();

            var normalizedTimes = sequence.NormalizedTimes;

            warnings.Enable();

            if (normalizedTimes == null ||
                normalizedTimes.Length <= 2)
            {
                sequence.CompactArrays();
                return;
            }

            var eventContext = Context.Current;
            var selectedEvent = eventContext?.Property != null
                ? eventContext.SelectedEvent
                : -1;

            var timeCount = normalizedTimes.Length - 1;

            var previousTime = normalizedTimes[0];

            // Bubble Sort based on the normalized times.
            for (int i = 1; i < timeCount; i++)
            {
                var time = normalizedTimes[i];
                if (time >= previousTime)
                {
                    previousTime = time;
                    continue;
                }

                normalizedTimes.Swap(i, i - 1);
                DynamicSwap(ref sequence.Callbacks, i);
                DynamicSwap(ref sequence.Names, i);

                if (selectedEvent == i)
                    selectedEvent = i - 1;
                else if (selectedEvent == i - 1)
                    selectedEvent = i;

                if (i == 1)
                {
                    i = 0;
                    previousTime = float.NegativeInfinity;
                }
                else
                {
                    i -= 2;
                    previousTime = normalizedTimes[i];
                }
            }

            // If the current animation is looping, clamp all times within the 0-1 range.
            var transitionContext = TransitionDrawer.Context;
            if (transitionContext.Transition != null &&
                transitionContext.Transition.IsLooping)
            {
                for (int i = normalizedTimes.Length - 1; i >= 0; i--)
                {
                    var time = normalizedTimes[i];
                    if (time < 0)
                        normalizedTimes[i] = 0;
                    else if (time > AnimancerEvent.AlmostOne)
                        normalizedTimes[i] = AnimancerEvent.AlmostOne;
                }
            }

            // If the selected event was moved adjust the selection.
            if (eventContext?.Property != null && eventContext.SelectedEvent != selectedEvent)
            {
                eventContext.SelectedEvent = selectedEvent;
                TransitionPreviewWindow.PreviewNormalizedTime = normalizedTimes[selectedEvent];
            }

            sequence.CompactArrays();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Swaps <c>array[index]</c> with <c>array[index - 1]</c>
        /// while accounting for the possibility of the `index` being beyond the bounds of the `array`.
        /// </summary>
        private static void DynamicSwap<T>(ref T[] array, int index)
        {
            var count = array != null ? array.Length : 0;

            if (index == count)
                Array.Resize(ref array, ++count);

            if (index < count)
                array.Swap(index, index - 1);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Context
        /************************************************************************************************************************/

        /// <summary>Details of an <see cref="AnimancerEvent.Sequence.Serializable"/>.</summary>
        public class Context : IDisposable
        {
            /************************************************************************************************************************/

            /// <summary>The main property representing the <see cref="Sequence"/> field.</summary>
            public SerializedProperty Property { get; private set; }

            private SerializableSequence _Sequence;

            /// <summary>Underlying value of the <see cref="Property"/>.</summary>
            public SerializableSequence Sequence
            {
                get
                {
                    if (_Sequence == null && Property.serializedObject.targetObjects.Length == 1)
                        _Sequence = Property.GetValue<SerializableSequence>();
                    return _Sequence;
                }
            }

            /// <summary>The property representing the <see cref="SerializableSequence.NormalizedTimes"/> backing field.</summary>
            public readonly SerializedArrayProperty Times = new();

            /// <summary>The property representing the <see cref="SerializableSequence.Names"/> backing field.</summary>
            public readonly SerializedArrayProperty Names = new();

            /// <summary>The property representing the <see cref="SerializableSequence.Callbacks"/> backing field.</summary>
            public readonly SerializedArrayProperty Callbacks = new();

            /************************************************************************************************************************/

            private int _SelectedEvent;

            /// <summary>The index of the currently selected event.</summary>
            public int SelectedEvent
            {
                get => _SelectedEvent;
                set
                {
                    if (Times != null && value >= 0 && (value < Times.Count || Times.Count == 0))
                    {
                        float normalizedTime;
                        if (Times.Count > 0)
                        {
                            normalizedTime = Times.GetElement(value).floatValue;
                        }
                        else
                        {
                            var transition = TransitionContext.Transition;
                            var speed = transition != null ? transition.Speed : 1;
                            normalizedTime = AnimancerEvent.Sequence.GetDefaultNormalizedEndTime(speed);
                        }

                        TransitionPreviewWindow.PreviewNormalizedTime = normalizedTime;
                    }

                    if (_SelectedEvent == value &&
                        Callbacks != null)
                        return;

                    _SelectedEvent = value;
                    TemporarySettings.SetSelectedEvent(Callbacks.Property, value);
                }
            }

            /************************************************************************************************************************/

            /// <summary>The stack of active contexts.</summary>
            private static readonly List<Context> Stack = new();

            /// <summary>The number of active items in the <see cref="Stack"/>.</summary>
            private static int _ActiveIndex = -1;

            /// <summary>The currently active instance.</summary>
            public static Context Current { get; private set; }

            /************************************************************************************************************************/

            /// <summary>Adds a new <see cref="Context"/> representing the `property` to the stack and returns it.</summary>
            public static Context Get(SerializedProperty property)
            {
                _ActiveIndex++;

                if (_ActiveIndex >= Stack.Count)
                {
                    Current = new();
                    Stack.Add(Current);
                }
                else
                {
                    Current = Stack[_ActiveIndex];
                }

                Current.Initialize(property);
                EditorGUI.BeginChangeCheck();
                return Current;
            }

            /// <summary>Sets this <see cref="Context"/> as the <see cref="Current"/> and returns it.</summary>
            public Context SetAsCurrent()
            {
                Current = this;
                EditorGUI.BeginChangeCheck();
                return this;
            }

            /************************************************************************************************************************/

            private void Initialize(SerializedProperty property)
            {
                if (Property == property)
                    return;

                Property = property;
                _Sequence = null;

                Times.Property = property.FindPropertyRelative(SerializableSequence.NormalizedTimesField);
                Names.Property = property.FindPropertyRelative(SerializableSequence.NamesField);
                Callbacks.Property = property.FindPropertyRelative(SerializableSequence.CallbacksField);

                if (Names.Count > Times.Count)
                    Names.Count = Times.Count;
                if (Callbacks.Count > Times.Count)
                    Callbacks.Count = Times.Count;

                _SelectedEvent = TemporarySettings.GetSelectedEvent(Callbacks.Property);
                _SelectedEvent = Mathf.Min(_SelectedEvent, Mathf.Max(Times.Count - 1, 0));
            }

            /************************************************************************************************************************/

            /// <summary>[<see cref="IDisposable"/>] Calls <see cref="SerializedObject.ApplyModifiedProperties"/>.</summary>
            public void Dispose()
            {
                if ((uint)_ActiveIndex < (uint)Stack.Count &&
                    this == Stack[_ActiveIndex])
                    _ActiveIndex--;

                Stack.TryGet(_ActiveIndex, out var current);
                Current = current;

                if (EditorGUI.EndChangeCheck())
                    Property.serializedObject.ApplyModifiedProperties();

                Property = null;
                _Sequence = null;
            }

            /************************************************************************************************************************/

            /// <summary>Shorthand for <see cref="TransitionDrawer.Context"/>.</summary>
            public TransitionDrawer.DrawerContext TransitionContext
                => TransitionDrawer.Context;

            /************************************************************************************************************************/

            /// <summary>Creates a copy of this <see cref="Context"/>.</summary>
            public Context Copy()
            {
                var copy = new Context
                {
                    Property = Property,
                    _SelectedEvent = _SelectedEvent,
                };

                copy.Times.Property = Times.Property;
                copy.Names.Property = Names.Property;
                copy.Callbacks.Property = Callbacks.Property;

                return copy;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/
    #region Settings
    /************************************************************************************************************************/

    /// <summary>[Editor-Only] Settings for <see cref="SerializableEventSequenceDrawer"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/SerializableEventSequenceDrawerSettings
    [Serializable, InternalSerializableType]
    public class SerializableEventSequenceDrawerSettings : AnimancerSettingsGroup
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string DisplayName
            => "Animancer Events";

        /// <inheritdoc/>
        public override int Index
            => 4;

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("Should Animancer Event Callbacks be hidden in the Inspector?")]
        private bool _HideEventCallbacks;

        /// <summary>Should Animancer Event Callbacks be hidden in the Inspector?</summary>
        public static bool HideEventCallbacks
            => AnimancerSettingsGroup<SerializableEventSequenceDrawerSettings>.Instance._HideEventCallbacks;

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/
    #endregion
    /************************************************************************************************************************/
}

#endif

