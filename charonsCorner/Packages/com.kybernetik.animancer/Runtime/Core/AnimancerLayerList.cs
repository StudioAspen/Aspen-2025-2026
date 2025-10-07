// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>A list of <see cref="AnimancerLayer"/>s with methods to control their mixing and masking.</summary>
    /// <remarks>
    /// The default implementation of this class is <see cref="AnimancerLayerMixerList"/>.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/layers">
    /// Layers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerLayerList
    public abstract class AnimancerLayerList :
        IEnumerable<AnimancerLayer>,
        IAnimationClipCollection
    {
        /************************************************************************************************************************/
        #region Fields
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerGraph"/> containing this list.</summary>
        public readonly AnimancerGraph Graph;

        /// <summary>The layers which each manage their own set of animations.</summary>
        /// <remarks>This field should never be null so it shouldn't need null-checking.</remarks>
        private AnimancerLayer[] _Layers;

        /// <summary>The number of layers that have actually been created.</summary>
        private int _Count;

        /// <summary>The <see cref="UnityEngine.Playables.Playable"/> which blends the layers.</summary>
        public Playable Playable { get; protected set; }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimancerLayerList"/>.</summary>
        /// <remarks>The <see cref="Playable"/> must be assigned by the end of the derived constructor.</remarks>
        protected AnimancerLayerList(AnimancerGraph graph)
        {
            Graph = graph;
            _Layers = new AnimancerLayer[DefaultCapacity];
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region List Operations
        /************************************************************************************************************************/

        /// <summary>[Pro-Only] The number of layers in this list.</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The value is set higher than the <see cref="DefaultCapacity"/>. This is simply a safety measure,
        /// so if you do actually need more layers you can just increase the limit.
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">The value is set to a negative number.</exception>
        public int Count
        {
            get => _Count;
            set
            {
                var count = _Count;

                if (value == count)
                    return;

                CheckAgain:

                if (value > count)// Increasing.
                {
                    Add();
                    count++;
                    goto CheckAgain;
                }
                else// Decreasing.
                {
                    while (value < count--)
                    {
                        var layer = _Layers[count];
                        if (layer._Playable.IsValid())
                            Graph._PlayableGraph.DestroySubgraph(layer._Playable);
                        layer.DestroyStates();
                    }

                    Array.Clear(_Layers, value, _Count - value);

                    _Count = value;

                    Playable.SetInputCount(value);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// If the <see cref="Count"/> is below the specified `min`, this method increases it to that value.
        /// </summary>
        public void SetMinCount(int min)
        {
            if (Count < min)
                Count = min;
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// The maximum number of layers that can be created before an <see cref="ArgumentOutOfRangeException"/> will
        /// be thrown (default 4).
        /// <para></para>
        /// Lowering this value will not affect layers that have already been created.
        /// </summary>
        /// <remarks>
        /// <strong>Example:</strong>
        /// To set this value automatically when the application starts, place a method like this in any class:
        /// <para></para><code>
        /// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        /// private static void SetMaxLayerCount()
        /// {
        ///     Animancer.AnimancerLayerList.DefaultCapacity = 8;
        /// }
        /// </code>
        /// Otherwise you can set the <see cref="Capacity"/> of each individual list:
        /// <para></para><code>
        /// AnimancerComponent animancer;
        /// animancer.Layers.Capacity = 8;
        /// </code></remarks>
        public static int DefaultCapacity { get; set; } = 4;

        /// <summary>[Pro-Only]
        /// If the <see cref="DefaultCapacity"/> is below the specified `min`, this method increases it to that value.
        /// </summary>
        public static void SetMinDefaultCapacity(int min)
        {
            if (DefaultCapacity < min)
                DefaultCapacity = min;
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// The maximum number of layers that can be created before an <see cref="ArgumentOutOfRangeException"/> will
        /// be thrown. The initial capacity is determined by <see cref="DefaultCapacity"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// Lowering this value will destroy any layers beyond the specified value.
        /// <para></para>
        /// Changing this value will cause the allocation of a new array and garbage collection of the old one,
        /// so you should generally set the <see cref="DefaultCapacity"/> before initializing this list.
        /// </remarks>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">The value is not greater than 0.</exception>
        public int Capacity
        {
            get => _Layers.Length;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(Capacity), $"must be greater than 0 ({value} <= 0)");

                if (_Count > value)
                    Count = value;

                Array.Resize(ref _Layers, value);
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only] Creates and returns a new <see cref="AnimancerLayer"/> at the end of this list.</summary>
        /// <remarks>If the <see cref="Capacity"/> would be exceeded, it will be doubled.</remarks>
        public AnimancerLayer Add()
        {
            var index = _Count;

            if (index >= _Layers.Length)
                Capacity *= 2;

            var layer = new AnimancerLayer(Graph, index);

            _Count = index + 1;
            Playable.SetInputCount(_Count);
            Graph._PlayableGraph.Connect(Playable, layer._Playable, index, 0);

            _Layers[index] = layer;
            return layer;
        }

        /************************************************************************************************************************/

        /// <summary>Returns the layer at the specified index. If it didn't already exist, this method creates it.</summary>
        /// <remarks>To only get an existing layer without creating new ones, use <see cref="GetLayer"/> instead.</remarks>
        public AnimancerLayer this[int index]
        {
            get
            {
                SetMinCount(index + 1);
                return _Layers[index];
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns the layer at the specified index.</summary>
        /// <remarks>To create a new layer if the target doesn't exist, use <see cref="this[int]"/> instead.</remarks>
        public AnimancerLayer GetLayer(int index)
            => _Layers[index];

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Enumeration
        /************************************************************************************************************************/

        /// <summary>Returns an enumerator that will iterate through all layers.</summary>
        public FastEnumerator<AnimancerLayer> GetEnumerator()
            => new(_Layers, _Count);

        /// <inheritdoc/>
        IEnumerator<AnimancerLayer> IEnumerable<AnimancerLayer>.GetEnumerator()
            => GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipCollection"/>] Gathers all the animations in all layers.</summary>
        public void GatherAnimationClips(ICollection<AnimationClip> clips)
            => clips.GatherFromSource(_Layers);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Layer Details
        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Is the layer at the specified index is set to additive blending?
        /// Otherwise it will override lower layers.
        /// </summary>
        public virtual bool IsAdditive(int index)
            => false;

        /// <summary>[Pro-Only]
        /// Sets the layer at the specified index to blend additively with earlier layers (if true)
        /// or to override them (if false). Newly created layers will override by default.
        /// </summary>
        public virtual void SetAdditive(int index, bool value) { }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Sets an <see cref="AvatarMask"/> to determine which bones the layer at the specified index will affect.
        /// </summary>
        /// <remarks>
        /// Don't assign the same mask repeatedly unless you have modified it.
        /// This property doesn't check if the mask is the same
        /// so repeatedly assigning the same thing will simply waste performance.
        /// </remarks>
        public virtual void SetMask(int index, AvatarMask mask) { }

        /************************************************************************************************************************/

        /// <summary>[Editor-Conditional] Sets the Inspector display name of the layer at the specified index.</summary>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public void SetDebugName(int index, string name)
            => this[index].SetDebugName(name);

        /************************************************************************************************************************/

        /// <summary>
        /// The average velocity of the root motion of all currently playing animations,
        /// taking their current <see cref="AnimancerNode.Weight"/> into account.
        /// </summary>
        public Vector3 AverageVelocity
        {
            get
            {
                var velocity = default(Vector3);

                for (int i = 0; i < _Count; i++)
                {
                    var layer = _Layers[i];
                    velocity += layer.AverageVelocity * layer.Weight;
                }

                return velocity;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

