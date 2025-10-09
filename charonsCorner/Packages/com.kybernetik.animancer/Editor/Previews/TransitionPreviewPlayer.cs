// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor.Previews
{
    /// <summary>[Editor-Only] Utility for playing through transition previews.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Previews/TransitionPreviewPlayer
    [Serializable]
    public class TransitionPreviewPlayer : IDisposable
    {
        /************************************************************************************************************************/

        [SerializeReference] private ITransition _FromTransition;
        [SerializeReference] private ITransition _ToTransition;

        /// <summary>The animation to play first.</summary>
        public ref ITransition FromTransition
            => ref _FromTransition;

        /// <summary>The animation to transition into.</summary>
        public ref ITransition ToTransition
            => ref _ToTransition;

        /************************************************************************************************************************/

        private AnimancerGraph _Graph;

        /// <summary>The graph used to play the animations.</summary>
        public AnimancerGraph Graph
        {
            get => _Graph;
            set
            {
                _Graph = value;
                Evaluate();
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The minimum amount of time to play the <see cref="FromTransition"/>
        /// before the <see cref="ToTransition"/> starts (in seconds).
        /// </summary>
        public float FromDuration { get; set; }

        /// <summary>
        /// The minimum amount of time to continue playing
        /// after the <see cref="ToTransition"/> started (in seconds).
        /// </summary>
        public float ToDuration { get; set; }

        /// <summary>The speed at which the preview plays.</summary>
        public float Speed { get; set; } = 1;

        /************************************************************************************************************************/

        private float _FadeDuration = float.NaN;

        /// <summary>The <see cref="ITransition.FadeDuration"/>.</summary>
        /// <remarks><see cref="float.NaN"/> uses the value from the <see cref="ToTransition"/>.</remarks>
        public float FadeDuration
        {
            get => float.IsNaN(_FadeDuration) && ToTransition.IsValid()
                ? ToTransition.FadeDuration
                : _FadeDuration;
            set => _FadeDuration = value;
        }

        /************************************************************************************************************************/

        /// <summary>The lowest allowed <see cref="CurrentTime"/>.</summary>
        /// <remarks>
        /// This is the lower of the negative <see cref="FromDuration"/>
        /// or the negative duration of the <see cref="FromTransition"/>.
        /// </remarks>
        public float MinTime { get; private set; }

        /// <summary>The highest allowed <see cref="CurrentTime"/>.</summary>
        /// <remarks>
        /// This is the higher of the <see cref="ToDuration"/> or <see cref="FadeDuration"/>
        /// or the duration of the <see cref="ToTransition"/>.
        /// </remarks>
        public float MaxTime { get; private set; }

        /************************************************************************************************************************/

        /// <summary>Recalculated the <see cref="MinTime"/> and <see cref="MaxTime"/>.</summary>
        public void RecalculateTimeBounds()
        {
            MinTime = -FromDuration;

            if (_FromTransition.IsValid() &&
                AnimancerUtilities.TryCalculateDuration(_FromTransition, out var duration))
                MinTime = Math.Min(MinTime, -duration);

            MaxTime = ToDuration;

            var fadeDuration = FadeDuration;
            if (!float.IsNaN(fadeDuration))
                MaxTime = Math.Max(ToDuration, fadeDuration);

            if (_ToTransition.IsValid() &&
                AnimancerUtilities.TryCalculateDuration(_ToTransition, out duration))
                MaxTime = Math.Max(MaxTime, duration);
        }

        /************************************************************************************************************************/

        /// <summary>Converts normalized time to seconds.</summary>
        public float LerpTimeUnclamped(float normalizedTime)
            => Mathf.LerpUnclamped(MinTime, MaxTime, normalizedTime);

        /// <summary>Converts seconds to normalized time.</summary>
        public float InverseLerpTimeUnclamped(float time)
            => AnimancerUtilities.InverseLerpUnclamped(MinTime, MaxTime, time);

        /************************************************************************************************************************/

        private float _CurrentTime = float.NaN;

        /// <summary>The amount of time that has passed since the <see cref="ToTransition"/> started (in seconds).</summary>
        /// <remarks>This value goes from the <see cref="MinTime"/> to the <see cref="MaxTime"/>.</remarks>
        public float CurrentTime
        {
            get => float.IsNaN(_CurrentTime)
                ? MinTime
                : _CurrentTime;
            set
            {
                _CurrentTime = value;
                Evaluate();
            }
        }

        /// <summary>The amount of time that has passed since the <see cref="ToTransition"/> started (normalized).</summary>
        /// <remarks>0 is at the <see cref="MinTime"/> and 1 is at the <see cref="MaxTime"/>.</remarks>
        public float NormalizedTime
        {
            get => InverseLerpTimeUnclamped(CurrentTime);
            set => CurrentTime = LerpTimeUnclamped(value);
        }

        /************************************************************************************************************************/

        private bool _IsPlaying;

        /// <summary>Is the preview currently playing?</summary>
        public bool IsPlaying
        {
            get => _IsPlaying;
            set
            {
                if (_IsPlaying == value)
                    return;

                _IsPlaying = value;

                if (_IsPlaying)
                {
                    _LastUpdateTime = TimeSinceStartup;

                    EditorApplication.update += Update;
                }
                else
                {
                    EditorApplication.update -= Update;
                }
            }
        }

        /// <summary>Cleans up this player.</summary>
        public void Dispose()
            => IsPlaying = false;

        /************************************************************************************************************************/

        /// <summary><see cref="EditorApplication.timeSinceStartup"/></summary>
        private static double TimeSinceStartup
            => EditorApplication.timeSinceStartup;

        private double _LastUpdateTime;

        /// <summary>Updates the preview time while playing.</summary>
        private void Update()
        {
            if (Graph == null || !Graph.IsValidOrDispose())
            {
                EditorApplication.update -= Update;
                return;
            }

            var time = TimeSinceStartup;
            var deltaTime = time - _LastUpdateTime;
            _LastUpdateTime = time;

            CurrentTime += ((float)deltaTime) * Speed;

            AnimancerGUI.RepaintEverything();
        }

        /************************************************************************************************************************/

        /// <summary>Applies the animations at the <see cref="CurrentTime"/>.</summary>
        private void Evaluate()
        {
            if (Graph == null)
                return;

            Graph.PauseGraph();
            Graph.Stop();

            if (_FromTransition.IsValid())
            {
                if (_ToTransition.IsValid())
                {
                    Apply(CurrentTime, _FromTransition, _ToTransition);
                }
                else
                {
                    var minTime = MinTime;
                    Apply(CurrentTime - minTime, -minTime, _FromTransition);
                }
            }
            else
            {
                if (_ToTransition.IsValid())
                    Apply(CurrentTime, MaxTime, _ToTransition);
                else
                    return;
            }

            Graph.Evaluate();
        }

        /************************************************************************************************************************/

        /// <summary>Applies the animations at the `currentTime`.</summary>
        private void Apply(float currentTime, ITransition from, ITransition to)
        {
            var layer = Graph.Layers[0];

            // Playing From.
            if (currentTime < 0)
            {
                var state = layer.Play(from);
                state.Time += state.NormalizedEndTime * state.Length + currentTime;
                return;
            }

            var maxTime = MaxTime;

            // Fading.
            if (currentTime < maxTime)
            {
                var state = layer.Play(from);
                state.Time += state.NormalizedEndTime * state.Length + currentTime;

                state = layer.Play(to, FadeDuration, to.FadeMode);
                state.Time += currentTime;

                var fade = state.FadeGroup;
                if (fade != null)
                {
                    fade.NormalizedTime += currentTime * fade.FadeSpeed;
                    fade.ApplyWeights();
                }
            }
            // Playing To.
            else
            {
                var state = layer.Play(to);
                state.Time += currentTime;

                // Finished.
                if (currentTime >= maxTime)
                {
                    _CurrentTime = MinTime;
                    state.Time = _CurrentTime;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Applies the animation at the `currentTime`.</summary>
        private void Apply(float currentTime, float endTime, ITransition transition)
        {
            var state = Graph.Layers[0].Play(transition);

            if (currentTime < endTime)// Playing.
            {
                state.Time = currentTime;
            }
            else// Finished.
            {
                _CurrentTime = MinTime;
                state.Time = _CurrentTime;
            }
        }

        /************************************************************************************************************************/
    }
}

#endif

