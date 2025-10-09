// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Animancer
{
    /// <summary>A dictionary of <see cref="AnimancerState"/>s mapped to their <see cref="AnimancerState.Key"/>.</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states">
    /// States</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerStateDictionary
    public class AnimancerStateDictionary :
        IAnimationClipCollection,
        IEnumerable<AnimancerState>
    {
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerGraph"/> at the root of the graph.</summary>
        private readonly AnimancerGraph Graph;

        /************************************************************************************************************************/

        /// <summary><see cref="AnimancerState.Key"/> mapped to <see cref="AnimancerState"/>.</summary>
        private readonly Dictionary<object, AnimancerState>
            States = new();

        /************************************************************************************************************************/

        /// <summary>[Internal] Creates a new <see cref="AnimancerStateDictionary"/>.</summary>
        internal AnimancerStateDictionary(AnimancerGraph graph)
            => Graph = graph;

        /************************************************************************************************************************/

        /// <summary>The number of states that have been registered with a <see cref="AnimancerState.Key"/>.</summary>
        public int Count
            => States.Count;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public void AppendDescriptionOrOrphans(
            StringBuilder text,
            string separator = "\n")
        {
            string stateSeparator = null;

            foreach (var state in States.Values)
            {
                if (state.Parent != null)
                    continue;

                if (stateSeparator is null)
                {
                    text.Append(separator)
                        .Append("Orphan States:");

                    separator += Strings.Indent;
                    stateSeparator = separator + Strings.Indent;
                }

                text.Append(separator);
                state.AppendDescription(text, stateSeparator);
            }
        }

        /************************************************************************************************************************/
        #region Create
        /************************************************************************************************************************/

        /// <summary>Creates and returns a new <see cref="ClipState"/> to play the `clip`.</summary>
        /// <remarks>
        /// To create a state on a specific layer, use <c>animancer.Layers[x].CreateState(clip)</c> instead.
        /// <para></para>
        /// <see cref="AnimancerGraph.GetKey"/> is used to determine the <see cref="AnimancerState.Key"/>.
        /// </remarks>
        public ClipState Create(AnimationClip clip)
            => Create(Graph.GetKey(clip), clip);

        /// <summary>
        /// Creates and returns a new <see cref="ClipState"/> to play the `clip` and registers it with the `key`.
        /// </summary>
        /// <remarks>
        /// To create a state on a specific layer, use <c>animancer.Layers[x].CreateState(key, clip)</c> instead.
        /// </remarks>
        public ClipState Create(object key, AnimationClip clip)
        {
            var state = new ClipState(clip);
            state.SetGraph(Graph);
            state._Key = key;
            Register(state);
            return state;
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="GetOrCreate(AnimationClip, bool)"/> for each of the specified clips.</summary>
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1)
        {
            GetOrCreate(clip0);
            GetOrCreate(clip1);
        }

        /// <summary>Calls <see cref="GetOrCreate(AnimationClip, bool)"/> for each of the specified clips.</summary>
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2)
        {
            GetOrCreate(clip0);
            GetOrCreate(clip1);
            GetOrCreate(clip2);
        }

        /// <summary>Calls <see cref="GetOrCreate(AnimationClip, bool)"/> for each of the specified clips.</summary>
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2, AnimationClip clip3)
        {
            GetOrCreate(clip0);
            GetOrCreate(clip1);
            GetOrCreate(clip2);
            GetOrCreate(clip3);
        }

        /// <summary>Calls <see cref="GetOrCreate(AnimationClip, bool)"/> for each of the specified `clips`.</summary>
        public void CreateIfNew(params AnimationClip[] clips)
        {
            if (clips == null)
                return;

            var count = clips.Length;
            for (int i = 0; i < count; i++)
            {
                var clip = clips[i];
                if (clip != null)
                    GetOrCreate(clip);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Access
        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="AnimancerLayer.CurrentState"/> on layer 0.
        /// <para></para>
        /// Specifically, this is the state that was most recently started using any of the Play methods on that layer.
        /// States controlled individually via methods in the <see cref="AnimancerState"/> itself will not register in
        /// this property.
        /// </summary>
        public AnimancerState Current
            => Graph.Layers[0].CurrentState;

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="AnimancerGraph.GetKey"/> then returns the state registered with that key.</summary>
        /// <exception cref="ArgumentNullException">The key is null.</exception>
        /// <exception cref="KeyNotFoundException">No state is registered with the key.</exception>
        public AnimancerState this[AnimationClip clip]
            => States[Graph.GetKey(clip)];

        /// <summary>Returns the state registered with the <see cref="IHasKey.Key"/>.</summary>
        /// <exception cref="ArgumentNullException">The `key` is null.</exception>
        /// <exception cref="KeyNotFoundException">No state is registered with the `key`.</exception>
        public AnimancerState this[IHasKey hasKey]
            => States[hasKey.Key];

        /// <summary>Returns the state registered with the `key`.</summary>
        /// <exception cref="ArgumentNullException">The `key` is null.</exception>
        /// <exception cref="KeyNotFoundException">No state is registered with the `key`.</exception>
        public AnimancerState this[object key]
            => States[key];

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="AnimancerGraph.GetKey"/> then passes the key to
        /// <see cref="TryGet(object, out AnimancerState)"/> and returns the result.
        /// </summary>
        public bool TryGet(AnimationClip clip, out AnimancerState state)
        {
            if (clip == null)
            {
                state = null;
                return false;
            }

            return TryGet(Graph.GetKey(clip), out state);
        }

        /// <summary>
        /// Passes the <see cref="IHasKey.Key"/> into <see cref="TryGet(object, out AnimancerState)"/>
        /// and returns the result.
        /// </summary>
        public bool TryGet(IHasKey hasKey, out AnimancerState state)
        {
            if (hasKey == null)
            {
                state = null;
                return false;
            }

            return TryGet(hasKey.Key, out state);
        }

        /// <summary>
        /// If a `state` is registered with the `key`, this method outputs it and returns true. Otherwise the
        /// `state` is set to null and this method returns false.
        /// </summary>
        public bool TryGet(object key, out AnimancerState state)
        {
            if (key == null)
            {
                state = null;
                return false;
            }

            return States.TryGetValue(key, out state);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="AnimancerGraph.GetKey"/> and returns the state registered with that key
        /// or creates one if it doesn't exist.
        /// </summary>
        /// <remarks>
        /// If the state already exists but has the wrong <see cref="AnimancerState.Clip"/>, the `allowSetClip`
        /// parameter determines what will happen. False causes it to throw an <see cref="ArgumentException"/> while
        /// true allows it to change the <see cref="AnimancerState.Clip"/>. Note that the change is somewhat costly to
        /// performance so use with caution.
        /// </remarks>
        /// <exception cref="ArgumentException"/>
        public AnimancerState GetOrCreate(AnimationClip clip, bool allowSetClip = false)
            => GetOrCreate(Graph.GetKey(clip), clip, allowSetClip);

        /// <summary>
        /// Returns the state registered with the `transition`s <see cref="IHasKey.Key"/> if there is one.
        /// Otherwise this method uses <see cref="ITransition.CreateState"/> to create a new one
        /// and registers it with that key before returning it.
        /// </summary>
        public AnimancerState GetOrCreate(ITransition transition)
        {
            var key = transition.Key;
            if (!TryGet(key, out var state))
            {
                state = transition.CreateState();
                state._Key = key;
                state.SetGraph(Graph);
            }

            return state;
        }

        /// <summary>
        /// Returns the state which registered with the `key` or creates one if it doesn't exist.
        /// <para></para>
        /// If the state already exists but has the wrong <see cref="AnimancerState.Clip"/>, the `allowSetClip`
        /// parameter determines what will happen. False causes it to throw an <see cref="ArgumentException"/>
        /// while true allows it to change the <see cref="AnimancerState.Clip"/>.
        /// Note that the change is somewhat costly to performance so use with caution.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <remarks>See also: <see cref="AnimancerLayer.GetOrCreateState(object, AnimationClip, bool)"/></remarks>
        public AnimancerState GetOrCreate(object key, AnimationClip clip, bool allowSetClip = false)
        {
            if (TryGet(key, out var state))
            {
                // If a state exists with the 'key' but has the wrong clip, either change it or complain.
                if (!ReferenceEquals(state.Clip, clip))
                {
                    if (allowSetClip)
                    {
                        state.Clip = clip;
                    }
                    else
                    {
                        throw new ArgumentException(GetClipMismatchError(key, state.Clip, clip));
                    }
                }
            }
            else
            {
                state = Create(key, clip);
            }

            return state;
        }

        /************************************************************************************************************************/

        /// <summary>Returns an error message explaining that a state already exists with the specified `key`.</summary>
        public static string GetClipMismatchError(object key, AnimationClip oldClip, AnimationClip newClip)
            => $"A state already exists using the specified '{nameof(key)}', but has a different {nameof(AnimationClip)}:" +
            $"\n• Key: {key}" +
            $"\n• Old Clip: {oldClip}" +
            $"\n• New Clip: {newClip}";

        /************************************************************************************************************************/

        /// <summary>[Internal]
        /// Registers the `state` in this dictionary so the <see cref="AnimancerState.Key"/> can be used to get it
        /// later on using any of the lookup methods such as <see cref="this[object]"/> or
        /// <see cref="TryGet(object, out AnimancerState)"/>.
        /// </summary>
        /// <remarks>Does nothing if the <see cref="AnimancerState.Key"/> is <c>null</c>.</remarks>
        internal void Register(AnimancerState state)
        {
            var key = state._Key;
            if (key != null)
            {
#if UNITY_ASSERTIONS
                if (state.Graph != Graph)
                    throw new ArgumentException(
                        $"{nameof(AnimancerStateDictionary)} cannot register a state with a different {nameof(Graph)}: " + state);
#endif

                States.Add(key, state);
            }
        }

        /// <summary>[Internal] Removes the `state` from this dictionary (the opposite of <see cref="Register"/>).</summary>
        internal void Unregister(AnimancerState state)
        {
            var key = state._Key;
            if (key != null)
                States.Remove(key);
        }

        /************************************************************************************************************************/
        #region Enumeration
        /************************************************************************************************************************/
        // IEnumerable for 'foreach' statements.
        /************************************************************************************************************************/

        /// <summary>Returns an enumerator that will iterate through all registered states.</summary>
        public Dictionary<object, AnimancerState>.ValueCollection.Enumerator GetEnumerator()
            => States.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator<AnimancerState> IEnumerable<AnimancerState>.GetEnumerator()
            => GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipCollection"/>]
        /// Adds all the animations of states with a <see cref="AnimancerState.Key"/> to the `clips`.
        /// </summary>
        public void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            foreach (var state in States.Values)
                clips.GatherFromSource(state);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Destroy
        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="AnimancerState.Destroy"/> on the state associated with the `clip` (if any).
        /// Returns true if the state existed.
        /// </summary>
        public bool Destroy(AnimationClip clip)
        {
            if (clip == null)
                return false;

            return Destroy(Graph.GetKey(clip));
        }

        /// <summary>
        /// Calls <see cref="AnimancerState.Destroy"/> on the state associated with the <see cref="IHasKey.Key"/>
        /// (if any). Returns true if the state existed.
        /// </summary>
        public bool Destroy(IHasKey hasKey)
        {
            if (hasKey == null)
                return false;

            return Destroy(hasKey.Key);
        }

        /// <summary>
        /// Calls <see cref="AnimancerState.Destroy"/> on the state associated with the `key` (if any).
        /// Returns true if the state existed.
        /// </summary>
        public bool Destroy(object key)
        {
            if (!TryGet(key, out var state))
                return false;

            state.Destroy();
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="Destroy(AnimationClip)"/> on each of the `clips`.</summary>
        public void DestroyAll(IList<AnimationClip> clips)
        {
            if (clips == null)
                return;

            for (int i = clips.Count - 1; i >= 0; i--)
                Destroy(clips[i]);
        }

        /// <summary>Calls <see cref="Destroy(AnimationClip)"/> on each of the `clips`.</summary>
        public void DestroyAll(IEnumerable<AnimationClip> clips)
        {
            if (clips == null)
                return;

            foreach (var clip in clips)
                Destroy(clip);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="Destroy(AnimationClip)"/> on all states gathered by
        /// <see cref="IAnimationClipSource.GetAnimationClips"/>.
        /// </summary>
        public void DestroyAll(IAnimationClipSource source)
        {
            if (source == null)
                return;

            var clips = ListPool.Acquire<AnimationClip>();
            source.GetAnimationClips(clips);
            DestroyAll(clips);
            ListPool.Release(clips);
        }

        /// <summary>
        /// Calls <see cref="Destroy(AnimationClip)"/> on all states gathered by
        /// <see cref="IAnimationClipCollection.GatherAnimationClips"/>.
        /// </summary>
        public void DestroyAll(IAnimationClipCollection source)
        {
            if (source == null)
                return;

            var clips = SetPool.Acquire<AnimationClip>();
            source.GatherAnimationClips(clips);
            DestroyAll(clips);
            SetPool.Release(clips);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Key Error Methods
#if UNITY_EDITOR
        /************************************************************************************************************************/
        // These are overloads of other methods that take a System.Object key to ensure the user doesn't try to use an
        // AnimancerState as a key, since the whole point of a key is to identify a state in the first place.
        /************************************************************************************************************************/

        /// <summary>[Warning]
        /// You should not use an <see cref="AnimancerState"/> as a key.
        /// The whole point of a key is to identify a state in the first place.
        /// </summary>
        [Obsolete("You should not use an AnimancerState as a key. The whole point of a key is to identify a state in the first place.", true)]
        public AnimancerState this[AnimancerState key]
            => key;

        /// <summary>[Warning]
        /// You should not use an <see cref="AnimancerState"/> as a key.
        /// The whole point of a key is to identify a state in the first place.
        /// </summary>
        [Obsolete("You should not use an AnimancerState as a key. The whole point of a key is to identify a state in the first place.", true)]
        public bool TryGet(AnimancerState key, out AnimancerState state)
        {
            state = key;
            return true;
        }

        /// <summary>[Warning]
        /// You should not use an <see cref="AnimancerState"/> as a key.
        /// The whole point of a key is to identify a state in the first place.
        /// </summary>
        [Obsolete("You should not use an AnimancerState as a key. The whole point of a key is to identify a state in the first place.", true)]
        public AnimancerState GetOrCreate(AnimancerState key, AnimationClip clip)
            => key;

        /// <summary>[Warning]
        /// You should not use an <see cref="AnimancerState"/> as a key.
        /// Just call <see cref="AnimancerState.Destroy"/>.
        /// </summary>
        [Obsolete("You should not use an AnimancerState as a key. Just call AnimancerState.Destroy.", true)]
        public bool Destroy(AnimancerState key)
        {
            key.Destroy();
            return true;
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
    }
}

