// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>An <see cref="AnimancerState"/> which plays an <see cref="AnimationClip"/>.</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states">
    /// States</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ClipState
    /// 
    public class ClipState : AnimancerState
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        private AnimationClip _Clip;

        /// <summary>The <see cref="AnimationClip"/> which this state plays.</summary>
        public override AnimationClip Clip
        {
            get => _Clip;
            set
            {
                Validate.AssertAnimationClip(value, true, $"set {nameof(ClipState)}.{nameof(Clip)}");
                if (ChangeMainObject(ref _Clip, value))
                {
                    _Length = value.length;

                    var isLooping = value.isLooping;
                    if (_IsLooping != isLooping)
                    {
                        _IsLooping = isLooping;
                        OnIsLoopingChangedRecursive(isLooping);
                    }
                }
            }
        }

        /// <summary>The <see cref="AnimationClip"/> which this state plays.</summary>
        public override Object MainObject
        {
            get => _Clip;
            set => Clip = (AnimationClip)value;
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        public override Type MainObjectType
            => typeof(AnimationClip);
#endif

        /************************************************************************************************************************/

        private float _Length;

        /// <summary>The <see cref="AnimationClip.length"/>.</summary>
        public override float Length
            => _Length;

        /************************************************************************************************************************/

        private bool _IsLooping;

        /// <summary>The <see cref="Motion.isLooping"/>.</summary>
        public override bool IsLooping
            => _IsLooping;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void GetEventDispatchInfo(
            out float length,
            out float normalizedTime,
            out bool isLooping)
        {
            length = _Length;

            normalizedTime = length != 0
                ? Time / length
                : 0;

            isLooping = _IsLooping;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Vector3 AverageVelocity
            => _Clip.averageSpeed;

        /************************************************************************************************************************/
        #region Inverse Kinematics
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool ApplyAnimatorIK
        {
            get => _Playable.IsValid() && ((AnimationClipPlayable)_Playable).GetApplyPlayableIK();
            set
            {
                Validate.AssertPlayable(this);
                ((AnimationClipPlayable)_Playable).SetApplyPlayableIK(value);
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool ApplyFootIK
        {
            get => _Playable.IsValid() && ((AnimationClipPlayable)_Playable).GetApplyFootIK();
            set
            {
                Validate.AssertPlayable(this);
                ((AnimationClipPlayable)_Playable).SetApplyFootIK(value);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Methods
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="ClipState"/> and sets its <see cref="Clip"/>.</summary>
        /// <exception cref="ArgumentNullException">The `clip` is null.</exception>
        public ClipState(AnimationClip clip)
        {
            Validate.AssertAnimationClip(clip, true, $"create {nameof(ClipState)}");
            _Clip = clip;
            _Length = clip.length;
            _IsLooping = clip.isLooping;
        }

        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="AnimationClipPlayable"/> managed by this node.</summary>
        protected override void CreatePlayable(out Playable playable)
        {
            playable = AnimationClipPlayable.Create(Graph._PlayableGraph, _Clip);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void RecreatePlayable()
        {
            var playable = (AnimationClipPlayable)_Playable;
            var footIK = playable.GetApplyFootIK();
            var playableIK = playable.GetApplyPlayableIK();

            base.RecreatePlayable();

            playable = (AnimationClipPlayable)_Playable;
            playable.SetApplyFootIK(footIK);
            playable.SetApplyPlayableIK(playableIK);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Destroy()
        {
            _Clip = null;
            base.Destroy();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerState Clone(CloneContext context)
        {
            var clip = context.GetCloneOrOriginal(_Clip);
            var clone = new ClipState(clip);
            clone.CopyFrom(this, context);
            return clone;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

