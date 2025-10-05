// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor.Previews
{
    /// <summary>Persistent settings for the <see cref="TransitionPreviewWindow"/>.</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#previews">
    /// Previews</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Previews/TransitionPreviewSettings
    [Serializable, InternalSerializableType]
    public class TransitionPreviewSettings : AnimancerSettingsGroup
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string DisplayName
            => "Transition Previews";

        /// <inheritdoc/>
        public override int Index
            => 3;

        /************************************************************************************************************************/

        private static TransitionPreviewSettings Instance
            => AnimancerSettingsGroup<TransitionPreviewSettings>.Instance;

        /************************************************************************************************************************/

        /// <summary>Draws the Inspector GUI for these settings.</summary>
        public static void DoInspectorGUI()
        {
            AnimancerSettings.SerializedObject.Update();

            EditorGUI.indentLevel++;

            DoMiscGUI();
            DoEnvironmentGUI();
            DoModelsGUI();
            DoHierarchyGUI();

            EditorGUI.indentLevel--;

            AnimancerSettings.SerializedObject.ApplyModifiedProperties();
        }

        /************************************************************************************************************************/
        #region Misc
        /************************************************************************************************************************/

        private static void DoMiscGUI()
        {
            Instance.DoPropertyField(nameof(_AutoClose));
        }

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("Should this window automatically close if the target object is destroyed?")]
        private bool _AutoClose = true;

        /// <summary>Should this window automatically close if the target object is destroyed?</summary>
        public static bool AutoClose
            => Instance._AutoClose;

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("Should the scene lighting be enabled?")]
        private bool _SceneLighting = false;

        /// <summary>Should the scene lighting be enabled?</summary>
        public static bool SceneLighting
        {
            get => Instance._SceneLighting;
            set
            {
                if (SceneLighting == value)
                    return;

                var property = Instance.GetSerializedProperty(nameof(_SceneLighting));
                property.boolValue = value;
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("Should the skybox be visible?")]
        private bool _ShowSkybox = false;

        /// <summary>Should the skybox be visible?</summary>
        public static bool ShowSkybox
        {
            get => Instance._ShowSkybox;
            set
            {
                if (ShowSkybox == value)
                    return;

                var property = Instance.GetSerializedProperty(nameof(_ShowSkybox));
                property.boolValue = value;
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        /************************************************************************************************************************/

        [SerializeField]
        [Seconds(Rule = Validate.Value.IsNotNegative)]
        [DefaultValue(0.02f)]
        [Tooltip("The amount of time that will be added by a single frame step")]
        private float _FrameStep = 0.02f;

        /// <summary>The amount of time that will be added by a single frame step (in seconds).</summary>
        public static float FrameStep
            => AnimancerSettingsGroup<TransitionPreviewSettings>.Instance._FrameStep;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Environment
        /************************************************************************************************************************/

        [SerializeField]
        [Tooltip("If set, the default preview scene lighting will be replaced with this prefab.")]
        private GameObject _SceneEnvironment;

        /// <summary>If set, the default preview scene lighting will be replaced with this prefab.</summary>
        public static GameObject SceneEnvironment
            => Instance._SceneEnvironment;

        /************************************************************************************************************************/

        private static void DoEnvironmentGUI()
        {
            EditorGUI.BeginChangeCheck();

            var property = Instance.DoPropertyField(nameof(_SceneEnvironment));

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                TransitionPreviewWindow.InstanceScene.OnEnvironmentPrefabChanged();
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Models
        /************************************************************************************************************************/

        private static void DoModelsGUI()
        {
            var property = ModelsProperty;

            var area = AnimancerGUI.LayoutSingleLineRect();

            var valueArea = area;
            valueArea.xMin = EditorGUIUtility.labelWidth - 10;

            area.xMax = valueArea.xMin - AnimancerGUI.StandardSpacing;

            EditorGUI.BeginChangeCheck();
            var count = property.arraySize = EditorGUI.DelayedIntField(valueArea, property.arraySize);
            if (EditorGUI.EndChangeCheck())
                property.isExpanded = true;

            HandleModelDragAndDrop(area);

            if (count == 0)
                return;

            property.isExpanded = EditorGUI.Foldout(area, property.isExpanded, nameof(Models), true);
            if (!property.isExpanded)
                return;

            EditorGUI.indentLevel++;

            var model = property.GetArrayElementAtIndex(0);
            for (int i = 0; i < count; i++)
            {
                GUILayout.BeginHorizontal();

                EditorGUILayout.ObjectField(model);

                if (GUILayout.Button(AnimancerIcons.ClearIcon("Remove model"), AnimancerGUI.NoPaddingButtonStyle))
                {
                    Serialization.RemoveArrayElement(property, i);
                    property.serializedObject.ApplyModifiedProperties();

                    AnimancerGUI.Deselect();
                    GUIUtility.ExitGUI();
                    return;
                }

                GUILayout.Space(EditorStyles.objectField.margin.right);
                GUILayout.EndHorizontal();
                model.Next(false);
            }

            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        private static DragAndDropHandler<GameObject> _ModelDropHandler;

        private static void HandleModelDragAndDrop(Rect area)
        {
            _ModelDropHandler ??= (gameObject, isDrop) =>
            {
                if (!EditorUtility.IsPersistent(gameObject) ||
                    Models.Contains(gameObject) ||
                    gameObject.GetComponentInChildren<Animator>() == null)
                    return false;

                if (isDrop)
                {
                    var modelsProperty = ModelsProperty;
                    modelsProperty.serializedObject.Update();

                    var i = modelsProperty.arraySize;
                    modelsProperty.arraySize = i + 1;
                    modelsProperty.GetArrayElementAtIndex(i).objectReferenceValue = gameObject;
                    modelsProperty.serializedObject.ApplyModifiedProperties();
                }

                return true;
            };

            _ModelDropHandler.Handle(area);
        }

        /************************************************************************************************************************/

        [SerializeField]
        private List<GameObject> _Models;

        /// <summary>The models previously used in the <see cref="TransitionPreviewWindow"/>.</summary>
        /// <remarks>Accessing this property removes missing and duplicate models from the list.</remarks>
        public static List<GameObject> Models
        {
            get
            {
                var instance = Instance;
                AnimancerEditorUtilities.RemoveMissingAndDuplicates(ref instance._Models);
                return instance._Models;
            }
        }

        private static SerializedProperty ModelsProperty
            => Instance.GetSerializedProperty(nameof(_Models));

        /************************************************************************************************************************/

        /// <summary>Adds a `model` to the list of preview models.</summary>
        public static void AddModel(GameObject model)
        {
            if (model == GetOrCreateDefaultHumanoid(null) ||
                model == GetOrCreateDefaultSprite(null))
                return;

            if (EditorUtility.IsPersistent(model))
            {
                AddModel(Models, model);
                AnimancerSettings.SetDirty();
            }
            else
            {
                AddModel(TemporarySettings.PreviewModels, model);
            }
        }

        private static void AddModel(List<GameObject> models, GameObject model)
        {
            // Remove if it was already there so that when we add it, it will be moved to the end.
            var index = models.LastIndexOf(model);// Search backwards because it's more likely to be near the end.
            if (index >= 0 && index < models.Count)
                models.RemoveAt(index);

            models.Add(model);
        }

        /************************************************************************************************************************/

        private static GameObject _DefaultHumanoid;

        /// <summary>
        /// Returns the default preview object for Humanoid animations
        /// if it has already been loaded.
        /// </summary>
        public static GameObject GetDefaultHumanoidIfAlreadyLoaded()
            => _DefaultHumanoid;

        /// <summary>Returns the default preview object for Humanoid animations.</summary>
        /// <remarks>A `parent` is only required if Animancer's or Unity's default objects fail to load.</remarks>
        public static GameObject GetOrCreateDefaultHumanoid(Transform parent)
        {
            if (_DefaultHumanoid != null)
                return _DefaultHumanoid;

            // Try to load Animancer Humanoid.
            var path = AssetDatabase.GUIDToAssetPath("f976ca0fb1329b44a8bc3dcca706751a");
            if (!string.IsNullOrEmpty(path))
            {
                _DefaultHumanoid = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (_DefaultHumanoid != null)
                    return _DefaultHumanoid;
            }

            // Otherwise try to load Unity's DefaultAvatar.
            _DefaultHumanoid = EditorGUIUtility.Load("Avatar/DefaultAvatar.fbx") as GameObject;

            if (_DefaultHumanoid != null)
                return _DefaultHumanoid;

            if (parent == null)
                return null;

            // Otherwise just create an empty object.
            _DefaultHumanoid = EditorUtility.CreateGameObjectWithHideFlags(
                "DummyAvatar",
                HideFlags.HideAndDontSave,
                typeof(Animator));
            _DefaultHumanoid.transform.parent = parent;
            return _DefaultHumanoid;
        }

        /************************************************************************************************************************/

        private static GameObject _DefaultSprite;

        /// <summary>
        /// Returns the default preview object for <see cref="Sprite"/> animations
        /// if it has already been created.
        /// </summary>
        public static GameObject GetDefaultSpriteIfAlreadyCreated()
            => _DefaultSprite;

        /// <summary>Returns the default preview object for <see cref="Sprite"/> animations.</summary>
        /// <remarks>A `parent` is required to create the object.</remarks>
        public static GameObject GetOrCreateDefaultSprite(Transform parent)
        {
            if (_DefaultSprite == null && parent != null)
            {
                _DefaultSprite = EditorUtility.CreateGameObjectWithHideFlags(
                    "DefaultSprite",
                    HideFlags.HideAndDontSave,
                    typeof(Animator),
                    typeof(SpriteRenderer));
                _DefaultSprite.transform.parent = parent;
            }

            return _DefaultSprite;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Tries to choose the most appropriate model to use
        /// based on the properties animated by the `animationClipSource`.
        /// </summary>
        public static Transform TrySelectBestModel(
            object animationClipSource,
            Transform parent)
        {
            if (animationClipSource.IsNullOrDestroyed())
                return null;

            using (SetPool<AnimationClip>.Instance.Acquire(out var clips))
            {
                clips.GatherFromSource(animationClipSource);
                if (clips.Count == 0)
                    return null;

                var model = TrySelectBestModel(clips, TemporarySettings.PreviewModels);
                if (model != null)
                    return model;

                model = TrySelectBestModel(clips, Models);
                if (model != null)
                    return model;

                foreach (var clip in clips)
                {
                    var type = AnimationBindings.GetAnimationType(clip);
                    switch (type)
                    {
                        case AnimationType.Humanoid:
                            return GetOrCreateDefaultHumanoid(parent).transform;

                        case AnimationType.Sprite:
                            return GetOrCreateDefaultSprite(parent).transform;
                    }
                }

                return null;
            }
        }

        /************************************************************************************************************************/

        private static Transform TrySelectBestModel(HashSet<AnimationClip> clips, List<GameObject> models)
        {
            var animatableBindings = new HashSet<EditorCurveBinding>[models.Count];

            for (int i = 0; i < models.Count; i++)
            {
                animatableBindings[i] = AnimationBindings.GetBindings(models[i]).ObjectBindings;
            }

            var bestMatchIndex = -1;
            var bestMatchCount = 0;
            foreach (var clip in clips)
            {
                var clipBindings = AnimationBindings.GetBindings(clip);

                for (int iModel = animatableBindings.Length - 1; iModel >= 0; iModel--)
                {
                    var modelBindings = animatableBindings[iModel];
                    var matches = 0;

                    for (int iBinding = 0; iBinding < clipBindings.Length; iBinding++)
                    {
                        if (modelBindings.Contains(clipBindings[iBinding]))
                            matches++;
                    }

                    if (bestMatchCount < matches && matches > clipBindings.Length / 2)
                    {
                        bestMatchCount = matches;
                        bestMatchIndex = iModel;

                        // If it matches all bindings, use it.
                        if (bestMatchCount == clipBindings.Length)
                            goto FoundBestMatch;
                    }
                }
            }

            FoundBestMatch:
            if (bestMatchIndex >= 0)
                return models[bestMatchIndex].transform;
            else
                return null;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Scene Hierarchy
        /************************************************************************************************************************/

        private static void DoHierarchyGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Preview Scene Hierarchy");
            DoHierarchyGUI(TransitionPreviewWindow.InstanceScene.PreviewSceneRoot);
            GUILayout.EndVertical();
        }

        /************************************************************************************************************************/

        private static GUIStyle _HierarchyButtonStyle;

        private static void DoHierarchyGUI(Transform root)
        {
            var area = AnimancerGUI.LayoutSingleLineRect();

            _HierarchyButtonStyle ??= new(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
            };

            if (GUI.Button(EditorGUI.IndentedRect(area), root.name, _HierarchyButtonStyle))
            {
                Selection.activeTransform = root;
                GUIUtility.ExitGUI();
            }

            var childCount = root.childCount;
            if (childCount == 0)
                return;

            var expandedHierarchy = TransitionPreviewWindow.InstanceScene.ExpandedHierarchy;
            var index = expandedHierarchy != null ? expandedHierarchy.IndexOf(root) : -1;
            var isExpanded = EditorGUI.Foldout(area, index >= 0, GUIContent.none);
            if (isExpanded)
            {
                if (index < 0)
                    expandedHierarchy.Add(root);

                EditorGUI.indentLevel++;
                for (int i = 0; i < childCount; i++)
                    DoHierarchyGUI(root.GetChild(i));
                EditorGUI.indentLevel--;
            }
            else if (index >= 0)
            {
                expandedHierarchy.RemoveAt(index);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

