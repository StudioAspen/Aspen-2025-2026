// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only]
    /// Manages the selection of pages in the <see cref="TransitionLibraryWindow"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibraryWindowPage
    [Serializable]
    public abstract class TransitionLibraryWindowPage : IComparable<TransitionLibraryWindowPage>
    {
        /************************************************************************************************************************/

        /// <summary>The window containing this page.</summary>
        public TransitionLibraryWindow Window { get; set; }

        /************************************************************************************************************************/

        /// <summary>The name of this page.</summary>
        public abstract string DisplayName { get; }

        /// <summary>The text to use for the tooltip on the help button while this page is visible.</summary>
        public abstract string HelpTooltip { get; }

        /************************************************************************************************************************/

        /// <summary>The sorting index of this page.</summary>
        public abstract int Index { get; }

        /// <summary>Compares the <see cref="Index"/>.</summary>
        public int CompareTo(TransitionLibraryWindowPage other)
            => Index.CompareTo(other.Index);

        /************************************************************************************************************************/

        /// <summary>Draws the GUI of this page.</summary>
        public abstract void OnGUI(Rect area);

        /************************************************************************************************************************/
    }
}

#endif

