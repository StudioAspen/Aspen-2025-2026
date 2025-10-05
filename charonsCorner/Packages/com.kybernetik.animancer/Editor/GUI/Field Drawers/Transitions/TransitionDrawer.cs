// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using Animancer.Editor.Previews;
using Animancer.Units.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="ITransitionDetailed"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TransitionDrawer
    [CustomPropertyDrawer(typeof(ITransitionDetailed), true)]
    public class TransitionDrawer : PropertyDrawer,
        IPolymorphic
    {
        /************************************************************************************************************************/

        /// <summary>The visual state of a drawer.</summary>
        private enum Mode
        {
            Uninitialized,
            Normal,
            AlwaysExpanded,
        }

        /// <summary>The current state of this drawer.</summary>
        private Mode _Mode;

        /************************************************************************************************************************/

        /// <summary>
        /// If set, the field with this name will be drawn on the header line with the foldout arrow instead of in its
        /// regular place.
        /// </summary>
        protected readonly string MainPropertyName;

        /// <summary>"." + <see cref="MainPropertyName"/> (to avoid creating garbage repeatedly).</summary>
        protected readonly string MainPropertyPathSuffix;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TransitionDrawer"/>.</summary>
        public TransitionDrawer() { }

        /// <summary>Creates a new <see cref="TransitionDrawer"/> and sets the <see cref="MainPropertyName"/>.</summary>
        public TransitionDrawer(string mainPropertyName)
        {
            MainPropertyName = mainPropertyName;
            MainPropertyPathSuffix = "." + mainPropertyName;
        }

        /************************************************************************************************************************/

        /// <summary>Returns the property specified by the <see cref="MainPropertyName"/>.</summary>
        private SerializedProperty GetMainProperty(SerializedProperty rootProperty)
            => MainPropertyName == null
            ? null
            : rootProperty.FindPropertyRelative(MainPropertyName);

        /************************************************************************************************************************/

        /// <summary>Returns the number of vertical pixels the `property` will occupy when it is drawn.</summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            using (new DrawerContext(property))
            {
                InitializeMode(property);

                var height = EditorGUI.GetPropertyHeight(property, label, true);

                if (property.isExpanded)
                {
                    if (property.propertyType != SerializedPropertyType.ManagedReference)
                    {
                        var mainProperty = GetMainProperty(property);
                        if (mainProperty != null)
                            height -= EditorGUI.GetPropertyHeight(mainProperty) + AnimancerGUI.StandardSpacing;
                    }

                    // The End Time from the Event Sequence is drawn out in the main transition so we need to add it.
                    // But rather than figuring out which array element actually holds the end time, we just use the
                    // Start Time field since it will have the same height.
                    var startTime = property.FindPropertyRelative(NormalizedStartTimeFieldName);
                    if (startTime != null)
                        height += EditorGUI.GetPropertyHeight(startTime) + AnimancerGUI.StandardSpacing;
                }

                return height;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws the root `property` GUI and calls <see cref="DoChildPropertyGUI"/> for each of its children.</summary>
        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            InitializeMode(property);

            // Highlight the whole area if this transition is currently being previewed.
            var isPreviewing = TransitionPreviewWindow.IsPreviewing(property);
            if (isPreviewing)
            {
                var highlightArea = area;
                highlightArea.xMin -= AnimancerGUI.IndentSize;
                EditorGUI.DrawRect(highlightArea, new(0.35f, 0.5f, 1, 0.2f));
            }

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                DoObjectReferenceGUI(area, property, label);
                return;
            }

            var headerArea = area;

            if (property.propertyType == SerializedPropertyType.ManagedReference)
                DoPreviewButtonGUI(ref headerArea, property, isPreviewing);

            using (new TypeSelectionButton(headerArea, property, true))
            {
                DoPropertyGUI(area, property, label, isPreviewing);
            }
        }

        /************************************************************************************************************************/

        private readonly CachedEditor NestedEditor = new();

        private static GUIStyle _NestAreaStyle;

        private void DoObjectReferenceGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(area, property, label, property.isExpanded);

            if (property.hasMultipleDifferentValues)
                return;

            var value = property.objectReferenceValue;
            if (value == null)
                return;

            property.isExpanded = EditorGUI.Foldout(area, property.isExpanded, GUIContent.none, true);
            if (!property.isExpanded)
                return;

            const float NegativePadding = 4;
            EditorGUIUtility.labelWidth -= NegativePadding;

            if (_NestAreaStyle == null)
            {
                _NestAreaStyle = new GUIStyle(GUI.skin.box);
                var rect = _NestAreaStyle.margin;
                rect.bottom = rect.top = 0;
                _NestAreaStyle.margin = rect;
            }

            EditorGUI.indentLevel++;
            GUILayout.BeginVertical(_NestAreaStyle);

            try
            {
                NestedEditor.GetEditor(value).OnInspectorGUI();
            }
            catch (ExitGUIException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            GUILayout.EndVertical();
            EditorGUI.indentLevel--;

            EditorGUIUtility.labelWidth += NegativePadding;
        }

        /************************************************************************************************************************/

        private void DoPropertyGUI(Rect area, SerializedProperty property, GUIContent label, bool isPreviewing)
        {
            using (new DrawerContext(property))
            {
                var indent = !string.IsNullOrEmpty(label.text);

                EditorGUI.BeginChangeCheck();

                var mainProperty = GetMainProperty(property);
                DoHeaderGUI(ref area, property, mainProperty, label, isPreviewing);
                DoChildPropertiesGUI(area, property, mainProperty, indent);

                if (EditorGUI.EndChangeCheck() && isPreviewing)
                    TransitionPreviewWindow.PreviewNormalizedTime = TransitionPreviewWindow.PreviewNormalizedTime;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="_Mode"/> is <see cref="Mode.Uninitialized"/>, this method determines how it should start
        /// based on the number of properties in the `serializedObject`. If the only serialized field is an
        /// <see cref="ITransition"/> then it should start expanded.
        /// </summary>
        protected void InitializeMode(SerializedProperty property)
        {
            if (_Mode == Mode.Uninitialized)
            {
                if (property.depth > 0)
                {
                    _Mode = Mode.Normal;
                    return;
                }

                _Mode = Mode.AlwaysExpanded;

                var iterator = property.serializedObject.GetIterator();
                iterator.Next(true);

                var count = 0;
                do
                {
                    switch (iterator.propertyPath)
                    {
                        // Ignore MonoBehaviour inherited fields.
                        case "m_ObjectHideFlags":
                        case "m_Script":
                            break;

                        default:
                            count++;
                            if (count > 1)
                            {
                                _Mode = Mode.Normal;
                                return;
                            }
                            break;
                    }
                }
                while (iterator.NextVisible(false));
            }

            if (_Mode == Mode.AlwaysExpanded)
                property.isExpanded = true;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the root property of a transition with an optional main property on the same line.</summary>
        protected virtual void DoHeaderGUI(
            ref Rect area,
            SerializedProperty rootProperty,
            SerializedProperty mainProperty,
            GUIContent label,
            bool isPreviewing)
        {
            area.height = AnimancerGUI.LineHeight;
            var labelArea = area;
            AnimancerGUI.NextVerticalArea(ref area);

            if (rootProperty.propertyType != SerializedPropertyType.ManagedReference)
                DoPreviewButtonGUI(ref labelArea, rootProperty, isPreviewing);

            // Draw the Root Property after the Main Property to give better spacing between the label and field.

            // Drawing the main property might assign its details to the label so we keep our own copy.
            using (var rootLabel = PooledGUIContent.Acquire(label.text, label.tooltip))
            {
                // Main Property.

                DoMainPropertyGUI(labelArea, out labelArea, rootProperty, mainProperty);

                // Root Property.

                var propertyLabel = EditorGUI.BeginProperty(labelArea, rootLabel, rootProperty);
                EditorGUI.LabelField(labelArea, propertyLabel);
                EditorGUI.EndProperty();

                if (_Mode != Mode.AlwaysExpanded)
                {
                    var hierarchyMode = EditorGUIUtility.hierarchyMode;
                    EditorGUIUtility.hierarchyMode = true;

                    rootProperty.isExpanded =
                        EditorGUI.Foldout(labelArea, rootProperty.isExpanded, GUIContent.none, true);

                    EditorGUIUtility.hierarchyMode = hierarchyMode;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI the the target transition's main property.</summary>
        protected virtual void DoMainPropertyGUI(
            Rect area,
            out Rect labelArea,
            SerializedProperty rootProperty,
            SerializedProperty mainProperty)
        {
            labelArea = area;
            if (mainProperty == null)
                return;

            var fullArea = area;

            labelArea = AnimancerGUI.StealFromLeft(
                ref area,
                EditorGUIUtility.labelWidth,
                AnimancerGUI.StandardSpacing);

            var mainPropertyReferenceIsMissing =
                mainProperty.propertyType == SerializedPropertyType.ObjectReference &&
                mainProperty.objectReferenceValue == null;

            var hierarchyMode = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = true;

            if (rootProperty.propertyType == SerializedPropertyType.ManagedReference)
            {
                if (rootProperty.isExpanded ||
                    _Mode == Mode.AlwaysExpanded)
                {
                    EditorGUI.indentLevel++;

                    AnimancerGUI.NextVerticalArea(ref fullArea);
                    using (var label = PooledGUIContent.Acquire(mainProperty))
                        EditorGUI.PropertyField(fullArea, mainProperty, label, true);

                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                var indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                EditorGUI.PropertyField(area, mainProperty, GUIContent.none, true);

                EditorGUI.indentLevel = indentLevel;
            }

            EditorGUIUtility.hierarchyMode = hierarchyMode;

            // If the main Object reference was just assigned and all fields were at their type default,
            // reset the value to run its default constructor and field initializers then reassign the reference.
            var reference = mainProperty.objectReferenceValue;
            if (mainPropertyReferenceIsMissing && reference != null)
            {
                mainProperty.objectReferenceValue = null;
                if (Serialization.IsDefaultValueByType(rootProperty))
                    rootProperty.GetAccessor().ResetValue(rootProperty);
                mainProperty.objectReferenceValue = reference;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws a small button using the <see cref="TransitionPreviewWindow.Icon"/>.</summary>
        private static void DoPreviewButtonGUI(ref Rect area, SerializedProperty property, bool isPreviewing)
        {
            if (property.serializedObject.targetObjects.Length != 1 ||
                !TransitionPreviewWindow.CanBePreviewed(property))
                return;

            var enabled = GUI.enabled;
            var currentEvent = Event.current;
            if (currentEvent.button == 1)// Ignore Right Clicks on the Preview Button.
            {
                switch (currentEvent.type)
                {
                    case EventType.MouseDown:
                    case EventType.MouseUp:
                    case EventType.ContextClick:
                        GUI.enabled = false;
                        break;
                }
            }

            var tooltip = isPreviewing ? TransitionPreviewWindow.Inspector.CloseTooltip : "Preview this transition";

            if (DoPreviewButtonGUI(ref area, isPreviewing, tooltip))
                TransitionPreviewWindow.OpenOrClose(property);

            GUI.enabled = enabled;
        }

        /// <summary>Draws a small button using the <see cref="TransitionPreviewWindow.Icon"/>.</summary>
        public static bool DoPreviewButtonGUI(ref Rect area, bool selected, string tooltip)
        {
            var width = AnimancerGUI.LineHeight + AnimancerGUI.StandardSpacing * 2;
            var buttonArea = AnimancerGUI.StealFromRight(ref area, width, AnimancerGUI.StandardSpacing);
            buttonArea.height = AnimancerGUI.LineHeight;

            using (var content = PooledGUIContent.Acquire("", tooltip))
            {
                content.image = TransitionPreviewWindow.Icon;

                return GUI.Toggle(buttonArea, selected, content, PreviewButtonStyle) != selected;
            }
        }

        /************************************************************************************************************************/

        private static GUIStyle _PreviewButtonStyle;

        /// <summary>The style used for the button that opens the <see cref="TransitionPreviewWindow"/>.</summary>
        public static GUIStyle PreviewButtonStyle
            => _PreviewButtonStyle ??= new(AnimancerGUI.MiniButtonStyle)
            {
                padding = new(0, 0, 0, 1),
                fixedWidth = 0,
                fixedHeight = 0,
            };

        /************************************************************************************************************************/

        private void DoChildPropertiesGUI(Rect area, SerializedProperty rootProperty, SerializedProperty mainProperty, bool indent)
        {
            if (!rootProperty.isExpanded && _Mode != Mode.AlwaysExpanded)
                return;

            // Skip over the main property if it was already drawn by the header.
            if (rootProperty.propertyType == SerializedPropertyType.ManagedReference &&
                mainProperty != null)
                AnimancerGUI.NextVerticalArea(ref area);

            if (indent)
                EditorGUI.indentLevel++;

            var property = rootProperty.Copy();

            SerializedProperty eventsProperty = null;

            var depth = property.depth;
            if (property.NextVisible(true))
            {
                while (property.depth > depth)
                {
                    // Grab the Events property and draw it last.
                    var path = property.propertyPath;
                    if (eventsProperty == null && path.EndsWith("._Events"))
                    {
                        eventsProperty = property.Copy();
                    }
                    // Don't draw the main property again.
                    else if (mainProperty != null && path.EndsWith(MainPropertyPathSuffix))
                    {
                    }
                    else
                    {
                        if (eventsProperty != null)
                        {
                            var type = Context.Transition.GetType();
                            var accessor = property.GetAccessor();
                            var field = Serialization.GetField(type, accessor.Name);
                            if (field != null && field.IsDefined(typeof(DrawAfterEventsAttribute), false))
                            {
                                using (var eventsLabel = PooledGUIContent.Acquire(eventsProperty))
                                    DoChildPropertyGUI(ref area, rootProperty, eventsProperty, eventsLabel);
                                AnimancerGUI.NextVerticalArea(ref area);
                                eventsProperty = null;
                            }
                        }

                        using (var label = PooledGUIContent.Acquire(property))
                            DoChildPropertyGUI(ref area, rootProperty, property, label);
                        AnimancerGUI.NextVerticalArea(ref area);
                    }

                    if (!property.NextVisible(false))
                        break;
                }
            }

            if (eventsProperty != null)
            {
                using (var label = PooledGUIContent.Acquire(eventsProperty))
                    DoChildPropertyGUI(ref area, rootProperty, eventsProperty, label);
            }

            if (indent)
                EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Draws the `property` GUI in relation to the `rootProperty` which was passed into <see cref="OnGUI"/>.
        /// </summary>
        protected virtual void DoChildPropertyGUI(
            ref Rect area,
            SerializedProperty rootProperty,
            SerializedProperty property,
            GUIContent label)
        {
            // If we keep using the GUIContent that was passed into OnGUI then GetPropertyHeight will change it to
            // match the 'property' which we don't want.

            using (var content = PooledGUIContent.Acquire(label.text, label.tooltip))
            {
                area.height = EditorGUI.GetPropertyHeight(property, content, true);

                if (TryDoStartTimeField(ref area, rootProperty, property, content))
                    return;

                EditorGUI.PropertyField(area, property, content, true);
            }
        }

        /************************************************************************************************************************/

        /// <summary>The name of the backing field of <c>ClipTransition.NormalizedStartTime</c>.</summary>
        public const string NormalizedStartTimeFieldName = "_NormalizedStartTime";

        /// <summary>
        /// If the `property` is a "Start Time" field, this method draws it as well as the "End Time" below it and
        /// returns true.
        /// </summary>
        public static bool TryDoStartTimeField(
            ref Rect area,
            SerializedProperty rootProperty,
            SerializedProperty property,
            GUIContent label)
        {
            if (!property.propertyPath.EndsWith("." + NormalizedStartTimeFieldName))
                return false;

            // Start Time.
            label.text = "Start Time";
            AnimationTimeAttributeDrawer.NextDefaultValue =
                AnimancerEvent.Sequence.GetDefaultNormalizedStartTime(Context.Transition.Speed);
            EditorGUI.PropertyField(area, property, label, false);

            AnimancerGUI.NextVerticalArea(ref area);

            // End Time.
            var events = rootProperty.FindPropertyRelative("_Events");
            using (var context = SerializableEventSequenceDrawer.Context.Get(events))
            {
                var areaCopy = area;
                var index = Mathf.Max(0, context.Times.Count - 1);
                SerializableEventSequenceDrawer.DoTimeGUI(ref areaCopy, context, index, true);
            }

            return true;
        }

        /************************************************************************************************************************/
        #region Context
        /************************************************************************************************************************/

        /// <summary>The current <see cref="DrawerContext"/>.</summary>
        public static DrawerContext Context { get; private set; }

        /************************************************************************************************************************/

        /// <summary>Details used to draw an <see cref="ITransition"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer.Editor/DrawerContext
        public readonly struct DrawerContext : IDisposable
        {
            /************************************************************************************************************************/

            /// <summary>The stack of active contexts.</summary>
            public static readonly List<DrawerContext> Stack = new();

            /************************************************************************************************************************/

            /// <summary>The main property representing the <see cref="ITransition"/> field.</summary>
            public readonly SerializedProperty Property;

            /// <summary>The actual transition object rerieved from the <see cref="Property"/>.</summary>
            public readonly ITransitionDetailed Transition;

            /// <summary>The cached value of <see cref="ITransitionDetailed.MaximumDuration"/>.</summary>
            public readonly float MaximumDuration;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="DrawerContext"/>.</summary>
            /// <remarks>Be sure to <see cref="Dispose"/> it when done.</remarks>
            public DrawerContext(
                SerializedProperty transitionProperty)
            {
                Property = transitionProperty;
                Transition = transitionProperty.GetValue<ITransitionDetailed>();
                AnimancerUtilities.TryGetLength(Transition, out MaximumDuration);

                EditorGUI.BeginChangeCheck();

                Stack.Add(this);
                Context = this;
            }

            /************************************************************************************************************************/

            /// <summary>Applies any modified properties and decrements the stack.</summary>
            public void Dispose()
            {
                Debug.Assert(
                    Transition == Context.Transition,
                    $"{nameof(DrawerContext)}.{nameof(Dispose)}" +
                    $" must be called in the reverse order in which instances were created." +
                    $" Recommended: using (new DrawerContext(property)) to ensure correct disposal.");

                if (EditorGUI.EndChangeCheck())
                    Property.serializedObject.ApplyModifiedProperties();

                Stack.RemoveAt(Stack.Count - 1);

                Context = Stack.Count > 0
                    ? Stack[^1]
                    : default;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

