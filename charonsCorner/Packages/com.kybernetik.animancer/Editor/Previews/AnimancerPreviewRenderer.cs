// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using System;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;
using Object = UnityEngine.Object;

namespace Animancer.Editor.Previews
{
    /// <summary>[Editor-Only] Utility for rendering previews of animated objects.</summary>
    /// <remarks>Parts of this class are based on Unity's <see cref="MeshPreview"/>.</remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Previews/AnimancerPreviewRenderer
    [Serializable]
    public class AnimancerPreviewRenderer :
        AnimancerPreviewObject.IEventHandler,
        IDisposable
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        [NonSerialized] private PreviewRenderUtility _PreviewRenderUtility;
        [NonSerialized] private Light[] _PreviewLights;

        [SerializeField] private AnimancerPreviewObject _PreviewObject;
        [SerializeField] private Vector3 _OrthographicPosition = new(0.5f, 0.5f, -1);
        [SerializeField] private Vector2 _PreviewDirection = new(135, -30);
        [SerializeField] private Vector2 _LightDirection = new(-40, -40);
        [SerializeField] private Vector3 _PivotPositionOffset;
        [SerializeField] private float _ZoomFactor = 1f;

        /// <summary>The root object in the preview scene.</summary>
        public Transform PreviewSceneRoot { get; private set; }

        /// <summary>
        /// An instance of the <see cref="TransitionPreviewSettings.SceneEnvironment"/>.
        /// A child of the <see cref="PreviewSceneRoot"/>.
        /// </summary>
        public GameObject EnvironmentInstance { get; private set; }

        /************************************************************************************************************************/

        /// <summary>[<see cref="SerializeField"/>] The object being previewed.</summary>
        public AnimancerPreviewObject PreviewObject
        {
            get
            {
                InitializePreviewRenderUtility();
                return AnimancerPreviewObject.Initialize(ref _PreviewObject, this, PreviewSceneRoot);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <summary>Cleans up this renderer.</summary>
        public void Dispose()
        {
            _PreviewObject?.Dispose();
            CleanupPreviewRenderUtility();
        }

        /************************************************************************************************************************/

        /// <summary>Calles when the <see cref="TransitionPreviewSettings.SceneEnvironment"/> is changed.</summary>
        private void OnEnvironmentPrefabChanged()
        {
            Object.DestroyImmediate(EnvironmentInstance);

            var prefab = TransitionPreviewSettings.SceneEnvironment;
            if (prefab != null)
                EnvironmentInstance = Object.Instantiate(prefab, PreviewSceneRoot);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        void AnimancerPreviewObject.IEventHandler.OnInstantiateObject()
        {
            _PivotPositionOffset = _PreviewObject.InstanceBounds.center;
        }

        /// <inheritdoc/>
        void AnimancerPreviewObject.IEventHandler.OnSetSelectedAnimator() { }

        /// <inheritdoc/>
        void AnimancerPreviewObject.IEventHandler.OnCreateGraph() { }

        /************************************************************************************************************************/
        #region GUI
        /************************************************************************************************************************/

        /// <summary>Draws the preview.</summary>
        public void DoGUI(Rect area, GUIStyle background)
        {
            if (!DrawIsRenderTextureSupported(area))
                return;

            var currentEvent = Event.current;

            HandleCameraControlEvent(area, currentEvent);

            if (currentEvent.type == EventType.Repaint)
                DrawPreview(area, background);
        }

        /************************************************************************************************************************/

        /// <summary>Shows a warning if the current device doesn't support render textures.</summary>
        private bool DrawIsRenderTextureSupported(Rect area)
        {
            if (ShaderUtil.hardwareSupportsRectRenderTexture)
                return true;

            EditorGUI.DropShadowLabel(
                area,
                "Device doesn't support Render Textures\nUnable to render previews");

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>Initializes the <see cref="_PreviewRenderUtility"/>.</summary>
        private void InitializePreviewRenderUtility()
        {
            if (_PreviewRenderUtility != null)
                return;

            _PreviewRenderUtility = new();
            _PreviewRenderUtility.camera.fieldOfView = 30;
            _PreviewRenderUtility.camera.allowHDR = false;
            _PreviewRenderUtility.camera.allowMSAA = false;
            _PreviewRenderUtility.ambientColor = Grey(0.1f, 0);

            _PreviewLights = _PreviewRenderUtility.lights;
            _PreviewLights[0].intensity = 1.4f;
            _PreviewLights[0].transform.rotation = Quaternion.Euler(40, 40, 0);
            _PreviewLights[1].intensity = 1.4f;

            var root = AnimancerPreviewObject.CreateEmpty(nameof(AnimancerPreviewRenderer));
            _PreviewRenderUtility.AddSingleGO(root);
            PreviewSceneRoot = root.transform;

            OnEnvironmentPrefabChanged();
        }

        /************************************************************************************************************************/

        /// <summary>Cleans up the <see cref="_PreviewRenderUtility"/>.</summary>
        private void CleanupPreviewRenderUtility()
        {
            _PreviewRenderUtility?.Cleanup();
            _PreviewRenderUtility = null;
            _PreviewLights = null;
        }

        /************************************************************************************************************************/

        /// <summary>Updates and renders the preview.</summary>
        private void DrawPreview(Rect area, GUIStyle background)
        {
            InitializePreviewRenderUtility();
            _PreviewRenderUtility.BeginPreview(area, background);
            UpdatePreviewRenderUtility();
            _PreviewRenderUtility.Render(true, true);
            _PreviewRenderUtility.EndAndDrawPreview(area);
        }

        /************************************************************************************************************************/

        /// <summary>Updates the preview rendering details.</summary>
        private void UpdatePreviewRenderUtility()
        {
            var previewObject = PreviewObject;
            if (previewObject.InstanceObject == null)
                return;

            var rotation = Quaternion.Euler(_PreviewDirection.y, 0, 0) * Quaternion.Euler(0, _PreviewDirection.x, 0);
            previewObject.InstanceObject.rotation = rotation;

            var size = previewObject.InstanceBounds.extents.magnitude;
            var position = _ZoomFactor * -4f * size * Vector3.forward + _PivotPositionOffset;

            var camera = _PreviewRenderUtility.camera;
            camera.transform.SetPositionAndRotation(position, Quaternion.identity);
            camera.nearClipPlane = 0.0001f;
            camera.farClipPlane = 1000;
            camera.orthographic = false;

            var lights = _PreviewLights;
            lights[0].intensity = 1.1f;
            lights[0].transform.rotation = Quaternion.Euler(-_LightDirection.y, -_LightDirection.x, 0);
            lights[1].intensity = 1.1f;
            lights[1].transform.rotation = Quaternion.Euler(_LightDirection.y, _LightDirection.x, 0);

            _PreviewRenderUtility.ambientColor = Grey(0.1f, 0);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Camera Controls
        /************************************************************************************************************************/

        private static readonly int CameraControlsHint = "CameraControls".GetHashCode();

        /// <summary>Handles GUI events for controlling the preview camera.</summary>
        private void HandleCameraControlEvent(Rect area, Event currentEvent)
        {
            if (currentEvent.button == 1)
            {
                if (currentEvent.alt)
                    _LightDirection = Drag2D(_LightDirection, area);// Could draw some lines to show the light directions.
                else
                    _PreviewDirection = Drag2D(_PreviewDirection, area);
            }

            var control = new GUIControl(area, currentEvent, CameraControlsHint);

            switch (control.EventType)
            {
                case EventType.ScrollWheel:
                    HandleScrollZoomEvent(area, currentEvent);
                    break;

                case EventType.MouseDown:
                    if (currentEvent.button <= 0 || currentEvent.button == 2)
                        control.TryUseMouseDown();
                    break;

                case EventType.MouseUp:
                    control.TryUseMouseUp();
                    break;

                case EventType.MouseDrag:
                    if (control.TryUseHotControl())
                        HandleDragPanEvent(area, currentEvent);
                    break;

                case EventType.ValidateCommand:
                case EventType.ExecuteCommand:
                    switch (currentEvent.commandName)
                    {
                        case Commands.FrameSelected:
                        case Commands.FrameSelectedWithLock:
                            FrameTarget();
                            currentEvent.Use();
                            break;
                    }
                    break;
            }

        }

        /************************************************************************************************************************/

        private static readonly int SliderHash = "Slider".GetHashCode();

        /// <summary>Handles drag input within a given `area`.</summary>
        /// <remarks>Copied from Unity's <see cref="PreviewGUI.Drag2D"/>.</remarks>
        public static Vector2 Drag2D(Vector2 scrollPosition, Rect area)
        {
            var control = new GUIControl(area, SliderHash);

            switch (control.EventType)
            {
                case EventType.MouseDown:
                    if (control.TryUseMouseDown() && area.width > 50)
                        EditorGUIUtility.SetWantsMouseJumping(1);

                    break;

                case EventType.MouseUp:
                    if (control.TryUseMouseUp())
                        EditorGUIUtility.SetWantsMouseJumping(0);
                    break;

                case EventType.MouseDrag:
                    if (control.TryUseHotControl())
                    {
                        var multiplier = control.Event.shift ? 3 : 1;
                        var size = Mathf.Min(area.width, area.height);
                        scrollPosition -= control.Event.delta * multiplier / size * 140;
                    }

                    break;
            }

            return scrollPosition;
        }

        /************************************************************************************************************************/

        /// <summary>Handles a mouse scroll event to zoom the preview camera.</summary>
        private void HandleScrollZoomEvent(Rect area, Event currentEvent)
        {
            var delta = HandleUtility.niceMouseDeltaZoom * -0.025f;
            var zoom = _ZoomFactor * (1 + delta);
            zoom = Mathf.Clamp(zoom, 0.1f, 10);
            var vector = new Vector2(
                currentEvent.mousePosition.x / area.width,
                1 - currentEvent.mousePosition.y / area.height);
            var origin = _PreviewRenderUtility.camera.ViewportToWorldPoint(vector);
            var direction = _OrthographicPosition - origin;
            var position = origin + direction * (zoom / _ZoomFactor);
            _PreviewRenderUtility.camera.transform.position = position;

            _ZoomFactor = zoom;
            currentEvent.Use();
        }

        /************************************************************************************************************************/

        /// <summary>Handles a mouse drag event to pan the preview camera.</summary>
        private void HandleDragPanEvent(Rect area, Event currentEvent)
        {
            var camera = _PreviewRenderUtility.camera;
            var direction = new Vector3(
                -currentEvent.delta.x * camera.pixelWidth / area.width,
                currentEvent.delta.y * camera.pixelHeight / area.height,
                0);

            var position = camera.WorldToScreenPoint(_PivotPositionOffset);
            position += direction;

            direction = camera.ScreenToWorldPoint(position) - _PivotPositionOffset;
            _PivotPositionOffset += direction;
        }

        /************************************************************************************************************************/

        /// <summary>Frames the preview object in the middle of the camera.</summary>
        private void FrameTarget()
        {
            _ZoomFactor = 1f;
            _OrthographicPosition = new(0.5f, 0.5f, -1f);
            _PivotPositionOffset = default;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

