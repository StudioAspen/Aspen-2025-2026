// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only] Custom Inspector for <see cref="TransitionLibraryEditorData"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibraryEditorDataEditor
    [CustomEditor(typeof(TransitionLibraryEditorData), true)]
    public class TransitionLibraryEditorDataEditor : UnityEditor.Editor
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            var target = this.target as TransitionLibraryEditorData;
            if (target == null)
                return;

            var transitionSortMode = target.TransitionSortMode;

            base.OnInspectorGUI();

            if (transitionSortMode != target.TransitionSortMode &&
                target.Library != null)
                TransitionLibrarySort.Sort(target.Library);
        }

        /************************************************************************************************************************/
    }
}

#endif

