// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Animancer
{
    /// <summary>Serializable data which defines how to control a <see cref="WeightedMaskLayerList"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/WeightedMaskLayersDefinition
    [Serializable]
    public class WeightedMaskLayersDefinition :
        ICopyable<WeightedMaskLayersDefinition>,
        IEquatable<WeightedMaskLayersDefinition>
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
        /************************************************************************************************************************/

        /// <summary>The name of the serialized backing field of <see cref="Transforms"/>.</summary>
        public const string
            TransformsField = nameof(_Transforms);

        [SerializeField]
        private Transform[] _Transforms;

        /// <summary><see cref="Transform"/>s being controlled by this definition.</summary>
        public ref Transform[] Transforms
            => ref _Transforms;

        /************************************************************************************************************************/

        /// <summary>The name of the serialized backing field of <see cref="Weights"/>.</summary>
        public const string
            WeightsField = nameof(_Weights);

        [SerializeField]
        private float[] _Weights;

        /// <summary>Groups of weights which will be applied to the <see cref="Transforms"/>.</summary>
        /// <remarks>
        /// This is a flattened 2D array containing groups of target weights corresponding to the transforms.
        /// With n transforms, indices 0 to n-1 are Group 0, n to n*2-1 are Group 1, etc.
        /// </remarks>
        public ref float[] Weights
            => ref _Weights;

        /************************************************************************************************************************/

        /// <summary>The number of weight groups in this definition.</summary>
        public int GroupCount
        {
            get => _Transforms == null || _Transforms.Length == 0 || _Weights == null
                ? 0
                : _Weights.Length / _Transforms.Length;
            set
            {
                if (_Transforms != null && value > 0)
                    Array.Resize(ref _Weights, _Transforms.Length * value);
                else
                    _Weights = Array.Empty<float>();
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Asserts that the `groupIndex` is valid.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public void AssertGroupIndex(int groupIndex)
        {
            if ((uint)groupIndex >= (uint)GroupCount)
                throw new ArgumentOutOfRangeException(
                    nameof(groupIndex),
                    groupIndex,
                    $"Must be 0 <= {nameof(groupIndex)} < Group Count ({GroupCount})");
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the index of each of the <see cref="Transforms"/>.</summary>
        public int[] CalculateIndices(WeightedMaskLayerList layers)
        {
            var indices = new int[_Transforms.Length];

            for (int i = 0; i < _Transforms.Length; i++)
            {
                indices[i] = layers.IndexOf(_Transforms[i]);
#if UNITY_ASSERTIONS
                if (indices[i] < 0)
                    Debug.LogWarning(
                        $"Unable to find index of {_Transforms[i]} in {nameof(WeightedMaskLayerList)}",
                        _Transforms[i]);
#endif
            }

            return indices;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Adds the `transform` at the specified `index`
        /// along with any associated <see cref="_Weights"/>.
        /// </summary>
        public void AddTransform(Transform transform)
        {
            var index = _Transforms.Length;

            AnimancerUtilities.InsertAt(ref _Transforms, index, transform);

            if (_Transforms.Length == 1 && _Weights.IsNullOrEmpty())
            {
                _Weights = new float[1];
                return;
            }

            while (index <= _Weights.Length)
            {
                AnimancerUtilities.InsertAt(ref _Weights, index, 0);

                index += _Transforms.Length;
            }
        }

        /// <summary>
        /// Removes the `index` from the <see cref="_Transforms"/>
        /// along with any associated <see cref="_Weights"/>.
        /// </summary>
        public void RemoveTransform(int index)
        {
            AnimancerUtilities.RemoveAt(ref _Transforms, index);

            while (index < _Weights.Length)
            {
                AnimancerUtilities.RemoveAt(ref _Weights, index);

                index += _Transforms.Length;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the index in the <see cref="Weights"/> corresponding to the specified values.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfGroup(int groupIndex)
            => groupIndex * _Transforms.Length;

        /// <summary>Calculates the index in the <see cref="Weights"/> corresponding to the specified values.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(int groupIndex, int transformIndex)
            => groupIndex * _Transforms.Length + transformIndex;

        /************************************************************************************************************************/

        /// <summary>Gets the specified weight.</summary>
        /// <remarks>Returns <see cref="float.NaN"/> if the indices are outside the <see cref="Weights"/>.</remarks>
        public float GetWeight(int groupIndex, int transformIndex)
        {
            if (Weights == null)
                return float.NaN;

            var index = IndexOf(groupIndex, transformIndex);
            return (uint)index < (uint)Weights.Length
                ? Weights[index]
                : float.NaN;
        }

        /// <summary>Sets the specified weight.</summary>
        /// <remarks>Returns false if the indices are outside the <see cref="Weights"/>.</remarks>
        public bool SetWeight(int groupIndex, int transformIndex, float value)
        {
            if (Weights == null)
                return false;

            var index = IndexOf(groupIndex, transformIndex);
            if ((uint)index < (uint)Weights.Length)
            {
                Weights[index] = value;
                return true;
            }

            return false;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public void CopyFrom(WeightedMaskLayersDefinition copyFrom, CloneContext context)
        {
            AnimancerUtilities.CopyExactArray(copyFrom._Transforms, ref _Transforms);
            AnimancerUtilities.CopyExactArray(copyFrom._Weights, ref _Weights);
        }

        /************************************************************************************************************************/

        /// <summary>Does this definition contain valid data?</summary>
        public bool IsValid
            => !_Transforms.IsNullOrEmpty()
            && _Weights != null && _Weights.Length >= _Transforms.Length;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public bool OnValidate()
            => ValidateArraySizes()
            || RemoveMissingAndDuplicate();

        /// <summary>Ensures that all the arrays have valid sizes.</summary>
        public bool ValidateArraySizes()
        {
            if (_Transforms.IsNullOrEmpty())
            {
                _Transforms = Array.Empty<Transform>();
                _Weights = Array.Empty<float>();
                return true;
            }

            if (_Weights == null ||
                _Weights.Length < _Transforms.Length)
            {
                AnimancerUtilities.SetLength(ref _Weights, _Transforms.Length);
                return true;
            }

            var expectedWeightCount = (int)Math.Ceiling(_Weights.Length / (double)_Transforms.Length);
            expectedWeightCount *= _Transforms.Length;
            return AnimancerUtilities.SetLength(ref _Weights, expectedWeightCount);
        }

        /// <summary>Removes any missing or identical <see cref="_Transforms"/>.</summary>
        public bool RemoveMissingAndDuplicate()
        {
            var removedAny = false;

            for (int i = 0; i < _Transforms.Length; i++)
            {
                var transform = _Transforms[i];
                if (transform == null)
                {
                    RemoveTransform(i);
                    removedAny = true;
                }
                else
                {
                    var nextIndex = i + 1;

                    RemoveDuplicates:

                    nextIndex = Array.IndexOf(_Transforms, transform, nextIndex);
                    if (nextIndex > i)
                    {
                        RemoveTransform(nextIndex);
                        removedAny = true;
                        goto RemoveDuplicates;
                    }
                }
            }

            return removedAny;
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <inheritdoc/>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
            => OnValidate();

        /// <inheritdoc/>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
            => OnValidate();

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/

        /// <summary>Returns a summary of this definition.</summary>
        public override string ToString()
            => $"{nameof(WeightedMaskLayersDefinition)}(" +
            $"{nameof(Transforms)}={(Transforms != null ? Transforms.Length : 0)}, " +
            $"{nameof(Weights)}={(Weights != null ? Weights.Length : 0)})";

        /************************************************************************************************************************/
        #region Equality
        /************************************************************************************************************************/

        /// <summary>Are all fields in this object equal to the equivalent in `obj`?</summary>
        public override bool Equals(object obj)
            => Equals(obj as WeightedMaskLayersDefinition);

        /// <summary>Are all fields in this object equal to the equivalent fields in `other`?</summary>
        public bool Equals(WeightedMaskLayersDefinition other)
            => other != null
            && AnimancerUtilities.ContentsAreEqual(_Transforms, other._Transforms)
            && AnimancerUtilities.ContentsAreEqual(_Weights, other._Weights);

        /// <summary>Are all fields in `a` equal to the equivalent fields in `b`?</summary>
        public static bool operator ==(WeightedMaskLayersDefinition a, WeightedMaskLayersDefinition b)
            => a is null
                ? b is null
                : a.Equals(b);

        /// <summary>Are any fields in `a` not equal to the equivalent fields in `b`?</summary>
        public static bool operator !=(WeightedMaskLayersDefinition a, WeightedMaskLayersDefinition b)
            => !(a == b);

        /************************************************************************************************************************/

        /// <summary>Returns a hash code based on the values of this object's fields.</summary>
        public override int GetHashCode()
            => AnimancerUtilities.Hash(-871379578,
                _Transforms.SafeGetHashCode(),
                _Weights.SafeGetHashCode());

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

