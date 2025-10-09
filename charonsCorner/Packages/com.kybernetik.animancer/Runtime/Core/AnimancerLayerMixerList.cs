// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using UnityEngine;
using UnityEngine.Animations;

namespace Animancer
{
    /// <summary>An <see cref="AnimancerLayerList"/> which uses an <see cref="AnimationLayerMixerPlayable"/>.</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/layers">
    /// Layers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerLayerMixerList
    public class AnimancerLayerMixerList : AnimancerLayerList
    {
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimancerLayerMixerList"/>.</summary>
        public AnimancerLayerMixerList(AnimancerGraph graph)
            : base(graph)
        {
            LayerMixer = AnimationLayerMixerPlayable.Create(graph._PlayableGraph, 1);
            Playable = LayerMixer;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimationLayerMixerPlayable"/> which blends the layers.</summary>
        public AnimationLayerMixerPlayable LayerMixer { get; protected set; }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool IsAdditive(int index)
            => LayerMixer.IsLayerAdditive((uint)index);

        /// <inheritdoc/>
        public override void SetAdditive(int index, bool value)
        {
            SetMinCount(index + 1);
            LayerMixer.SetLayerAdditive((uint)index, value);
        }

        /************************************************************************************************************************/

        private static AvatarMask _DefaultMask;

        /// <inheritdoc/>
        public override void SetMask(int index, AvatarMask mask)
        {
            var layer = this[index];

            if (mask == null)
            {
                // If the existing mask was already null, do nothing.
                // If it was destroyed, we still need to continue and set it to the default.
                if (layer._Mask is null)
                    return;

                _DefaultMask ??= new();

                mask = _DefaultMask;
            }

            // Don't check if the same mask was already assigned because it might have been modified.
            layer._Mask = mask;

            LayerMixer.SetLayerMaskFromAvatarMask((uint)index, mask);
        }

        /************************************************************************************************************************/
    }
}

