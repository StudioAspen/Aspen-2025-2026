// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEngine;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only]
    /// Sorting algorithms for <see cref="Animancer.TransitionLibraries.TransitionLibraryDefinition.Transitions"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionSortMode
    public enum TransitionSortMode
    {
        /************************************************************************************************************************/

        /// <summary>Manual sorting.</summary>
        Custom,

        /// <summary>Based on the transition file names.</summary>
        Name,

        /// <summary>Based on the transition file paths.</summary>
        Path,

        /// <summary>Based on the transition types then file names.</summary>
        TypeThenName,

        /// <summary>Based on the transition types then file paths.</summary>
        TypeThenPath,

        /************************************************************************************************************************/
    }

    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibraryEditorData
    public partial class TransitionLibraryEditorData
    {
        /************************************************************************************************************************/

        [SerializeField]
        private TransitionSortMode _TransitionSortMode;

        /// <summary>[<see cref="SerializeField"/>] The algorithm to use for sorting transitions.</summary>
        public TransitionSortMode TransitionSortMode
        {
            get => _TransitionSortMode;
            set
            {
                if (_TransitionSortMode == value)
                    return;

                _TransitionSortMode = value;

                if (Library != null)
                    TransitionLibrarySort.Sort(Library);
            }
        }

        /************************************************************************************************************************/
    }
}

#endif

