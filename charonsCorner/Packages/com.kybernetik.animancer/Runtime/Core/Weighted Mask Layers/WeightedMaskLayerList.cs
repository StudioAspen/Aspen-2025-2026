// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace Animancer
{
    /// <summary>
    /// A replacement for the default <see cref="AnimationLayerMixerPlayable"/> which uses custom
    /// <see cref="BoneWeights"/> for each individual bone instead of just using an <see cref="AvatarMask"/>
    /// to include or exclude them entirely.
    /// </summary>
    /// <remarks>
    /// This system currently only supports 2 layers (Base + 1). Adding support for more would require additional
    /// <see cref="BoneWeights"/> for each additional layer and modifications to <see cref="WeightedMaskMixerJob"/>
    /// to iterate through each layer instead of just using the first two.
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/WeightedMaskLayerList
    public class WeightedMaskLayerList : AnimancerLayerList, IDisposable
    {
        /************************************************************************************************************************/

        /// <summary>The objects being masked.</summary>
        public readonly Transform[] Bones;

        /// <summary>The job data.</summary>
        private readonly WeightedMaskMixerJob _Job;

        /************************************************************************************************************************/

        /// <summary>The blend weight of each of the <see cref="Bones"/>.</summary>
        public NativeArray<float> BoneWeights
            => _Job.boneWeights;

        /************************************************************************************************************************/

        /// <summary>Returns the index of the value corresponding to the 'bone' in the <see cref="BoneWeights"/> array.</summary>
        public int IndexOf(Transform bone)
            => Array.IndexOf(Bones, bone) - 1;// Index - 1 since the root is ignored.

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimancerGraph"/> and <see cref="WeightedMaskLayerList"/>.</summary>
        /// <remarks>
        /// This method can't be a constructor because it would need to
        /// assign itself to the graph before being fully constructed.
        /// </remarks>
        public static WeightedMaskLayerList Create(Animator animator)
        {
            var graph = new AnimancerGraph();
            var layers = new WeightedMaskLayerList(graph, animator);
            graph.Layers = layers;
            return layers;
        }

        /// <summary>Creates a new <see cref="WeightedMaskLayerList"/>.</summary>
        public WeightedMaskLayerList(AnimancerGraph graph, Animator animator)
            : base(graph)
        {
            graph.Layers = this;

            Bones = animator.GetComponentsInChildren<Transform>();

            var boneCount = Bones.Length - 1;// Ignore the root bone.

            _Job = new WeightedMaskMixerJob()
            {
                boneTransforms = new(boneCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
                boneWeights = new(boneCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
            };

            graph.Disposables.Add(this);

            for (var i = 0; i < boneCount; i++)
            {
                _Job.boneTransforms[i] = animator.BindStreamTransform(Bones[i + 1]);
                _Job.boneWeights[i] = 1;
            }

            var playable = AnimationScriptPlayable.Create(graph, _Job, Capacity);
            playable.SetProcessInputs(false);
            Playable = playable;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        void IDisposable.Dispose()
        {
            _Job.boneTransforms.Dispose();
            _Job.boneWeights.Dispose();
        }

        /************************************************************************************************************************/
    }
}

