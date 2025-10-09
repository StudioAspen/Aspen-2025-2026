// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using System;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;
using Object = UnityEngine.Object;

namespace Animancer.Editor.Previews
{
    /// <summary>[Editor-Only]
    /// An interactive preview which displays the internal details of an <see cref="AnimancerComponent"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Previews/AnimancerComponentPreview
    [CustomPreview(typeof(AnimancerComponent))]
    public class AnimancerComponentPreview : ObjectPreview
    {
        /************************************************************************************************************************/

        private static readonly GUIContent
            Title = new(nameof(Animancer));

        /// <inheritdoc/>
        public override GUIContent GetPreviewTitle()
            => Title;

        /************************************************************************************************************************/

        [NonSerialized] private IAnimancerComponent _Animancer;
        [NonSerialized] private UnityEditor.Editor _Editor;

        /// <summary>The drawer for the <see cref="IAnimancerComponent.Graph"/>.</summary>
        private readonly AnimancerGraphDrawer
            GraphDrawer = new();

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Initialize(Object[] targets)
        {
            _Animancer = targets.Length == 1
                ? targets[0] as IAnimancerComponent
                : null;

            _Editor = UnityEditor.Editor.CreateEditor(targets);

            base.Initialize(targets);

            EditorApplication.update += Update;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Cleanup()
        {
            EditorApplication.update -= Update;

            Object.DestroyImmediate(_Editor);

            base.Cleanup();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool HasPreviewGUI()
            => !_Animancer.IsNullOrDestroyed()
            && _Animancer.IsGraphInitialized;

        /************************************************************************************************************************/

        private static GUIStyle _ToolbarButtonStyle;

        /// <inheritdoc/>
        public override void OnPreviewSettings()
        {
            base.OnPreviewSettings();

            _ToolbarButtonStyle ??= new(EditorStyles.toolbarButton)
            {
                padding = new(),
            };

            var graph = _Animancer.Graph;

            if (!graph.IsGraphPlaying)
            {
                var stepArea = GUILayoutUtility.GetRect(LineHeight * 1.5f, LineHeight);
                AnimancerGraphControls.DoFrameStepButton(stepArea, graph, _ToolbarButtonStyle);
            }

            var area = GUILayoutUtility.GetRect(LineHeight * 1.5f, LineHeight);
            AnimancerGraphControls.DoPlayPauseToggle(area, graph, _ToolbarButtonStyle);

            area = GUILayoutUtility.GetRect(LineHeight * 2f, LineHeight);
            AnimancerGraphSpeedSlider.Instance.Graph = graph;
            AnimancerGraphSpeedSlider.Instance.DoToggleGUI(area, _ToolbarButtonStyle);
        }

        /************************************************************************************************************************/

        [NonSerialized]
        private static GUIStyle _PaddingStyle;

        [NonSerialized]
        private Rect _Area;

        [SerializeField]
        private Vector2 _ScrollPosition;

        /// <inheritdoc/>
        public override void OnInteractivePreviewGUI(Rect area, GUIStyle background)
        {
            _PaddingStyle ??= new()
            {
                padding = new((int)StandardSpacing, (int)StandardSpacing, (int)StandardSpacing, (int)StandardSpacing),
            };

            // The area isn't properly set during Layout events so remember it after each Repaint.

            if (area.y == 0)
                area.y = EditorStyles.toolbar.fixedHeight + 1;

            if (Event.current.type == EventType.Repaint)
                _Area = area;

            // Draw the graph.

            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth += IndentSize;

            GUILayout.BeginArea(_Area);
            _ScrollPosition = GUILayout.BeginScrollView(_ScrollPosition, _PaddingStyle);

            GraphDrawer.DoGUI(_Animancer);

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            EditorGUIUtility.labelWidth = labelWidth;

            _LastRepaintTime = EditorApplication.timeSinceStartup;
        }

        /************************************************************************************************************************/

        [NonSerialized]
        private double _LastRepaintTime = double.NegativeInfinity;

        /// <summary>Repaints the preview if necessary.</summary>
        private void Update()
        {
            if (!HasPreviewGUI() ||
                !UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                return;

            var targetDeltaTime = 1f / AnimancerComponentPreviewSettings.RepaintRate;
            var nextRepaintTime = _LastRepaintTime + targetDeltaTime;

            if (EditorApplication.timeSinceStartup > nextRepaintTime)
                _Editor.Repaint();

            // This seems to be the least hacky way to repaint only the Inspector window.
            // Ideally an interactive preview would have a way to repaint itself.
        }

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/
    #region Settings
    /************************************************************************************************************************/

    /// <summary>[Editor-Only] Settings for <see cref="AnimancerComponentPreview"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Previews/AnimancerComponentPreviewSettings
    [Serializable, InternalSerializableType]
    public class AnimancerComponentPreviewSettings : AnimancerSettingsGroup
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string DisplayName
            => "Live Inspector";

        /// <inheritdoc/>
        public override int Index
            => 1;

        /************************************************************************************************************************/

        [SerializeField, Range(1, 100)]
        [Tooltip("The target frame rate of repaint commands (FPS)")]
        private float _RepaintRate = 30;

        /// <summary>The target frame rate of repaint commands (FPS).</summary>
        public static float RepaintRate
            => AnimancerSettingsGroup<AnimancerComponentPreviewSettings>.Instance._RepaintRate;

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/
    #endregion
    /************************************************************************************************************************/
}

#endif
