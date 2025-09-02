// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Playables;
using static Animancer.Editor.AnimancerGUI;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="IAnimancerComponent.Graph"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerGraphDrawer
    /// 
    public class AnimancerGraphDrawer
    {
        /************************************************************************************************************************/

        /// <summary>The currently drawing instance.</summary>
        public static AnimancerGraphDrawer Current { get; private set; }

        /// <summary>A lazy list of information about the layers currently being displayed.</summary>
        private readonly List<AnimancerLayerDrawer>
            LayerDrawers = new();

        /// <summary>The number of elements in <see cref="LayerDrawers"/> that are currently being used.</summary>
        private int _LayerCount;

        /************************************************************************************************************************/

        /// <summary>Draws the GUI of the <see cref="IAnimancerComponent.Graph"/> if there is only one target.</summary>
        public void DoGUI(IAnimancerComponent[] targets)
        {
            if (targets.Length != 1)
                return;

            DoGUI(targets[0]);
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI of the <see cref="IAnimancerComponent.Graph"/>.</summary>
        public void DoGUI(IAnimancerComponent target)
        {
            Current = this;

            DoNativeAnimatorControllerGUI(target);

            if (!target.IsGraphInitialized)
            {
                DoGraphNotInitializedGUI(target);
                return;
            }

            GUILayout.BeginVertical();

            var hierarchyMode = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = true;

            EditorGUI.BeginChangeCheck();

            var graph = target.Graph;

            // Gather the during the layout event and use the same ones during subsequent events to avoid GUI errors
            // in case they change (they shouldn't, but this is also more efficient).
            if (Event.current.type == EventType.Layout)
            {
                AnimancerLayerDrawer.GatherLayerEditors(graph, LayerDrawers, out _LayerCount);
                GatherMainObjectUsage(graph);
            }

            AnimancerGraphControls.DoGraphGUI(graph, out var area);
            CheckContextMenu(area, graph);

            for (int i = 0; i < _LayerCount; i++)
                LayerDrawers[i].DoGUI();

            DoOrphanStatesGUI(graph);

            GUILayout.Space(StandardSpacing);

            DoLayerWeightWarningGUI(target);

            ParameterDictionaryDrawer.DoParametersGUI(graph);
            NamedEventDictionaryDrawer.DoEventsGUI(graph);

            if (ShowInternalDetails)
                DoInternalDetailsGUI(graph);

            if (EditorGUI.EndChangeCheck() && !graph.IsGraphPlaying)
                graph.Evaluate();

            DoMultipleAnimationSystemWarningGUI(target);

            EditorGUIUtility.hierarchyMode = hierarchyMode;

            GUILayout.EndVertical();

            AnimancerLayerDrawer.HandleDragAndDropToPlay(GUILayoutUtility.GetLastRect(), graph);

            Current = null;
        }

        /************************************************************************************************************************/

        /// <summary>Draws a GUI for the <see cref="Animator.runtimeAnimatorController"/> if there is one.</summary>
        private void DoNativeAnimatorControllerGUI(IAnimancerComponent target)
        {
            if (!EditorApplication.isPlaying &&
                !target.IsGraphInitialized)
                return;

            var animator = target.Animator;
            if (animator == null)
                return;

            var controller = animator.runtimeAnimatorController;
            if (controller == null)
                return;

            BeginVerticalBox(GUI.skin.box);

            var label = "Native Animator Controller";

            EditorGUI.BeginChangeCheck();
            controller = DoObjectFieldGUI(label, controller, false);
            if (EditorGUI.EndChangeCheck())
                animator.runtimeAnimatorController = controller;

            if (controller is AnimatorController editorController)
            {
                var layers = editorController.layers;
                for (int i = 0; i < layers.Length; i++)
                {
                    var layer = layers[i];

                    var runtimeState = animator.IsInTransition(i) ?
                        animator.GetNextAnimatorStateInfo(i) :
                        animator.GetCurrentAnimatorStateInfo(i);

                    var states = layer.stateMachine.states;
                    var editorState = GetState(states, runtimeState.shortNameHash);

                    var area = LayoutSingleLineRect(SpacingMode.Before);

                    var weight = i == 0 ? 1 : animator.GetLayerWeight(i);

                    string stateName;
                    if (editorState != null)
                    {
                        stateName = editorState.GetCachedName();

                        var isLooping = editorState.motion != null && editorState.motion.isLooping;
                        AnimancerStateDrawer<ClipState>.DoTimeHighlightBarGUI(
                            area,
                            true,
                            weight,
                            runtimeState.normalizedTime * runtimeState.length,
                            runtimeState.speed,
                            runtimeState.length,
                            isLooping);
                    }
                    else
                    {
                        stateName = "State Not Found";
                    }

                    DoWeightLabel(ref area, weight, weight);

                    EditorGUI.LabelField(area, layer.name, stateName);
                }
            }

            EndVerticalBox(GUI.skin.box);
        }

        /************************************************************************************************************************/

        /// <summary>Returns the state with the specified <see cref="AnimatorState.nameHash"/>.</summary>
        private static AnimatorState GetState(ChildAnimatorState[] states, int nameHash)
        {
            for (int i = 0; i < states.Length; i++)
            {
                var state = states[i].state;
                if (state.nameHash == nameHash)
                {
                    return state;
                }
            }

            return null;
        }

        /************************************************************************************************************************/

        private void DoGraphNotInitializedGUI(IAnimancerComponent target)
        {
            if (!EditorApplication.isPlaying ||
                target.Animator == null ||
                EditorUtility.IsPersistent(target.Animator))
                return;

            EditorGUILayout.HelpBox("Animancer is not initialized." +
                " It will be initialized automatically when something uses it, such as playing an animation.",
                 MessageType.Info);

            if (TryUseClickEventInLastRect(1))
            {
                var menu = new GenericMenu();

                menu.AddItem(new("Initialize"), false, () => target.Graph.Evaluate());

                AnimancerEditorUtilities.AddDocumentationLink(menu, "Layer Documentation", Strings.DocsURLs.Layers);

                menu.ShowAsContext();
            }
        }

        /************************************************************************************************************************/

        private readonly AnimancerLayerDrawer OrphanStatesDrawer = new();

        private void DoOrphanStatesGUI(AnimancerGraph graph)
        {
            var states = OrphanStatesDrawer.ActiveStates;
            states.Clear();
            foreach (var state in graph.States)
                if (state.Parent == null)
                    states.Add(state);

            if (states.Count > 0)
            {
                ApplySortStatesByName(states);

                OrphanStatesDrawer.DoStatesGUI("Orphans", states);
            }
        }

        /************************************************************************************************************************/

        private void DoLayerWeightWarningGUI(IAnimancerComponent target)
        {
            if (_LayerCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "No layers have been created, which likely means no animations have been played yet.",
                    MessageType.Warning);

                if (GUILayout.Button("Create Base Layer"))
                    target.Graph.Layers.Count = 1;

                return;
            }

            if (!target.gameObject.activeInHierarchy ||
                !target.enabled ||
                (target.Animator != null && target.Animator.runtimeAnimatorController != null))
                return;

            if (_LayerCount == 1)
            {
                var layer = LayerDrawers[0].Value;
                if (layer.Weight == 0)
                    EditorGUILayout.HelpBox(
                        layer + " is at 0 weight, which likely means no animations have been played yet.",
                        MessageType.Warning);
                return;
            }

            for (int i = 0; i < _LayerCount; i++)
            {
                var layer = LayerDrawers[i].Value;
                if (layer.Weight == 1 &&
                    !layer.IsAdditive &&
                    layer._Mask == null &&
                    Mathf.Approximately(layer.GetTotalChildWeight(), 1))
                    return;
            }

            EditorGUILayout.HelpBox(
                "There are no Override layers at weight 1, which will likely give undesirable results." +
                " Click here for more information.",
                MessageType.Warning);

            if (TryUseClickEventInLastRect())
                EditorUtility.OpenWithDefaultApp(Strings.DocsURLs.Layers + "#blending");
        }

        /************************************************************************************************************************/

        private void DoMultipleAnimationSystemWarningGUI(IAnimancerComponent target)
        {
            const string OnlyOneSystemWarning =
                "This is not supported. Each object can only be controlled by one system at a time.";

            using (ListPool<IAnimancerComponent>.Instance.Acquire(out var animancers))
            {
                target.gameObject.GetComponents(animancers);
                if (animancers.Count > 1)
                {
                    for (int i = 0; i < animancers.Count; i++)
                    {
                        var other = animancers[i];
                        if (other != target && other.Animator == target.Animator)
                        {
                            EditorGUILayout.HelpBox(
                                $"There are multiple {nameof(IAnimancerComponent)}s trying to control the target" +
                                $" {nameof(Animator)}. {OnlyOneSystemWarning}",
                                MessageType.Warning);

                            break;
                        }
                    }
                }
            }

            if (target.Animator.TryGetComponent<Animation>(out _))
            {
                EditorGUILayout.HelpBox(
                    $"There is a Legacy {nameof(Animation)} component on the same object as the target" +
                    $" {nameof(Animator)}. {OnlyOneSystemWarning}",
                    MessageType.Warning);
            }
        }

        /************************************************************************************************************************/

        private static readonly BoolPref
            ArePreUpdatablesExpanded = new(KeyPrefix + nameof(ArePreUpdatablesExpanded), false),
            ArePostUpdatablesExpanded = new(KeyPrefix + nameof(ArePostUpdatablesExpanded), false),
            AreDisposablesExpanded = new(KeyPrefix + nameof(AreDisposablesExpanded), false);

        /// <summary>Draws a box describing the internal details of the `graph`.</summary>
        private void DoInternalDetailsGUI(AnimancerGraph graph)
        {
            EditorGUI.indentLevel++;

            DoGroupDetailsGUI(graph.PreUpdatables, "Pre-Updatables", ArePreUpdatablesExpanded);
            DoGroupDetailsGUI(graph.PostUpdatables, "Post-Updatables", ArePostUpdatablesExpanded);
            DoGroupDetailsGUI(graph.Disposables, "Disposables", AreDisposablesExpanded);

            EditorGUI.indentLevel--;
        }

        /// <summary>Draws the `items`.</summary>
        private static void DoGroupDetailsGUI<T>(IReadOnlyList<T> items, string groupName, BoolPref isExpanded)
        {
            var count = items.Count;

            isExpanded.Value = DoLabelFoldoutFieldGUI(groupName, count.ToStringCached(), isExpanded);

            EditorGUI.indentLevel++;

            if (isExpanded)
                for (int i = 0; i < count; i++)
                    DoDetailsGUI(items[i]);

            EditorGUI.indentLevel--;
        }

        /// <summary>Draws the details of the `item`.</summary>
        private static void DoDetailsGUI(object item)
        {
            if (item is AnimancerNode node)
            {
                var area = LayoutSingleLineRect(SpacingMode.Before);
                area = EditorGUI.IndentedRect(area);

                var field = new FastObjectField();
                field.Set(node, node.GetPath(), FastObjectField.GetIcon(node));
                field.Draw(area);
                return;
            }

            var gui = CustomGUIFactory.GetOrCreateForObject(item);
            if (gui != null)
            {
                gui.DoGUI();
                return;
            }

            EditorGUILayout.LabelField(item.ToString());
        }

        /************************************************************************************************************************/
        #region Main Object Lookup
        /************************************************************************************************************************/

        private readonly Dictionary<Object, bool>
            MainObjectDuplicateUsage = new();

        /************************************************************************************************************************/

        /// <summary>Is the given `mainObject` used as the <see cref="AnimancerState.MainObject"/> of multiple states?</summary>
        public bool IsMainObjectUsedMultipleTimes(Object mainObject)
            => MainObjectDuplicateUsage.TryGetValue(mainObject, out var duplicate)
            && duplicate;

        /************************************************************************************************************************/

        private void GatherMainObjectUsage(AnimancerGraph graph)
        {
            MainObjectDuplicateUsage.Clear();

            var layers = graph.Layers;
            var layerCount = layers.Count;

            for (int iLayer = 0; iLayer < layerCount; iLayer++)
            {
                var layer = layers[iLayer];
                var childCount = layer.ChildCount;
                for (int iState = 0; iState < childCount; iState++)
                {
                    var state = layer.GetChild(iState);
                    var mainObject = state.MainObject;
                    if (mainObject == null)
                        continue;

                    if (MainObjectDuplicateUsage.TryGetValue(mainObject, out var duplicate))
                    {
                        if (!duplicate)
                            MainObjectDuplicateUsage[mainObject] = true;
                    }
                    else
                    {
                        MainObjectDuplicateUsage.Add(mainObject, false);
                    }
                }
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Context Menu
        /************************************************************************************************************************/

        /// <summary>
        /// Checks if the current event is a context menu click within the `clickArea`
        /// and opens a context menu with various functions for the `graph`.
        /// </summary>
        private void CheckContextMenu(Rect clickArea, AnimancerGraph graph)
        {
            if (!TryUseClickEvent(clickArea, 1))
                return;

            var menu = new GenericMenu();

            menu.AddDisabledItem(new(graph._PlayableGraph.GetEditorName() ?? "Unnamed Graph"), false);
            menu.AddDisabledItem(new("Frame ID: " + graph.FrameID), false);
            AddDisposablesFunctions(menu, graph.Disposables);

            AddUpdateModeFunctions(menu, graph);
            AnimancerNodeBase.AddContextMenuIK(menu, graph);

            AddRootFunctions(menu, graph);

            menu.AddSeparator("");

            AddDisplayOptions(menu);

            menu.AddItem(new("Log Details Of Everything"), false,
                () => Debug.Log(graph.GetDescription(), graph.Component as Object));
            AddPlayableGraphVisualizerFunction(menu, "", graph._PlayableGraph);

            AnimancerEditorUtilities.AddDocumentationLink(menu, "Inspector Documentation", Strings.DocsURLs.Inspector);

            menu.ShowAsContext();
        }

        /************************************************************************************************************************/

        /// <summary>Adds functions for controlling the `graph`.</summary>
        public static void AddRootFunctions(GenericMenu menu, AnimancerGraph graph)
        {
            menu.AddFunction("Add Layer",
                graph.Layers.Count < AnimancerLayerList.DefaultCapacity,
                () => graph.Layers.Count++);
            menu.AddFunction("Remove Layer",
                graph.Layers.Count > 0,
                () => graph.Layers.Count--);

            menu.AddItem(new("Keep Children Connected ?"),
                graph.KeepChildrenConnected,
                () => graph.SetKeepChildrenConnected(!graph.KeepChildrenConnected));
        }

        /************************************************************************************************************************/

        /// <summary>Adds menu functions to set the <see cref="DirectorUpdateMode"/>.</summary>
        private void AddUpdateModeFunctions(GenericMenu menu, AnimancerGraph graph)
        {
            var modes = Enum.GetValues(typeof(DirectorUpdateMode));
            for (int i = 0; i < modes.Length; i++)
            {
                var mode = (DirectorUpdateMode)modes.GetValue(i);
                menu.AddItem(new("Update Mode/" + mode), graph.UpdateMode == mode,
                    () => graph.UpdateMode = mode);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Adds disabled items for each disposable.</summary>
        private void AddDisposablesFunctions(GenericMenu menu, List<IDisposable> disposables)
        {
            var prefix = $"{nameof(AnimancerGraph.Disposables)}: {disposables.Count}";
            if (disposables.Count == 0)
            {
                menu.AddDisabledItem(new(prefix), false);
            }
            else
            {
                prefix += "/";
                for (int i = 0; i < disposables.Count; i++)
                {
                    menu.AddDisabledItem(new(prefix + disposables[i]), false);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Adds a menu function to open the Playable Graph Visualiser if it exists in the project.</summary>
        public static void AddPlayableGraphVisualizerFunction(GenericMenu menu, string prefix, PlayableGraph graph)
        {
            var type = Type.GetType(
                "GraphVisualizer.PlayableGraphVisualizerWindow, Unity.PlayableGraphVisualizer.Editor");

            menu.AddFunction(prefix + "Playable Graph Visualizer", type != null, () =>
            {
                var window = EditorWindow.GetWindow(type);

                var field = type.GetField("m_CurrentGraph", AnimancerReflection.AnyAccessBindings);

                field?.SetValue(window, graph);
            });
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Prefs
        /************************************************************************************************************************/

        internal const string
            KeyPrefix = "Inspector/",
            MenuPrefix = "Display Options/";

        internal static readonly BoolPref
            SortStatesByName = new(KeyPrefix, MenuPrefix + "Sort States By Name", true),
            SeparateActiveFromInactiveStates = new(KeyPrefix, MenuPrefix + "Separate Active From Inactive States", false),
            ShowInactiveStates = new(KeyPrefix, MenuPrefix + "Show Inactive States", true),
            ShowSingleLayerHeader = new(KeyPrefix, MenuPrefix + "Show Single Layer Header", false),
            ShowEvents = new(KeyPrefix, MenuPrefix + "Show Events", true),
            ShowInternalDetails = new(KeyPrefix, MenuPrefix + "Show Internal Details", false),
            ShowAddAnimation = new(KeyPrefix, MenuPrefix + "Show 'Add Animation' Field", false),
            RepaintConstantly = new(KeyPrefix, MenuPrefix + "Repaint Constantly", true),
            ScaleTimeBarByWeight = new(KeyPrefix, MenuPrefix + "Scale Time Bar by Weight", true),
            VerifyAnimationBindings = new(KeyPrefix, MenuPrefix + "Verify Animation Bindings", true),
            AutoNormalizeWeights = new(KeyPrefix, MenuPrefix + "Auto Normalize Weights", true),
            UseNormalizedTimeSliders = new("Inspector", nameof(UseNormalizedTimeSliders), false);

        /************************************************************************************************************************/

        /// <summary>Adds functions to the `menu` for each of the Display Options.</summary>
        public static void AddDisplayOptions(GenericMenu menu)
        {
            RepaintConstantly.AddToggleFunction(menu);
            SortStatesByName.AddToggleFunction(menu);
            SeparateActiveFromInactiveStates.AddToggleFunction(menu);
            ShowInactiveStates.AddToggleFunction(menu);
            ShowSingleLayerHeader.AddToggleFunction(menu);
            ShowEvents.AddToggleFunction(menu);
            ShowInternalDetails.AddToggleFunction(menu);
            ShowAddAnimation.AddToggleFunction(menu);
            ScaleTimeBarByWeight.AddToggleFunction(menu);
            VerifyAnimationBindings.AddToggleFunction(menu);
            AutoNormalizeWeights.AddToggleFunction(menu);
        }

        /************************************************************************************************************************/

        /// <summary>Sorts the `states` if <see cref="SortStatesByName"/> is enabled.</summary>
        public static void ApplySortStatesByName(List<AnimancerState> states)
        {
            if (SortStatesByName)
                states.Sort((x, y) => x.ToString().CompareTo(y.ToString()));
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

