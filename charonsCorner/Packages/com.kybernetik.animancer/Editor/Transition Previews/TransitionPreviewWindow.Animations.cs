// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using System;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;

namespace Animancer.Editor.Previews
{
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Previews/TransitionPreviewWindow
    partial class TransitionPreviewWindow
    {
        /// <summary>Animation details for the <see cref="TransitionPreviewWindow"/>.</summary>
        /// <remarks>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#previews">
        /// Previews</see>
        /// </remarks>
        [Serializable]
        internal class Animations
        {
            /************************************************************************************************************************/

            public const string
                PreviousAnimationKey = "Previous Animation",
                NextAnimationKey = "Next Animation";

            /************************************************************************************************************************/

            [NonSerialized] private AnimationClip[] _OtherAnimations;

            [SerializeField]
            private AnimationClip _PreviousAnimation;
            public AnimationClip PreviousAnimation => _PreviousAnimation;

            [SerializeField]
            private AnimationClip _NextAnimation;
            public AnimationClip NextAnimation => _NextAnimation;

            /************************************************************************************************************************/

            private static AnimancerPreviewObject PreviewObject
                => _Instance._Scene.PreviewObject;

            /************************************************************************************************************************/

            public void DoGUI()
            {
                GUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Preview Details", "(Not Serialized)");

                var previewObject = PreviewObject;
                AnimancerPreviewObjectGUI.DoModelGUI(previewObject);

                using (var label = PooledGUIContent.Acquire("Previous Animation",
                    "The animation for the preview to play before the target transition"))
                {
                    DoAnimationFieldGUI(label, ref _PreviousAnimation, (clip) => _PreviousAnimation = clip);
                }

                var graph = previewObject.Graph;
                DoCurrentAnimationGUI(graph);

                using (var label = PooledGUIContent.Acquire("Next Animation",
                    "The animation for the preview to play after the target transition"))
                {
                    DoAnimationFieldGUI(label, ref _NextAnimation, (clip) => _NextAnimation = clip);
                }

                if (graph != null)
                {
                    using (new EditorGUI.DisabledScope(!Transition.IsValid()))
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        if (graph.IsGraphPlaying)
                        {
                            if (CompactMiniButton(AnimancerIcons.PauseIcon))
                                graph.PauseGraph();
                        }
                        else
                        {
                            if (CompactMiniButton(AnimancerIcons.StepBackwardIcon))
                                StepBackward();

                            if (CompactMiniButton(AnimancerIcons.PlayIcon))
                                PlaySequence(graph);

                            if (CompactMiniButton(AnimancerIcons.StepForwardIcon))
                                StepForward();
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndVertical();
            }

            /************************************************************************************************************************/

            public void GatherAnimations()
            {
                AnimationGatherer.GatherFromGameObject(
                    PreviewObject.OriginalObject.gameObject,
                    ref _OtherAnimations,
                    true);

                if (_OtherAnimations.Length > 0 &&
                    (_PreviousAnimation == null || _NextAnimation == null))
                {
                    var defaultClip = _OtherAnimations[0];
                    var defaultClipIsIdle = false;

                    for (int i = 0; i < _OtherAnimations.Length; i++)
                    {
                        var clip = _OtherAnimations[i];

                        if (defaultClipIsIdle && clip.name.Length > defaultClip.name.Length)
                            continue;

                        if (clip.name.IndexOf("idle", StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            defaultClip = clip;
                            break;
                        }
                    }

                    if (_PreviousAnimation == null)
                        _PreviousAnimation = defaultClip;
                    if (_NextAnimation == null)
                        _NextAnimation = defaultClip;
                }
            }

            /************************************************************************************************************************/

            private void DoAnimationFieldGUI(GUIContent label, ref AnimationClip clip, Action<AnimationClip> setClip)
            {
                var showDropdown = !_OtherAnimations.IsNullOrEmpty();

                var area = LayoutSingleLineRect();
                if (DoDropdownObjectFieldGUI(area, label, showDropdown, ref clip))
                {
                    var menu = new GenericMenu();

                    menu.AddItem(new("None"), clip == null, () => setClip(null));

                    for (int i = 0; i < _OtherAnimations.Length; i++)
                    {
                        var animation = _OtherAnimations[i];
                        menu.AddItem(new(animation.name), animation == clip, () => setClip(animation));
                    }

                    menu.ShowAsContext();
                }
            }

            /************************************************************************************************************************/

            private void DoCurrentAnimationGUI(AnimancerGraph animancer)
            {
                string text;

                if (animancer != null)
                {
                    var transition = Transition;
                    if (transition.IsValid() && transition.Key != null)
                        text = animancer.States.GetOrCreate(transition).ToString();
                    else
                        text = transition?.ToString();
                }
                else
                {
                    text = _Instance._TransitionProperty.Property.GetFriendlyPath();
                }

                if (text != null)
                    EditorGUILayout.LabelField("Current Animation", text);
            }

            /************************************************************************************************************************/

            private void PlaySequence(AnimancerGraph animancer)
            {
                if (_PreviousAnimation != null && _PreviousAnimation.length > 0)
                {
                    PreviewObject.Graph.Stop();
                    var fromState = animancer.States.GetOrCreate(PreviousAnimationKey, _PreviousAnimation, true);
                    animancer.Layers[0].Play(fromState);
                    OnPlayAnimation();
                    fromState.TimeD = 0;

                    var warnings = OptionalWarning.UnsupportedEvents.DisableTemporarily();
                    fromState.Events(this).EndEvent = new(1 / fromState.Length, PlayTransition);
                    warnings.Enable();
                }
                else
                {
                    PlayTransition();
                }

                PreviewObject.Graph.UnpauseGraph();
            }

            private void PlayTransition()
            {
                var transition = Transition;
                var animancer = PreviewObject.Graph;
                animancer.States.TryGet(transition, out var oldState);

                var targetState = animancer.Layers[0].Play(transition);
                OnPlayAnimation();

                if (oldState != null && oldState != targetState)
                    oldState.Destroy();

                var warnings = OptionalWarning.UnsupportedEvents.DisableTemporarily();
                targetState.Events(this).OnEnd = () =>
                {
                    if (_NextAnimation != null)
                    {
                        var fadeDuration = AnimancerEvent.GetFadeOutDuration(
                            targetState,
                            AnimancerGraph.DefaultFadeDuration);
                        PlayOther(NextAnimationKey, _NextAnimation, 0, fadeDuration);
                        OnPlayAnimation();
                    }
                    else
                    {
                        animancer.Layers[0].IncrementCommandCount();
                    }
                };
                warnings.Enable();
            }

            /************************************************************************************************************************/

            /// <summary>Does nothing.</summary>
            /// <remarks>
            /// Doesn't trigger <see cref="OptionalWarning.UselessEvent"/>
            /// (unlike <see cref="AnimancerEvent.DummyCallback"/>).
            /// </remarks>
            public static readonly Action
                DummyCallback = () => { };

            public void OnPlayAnimation()
            {
                var animancer = PreviewObject.Graph;
                if (animancer == null ||
                    animancer.States.Current == null)
                    return;

                var state = animancer.States.Current;

                state.RecreatePlayableRecursive();

                if (state.HasEvents)
                {
                    var warnings = OptionalWarning.UnsupportedEvents | OptionalWarning.ProOnly;
                    warnings = warnings.DisableTemporarily();

                    var events = state.Events(this);
                    events.OnEnd = DummyCallback;

                    for (int i = events.Count - 1; i >= 0; i--)
                        events.SetCallback(i, DummyCallback);

                    warnings.Enable();
                }
            }

            /************************************************************************************************************************/

            private void StepBackward()
                => StepTime(-TransitionPreviewSettings.FrameStep);

            private void StepForward()
                => StepTime(TransitionPreviewSettings.FrameStep);

            private void StepTime(float timeOffset)
            {
                if (!TryShowTransitionPaused(out _, out _, out var state))
                    return;

                var length = state.Length;
                if (length != 0)
                    timeOffset /= length;

                NormalizedTime += timeOffset;
            }

            /************************************************************************************************************************/

            [SerializeField]
            private float _NormalizedTime;

            public float NormalizedTime
            {
                get => _NormalizedTime;
                set
                {
                    if (!value.IsFinite())
                        return;

                    _NormalizedTime = value;

                    if (!TryShowTransitionPaused(out var animancer, out var transition, out var state))
                        return;

                    var length = state.Length;
                    var speed = state.Speed;
                    var time = value * length;
                    var fadeDuration = transition.FadeDuration * Math.Abs(speed);

                    var startTime = TimelineGUI.GetStartTime(transition.NormalizedStartTime, speed, length);
                    var normalizedEndTime = state.NormalizedEndTime;
                    var endTime = normalizedEndTime * length;
                    var fadeOutEnd = TimelineGUI.GetFadeOutEnd(speed, endTime, length);

                    if (speed < 0)
                    {
                        time = length - time;
                        startTime = length - startTime;
                        value = 1 - value;
                        normalizedEndTime = 1 - normalizedEndTime;
                        endTime = length - endTime;
                        fadeOutEnd = length - fadeOutEnd;
                    }

                    if (time < startTime)// Previous animation.
                    {
                        if (_PreviousAnimation != null)
                        {
                            PlayOther(PreviousAnimationKey, _PreviousAnimation, value);
                            value = 0;
                        }
                    }
                    else if (time < startTime + fadeDuration)// Fade from previous animation to the target.
                    {
                        if (_PreviousAnimation != null)
                        {
                            var fromState = PlayOther(PreviousAnimationKey, _PreviousAnimation, value);

                            state.IsPlaying = true;
                            state.Weight = (time - startTime) / fadeDuration;
                            fromState.Weight = 1 - state.Weight;
                        }
                    }
                    else if (_NextAnimation != null)
                    {
                        if (value < normalizedEndTime)
                        {
                            // Just the main state.
                        }
                        else
                        {
                            var toState = PlayOther(NextAnimationKey, _NextAnimation, value - normalizedEndTime);

                            if (time < fadeOutEnd)// Fade from the target transition to the next animation.
                            {
                                state.IsPlaying = true;
                                toState.Weight = (time - endTime) / (fadeOutEnd - endTime);
                                state.Weight = 1 - toState.Weight;
                            }
                            // Else just the next animation.
                        }
                    }

                    if (speed < 0)
                        value = 1 - value;

                    state.MoveTime(state.Weight > 0 ? value : 0, true);
                    animancer.Evaluate();

                    RepaintEverything();
                }
            }

            /************************************************************************************************************************/

            private bool TryShowTransitionPaused(
                out AnimancerGraph animancer, out ITransitionDetailed transition, out AnimancerState state)
            {
                animancer = PreviewObject.Graph;
                transition = Transition;

                if (animancer == null || !transition.IsValid())
                {
                    state = null;
                    return false;
                }

                state = animancer.Layers[0].Play(transition, 0);
                OnPlayAnimation();
                animancer.PauseGraph();
                return true;
            }

            /************************************************************************************************************************/

            private AnimancerState PlayOther(
                object key,
                AnimationClip animation,
                float normalizedTime,
                float fadeDuration = 0)
            {
                var animancer = PreviewObject.Graph;
                var state = animancer.States.GetOrCreate(key, animation, true);
                state = animancer.Layers[0].Play(state, fadeDuration);
                OnPlayAnimation();

                normalizedTime *= state.Length;
                state.Time = normalizedTime.IsFinite() ? normalizedTime : 0;

                return state;
            }

            /************************************************************************************************************************/

            internal class WindowMatchStateTime : Updatable
            {
                /************************************************************************************************************************/

                public override void Update()
                {
                    if (_Instance == null ||
                        !AnimancerGraph.Current.IsGraphPlaying)
                        return;

                    var transition = Transition;
                    if (transition == null)
                        return;

                    if (AnimancerGraph.Current.States.TryGet(transition, out var state))
                        _Instance._Animations._NormalizedTime = state.NormalizedTime;
                }

                /************************************************************************************************************************/

                public override string ToString()
                    => nameof(WindowMatchStateTime);

                /************************************************************************************************************************/
            }

            /************************************************************************************************************************/
        }
    }
}

#endif

