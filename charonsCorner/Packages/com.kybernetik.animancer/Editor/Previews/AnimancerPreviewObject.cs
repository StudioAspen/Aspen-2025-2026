// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor.Previews
{
    /// <summary>[Editor-Only] Manages the selection and instantiation of models for previewing animations.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Previews/AnimancerPreviewObject
    [Serializable]
    public class AnimancerPreviewObject : IDisposable
    {
        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Handles events from an <see cref="AnimancerPreviewObject"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Previews/IEventHandler
        public interface IEventHandler
        {
            /// <summary>Called after the <see cref="InstanceObject"/> is instantiated.</summary>
            void OnInstantiateObject();

            /// <summary>Called after the <see cref="SelectedInstanceAnimator"/> is changed.</summary>
            void OnSetSelectedAnimator();

            /// <summary>Called after the <see cref="Graph"/> is initialized.</summary>
            void OnCreateGraph();
        }

        /// <summary>An optional listener for this object's events.</summary>
        [field: NonSerialized] public IEventHandler EventHandler { get; set; }

        /************************************************************************************************************************/

        [SerializeField]
        private Transform _OriginalObject;

        /// <summary>[<see cref="SerializeField"/>]
        /// The original model which was instantiated to create the <see cref="InstanceObject"/>.
        /// </summary>
        public Transform OriginalObject
        {
            get => _OriginalObject;
            set
            {
                _OriginalObject = value;
                InstantiateObject();

                if (value != null)
                    TransitionPreviewSettings.AddModel(value.gameObject);
            }
        }

        /************************************************************************************************************************/

        /// <summary>The object to instantiate the <see cref="InstanceObject"/> under.</summary>
        [field: NonSerialized]
        public Transform InstanceRoot { get; private set; }

        /// <summary>The preview copy of the <see cref="OriginalObject"/>.</summary>
        [field: NonSerialized]
        public Transform InstanceObject { get; private set; }

        /// <summary>The bounds of the <see cref="InstanceObject"/>.</summary>
        [field: NonSerialized]
        public Bounds InstanceBounds { get; private set; }

        /************************************************************************************************************************/

        /// <summary>The <see cref="Animator"/>s on the <see cref="InstanceObject"/> and its children.</summary>
        [field: NonSerialized]
        public Animator[] InstanceAnimators { get; private set; }

        /// <summary>The type of the <see cref="SelectedInstanceAnimator"/>.</summary>
        [field: NonSerialized]
        public AnimationType SelectedInstanceType { get; private set; }

        [SerializeField]
        private int _SelectedInstanceAnimator;

        /// <summary>The <see cref="Animator"/> component currently being used for the preview.</summary>
        public Animator SelectedInstanceAnimator
        {
            get
            {
                if (InstanceAnimators.IsNullOrEmpty())
                    return null;

                if (_SelectedInstanceAnimator >= InstanceAnimators.Length)
                    _SelectedInstanceAnimator = InstanceAnimators.Length - 1;

                return InstanceAnimators[_SelectedInstanceAnimator];
            }
        }

        /************************************************************************************************************************/

        [NonSerialized]
        private AnimancerGraph _Graph;

        /// <summary>The <see cref="AnimancerGraph"/> being used for the preview.</summary>
        public AnimancerGraph Graph
        {
            get
            {
                if ((_Graph == null || !_Graph.IsValidOrDispose()) &&
                    InstanceObject != null)
                {
                    _Graph = null;

                    var animator = SelectedInstanceAnimator;
                    if (animator != null)
                    {
                        AnimancerGraph.SetNextGraphName($"{animator.name} (Animancer Preview)");
                        _Graph = new AnimancerGraph();
                        _Graph.Initialize(
                            new DummyAnimancerComponent(animator, _Graph));
                        EventHandler?.OnCreateGraph();
                    }
                }

                return _Graph;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="AnimancerPreviewObject"/>
        /// and calls <see cref="Initialize(Transform)"/>.
        /// </summary>
        public static AnimancerPreviewObject Initialize(
            ref AnimancerPreviewObject preview,
            IEventHandler eventHandler,
            Transform instanceRoot)
        {
            preview ??= new();
            preview.EventHandler = eventHandler;
            preview.Initialize(instanceRoot);
            return preview;
        }

        /************************************************************************************************************************/

        [NonSerialized] private bool _HasInitialized;

        /// <summary>Sets the <see cref="InstanceRoot"/> for this preview to work under.</summary>
        public void Initialize(Transform instanceRoot)
        {
            if (InstanceRoot != instanceRoot)
            {
                DestroyInstanceObject();
                InstanceRoot = instanceRoot;
            }

            if (InstanceObject == null)
                InstantiateObject();

            if (_HasInitialized)
                return;

            _HasInitialized = true;

            EditorSceneManager.sceneOpening += OnSceneOpening;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        /************************************************************************************************************************/

        /// <summary>Cleans up this preview.</summary>
        public void Dispose()
        {
            EditorSceneManager.sceneOpening -= OnSceneOpening;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        /************************************************************************************************************************/

        /// <summary>Called when entering or exiting Play Mode to destroy the <see cref="InstanceObject"/>.</summary>
        private void OnPlayModeChanged(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.ExitingPlayMode:
                    DestroyInstanceObject();
                    break;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Called when opening a scene to destroy the <see cref="InstanceObject"/>.</summary>
        private void OnSceneOpening(string path, OpenSceneMode mode)
        {
            if (mode == OpenSceneMode.Single)
                DestroyInstanceObject();
        }

        /************************************************************************************************************************/

        /// <summary>Destroys and re-instantiates the <see cref="InstanceObject"/>.</summary>
        private void InstantiateObject()
        {
            if (AnimancerEditorUtilities.IsChangingPlayMode)
                return;

            DestroyInstanceObject();

            if (_OriginalObject == null ||
                InstanceRoot == null)
                return;

            InstanceRoot.gameObject.SetActive(false);
            InstanceObject = Object.Instantiate(_OriginalObject, InstanceRoot);
            InstanceObject.localPosition = default;
            InstanceObject.name = _OriginalObject.name;

            InstanceBounds = AnimancerEditorUtilities.CalculateBounds(InstanceObject);

            DisableUnnecessaryComponents(InstanceObject.gameObject);

            InstanceAnimators = InstanceObject.GetComponentsInChildren<Animator>();
            for (int i = 0; i < InstanceAnimators.Length; i++)
            {
                var animator = InstanceAnimators[i];
                animator.enabled = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.fireEvents = false;
                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.applyRootMotion = true;
            }

            InstanceRoot.gameObject.SetActive(true);

            SetSelectedAnimator(_SelectedInstanceAnimator);

            EventHandler?.OnInstantiateObject();
        }

        /************************************************************************************************************************/

        /// <summary>Disables all unnecessary components on the `root` or its children.</summary>
        private static void DisableUnnecessaryComponents(GameObject root)
        {
            var behaviours = root.GetComponentsInChildren<Behaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];

                // Other undesirable components aren't Behaviours anyway: Transform, MeshFilter, Renderer.
                if (behaviour is Animator)
                    continue;

                var type = behaviour.GetType();
                if (type.IsDefined(typeof(ExecuteAlways), true) ||
                    type.IsDefined(typeof(ExecuteInEditMode), true))
                    continue;

                behaviour.enabled = false;
                behaviour.hideFlags |= HideFlags.NotEditable;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sets the <see cref="SelectedInstanceAnimator"/>.</summary>
        public void SetSelectedAnimator(int index)
        {
            DestroyGraph();

            var animator = SelectedInstanceAnimator;
            if (animator != null && animator.enabled)
            {
                animator.Rebind();
                animator.enabled = false;
                return;
            }

            _SelectedInstanceAnimator = index;

            animator = SelectedInstanceAnimator;
            if (animator != null)
            {
                animator.enabled = true;
                SelectedInstanceType = AnimationBindings.GetAnimationType(animator);
            }
            else
            {
                SelectedInstanceType = default;
            }

            EventHandler?.OnSetSelectedAnimator();
        }

        /************************************************************************************************************************/

        /// <summary>Destroys the <see cref="InstanceObject"/>.</summary>
        public void DestroyInstanceObject()
        {
            DestroyGraph();

            if (InstanceObject == null)
                return;

            Object.DestroyImmediate(InstanceObject.gameObject);
            InstanceObject = null;
            InstanceAnimators = null;
        }

        /************************************************************************************************************************/

        /// <summary>Destroys the <see cref="Graph"/>.</summary>
        private void DestroyGraph()
        {
            if (_Graph == null)
                return;

            _Graph.Destroy();
            _Graph = null;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="TransitionPreviewSettings.TrySelectBestModel(object, Transform)"/>
        /// if there is no <see cref="OriginalObject"/> yet.
        /// </summary>
        public void TrySelectBestModel(object animationClipSource)
        {
            if (OriginalObject == null)
                OriginalObject = TransitionPreviewSettings.TrySelectBestModel(animationClipSource, InstanceRoot);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates an object using <see cref="HideFlags.HideAndDontSave"/>
        /// without <see cref="HideFlags.NotEditable"/>.
        /// </summary>
        public static GameObject CreateEmpty(string name)
            => EditorUtility.CreateGameObjectWithHideFlags(
                name,
                HideFlags.HideInHierarchy | HideFlags.DontSave);

        /************************************************************************************************************************/
    }
}

#endif

