// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Animancer.Editor.Previews
{
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Previews/TransitionPreviewWindow
    partial class TransitionPreviewWindow
    {
        /************************************************************************************************************************/

        /// <summary>The <see cref="Scene"/> of the current <see cref="TransitionPreviewWindow"/> instance.</summary>
        public static Scene InstanceScene
            => _Instance != null
            ? _Instance._Scene
            : null;

        /************************************************************************************************************************/

        /// <summary>Temporary scene management for the <see cref="TransitionPreviewWindow"/>.</summary>
        /// <remarks>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#previews">
        /// Previews</see>
        /// </remarks>
        [Serializable]
        public class Scene :
            AnimancerPreviewObject.IEventHandler
        {
            /************************************************************************************************************************/
            #region Fields and Properties
            /************************************************************************************************************************/

            /// <summary>The scene displayed by the <see cref="TransitionPreviewWindow"/>.</summary>
            [SerializeField]
            private UnityEngine.SceneManagement.Scene _Scene;

            /// <summary>The root object in the preview scene.</summary>
            public Transform PreviewSceneRoot { get; private set; }

            /// <summary>
            /// An instance of the <see cref="TransitionPreviewSettings.SceneEnvironment"/>.
            /// A child of the <see cref="PreviewSceneRoot"/>.
            /// </summary>
            public GameObject EnvironmentInstance { get; private set; }

            /************************************************************************************************************************/

            [SerializeField]
            private AnimancerPreviewObject _PreviewObject;

            /// <summary>[<see cref="SerializeField"/>] The object being previewed.</summary>
            public AnimancerPreviewObject PreviewObject
                => AnimancerPreviewObject.Initialize(ref _PreviewObject, this, PreviewSceneRoot);

            /************************************************************************************************************************/

            private Vector3 _PreviousPreviewObjectPosition;

            private Vector3 CurrentPreviewObjectPosition
            {
                get
                {
                    var animator = PreviewObject.SelectedInstanceAnimator;
                    return animator != null
                        ? animator.transform.position
                        : default;
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Initialization
            /************************************************************************************************************************/

            /// <summary>Initializes this <see cref="Scene"/>.</summary>
            public void OnEnable()
            {
                duringSceneGui += DoCustomGUI;

                CreateScene();

                PreviewObject.TrySelectBestModel(Transition);
            }

            /************************************************************************************************************************/

            private void CreateScene()
            {
                _Scene = EditorSceneManager.NewPreviewScene();
                _Scene.name = "Transition Preview";
                _Instance.customScene = _Scene;

                var root = AnimancerPreviewObject.CreateEmpty(nameof(TransitionPreviewWindow));
                PreviewSceneRoot = root.transform;
                SceneManager.MoveGameObjectToScene(root, _Scene);
                _Instance.customParentForDraggedObjects = PreviewSceneRoot;

                OnEnvironmentPrefabChanged();
            }

            /************************************************************************************************************************/

            internal void OnEnvironmentPrefabChanged()
            {
                DestroyImmediate(EnvironmentInstance);

                var prefab = TransitionPreviewSettings.SceneEnvironment;
                if (prefab != null)
                    EnvironmentInstance = Instantiate(prefab, PreviewSceneRoot);
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            void AnimancerPreviewObject.IEventHandler.OnInstantiateObject()
            {
                FocusCamera();
                _Instance._Animations.GatherAnimations();
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            void AnimancerPreviewObject.IEventHandler.OnSetSelectedAnimator()
            {
                _Instance.in2DMode = PreviewObject.SelectedInstanceType == AnimationType.Sprite;
            }

            /// <inheritdoc/>
            void AnimancerPreviewObject.IEventHandler.OnCreateGraph()
            {
                PreviewObject.Graph.RequirePostUpdate(new Animations.WindowMatchStateTime());
                _Instance._Animations.NormalizedTime = _Instance._Animations.NormalizedTime;
            }

            /************************************************************************************************************************/

            /// <summary>Called when the target transition property is changed.</summary>
            public void OnTargetPropertyChanged()
            {
                _ExpandedHierarchy?.Clear();

                var previewObject = PreviewObject;

                previewObject.OriginalObject = AnimancerUtilities.FindRoot(_Instance._TransitionProperty.TargetObject);
                previewObject.TrySelectBestModel(Transition);

                _Instance._Animations.NormalizedTime = 0;

                _Instance.in2DMode = previewObject.SelectedInstanceType == AnimationType.Sprite;
            }

            /************************************************************************************************************************/

            private void FocusCamera()
            {
                var instance = _PreviewObject.InstanceObject;
                if (instance == null)
                    return;

                var bounds = CalculateBounds(instance);

                var rotation = _Instance.in2DMode ?
                    Quaternion.identity :
                    Quaternion.Euler(15, 225, 0);

                var size = bounds.extents.magnitude * 1.2f;
                if (size == float.PositiveInfinity)
                    return;
                else if (size == 0)
                    size = 10;

                _Instance.LookAt(bounds.center, rotation, size, _Instance.in2DMode, true);
            }

            /************************************************************************************************************************/

            private static Bounds CalculateBounds(Transform transform)
            {
                if (transform == null)
                    return default;

                var renderers = transform.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                    return default;

                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                return bounds;
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Execution
            /************************************************************************************************************************/

            /// <summary>Called when the window GUI is drawn.</summary>
            public void OnGUI()
            {
                if (_PreviewObject != null &&
                    _PreviewObject.Graph != null &&
                    _PreviewObject.Graph.IsGraphPlaying)
                    AnimancerGUI.RepaintEverything();

                if (Selection.activeObject == _Instance &&
                    Event.current.type == EventType.KeyUp &&
                    Event.current.keyCode == KeyCode.F)
                    FocusCamera();
            }

            /************************************************************************************************************************/

            private void DoCustomGUI(SceneView sceneView)
            {
                FollowPreviewObject(sceneView);

                var animancer = PreviewObject.Graph;
                if (animancer == null ||
                    sceneView is not TransitionPreviewWindow instance ||
                    !AnimancerUtilities.TryGetWrappedObject(Transition, out ITransitionGUI gui) ||
                    instance._TransitionProperty == null)
                    return;

                EditorGUI.BeginChangeCheck();

                using (new TransitionDrawer.DrawerContext(instance._TransitionProperty))
                {
                    try
                    {
                        gui.OnPreviewSceneGUI(new(animancer));
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }

                if (EditorGUI.EndChangeCheck())
                    AnimancerGUI.RepaintEverything();
            }

            /************************************************************************************************************************/

            private void FollowPreviewObject(SceneView sceneView)
            {
                var currentPreviewObjectPosition = CurrentPreviewObjectPosition;
                if (_PreviousPreviewObjectPosition == currentPreviewObjectPosition)
                    return;

                sceneView.pivot += currentPreviewObjectPosition - _PreviousPreviewObjectPosition;

                _PreviousPreviewObjectPosition = currentPreviewObjectPosition;
            }

            /************************************************************************************************************************/

            /// <summary>Is the `obj` a <see cref="GameObject"/> in the preview scene?</summary>
            public bool IsSceneObject(Object obj)
                => obj is GameObject gameObject
                && gameObject.transform.IsChildOf(PreviewSceneRoot);

            /************************************************************************************************************************/

            [SerializeField]
            private List<Transform> _ExpandedHierarchy;

            /// <summary>A list of all objects with their child hierarchy expanded.</summary>
            public List<Transform> ExpandedHierarchy
                => _ExpandedHierarchy ??= new();

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Cleanup
            /************************************************************************************************************************/

            /// <summary>Called by <see cref="TransitionPreviewWindow.OnDisable"/>.</summary>
            public void OnDisable()
            {
                duringSceneGui -= DoCustomGUI;

                _PreviewObject?.Dispose();

                EditorSceneManager.ClosePreviewScene(_Scene);
            }

            /************************************************************************************************************************/

            /// <summary>Called by <see cref="TransitionPreviewWindow.OnDestroy"/>.</summary>
            public void OnDestroy()
            {
                if (PreviewSceneRoot != null)
                {
                    DestroyImmediate(PreviewSceneRoot.gameObject);
                    PreviewSceneRoot = null;
                }
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
        }
    }
}

#endif

