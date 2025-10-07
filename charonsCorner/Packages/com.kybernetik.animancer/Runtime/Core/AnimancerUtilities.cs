// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>Various extension methods and utilities.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerUtilities
    /// 
    public static partial class AnimancerUtilities
    {
        /************************************************************************************************************************/
        #region Misc
        /************************************************************************************************************************/

        /// <summary>This is Animancer Pro.</summary>
        public const bool IsAnimancerPro = true;

        /************************************************************************************************************************/

        /// <summary>
        /// If `obj` exists, this method returns <see cref="object.ToString"/>.
        /// Or if it is <c>null</c>, this method returns <c>"Null"</c>.
        /// Or if it is an <see cref="Object"/> that has been destroyed, this method returns <c>"Null (ObjectType)"</c>.
        /// </summary>
        public static string ToStringOrNull(object obj)
        {
            if (obj == null)
                return "Null";

            if (obj is Object unityObject && unityObject == null)
                return $"Null ({obj.GetType()})";

            return obj.ToString();
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Is the `node` is not null and its <see cref="AnimancerNodeBase.Playable"/> valid?
        /// </summary>
        public static bool IsValid(this AnimancerNode node)
            => node != null
            && node.Playable.IsValid();

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] Calls <see cref="ITransition.CreateState"/> and <see cref="ITransition.Apply"/>.</summary>
        public static AnimancerState CreateStateAndApply(this ITransition transition, AnimancerGraph graph = null)
        {
            var state = transition.CreateState();
            state.SetGraph(graph);
            transition.Apply(state);
            return state;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the `key` is an <see cref="AnimancerState"/>,
        /// this method gets its <see cref="AnimancerState.Key"/>
        /// and repeats that check until it finds another kind of key, which it returns.
        /// </summary>
        public static object GetRootKey(object key)
        {
            while (key is AnimancerState state)
            {
                var stateKey = state.Key;
                if (stateKey == null)
                    break;

                key = stateKey;
            }

            return key;
        }

        /// <summary>
        /// If a state is registered with the `key`, this method gets it and repeats that check then returns the last
        /// state found.
        /// </summary>
        public static object GetLastKey(AnimancerStateDictionary states, object key)
        {
            while (states.TryGet(key, out var state))
                key = state;

            return key;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="PlayableGraph.Connect{U, V}(U, int, V, int)"/> using output 0 from the `child` and
        /// <see cref="PlayableExtensions.SetInputWeight{U}(U, int, float)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Connect<TParent, TChild>(
            this PlayableGraph graph,
            TParent parent,
            TChild child,
            int parentInputIndex,
            float weight)
            where TParent : struct, IPlayable
            where TChild : struct, IPlayable
        {
            graph.Connect(child, 0, parent, parentInputIndex);
            parent.SetInputWeight(parentInputIndex, weight);
        }

        /************************************************************************************************************************/

        /// <summary>Applies the `child`'s current <see cref="AnimancerNode.Weight"/>.</summary>
        public static void ApplyChildWeight(this Playable parent, AnimancerNode child)
            => parent.SetInputWeight(child.Index, child.Weight);

        /// <summary>
        /// Sets and applies the `child`'s <see cref="AnimancerNode.Weight"/>
        /// and <see cref="AnimancerState.IsActive"/>.
        /// </summary>
        public static void SetChildWeight(this Playable parent, AnimancerState child, float weight)
        {
            if (child._Weight == weight)
                return;

            Validate.AssertSetWeight(child, weight);

            child._Weight = weight;
            child.ShouldBeActive = weight > 0 || child.IsPlaying;
            parent.SetInputWeight(child.Index, weight);
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only] Reconnects the input of the specified `playable` to its output.</summary>
        public static void RemovePlayable(Playable playable, bool destroy = true)
        {
            if (!playable.IsValid())
                return;

            Assert(playable.GetInputCount() == 1,
                $"{nameof(RemovePlayable)} can only be used on playables with 1 input.");
            Assert(playable.GetOutputCount() == 1,
                $"{nameof(RemovePlayable)} can only be used on playables with 1 output.");

            var input = playable.GetInput(0);
            if (!input.IsValid())
            {
                if (destroy)
                    playable.Destroy();
                return;
            }

            var graph = playable.GetGraph();
            var output = playable.GetOutput(0);

            if (output.IsValid())// Connected to another Playable.
            {
                if (destroy)
                {
                    playable.Destroy();
                }
                else
                {
                    Assert(output.GetInputCount() == 1,
                        $"{nameof(RemovePlayable)} can only be used on playables connected to a playable with 1 input.");
                    graph.Disconnect(output, 0);
                    graph.Disconnect(playable, 0);
                }

                graph.Connect(input, 0, output, 0);
            }
            else// Connected to the graph output.
            {
                var playableOutput = graph.FindOutput(playable);
                if (playableOutput.IsOutputValid())
                    playableOutput.SetSourcePlayable(input);

                if (destroy)
                    playable.Destroy();
                else
                    graph.Disconnect(playable, 0);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns the output connected to the `sourcePlayable` (if any).</summary>
        public static PlayableOutput FindOutput(this PlayableGraph graph, Playable sourcePlayable)
        {
            var handle = sourcePlayable.GetHandle();
            var outputCount = graph.GetOutputCount();
            for (int i = outputCount - 1; i >= 0; i--)
            {
                var output = graph.GetOutput(i);
                if (output.GetSourcePlayable().GetHandle() == handle)
                    return output;
            }

            return default;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Checks if any <see cref="AnimationClip"/> in the `source` has an animation event with the specified
        /// `functionName`.
        /// </summary>
        public static bool HasEvent(IAnimationClipCollection source, string functionName)
        {
            var clips = SetPool.Acquire<AnimationClip>();
            source.GatherAnimationClips(clips);

            foreach (var clip in clips)
            {
                if (HasEvent(clip, functionName))
                {
                    SetPool.Release(clips);
                    return true;
                }
            }
            SetPool.Release(clips);
            return false;
        }

        /// <summary>Checks if the `clip` has an animation event with the specified `functionName`.</summary>
        public static bool HasEvent(AnimationClip clip, string functionName)
        {
            var events = clip.events;
            for (int i = events.Length - 1; i >= 0; i--)
                if (events[i].functionName == functionName)
                    return true;

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] [Pro-Only]
        /// Calculates all thresholds in the `mixer` using the <see cref="AnimancerState.AverageVelocity"/> of each
        /// state on the X and Z axes.
        /// <para></para>
        /// Note that this method requires the <c>Root Transform Position (XZ) -> Bake Into Pose</c> toggle to be
        /// disabled in the Import Settings of each <see cref="AnimationClip"/> in the mixer.
        /// </summary>
        public static void CalculateThresholdsFromAverageVelocityXZ(this MixerState<Vector2> mixer)
        {
            mixer.ValidateThresholdCount();

            for (int i = mixer.ChildCount - 1; i >= 0; i--)
            {
                var state = mixer.GetChild(i);
                if (state == null)
                    continue;

                var averageVelocity = state.AverageVelocity;
                mixer.SetThreshold(i, new(averageVelocity.x, averageVelocity.z));
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a <see cref="NativeArray{T}"/> containing a single element so that it can be used like a reference
        /// in Unity's C# Job system which does not allow regular reference types.
        /// </summary>
        /// <remarks>Note that you must call <see cref="NativeArray{T}.Dispose()"/> when you're done with the array.</remarks>
        public static NativeArray<T> CreateNativeReference<T>()
            where T : struct
            => new(1, Allocator.Persistent, NativeArrayOptions.ClearMemory);

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a <see cref="NativeArray{T}"/> of <see cref="TransformStreamHandle"/>s for each of the `transforms`.
        /// </summary>
        /// <remarks>Note that you must call <see cref="NativeArray{T}.Dispose()"/> when you're done with the array.</remarks>
        public static NativeArray<TransformStreamHandle> ConvertToTransformStreamHandles(
            IList<Transform> transforms, Animator animator)
        {
            var count = transforms.Count;

            var boneHandles = new NativeArray<TransformStreamHandle>(
                count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < count; i++)
                boneHandles[i] = animator.BindStreamTransform(transforms[i]);

            return boneHandles;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a string stating that the `value` is unsupported.</summary>
        public static string GetUnsupportedMessage<T>(T value)
            => $"Unsupported {typeof(T).FullName}: {value}";

        /// <summary>Returns an exception stating that the `value` is unsupported.</summary>
        public static ArgumentException CreateUnsupportedArgumentException<T>(T value)
            => new(GetUnsupportedMessage(value));

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Collections
        /************************************************************************************************************************/

        /// <summary>
        /// If the `index` is within the `list`,
        /// this method outputs the `item` at that `index` and returns true.
        /// </summary>
        public static bool TryGet<T>(this IList<T> list, int index, out T item)
        {
            if ((uint)index < (uint)list.Count)
            {
                item = list[index];
                return true;
            }
            else
            {
                item = default;
                return false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the `index` is within the `list` and that `item` is not null,
        /// this method outputs it and returns true.
        /// </summary>
        public static bool TryGetObject<T>(this IList<T> list, int index, out T item)
            where T : Object
        {
            if (list.TryGet(index, out item) &&
                item != null)
                return true;

            item = default;
            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the `obj` is a <see cref="Component"/> or <see cref="GameObject"/>,
        /// this method outputs its `transform` and returns true.
        /// </summary>
        public static bool TryGetTransform(Object obj, out Transform transform)
        {
            if (obj is Component component)
            {
                transform = component.transform;
                return true;
            }
            else if (obj is GameObject gameObject)
            {
                transform = gameObject.transform;
                return true;
            }
            else
            {
                transform = null;
                return false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Ensures that the length and contents of `copyTo` match `copyFrom`.</summary>
        public static void CopyExactArray<T>(T[] copyFrom, ref T[] copyTo)
        {
            if (copyFrom == null)
            {
                copyTo = null;
                return;
            }

            var length = copyFrom.Length;
            SetLength(ref copyTo, length);
            Array.Copy(copyFrom, copyTo, length);
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] Swaps <c>array[a]</c> with <c>array[b]</c>.</summary>
        public static void Swap<T>(this T[] array, int a, int b)
            => (array[b], array[a]) = (array[a], array[b]);

        /************************************************************************************************************************/

        /// <summary>Are both lists the same size with the same items in the same order?</summary>
        public static bool ContentsAreEqual<T>(IList<T> a, IList<T> b)
        {
            if (a == null)
                return b == null;

            if (b == null)
                return false;

            var aCount = a.Count;
            var bCount = b.Count;
            if (aCount != bCount)
                return false;

            for (int i = 0; i < aCount; i++)
                if (!EqualityComparer<T>.Default.Equals(a[i], b[i]))
                    return false;

            return true;
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Is the `array` <c>null</c> or its <see cref="Array.Length"/> <c>0</c>?
        /// </summary>
        public static bool IsNullOrEmpty<T>(this T[] array)
            => array == null
            || array.Length == 0;

        /************************************************************************************************************************/

        /// <summary>
        /// If the `array` is <c>null</c> or its <see cref="Array.Length"/> isn't equal to the specified `length`, this
        /// method creates a new array with that `length` and returns <c>true</c>. Otherwise, it returns <c>false</c>
        /// and the array us unchanged.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="Array.Resize{T}(ref T[], int)"/>, this method doesn't copy over the contents of the old
        /// `array` into the new one.
        /// </remarks>
        public static bool SetLength<T>(ref T[] array, int length)
        {
            if (array != null && array.Length == length)
                return false;

            array = new T[length];
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>Resizes the `array` to be at least 1 larger and inserts the `item` at the specified `index`.</summary>
        /// <remarks>If the `index` is beyond the end of the array, it will be resized large enough to fit.</remarks>
        public static void InsertAt<T>(ref T[] array, int index, T item)
        {
            if (array == null)
            {
                array = new T[] { item };
            }
            else if (index >= array.Length)
            {
                Array.Resize(ref array, index + 1);
                array[index] = item;
            }
            else
            {
                var newArray = new T[array.Length + 1];
                Array.Copy(array, 0, newArray, 0, index);
                Array.Copy(array, index, newArray, index + 1, array.Length - index);
                newArray[index] = item;
                array = newArray;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Removes the item at the specified `index` and resizes the `array` to be 1 smaller.</summary>
        public static void RemoveAt<T>(ref T[] array, int index)
        {
            if (array == null ||
                array.Length == 0)
                return;

            var newArray = new T[array.Length - 1];
            Array.Copy(array, 0, newArray, 0, index);
            Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
            array = newArray;
        }

        /************************************************************************************************************************/

        /// <summary>Returns the `array`, or <see cref="Array.Empty{T}"/> if it was <c>null</c>.</summary>
        public static T[] NullIsEmpty<T>(this T[] array)
            => array
            ?? Array.Empty<T>();

        /************************************************************************************************************************/

        /// <summary>Returns a string containing the value of each element in `collection`.</summary>
        public static string DeepToString(
            this IEnumerable collection,
            string separator,
            Func<object, object> toString = null)
        {
            if (collection == null)
                return "null";
            else
                return DeepToString(collection.GetEnumerator(), separator, toString);
        }

        /// <summary>Returns a string containing the value of each element in `collection` (each on a new line).</summary>
        public static string DeepToString(
            this IEnumerable collection,
            Func<object, object> toString = null)
            => DeepToString(collection, Environment.NewLine, toString);

        /// <summary>Returns a string containing the value of each element in `enumerator`.</summary>
        public static string DeepToString(
            this IEnumerator enumerator,
            string separator,
            Func<object, object> toString = null)
        {
            var text = StringBuilderPool.Instance.Acquire();
            AppendDeepToString(text, enumerator, separator, toString);
            return text.ReleaseToString();
        }

        /// <summary>Returns a string containing the value of each element in `enumerator` (each on a new line).</summary>
        public static string DeepToString(
            this IEnumerator enumerator,
            Func<object, object> toString = null)
            => DeepToString(enumerator, Environment.NewLine, toString);

        /************************************************************************************************************************/

        /// <summary>Each element returned by `enumerator` is appended to `text`.</summary>
        public static void AppendDeepToString(
            StringBuilder text,
            IEnumerator enumerator,
            string separator,
            Func<object, object> toString = null)
        {
            text.Append("[]");
            var countIndex = text.Length - 1;
            var count = 0;

            while (enumerator.MoveNext())
            {
                text.Append(separator);
                text.Append('[');
                text.Append(count);
                text.Append("] = ");

                var value = enumerator.Current;
                if (toString != null)
                    value = toString(value);
                text.Append(ToStringOrNull(value));

                count++;
            }

            text.Insert(countIndex, count);
        }

        /************************************************************************************************************************/

        /// <summary>Returns the value registered in the `dictionary` using the `key`.</summary>
        /// <remarks>Returns <c>default</c>(<typeparamref name="TValue"/>) if nothing was registered.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue Get<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key)
        {
            dictionary.TryGetValue(key, out var value);
            return value;
        }

        /// <summary>Registers the `value` in the `dictionary` using the `key`, replacing any previous value.</summary>
        /// <remarks>
        /// This is identical to setting <c>dictionary[key] = value;</c>
        /// except the syntax matches <c>dictionary.Add(key, value);</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key,
            TValue value)
            => dictionary[key] = value;

        /************************************************************************************************************************/

        /// <summary>Removes any items from the `dictionary` that use destroyed objects as their key.</summary>
        public static void RemoveDestroyedObjects<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
            where TKey : Object
        {
            using (ListPool<TKey>.Instance.Acquire(out var oldObjects))
            {
                foreach (var obj in dictionary.Keys)
                    if (obj == null)
                        oldObjects.Add(obj);

                for (int i = 0; i < oldObjects.Count; i++)
                    dictionary.Remove(oldObjects[i]);
            }
        }

        /// <summary>
        /// Creates a new dictionary and returns true if it was null or calls <see cref="RemoveDestroyedObjects"/> and
        /// returns false if it wasn't.
        /// </summary>
        public static bool InitializeCleanDictionary<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary)
            where TKey : Object
        {
            if (dictionary == null)
            {
                dictionary = new();
                return true;
            }
            else
            {
                RemoveDestroyedObjects(dictionary);
                return false;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Animator Controllers
        /************************************************************************************************************************/

        /// <summary>Copies the value of the `parameter` from `copyFrom` to `copyTo`.</summary>
        public static void CopyParameterValue(Animator copyFrom, Animator copyTo, AnimatorControllerParameter parameter)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    copyTo.SetFloat(parameter.nameHash, copyFrom.GetFloat(parameter.nameHash));
                    break;

                case AnimatorControllerParameterType.Int:
                    copyTo.SetInteger(parameter.nameHash, copyFrom.GetInteger(parameter.nameHash));
                    break;

                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    copyTo.SetBool(parameter.nameHash, copyFrom.GetBool(parameter.nameHash));
                    break;

                default:
                    throw CreateUnsupportedArgumentException(parameter.type);
            }
        }

        /// <summary>Copies the value of the `parameter` from `copyFrom` to `copyTo`.</summary>
        public static void CopyParameterValue(AnimatorControllerPlayable copyFrom, AnimatorControllerPlayable copyTo, AnimatorControllerParameter parameter)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    copyTo.SetFloat(parameter.nameHash, copyFrom.GetFloat(parameter.nameHash));
                    break;

                case AnimatorControllerParameterType.Int:
                    copyTo.SetInteger(parameter.nameHash, copyFrom.GetInteger(parameter.nameHash));
                    break;

                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    copyTo.SetBool(parameter.nameHash, copyFrom.GetBool(parameter.nameHash));
                    break;

                default:
                    throw CreateUnsupportedArgumentException(parameter.type);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Gets the value of the `parameter` in the `animator`.</summary>
        public static object GetParameterValue(Animator animator, AnimatorControllerParameter parameter)
        {
            return parameter.type switch
            {
                AnimatorControllerParameterType.Float => animator.GetFloat(parameter.nameHash),
                AnimatorControllerParameterType.Int => animator.GetInteger(parameter.nameHash),
                AnimatorControllerParameterType.Bool or
                AnimatorControllerParameterType.Trigger => animator.GetBool(parameter.nameHash),
                _ => throw CreateUnsupportedArgumentException(parameter.type),
            };
        }

        /// <summary>Gets the value of the `parameter` in the `playable`.</summary>
        public static object GetParameterValue(AnimatorControllerPlayable playable, AnimatorControllerParameter parameter)
        {
            return parameter.type switch
            {
                AnimatorControllerParameterType.Float => playable.GetFloat(parameter.nameHash),
                AnimatorControllerParameterType.Int => playable.GetInteger(parameter.nameHash),
                AnimatorControllerParameterType.Bool or
                AnimatorControllerParameterType.Trigger => playable.GetBool(parameter.nameHash),
                _ => throw CreateUnsupportedArgumentException(parameter.type),
            };
        }

        /************************************************************************************************************************/

        /// <summary>Sets the `value` of the `parameter` in the `animator`.</summary>
        public static void SetParameterValue(Animator animator, AnimatorControllerParameter parameter, object value)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(parameter.nameHash, (float)value);
                    break;

                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(parameter.nameHash, (int)value);
                    break;

                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(parameter.nameHash, (bool)value);
                    break;

                case AnimatorControllerParameterType.Trigger:
                    if ((bool)value)
                        animator.SetTrigger(parameter.nameHash);
                    else
                        animator.ResetTrigger(parameter.nameHash);
                    break;

                default:
                    throw CreateUnsupportedArgumentException(parameter.type);
            }
        }

        /// <summary>Sets the `value` of the `parameter` in the `playable`.</summary>
        public static void SetParameterValue(AnimatorControllerPlayable playable, AnimatorControllerParameter parameter, object value)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    playable.SetFloat(parameter.nameHash, (float)value);
                    break;

                case AnimatorControllerParameterType.Int:
                    playable.SetInteger(parameter.nameHash, (int)value);
                    break;

                case AnimatorControllerParameterType.Bool:
                    playable.SetBool(parameter.nameHash, (bool)value);
                    break;

                case AnimatorControllerParameterType.Trigger:
                    if ((bool)value)
                        playable.SetTrigger(parameter.nameHash);
                    else
                        playable.ResetTrigger(parameter.nameHash);
                    break;

                default:
                    throw CreateUnsupportedArgumentException(parameter.type);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Math
        /************************************************************************************************************************/

        /// <summary>Loops the `value` so that <c>0 &lt;= value &lt; 1</c>.</summary>
        /// <remarks>This is more efficient than using <see cref="Wrap"/> with a <c>length</c> of 1.</remarks>
        public static float Wrap01(float value)
        {
            var valueAsDouble = (double)value;
            value = (float)(valueAsDouble - Math.Floor(valueAsDouble));
            return value < 1
                ? value
                : 0;
        }

        /// <summary>Loops the `value` so that <c>0 &lt;= value &lt; length</c>.</summary>
        /// <remarks>Unike <see cref="Mathf.Repeat"/>, this method will never return the `length`.</remarks>
        public static float Wrap(float value, float length)
        {
            var valueAsDouble = (double)value;
            var lengthAsDouble = (double)length;
            value = (float)(valueAsDouble - Math.Floor(valueAsDouble / lengthAsDouble) * lengthAsDouble);
            return value < length
                ? value
                : 0;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Rounds the `value` to the nearest integer using <see cref="MidpointRounding.AwayFromZero"/>.
        /// </summary>
        public static float Round(float value)
            => (float)Math.Round(value, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Rounds the `value` to be a multiple of the `multiple` using <see cref="MidpointRounding.AwayFromZero"/>.
        /// </summary>
        public static float Round(float value, float multiple)
            => Round(value / multiple) * multiple;

        /************************************************************************************************************************/

        /// <summary>The opposite of <see cref="Mathf.LerpUnclamped(float, float, float)"/>.</summary>
        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            if (a == b)
                return 0;
            else
                return (value - a) / (b - a);
        }

        /************************************************************************************************************************/

        /// <summary>Are the given values equal or both <see cref="float.NaN"/> (which wouldn't normally be equal)?</summary>
        public static bool IsEqualOrBothNaN(this float a, float b)
            => a == b
            || (float.IsNaN(a) && float.IsNaN(b));

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] Is the `value` not NaN or Infinity?</summary>
        /// <remarks>Newer versions of the .NET framework apparently have a <c>float.IsFinite</c> method.</remarks>
        public static bool IsFinite(this float value)
            => !float.IsNaN(value)
            && !float.IsInfinity(value);

        /// <summary>[Animancer Extension] Is the `value` not NaN or Infinity?</summary>
        /// <remarks>Newer versions of the .NET framework apparently have a <c>double.IsFinite</c> method.</remarks>
        public static bool IsFinite(this double value)
            => !double.IsNaN(value)
            && !double.IsInfinity(value);

        /// <summary>[Animancer Extension] Are all components of the `value` not NaN or Infinity?</summary>
        public static bool IsFinite(this Vector2 value)
            => value.x.IsFinite()
            && value.y.IsFinite();

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Hashing
        /************************************************************************************************************************/

        /// <summary>Returns a hash value from the given parameters.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash(int seed, int hash1, int hash2)
        {
            AddHash(ref seed, hash1);
            AddHash(ref seed, hash2);
            return seed;
        }

        /// <summary>Returns a hash value from the given parameters.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash(int seed, int hash1, int hash2, int hash3)
        {
            AddHash(ref seed, hash1);
            AddHash(ref seed, hash2);
            AddHash(ref seed, hash3);
            return seed;
        }

        /// <summary>Returns a hash value from the given parameters.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash(int seed, int hash1, int hash2, int hash3, int hash4)
        {
            AddHash(ref seed, hash1);
            AddHash(ref seed, hash2);
            AddHash(ref seed, hash3);
            AddHash(ref seed, hash4);
            return seed;
        }

        /************************************************************************************************************************/

        /// <summary>Includes `add` in the `hash`.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddHash(ref int hash, int add)
            => hash = hash * -1521134295 + add;

        /************************************************************************************************************************/

        /// <summary>Uses <see cref="EqualityComparer{T}.Default"/> to get a hash code.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SafeGetHashCode<T>(this T value)
            => EqualityComparer<T>.Default.GetHashCode(value);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Components
        /************************************************************************************************************************/

        /// <summary>Is the `obj` <c>null</c> or a destroyed <see cref="Object"/>?</summary>
        public static bool IsNullOrDestroyed(this object obj)
            => obj == null
            || (obj is Object unityObject && unityObject == null);

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Adds the specified type of <see cref="IAnimancerComponent"/>, links it to the `animator`, and returns it.
        /// </summary>
        public static T AddAnimancerComponent<T>(this Animator animator)
            where T : Component, IAnimancerComponent
        {
            var animancer = animator.gameObject.AddComponent<T>();
            animancer.Animator = animator;
            return animancer;
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Returns the <see cref="IAnimancerComponent"/> on the same <see cref="GameObject"/> as the `animator` if
        /// there is one. Otherwise this method adds a new one and returns it.
        /// </summary>
        public static T GetOrAddAnimancerComponent<T>(this Animator animator)
            where T : Component, IAnimancerComponent
        {
            if (animator.TryGetComponent<T>(out var component))
                return component;
            else
                return animator.AddAnimancerComponent<T>();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the first <typeparamref name="T"/> component on the `gameObject` or its parents or children (in
        /// that order).
        /// </summary>
        public static T GetComponentInParentOrChildren<T>(this GameObject gameObject)
            where T : class
        {
            if (gameObject == null)
                return null;

            var component = gameObject.GetComponentInParent<T>();
            if (component != null)
                return component;

            return gameObject.GetComponentInChildren<T>();
        }

        /// <summary>
        /// If the `component` is <c>null</c>, this method tries to find one on the `gameObject` or its parents or
        /// children (in that order).
        /// </summary>
        public static bool GetComponentInParentOrChildren<T>(this GameObject gameObject, ref T component)
            where T : class
        {
            if (gameObject == null)
                return false;

            if (component != null &&
                (component is not Object obj || obj != null))
                return false;

            component = gameObject.GetComponentInParentOrChildren<T>();
            return component is not null;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="GameObject"/> and `singleton` instance if it was null.</summary>
        /// <remarks>Calls <see cref="Object.DontDestroyOnLoad"/> on the instance.</remarks>
        public static T InitializeSingleton<T>(ref T singleton)
            where T : Behaviour
        {
            if (singleton != null)
                return singleton;

#if UNITY_EDITOR
            // In Edit Mode or if we enter Play Mode without a Domain Reload
            // there might already be an existing instance.
            // Object.FindObjectOfType won't find it for whatever reason.

            var instances = Resources.FindObjectsOfTypeAll<T>();
            for (int i = 0; i < instances.Length; i++)
            {
                singleton = instances[i];

                // Ignore prefabs if an instance gets saved in one.
                if (string.IsNullOrEmpty(singleton.gameObject.scene.path))
                    continue;

                singleton.enabled = true;
                return singleton;
            }

            // In Edit Mode, create a hidden object so we don't dirty the scene.
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                var gameObject = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags(
                    typeof(T).Name,
                    HideFlags.HideAndDontSave);
                singleton = gameObject.AddComponent<T>();
                return singleton;
            }
#endif

            // Otherwise, just create a regular instance.
            {
                var gameObject = new GameObject(typeof(T).Name);
                singleton = gameObject.AddComponent<T>();

                Object.DontDestroyOnLoad(gameObject);

                return singleton;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Editor
        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional]
        /// Throws an <see cref="UnityEngine.Assertions.AssertionException"/> if the `condition` is false.
        /// </summary>
        /// <remarks>
        /// This method is similar to <see cref="Debug.Assert(bool, object)"/>, but it throws an exception instead of
        /// just logging the `message`.
        /// </remarks>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void Assert(bool condition, object message)
        {
#if UNITY_ASSERTIONS
            if (!condition)
                throw new UnityEngine.Assertions.AssertionException(
                    message?.ToString() ?? "Assertion failed.",
                    null);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Conditional] Indicates that the `target` needs to be re-serialized.</summary>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void SetDirty(Object target)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(target);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Conditional]
        /// Applies the effects of the animation `clip` to the <see cref="Component.gameObject"/>.
        /// </summary>
        /// <remarks>This method is safe to call during <see cref="MonoBehaviour"/><c>.OnValidate</c>.</remarks>
        /// <param name="clip">The animation to apply. If <c>null</c>, this method does nothing.</param>
        /// <param name="component">
        /// The animation will be applied to an <see cref="Animator"/> or <see cref="Animation"/> component on the same
        /// object as this or on any of its parents or children. If <c>null</c>, this method does nothing.
        /// </param>
        /// <param name="time">Determines which part of the animation to apply (in seconds).</param>
        /// <seealso cref="EditModePlay"/>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void EditModeSampleAnimation(this AnimationClip clip, Component component, float time = 0)
        {
#if UNITY_EDITOR
            if (!ShouldEditModeSample(clip, component))
                return;

            var gameObject = component.gameObject;
            component = gameObject.GetComponentInParentOrChildren<Animator>();
            if (component == null)
            {
                component = gameObject.GetComponentInParentOrChildren<Animation>();
                if (component == null)
                    return;
            }

            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (!ShouldEditModeSample(clip, component))
                    return;

                clip.SampleAnimation(component.gameObject, time);
            };
        }

        private static bool ShouldEditModeSample(AnimationClip clip, Component component)
        {
            return
                !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode &&
                clip != null &&
                component != null &&
                !UnityEditor.EditorUtility.IsPersistent(component);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Conditional] Plays the specified `clip` if called in Edit Mode.</summary>
        /// <remarks>This method is safe to call during <see cref="MonoBehaviour"/><c>.OnValidate</c>.</remarks>
        /// <param name="clip">The animation to apply. If <c>null</c>, this method does nothing.</param>
        /// <param name="component">
        /// The animation will be played on an <see cref="IAnimancerComponent"/> on the same object as this or on any
        /// of its parents or children. If <c>null</c>, this method does nothing.
        /// </param>
        /// <seealso cref="EditModeSampleAnimation"/>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void EditModePlay(this AnimationClip clip, Component component)
        {
#if UNITY_EDITOR
            if (!ShouldEditModeSample(clip, component))
                return;

            if (component is not IAnimancerComponent animancer)
                animancer = component.gameObject.GetComponentInParentOrChildren<IAnimancerComponent>();

            if (!ShouldEditModePlay(animancer, clip))
                return;

            // If it's already initialized, play immediately.
            if (animancer.IsGraphInitialized)
            {
                animancer.Graph.Layers[0].Play(clip);
                return;
            }

            // Otherwise, delay it in case this was called at a bad time (such as during OnValidate).
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (ShouldEditModePlay(animancer, clip))
                    animancer.Graph.Layers[0].Play(clip);
            };
#endif
        }

#if UNITY_EDITOR
        private static bool ShouldEditModePlay(IAnimancerComponent animancer, AnimationClip clip)
            => ShouldEditModeSample(clip, animancer?.Animator)
            && (animancer is not Object obj || obj != null);
#endif

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

