// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using Animancer.Editor.Previews;
using Animancer.TransitionLibraries;
using System;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;
using Object = UnityEngine.Object;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only] Custom preview for <see cref="TransitionLibrarySelection"/>.</summary>
    /// <remarks>Parts of this class are based on Unity's <see cref="MeshPreview"/>.</remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibrarySelectionPreview
    [CustomPreview(typeof(TransitionLibrarySelection))]
    public class TransitionLibrarySelectionPreview : ObjectPreview
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerPreviewRenderer _PreviewRenderer;
        [SerializeField] private TransitionPreviewPlayer _PreviewPlayer;

        [NonSerialized] private TransitionLibrarySelection _Target;
        [NonSerialized] private int _TargetVersion = -1;

        [NonSerialized] private readonly TransitionLibrarySelectionPreviewSpeed Speed = new();

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Initialize(Object[] targets)
        {
            _PreviewRenderer ??= new();
            _PreviewPlayer ??= new();

            if (targets.Length == 1)
            {
                _Target = targets[0] as TransitionLibrarySelection;
                if (_Target != null)
                {
                    _TargetVersion = _Target.Version - 1;
                    if (_Target.Window != null)
                        _PreviewRenderer.PreviewObject.TrySelectBestModel(_Target.Window.Data);
                    CheckTarget();
                }
            }

            base.Initialize(targets);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Cleanup()
        {
            base.Cleanup();
            _PreviewPlayer?.Dispose();
            _PreviewPlayer = null;
            _PreviewRenderer?.Dispose();
            _PreviewRenderer = null;
        }

        /************************************************************************************************************************/

        /// <summary>Handles changes to the target object.</summary>
        private void CheckTarget()
        {
            if (_TargetVersion == _Target.Version)
                return;

            _TargetVersion = _Target.Version;
            _PreviewPlayer.IsPlaying = false;

            switch (_Target.Type)
            {
                case TransitionLibrarySelection.SelectionType.FromTransition:
                    _PreviewPlayer.FromTransition = _Target.FromTransition;
                    _PreviewPlayer.ToTransition = null;
                    break;

                case TransitionLibrarySelection.SelectionType.ToTransition:
                    _PreviewPlayer.FromTransition = null;
                    _PreviewPlayer.ToTransition = _Target.ToTransition;
                    break;

                case TransitionLibrarySelection.SelectionType.Modifier:
                    _PreviewPlayer.FromTransition = _Target.FromTransition;
                    _PreviewPlayer.ToTransition = _Target.ToTransition;
                    break;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Updates the settings of the <see cref="TransitionPreviewPlayer"/>.</summary>
        private void UpdatePlayerSettings()
        {
            _PreviewPlayer.Graph = _PreviewRenderer.PreviewObject.Graph;

            _PreviewPlayer.FadeDuration = _Target.FadeDuration;
            _PreviewPlayer.Speed = Speed.Speed;
            _PreviewPlayer.RecalculateTimeBounds();
        }

        /************************************************************************************************************************/

        private static readonly GUIContent
            Title = new("Preview");

        /// <inheritdoc/>
        public override GUIContent GetPreviewTitle()
            => Title;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool HasPreviewGUI()
            => _Target != null
            && _Target.Type switch
            {
                TransitionLibrarySelection.SelectionType.FromTransition or
                TransitionLibrarySelection.SelectionType.ToTransition or
                TransitionLibrarySelection.SelectionType.Modifier
                => true,

                _ => false,
            };

        /************************************************************************************************************************/
        #region Header Settings
        /************************************************************************************************************************/

        private static GUIStyle _ToolbarButtonStyle;

        /// <inheritdoc/>
        public override void OnPreviewSettings()
        {
            CheckTarget();

            _ToolbarButtonStyle ??= new(EditorStyles.toolbarButton)
            {
                padding = new(),
            };

            var area = GUILayoutUtility.GetRect(LineHeight * 1.5f, LineHeight);
            DoPlayPauseToggle(area, _ToolbarButtonStyle);

            area = GUILayoutUtility.GetRect(LineHeight * 2f, LineHeight);
            Speed.DoToggleGUI(area, _ToolbarButtonStyle);
        }

        /************************************************************************************************************************/

        /// <summary>Draws a toggle to play and pause the preview.</summary>
        private void DoPlayPauseToggle(Rect area, GUIStyle style)
        {
            if (TryUseClickEvent(area, 1) || TryUseClickEvent(area, 2))
                _PreviewPlayer.CurrentTime = _PreviewPlayer.MinTime;

            _PreviewPlayer.IsPlaying = AnimancerGUI.DoPlayPauseToggle(
                area,
                _PreviewPlayer.IsPlaying,
                style,
                "Left Click = Play/Pause\nRight Click = Reset Time");
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnInteractivePreviewGUI(Rect area, GUIStyle background)
        {
            if (_Target == null)
                return;

            CheckTarget();
            UpdatePlayerSettings();

            DoSettingsGUI(ref area);

            DoTimelineGUI(ref area);

            _PreviewRenderer.DoGUI(area, background);

            AnimancerPreviewObjectGUI.HandleDragAndDrop(area, _PreviewRenderer.PreviewObject);
        }

        /************************************************************************************************************************/

        /// <summary>Draws settings for modifying the preview.</summary>
        private void DoSettingsGUI(ref Rect area)
        {
            if (!Speed.IsOn)
                return;

            area.yMin += StandardSpacing;

            Speed.DoSpeedSlider(ref area, EditorStyles.toolbar);

            var preview = _PreviewRenderer.PreviewObject;
            var height = AnimancerPreviewObjectGUI.CalculateHeight(preview);
            var settingsArea = StealFromTop(ref area, height, StandardSpacing);
            settingsArea = settingsArea.Expand(-StandardSpacing, 0);

            GUI.Label(settingsArea, GUIContent.none, EditorStyles.toolbar);

            AnimancerPreviewObjectGUI.DoModelGUI(settingsArea, preview);
        }

        /************************************************************************************************************************/
        #region Timeline
        /************************************************************************************************************************/

        /// <summary>Draws the preview timeline.</summary>
        private void DoTimelineGUI(ref Rect area)
        {
            var timelineArea = StealFromTop(ref area, EditorStyles.toolbar.fixedHeight, StandardSpacing);

            EditorGUI.DrawRect(timelineArea, Grey(0.25f, 0.3f));
            EditorGUI.DrawRect(new(timelineArea.x, timelineArea.yMax - 1, timelineArea.width, 1), Grey(0, 0.5f));

            DoFadeDurationSliderGUI(timelineArea);
            DoTimeSliderGUI(timelineArea);
        }

        /************************************************************************************************************************/

        private static readonly int SliderHash = "Slider".GetHashCode();

        /************************************************************************************************************************/

        /// <summary>Draws the fade duration slider.</summary>
        private void DoFadeDurationSliderGUI(Rect area)
        {
            if (!CalculateFadeBounds(area, out var startFadeX, out var endFadeX))
                return;

            switch (_Target.Type)
            {
                default:
                    return;

                case TransitionLibrarySelection.SelectionType.FromTransition:
                case TransitionLibrarySelection.SelectionType.ToTransition:
                case TransitionLibrarySelection.SelectionType.Modifier:
                    break;
            }

            var sliderArea = area;
            sliderArea.width = LineHeight * 0.5f;
            sliderArea.x = endFadeX - sliderArea.width * 0.5f;

            var control = new GUIControl(sliderArea, SliderHash);

            switch (control.EventType)
            {
                case EventType.MouseDown:
                    if (control.TryUseMouseDown())
                        _PreviewPlayer.IsPlaying = false;
                    break;

                case EventType.MouseUp:
                    control.TryUseMouseUp();
                    break;

                case EventType.MouseDrag:
                    if (control.TryUseHotControl())
                    {
                        var x = Math.Max(startFadeX, control.Event.mousePosition.x);
                        var normalizedTime = area.InverseLerpUnclampedX(x);
                        var normalizedStartFade = area.InverseLerpUnclampedX(startFadeX);

                        _PreviewPlayer.NormalizedTime = normalizedTime;
                        var fadeDuration =
                            _PreviewPlayer.LerpTimeUnclamped(normalizedTime) -
                            _PreviewPlayer.LerpTimeUnclamped(normalizedStartFade);

                        var selected = _Target.Selected;
                        if (selected is TransitionModifierDefinition modifier)
                        {
                            _Target.Window.RecordUndo()
                                .SetModifier(modifier.WithFadeDuration(fadeDuration));
                        }
                        else if (selected is TransitionAssetBase transitionAsset)
                        {
                            if (fadeDuration < 0)
                                fadeDuration = 0;

                            using var serializedObject = new SerializedObject(transitionAsset);
                            var property = serializedObject.FindProperty(TransitionAssetBase.TransitionField);
                            property = property.FindPropertyRelative("_" + nameof(ITransition.FadeDuration));
                            property.floatValue = fadeDuration;
                            serializedObject.ApplyModifiedProperties();
                        }

                        _Target.Window.Repaint();
                    }

                    break;

                case EventType.Repaint:

                    var color = AnimancerStateDrawerColors.FadeLineColor;

                    var showCursor = GUIUtility.hotControl == 0 || GUIUtility.hotControl == control.ID;
                    if (showCursor)
                        EditorGUIUtility.AddCursorRect(sliderArea, MouseCursor.ResizeHorizontal);

                    if (!showCursor || !sliderArea.Contains(control.Event.mousePosition))
                        color.a *= 0.5f;

                    EditorGUI.DrawRect(
                        new(endFadeX, sliderArea.y, 1, sliderArea.height - 1),
                        color);

                    break;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws the preview time slider.</summary>
        private void DoTimeSliderGUI(Rect area)
        {
            var control = new GUIControl(area, SliderHash);

            switch (control.EventType)
            {
                case EventType.MouseDown:
                    if (control.TryUseMouseDown())
                    {
                        _ForceClampTime = true;
                        _DidWrapTime = false;
                        HandleDragTime(area, control.Event);

                        _ForceClampTime = control.Event.control;
                        if (!_ForceClampTime)
                            EditorGUIUtility.SetWantsMouseJumping(1);

                        _PreviewPlayer.IsPlaying = control.Event.clickCount > 1;
                    }

                    break;

                case EventType.MouseUp:
                    if (control.TryUseMouseUp())
                        EditorGUIUtility.SetWantsMouseJumping(0);
                    break;

                case EventType.MouseDrag:
                    if (control.TryUseHotControl())
                        HandleDragTime(area, control.Event);
                    break;

                case EventType.Repaint:

                    BeginTriangles(AnimancerStateDrawerColors.FadeLineColor);

                    if (CalculateFadeBounds(area, out var startFadeX, out var endFadeX))
                    {
                        // Fade.
                        DrawLineBatched(
                            new(startFadeX, area.yMin + 1),
                            new(endFadeX, area.yMax - 1),
                            1);

                        // To.
                        if (endFadeX < area.xMax)
                            DrawLineBatched(
                            new(endFadeX, area.yMax - 1),
                            new(area.xMax, area.yMax - 1),
                            1);
                    }

                    // From.
                    if (area.xMin < startFadeX)
                        DrawLineBatched(
                            new(area.xMin, area.yMin + 1),
                            new(startFadeX, area.yMin + 1),
                            1);

                    var color = _PreviewPlayer.IsPlaying
                        ? AnimancerStateDrawerColors.PlayingBarColor
                        : AnimancerStateDrawerColors.PausedBarColor;
                    color.a = 1;

                    var timeX = area.LerpUnclampedX(_PreviewPlayer.NormalizedTime);

                    GL.Color(color);
                    DrawLineBatched(new(timeX, area.yMin), new(timeX, area.yMax), 2);

                    EndTriangles();

                    DoTransitionLabels(area);
                    break;
            }
        }

        /************************************************************************************************************************/

        private bool _ForceClampTime;
        private bool _DidWrapTime;

        /// <summary>Draws handles drag events to control the preview time.</summary>
        private void HandleDragTime(Rect area, Event currentEvent)
        {
            if (_ForceClampTime)
            {
                _PreviewPlayer.NormalizedTime = area.InverseLerpUnclampedX(currentEvent.mousePosition.x);
                return;
            }

            var delta = currentEvent.delta.x;

            var normalizedTime = _PreviewPlayer.NormalizedTime;
            if (normalizedTime == 0 && !_DidWrapTime && delta > 0)
            {
                var x = currentEvent.mousePosition.x;
                if (area.xMin > x || area.xMax < x)
                    return;
            }

            normalizedTime += delta / area.width;
            if (normalizedTime >= 0 || _DidWrapTime)
            {

                if (normalizedTime > 1)
                    _DidWrapTime = true;

                normalizedTime = AnimancerUtilities.Wrap01(normalizedTime);
            }
            else
            {
                normalizedTime = 0;
            }

            _PreviewPlayer.NormalizedTime = normalizedTime;
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the start and end pixels of the fade.</summary>
        private bool CalculateFadeBounds(
            Rect area,
            out float startFadeX,
            out float endFadeX)
        {
            var fadeDuration = _Target.FadeDuration;
            if (!float.IsNaN(fadeDuration))
            {
                startFadeX = area.LerpUnclampedX(_PreviewPlayer.InverseLerpTimeUnclamped(0));

                endFadeX = area.LerpUnclampedX(_PreviewPlayer.InverseLerpTimeUnclamped(fadeDuration));

                if (_Target.FromTransition.IsValid())
                {
                    if (!_Target.ToTransition.IsValid())
                    {
                        endFadeX -= startFadeX;
                        startFadeX = area.xMin;
                    }

                    return true;
                }
                else
                {
                    if (_Target.ToTransition.IsValid())
                    {
                        return true;
                    }
                }
            }

            startFadeX = area.LerpUnclampedX(_PreviewPlayer.InverseLerpTimeUnclamped(0));
            endFadeX = startFadeX;
            return false;
        }

        /************************************************************************************************************************/

        /// <summary>Draws labels for the selected transitions.</summary>
        private void DoTransitionLabels(Rect area)
        {
            area.xMin += 1;
            area.xMax -= 2;

            var mid = area.width * 0.5f;
            var leftArea = area;
            var rightArea = area;

            var fromTransition = _Target.FromTransition;
            var toTransition = _Target.ToTransition;

            var hasFrom = fromTransition.IsValid();
            var hasTo = toTransition.IsValid();

            if (hasFrom && hasTo)
            {
                leftArea.width = mid - StandardSpacing * 0.5f;

                rightArea.x = area.xMax - leftArea.width;
                rightArea.width = leftArea.width;
            }

            if (hasFrom)
                GUI.Label(leftArea, _Target.FromTransition.GetCachedName());

            if (hasTo)
                GUI.Label(rightArea, _Target.ToTransition.GetCachedName(), RightLabelStyle);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

