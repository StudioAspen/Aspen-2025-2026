// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using Animancer.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>Plays a single <see cref="AnimationClip"/>.</summary>
    /// 
    /// <remarks>
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/playing/component-types">
    /// Component Types</see>
    /// <para></para>
    /// <strong>Sample:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/samples/fine-control/doors">Doors</see>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/SoloAnimation
    /// 
    [AddComponentMenu(Strings.MenuPrefix + "Solo Animation")]
    [AnimancerHelpUrl(typeof(SoloAnimation))]
    [DefaultExecutionOrder(DefaultExecutionOrder)]
    public class SoloAnimation : MonoBehaviour, IAnimationClipSource
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        /// <summary>Initialize before anything else tries to use this component.</summary>
        public const int DefaultExecutionOrder = -5000;

        /************************************************************************************************************************/

        [SerializeField, Tooltip("The Animator component which this script controls")]
        private Animator _Animator;

        /// <summary>[<see cref="SerializeField"/>]
        /// The <see cref="UnityEngine.Animator"/> component which this script controls.
        /// </summary>
        /// <remarks>
        /// If you need to set this value at runtime you are likely better off using a proper
        /// <see cref="AnimancerComponent"/>.
        /// </remarks>
        public Animator Animator
        {
            get => _Animator;
            set
            {
                _Animator = value;
                if (IsInitialized)
                    Play();
            }
        }

        /************************************************************************************************************************/

        [SerializeField, Tooltip("The animation that will be played")]
        private AnimationClip _Clip;

        /// <summary>[<see cref="SerializeField"/>] The <see cref="AnimationClip"/> that will be played.</summary>
        /// <remarks>
        /// If you need to set this value at runtime you are likely better off using a proper
        /// <see cref="AnimancerComponent"/>.
        /// </remarks>
        public AnimationClip Clip
        {
            get => _Clip;
            set
            {
                _Clip = value;
                if (IsInitialized)
                    Play();
            }
        }

        /// <summary><see cref="AnimationClip.length"/></summary>
        /// <remarks>
        /// This value is cached on startup
        /// and is <see cref="float.NaN"/> if there's no <see cref="Clip"/>.
        /// </remarks>
        public float Length { get; private set; } = float.NaN;

        /// <summary><see cref="Motion.isLooping"/></summary>
        /// <remarks>This value is cached on startup.</remarks>
        public bool IsLooping { get; private set; }

        /************************************************************************************************************************/

        /// <summary>
        /// Should disabling this object stop and rewind the animation?
        /// Otherwise, it will simply be paused and will resume from its current state when re-enabled.
        /// </summary>
        /// <remarks>
        /// The default value is true.
        /// <para></para>
        /// This property inverts <see cref="Animator.keepAnimatorStateOnDisable"/>
        /// and is serialized by the <see cref="UnityEngine.Animator"/>.
        /// </remarks>
        public bool StopOnDisable
        {
            get => !_Animator.keepAnimatorStateOnDisable;
            set => _Animator.keepAnimatorStateOnDisable = !value;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="PlayableGraph"/> being used to play the <see cref="Clip"/>.</summary>
        private PlayableGraph _Graph;

        /// <summary>The <see cref="AnimationClipPlayable"/> being used to play the <see cref="Clip"/>.</summary>
        private AnimationClipPlayable _Playable;

        /************************************************************************************************************************/

        private bool _IsPlaying;

        /// <summary>Is the animation playing (true) or paused (false)?</summary>
        public bool IsPlaying
        {
            get => _IsPlaying;
            set
            {
                _IsPlaying = value;

                if (value)
                {
                    if (!IsInitialized)
                    {
                        Play();
                    }
                    else
                    {
                        _Graph.Play();

#if UNITY_EDITOR
                        // In Edit Mode, unpausing the graph doesn't work properly unless we force it to change.
                        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                            _Graph.Evaluate(0.00001f);
#endif
                    }
                }
                else
                {
                    if (IsInitialized)
                        _Graph.Stop();
                }
            }
        }

        /************************************************************************************************************************/

        [SerializeField, Range(0, 1)]
        [Tooltip("The normalized time that the animation will start at")]
        private float _NormalizedStartTime;

        /// <summary>[<see cref="SerializeField"/>] The normalized time that the animation will start at.</summary>
        public float NormalizedStartTime
        {
            get => _NormalizedStartTime;
            set => _NormalizedStartTime = value;
        }

        /************************************************************************************************************************/

        /// <summary>[<see cref="SerializeField"/>]
        /// The number of seconds that have passed since the start of the animation.
        /// </summary>
        /// <remarks>
        /// This value will continue increasing after the animation passes the end of its length
        /// and it will either freeze in place or start again from the beginning according to
        /// whether it's looping or not.
        /// </remarks>
        public float Time
        {
            get => _Playable.IsValid()
                ? (float)_Playable.GetTime()
                : _NormalizedStartTime * Length;
            set
            {
                if (_Playable.IsValid())
                    SetTime(value);
            }
        }

        /// <summary>
        /// Calls <see cref="PlayableExtensions.SetTime{U}"/> twice
        /// to ensure that animation events aren't triggered incorrectly.
        /// </summary>
        private void SetTime(double value)
        {
            _Playable.SetTime(value);
            _Playable.SetTime(value);
        }

        /// <summary>[<see cref="SerializeField"/>]
        /// The <see cref="Time"/> of this state as a portion of the <see cref="AnimationClip.length"/>,
        /// meaning the value goes from 0 to 1 as it plays from start to end,
        /// regardless of how long that actually takes.
        /// </summary>
        /// <remarks>
        /// This value will continue increasing after the animation passes the end of its length
        /// and it will either freeze in place or start again from the beginning according to
        /// whether it's looping or not.
        /// <para></para>
        /// The fractional part of the value (<c>NormalizedTime % 1</c>) is the percentage (0-1)
        /// of progress in the current loop while the integer part (<c>(int)NormalizedTime</c>)
        /// is the number of times the animation has been looped.
        /// </remarks>
        public float NormalizedTime
        {
            get => _Playable.IsValid()
                ? (float)_Playable.GetTime() / Length
                : _NormalizedStartTime;
            set
            {
                if (_Playable.IsValid())
                    SetTime(value * Length);
            }
        }

        /************************************************************************************************************************/

        [SerializeField, Multiplier, Tooltip("The speed at which the animation plays (default 1)")]
        private float _Speed = 1;

        /// <summary>[<see cref="SerializeField"/>] The speed at which the animation is playing (default 1).</summary>
        /// <exception cref="ArgumentException">This component is not yet <see cref="IsInitialized"/>.</exception>
        public float Speed
        {
            get => _Speed;
            set
            {
                _Speed = value;
                if (_Playable.IsValid())
                    _Playable.SetSpeed(value);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Indicates whether the <see cref="PlayableGraph"/> is valid.</summary>
        public bool IsInitialized
            => _Graph.IsValid();

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

#if UNITY_EDITOR
        /************************************************************************************************************************/

        [SerializeField, Tooltip("Should the " + nameof(Clip) + " be automatically applied to the object in Edit Mode?")]
        private bool _ApplyInEditMode;

        /// <summary>[Editor-Only] Should the <see cref="Clip"/> be automatically applied to the object in Edit Mode?</summary>
        public ref bool ApplyInEditMode
            => ref _ApplyInEditMode;

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Tries to find an <see cref="UnityEngine.Animator"/> component on this <see cref="GameObject"/> or its
        /// children or parents (in that order).
        /// </summary>
        /// <remarks>
        /// Called by the Unity Editor when this component is first added (in Edit Mode) and whenever the Reset command
        /// is executed from its context menu.
        /// </remarks>
        protected virtual void Reset()
        {
            gameObject.GetComponentInParentOrChildren(ref _Animator);
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Applies the <see cref="Speed"/>, <see cref="FootIK"/>, and <see cref="ApplyInEditMode"/>.
        /// </summary>
        /// <remarks>Called in Edit Mode whenever this script is loaded or a value is changed in the Inspector.</remarks>
        protected virtual void OnValidate()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                if (_ApplyInEditMode && enabled)
                {
                    if (!IsInitialized)
                    {
                        Play();
                        IsPlaying = false;
                        _Graph.Evaluate();
                    }

                    if (NormalizedTime != _NormalizedStartTime)
                    {
                        NormalizedTime = _NormalizedStartTime;
                        _Graph.Evaluate();
                    }
                }
                else
                {
                    if (IsInitialized)
                        _Graph.Destroy();
                }
            }

            if (IsInitialized)
                Speed = Speed;
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/

        /// <summary>Plays the <see cref="Clip"/>.</summary>
        public void Play()
            => Play(_Clip);

        /// <summary>Plays the `clip`.</summary>
        public void Play(AnimationClip clip)
        {
            if (clip == null)
            {
                Length = 0;
                IsLooping = false;
                return;
            }

            Length = clip.length;
            IsLooping = clip.isLooping;

            if (_Animator == null)
                return;

            if (_Graph.IsValid())
                _Graph.Destroy();

            _Playable = AnimationPlayableUtilities.PlayClip(_Animator, clip, out _Graph);

            _Playable.SetSpeed(_Speed);

            SetTime(_NormalizedStartTime * Length);

            if (_Speed != 0)
            {
                _IsPlaying = true;
            }
            else
            {
                _IsPlaying = false;
                _Graph.Stop();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="PlayableGraph.Evaluate()"/>.</summary>
        public void Evaluate()
        {
            if (_Graph.IsValid())
                _Graph.Evaluate();
        }

        /// <summary>Calls <see cref="PlayableGraph.Evaluate(float)"/>.</summary>
        public void Evaluate(float deltaTime)
        {
            if (_Graph.IsValid())
                _Graph.Evaluate(deltaTime);
        }

        /************************************************************************************************************************/

        /// <summary>Plays the <see cref="Clip"/> on the target <see cref="Animator"/>.</summary>
        protected virtual void OnEnable()
        {
            if (!_IsPlaying)
                Play();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Checks if the animation is done
        /// so it can pause the <see cref="PlayableGraph"/> to improve performance.
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (!IsPlaying ||
                IsLooping ||
                !_Playable.IsValid())
                return;

            var time = (float)_Playable.GetTime();
            if (_Speed >= 0)
            {
                if (time >= Length)
                {
                    IsPlaying = false;
                    Time = Length;
                }
            }
            else
            {
                if (time <= 0)
                {
                    IsPlaying = false;
                    Time = 0;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Stops playing and rewinds if <see cref="StopOnDisable"/>.</summary>
        protected virtual void OnDisable()
        {
            if (!IsInitialized)
                return;

            _IsPlaying = false;
            _Graph.Stop();

            if (StopOnDisable)
                SetTime(0);
        }

        /************************************************************************************************************************/

        /// <summary>Ensures that the <see cref="PlayableGraph"/> is properly cleaned up.</summary>
        protected virtual void OnDestroy()
        {
            if (IsInitialized)
                _Graph.Destroy();
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only] Ensures that the <see cref="PlayableGraph"/> is destroyed.</summary>
        ~SoloAnimation()
        {
            UnityEditor.EditorApplication.delayCall += OnDestroy;
        }
#endif

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipSource"/>] Adds the <see cref="Clip"/> to the list.</summary>
        public void GetAnimationClips(List<AnimationClip> clips)
        {
            if (_Clip != null)
                clips.Add(_Clip);
        }

        /************************************************************************************************************************/
    }
}

