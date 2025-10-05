// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace Animancer
{
    /// <summary>
    /// An <see cref="IAnimationJob"/> which mixes its inputs based on individual <see cref="boneWeights"/>.
    /// </summary>
    public struct WeightedMaskMixerJob : IAnimationJob
    {
        /************************************************************************************************************************/

        /// <summary>The handles for each bone being mixed.</summary>
        /// <remarks>All animated bones must be included, even if their individual weight isn't modified.</remarks>
        public NativeArray<TransformStreamHandle> boneTransforms;

        /// <summary>The blend weight of each bone. This array corresponds to the <see cref="boneTransforms"/>.</summary>
        public NativeArray<float> boneWeights;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        readonly void IAnimationJob.ProcessRootMotion(AnimationStream stream)
        {
            var stream0 = stream.GetInputStream(0);
            var stream1 = stream.GetInputStream(1);

            if (stream1.isValid)
            {
                var layerWeight = stream.GetInputWeight(1);
                var velocity = Vector3.LerpUnclamped(stream0.velocity, stream1.velocity, layerWeight);
                var angularVelocity = Vector3.LerpUnclamped(stream0.angularVelocity, stream1.angularVelocity, layerWeight);
                stream.velocity = velocity;
                stream.angularVelocity = angularVelocity;
            }
            else
            {
                stream.velocity = stream0.velocity;
                stream.angularVelocity = stream0.angularVelocity;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        readonly void IAnimationJob.ProcessAnimation(AnimationStream stream)
        {
            var stream0 = stream.GetInputStream(0);
            var stream1 = stream.GetInputStream(1);

            if (stream1.isValid)
            {
                var layerWeight = stream.GetInputWeight(1);
                var handleCount = boneTransforms.Length;
                for (var i = 0; i < handleCount; i++)
                {
                    var handle = boneTransforms[i];
                    var weight = layerWeight * boneWeights[i];

                    var position0 = handle.GetLocalPosition(stream0);
                    var position1 = handle.GetLocalPosition(stream1);
                    handle.SetLocalPosition(stream, Vector3.LerpUnclamped(position0, position1, weight));

                    var rotation0 = handle.GetLocalRotation(stream0);
                    var rotation1 = handle.GetLocalRotation(stream1);
                    handle.SetLocalRotation(stream, Quaternion.SlerpUnclamped(rotation0, rotation1, weight));
                }
            }
            else
            {
                var handleCount = boneTransforms.Length;
                for (var i = 0; i < handleCount; i++)
                {
                    var handle = boneTransforms[i];
                    handle.SetLocalPosition(stream, handle.GetLocalPosition(stream0));
                    handle.SetLocalRotation(stream, handle.GetLocalRotation(stream0));
                }
            }
        }

        /************************************************************************************************************************/
    }
}

