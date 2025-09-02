// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using Unity.Collections;
using UnityEngine;

namespace Animancer
{
    /// <summary>
    /// Replaces the default <see cref="AnimancerLayerMixerList"/>
    /// with a <see cref="WeightedMaskLayerList"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/WeightedMaskLayers
    [AddComponentMenu(Strings.MenuPrefix + "Weighted Mask Layers")]
    [AnimancerHelpUrl(typeof(WeightedMaskLayers))]
    [DefaultExecutionOrder(-10000)]// Awake before anything else initializes Animancer.
    public class WeightedMaskLayers : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField] private AnimancerComponent _Animancer;

        /// <summary>[<see cref="SerializeField"/>] The component to apply the layers to.</summary>
        public AnimancerComponent Animancer
            => _Animancer;

        /************************************************************************************************************************/

        [SerializeField] private WeightedMaskLayersDefinition _Definition;

        /// <summary>[<see cref="SerializeField"/>]
        /// The definition of transforms to control and weights to apply to them.
        /// </summary>
        public ref WeightedMaskLayersDefinition Definition
            => ref _Definition;

        /************************************************************************************************************************/

        /// <summary>The layer list created at runtime and assigned to <see cref="AnimancerGraph.Layers"/>.</summary>
        public WeightedMaskLayerList Layers { get; protected set; }

        /************************************************************************************************************************/

        /// <summary>The index of each of the <see cref="WeightedMaskLayersDefinition.Transforms"/>.</summary>
        public int[] Indices { get; protected set; }

        /************************************************************************************************************************/

        /// <summary>Finds the <see cref="Animancer"/> reference if it was missing.</summary>
        protected virtual void OnValidate()
        {
            gameObject.GetComponentInParentOrChildren(ref _Animancer);
        }

        /************************************************************************************************************************/

        /// <summary>Initializes the <see cref="Layers"/> and applies the default group weights.</summary>
        protected virtual void Awake()
        {
            if (Definition == null ||
                !Definition.IsValid)
                return;

            if (_Animancer == null)
                TryGetComponent(out _Animancer);

            Layers = WeightedMaskLayerList.Create(_Animancer.Animator);
            _Animancer.InitializeGraph(Layers.Graph);

            Indices = Definition.CalculateIndices(Layers);

            SetWeights(0);
        }

        /************************************************************************************************************************/

        /// <summary>Applies the weights of the specified group.</summary>
        public void SetWeights(int groupIndex)
        {
            Definition.AssertGroupIndex(groupIndex);

            var boneWeights = Layers.BoneWeights;
            var definitionWeights = Definition.Weights;

            var start = groupIndex * Indices.Length;

            for (int i = 0; i < Indices.Length; i++)
            {
                var index = Indices[i];
                var weight = definitionWeights[start + i];
                boneWeights[index] = weight;
            }
        }

        /************************************************************************************************************************/

        private Fade _Fade;

        /// <summary>Fades the weights towards the specified group.</summary>
        public void FadeWeights(
            int groupIndex,
            float fadeDuration,
            Func<float, float> easing = null)
        {
            if (fadeDuration > 0)
            {
                _Fade ??= new();
                _Fade.Start(this, groupIndex, fadeDuration, easing);
            }
            else
            {
                SetWeights(groupIndex);
            }
        }

        /************************************************************************************************************************/

        /// <summary>An <see cref="IUpdatable"/> which fades <see cref="WeightedMaskLayers"/> over time.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/Fade
        public class Fade : Updatable
        {
            /************************************************************************************************************************/

            private NativeArray<float> _CurrentWeights;
            private float[] _OriginalWeights;
            private WeightedMaskLayers _Layers;
            private int _TargetWeightIndex;
            private Func<float, float> _Easing;

            /// <summary>The amount of time that has passed since the start of this fade (in seconds).</summary>
            public float ElapsedTime { get; set; }

            /// <summary>The total amount of time this fade will take (in seconds).</summary>
            public float Duration { get; set; }

            /************************************************************************************************************************/

            /// <summary>Initializes this fade and registers it to receive updates.</summary>
            public void Start(
                WeightedMaskLayers layers,
                int groupIndex,
                float duration,
                Func<float, float> easing = null)
            {
                layers.Definition.AssertGroupIndex(groupIndex);

                _CurrentWeights = layers.Layers.BoneWeights;
                _Easing = easing;
                _Layers = layers;
                _TargetWeightIndex = layers.Definition.IndexOf(groupIndex, 0);
                Duration = duration;

                var indices = _Layers.Indices;
                AnimancerUtilities.SetLength(ref _OriginalWeights, indices.Length);
                for (int i = 0; i < _OriginalWeights.Length; i++)
                {
                    var index = indices[i];
                    _OriginalWeights[i] = _CurrentWeights[index];
                }

                ElapsedTime = 0;

                layers.Layers.Graph.RequirePreUpdate(this);
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void Update()
            {
                ElapsedTime += AnimancerGraph.DeltaTime;
                if (ElapsedTime < Duration)
                {
                    ApplyFade(ElapsedTime / Duration);
                }
                else
                {
                    ApplyTargetWeights();

                    AnimancerGraph.Current.CancelPreUpdate(this);
                }
            }

            /************************************************************************************************************************/

            /// <summary>Recalculates the weights by interpolating based on `t`.</summary>
            private void ApplyFade(float t)
            {
                if (_Easing != null)
                    t = _Easing(t);

                var targetWeights = _Layers.Definition.Weights;
                var indices = _Layers.Indices;
                var boneWeights = _CurrentWeights;

                for (int i = 0; i < indices.Length; i++)
                {
                    var index = indices[i];
                    var from = _OriginalWeights[i];
                    var to = targetWeights[_TargetWeightIndex + i];
                    boneWeights[index] = Mathf.LerpUnclamped(from, to, t);
                }
            }

            /// <summary>Recalculates the target weights.</summary>
            private void ApplyTargetWeights()
            {
                var targetWeights = _Layers.Definition.Weights;
                var indices = _Layers.Indices;
                var boneWeights = _CurrentWeights;

                for (int i = 0; i < indices.Length; i++)
                {
                    var index = indices[i];
                    var to = targetWeights[_TargetWeightIndex + i];
                    boneWeights[index] = to;
                }
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
}
