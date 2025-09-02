// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using Animancer.TransitionLibraries;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only] Utility for sorting a <see cref="TransitionLibraryAsset"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibrarySort
    public class TransitionLibrarySort : AssetModificationProcessor
    {
        /************************************************************************************************************************/
        #region Automation
        /************************************************************************************************************************/

        /// <summary>Ensures that a <see cref="TransitionLibraryAsset"/> is sorted before being saved.</summary>
        private static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                if (!path.EndsWith(".asset", StringComparison.Ordinal))
                    continue;

                var library = AssetDatabase.LoadAssetAtPath<TransitionLibraryAsset>(path);
                if (library == null)
                    continue;

                Sort(library);
            }

            return paths;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Sort Modes
        /************************************************************************************************************************/

        /// <summary>Applies the <see cref="TransitionLibraryEditorData.TransitionSortMode"/>.</summary>
        public static void Sort(TransitionLibraryAsset library)
        {
            // Can't have editor data if not an asset, so the sort mode will be custom anyway.
            if (!AssetDatabase.Contains(library))
                return;

            var data = library.GetOrCreateEditorData();
            if (data.TransitionSortMode == TransitionSortMode.Custom)
                return;

            NameCache.Clear();

            switch (data.TransitionSortMode)
            {
                case TransitionSortMode.Name:
                    Sort(library.Definition, Static<CompareName>.Instance);
                    break;

                case TransitionSortMode.Path:
                    Sort(library.Definition, Static<ComparePath>.Instance);
                    break;

                case TransitionSortMode.TypeThenName:
                    Sort(library.Definition, Static<CompareTypeThenName>.Instance);
                    break;

                case TransitionSortMode.TypeThenPath:
                    Sort(library.Definition, Static<CompareTypeThenPath>.Instance);
                    break;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Compares the asset names then GUIDs.</summary>
        private class CompareName : IComparer<TransitionAssetBase>
        {
            public int Compare(TransitionAssetBase a, TransitionAssetBase b)
            {
                var result = CompareNulls(a, b);
                if (result != 0)
                    return result;

                result = CompareCachedNames(a, b);
                if (result != 0)
                    return result;

                return CompareGUIDs(a, b);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Compares the asset paths then GUIDs.</summary>
        private class ComparePath : IComparer<TransitionAssetBase>
        {
            public int Compare(TransitionAssetBase a, TransitionAssetBase b)
            {
                var result = CompareNulls(a, b);
                if (result != 0)
                    return result;

                result = ComparePaths(a, b);
                if (result != 0)
                    return result;

                result = CompareCachedNames(a, b);
                if (result != 0)
                    return result;

                return CompareGUIDs(a, b);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Compares the transition types then asset names then GUIDs.</summary>
        private class CompareTypeThenName : IComparer<TransitionAssetBase>
        {
            public int Compare(TransitionAssetBase a, TransitionAssetBase b)
            {
                var result = CompareNulls(a, b);
                if (result != 0)
                    return result;

                result = CompareTypes(a, b);
                if (result != 0)
                    return result;

                result = CompareCachedNames(a, b);
                if (result != 0)
                    return result;

                return CompareGUIDs(a, b);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Compares the transition types then asset paths then GUIDs.</summary>
        private class CompareTypeThenPath : IComparer<TransitionAssetBase>
        {
            public int Compare(TransitionAssetBase a, TransitionAssetBase b)
            {
                var result = CompareNulls(a, b);
                if (result != 0)
                    return result;

                result = CompareTypes(a, b);
                if (result != 0)
                    return result;

                result = ComparePaths(a, b);
                if (result != 0)
                    return result;

                result = CompareCachedNames(a, b);
                if (result != 0)
                    return result;

                return CompareGUIDs(a, b);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Compares objects to put null or destroyed ones at the end.</summary>
        private static int CompareNulls(TransitionAssetBase a, TransitionAssetBase b)
            => (a == null).CompareTo(b == null);

        /// <summary>Compares the asset GUIDs.</summary>
        private static int CompareGUIDs(TransitionAssetBase a, TransitionAssetBase b)
        {
            var gotA = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(a, out var aGUID, out long aLocalID);
            var gotB = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(b, out var bGUID, out long bLocalID);
            var result = gotA.CompareTo(gotB);
            if (result != 0)
                return result;

            result = aGUID.CompareTo(bGUID);
            if (result != 0)
                return result;

            return aLocalID.CompareTo(bLocalID);
        }

        /// <summary>Compares the asset names.</summary>
        private static int CompareCachedNames(TransitionAssetBase a, TransitionAssetBase b)
            => a.GetCachedName().CompareTo(b.GetCachedName());

        /// <summary>Compares the asset paths.</summary>
        private static int ComparePaths(TransitionAssetBase a, TransitionAssetBase b)
            => AssetDatabase.GetAssetPath(a).CompareTo(AssetDatabase.GetAssetPath(b));

        /// <summary>Compares the transition types.</summary>
        private static int CompareTypes(TransitionAssetBase a, TransitionAssetBase b)
        {
            if (AnimancerUtilities.TryGetWrappedObject<ITransition>(a, out var transitionA) &&
                AnimancerUtilities.TryGetWrappedObject<ITransition>(b, out var transitionB))
            {
                var result = transitionA.GetType().GetNameCS().CompareTo(transitionB.GetType().GetNameCS());
                if (result != 0)
                    return result;
            }

            return a.GetType().GetNameCS().CompareTo(b.GetType().GetNameCS());
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Sorting
        /************************************************************************************************************************/

        private static TransitionAssetBase[]
            _SortingTransitions = Array.Empty<TransitionAssetBase>();

        private static int[] _OldIndexToNew;

        /************************************************************************************************************************/

        /// <summary>Sorts the <see cref="TransitionLibraryDefinition.Transitions"/>.</summary>
        public static void Sort(
            TransitionLibraryDefinition library,
            Comparison<TransitionAssetBase> comparison)
            => Sort(library, new Comparison<TransitionAssetBase>(comparison));

        /// <summary>Sorts the <see cref="TransitionLibraryDefinition.Transitions"/>.</summary>
        public static void Sort(
            TransitionLibraryDefinition library,
            IComparer<TransitionAssetBase> comparer)
        {
            var transitions = library.Transitions;
            var count = transitions.Length;

            if (_SortingTransitions.Length < count)
            {
                var length = Mathf.NextPowerOfTwo(count);
                _SortingTransitions = new TransitionAssetBase[length];
                _OldIndexToNew = new int[length];
            }

            Array.Copy(transitions, _SortingTransitions, count);

            // Indices 0 -> Count.
            var newIndexToOld = GetTempSequentialIndices(count);

            Array.Sort(_SortingTransitions, newIndexToOld, 0, count, comparer);

            // Remove nulls which should have been sorted to the end.
            for (int i = count - 1; i >= 0; i--)
                if (_SortingTransitions[i] == null)
                    count--;
                else
                    break;

            // _NewIndexToOld[x] is now the index that Transitions[x] was at previously.
            // We need to invert that so _OldIndexToNew[x] is the new index of whatever was previously at Transitions[x].
            // That allows the library to update any index references using a simple x = _OldIndexToNew[x];

            for (int i = 0; i < count; i++)
                _OldIndexToNew[newIndexToOld[i]] = i;

            SetTransitions(library, _SortingTransitions, _OldIndexToNew, count);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the <see cref="TransitionLibraryDefinition.Transitions"/>
        /// using `oldIndexToNew` to remap any references to the old order.
        /// </summary>
        public static void SetTransitions(
            TransitionLibraryDefinition library,
            TransitionAssetBase[] transitions,
            int[] oldIndexToNew,
            int count)
        {
            var libraryTransitions = library.Transitions;
            if (libraryTransitions != transitions)
            {
                AnimancerUtilities.SetLength(ref libraryTransitions, count);
                Array.Copy(transitions, libraryTransitions, count);
                library.Transitions = libraryTransitions;
            }

            var modifiers = library.Modifiers;
            for (int i = modifiers.Length - 1; i >= 0; i--)
            {
                var modifier = modifiers[i];
                var isValid = true;
                var fromIndex = ConvertIndex(modifier.FromIndex, oldIndexToNew, count, ref isValid);
                var toIndex = ConvertIndex(modifier.ToIndex, oldIndexToNew, count, ref isValid);

                if (isValid)
                    modifiers[i] = modifier.WithIndices(fromIndex, toIndex);
                else
                    AnimancerUtilities.RemoveAt(ref modifiers, i);
            }

            var aliases = library.Aliases;
            for (int i = aliases.Length - 1; i >= 0; i--)
            {
                var alias = aliases[i];
                var isValid = true;
                var index = ConvertIndex(alias.Index, oldIndexToNew, count, ref isValid);

                if (isValid)
                    aliases[i] = alias.With(index);
                else
                    AnimancerUtilities.RemoveAt(ref aliases, i);
            }

            library.SortAliases();
        }

        /************************************************************************************************************************/

        /// <summary>Converts an old index to a new one.</summary>
        private static int ConvertIndex(int index, int[] oldIndexToNew, int count, ref bool isValid)
        {
            if ((uint)index >= (uint)count)
            {
                isValid = false;
                return -1;
            }

            index = oldIndexToNew[index];

            if ((uint)index >= (uint)count)
            {
                isValid = false;
                return -1;
            }

            return index;
        }

        /************************************************************************************************************************/

        private static int[] _SequentialIndices = Array.Empty<int>();

        /// <summary>Returns a cached array containing sequential indices, i.e. <c>array[i] = i</c>.</summary>
        public static int[] GetTempSequentialIndices(int count)
        {
            if (_SequentialIndices.Length < count)
                _SequentialIndices = new int[Mathf.NextPowerOfTwo(count)];

            for (int i = 0; i < _SequentialIndices.Length; i++)
                _SequentialIndices[i] = i;

            return _SequentialIndices;
        }

        /************************************************************************************************************************/

        /// <summary>Changes the index of a transition.</summary>
        public static void MoveTransition(TransitionLibraryWindow window, int from, int to)
        {
            var transitions = window.Data.Transitions;

            to = Mathf.Clamp(to, 0, transitions.Length - 1);
            if (from == to)
                return;

            var editorData = window.SourceObject.GetOrCreateEditorData();

            var definition = window.RecordUndo();

            editorData.TransitionSortMode = TransitionSortMode.Custom;

            var moving = transitions[from];
            var indices = GetTempSequentialIndices(transitions.Length);

            if (to > from)// Moving forwards.
            {
                Array.Copy(transitions, from + 1, transitions, from, to - from);
                Array.Copy(indices, from, indices, from + 1, to - from);
            }
            else// Moving backwards.
            {
                Array.Copy(transitions, to, transitions, to + 1, from - to);
                Array.Copy(indices, to + 1, indices, to, from - to);
            }

            transitions[to] = moving;
            indices[from] = to;

            SetTransitions(
                definition,
                transitions,
                indices,
                transitions.Length);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

