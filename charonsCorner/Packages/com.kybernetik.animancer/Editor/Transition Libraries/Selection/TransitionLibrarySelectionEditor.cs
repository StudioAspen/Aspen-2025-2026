// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using Animancer.TransitionLibraries;
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only]
    /// A custom Inspector for <see cref="TransitionLibrarySelection"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibrarySelectionEditor
    [CustomEditor(typeof(TransitionLibrarySelection), true)]
    public class TransitionLibrarySelectionEditor : UnityEditor.Editor
    {
        /************************************************************************************************************************/

        /// <summary>Casts the <see cref="UnityEditor.Editor.target"/>.</summary>
        public TransitionLibrarySelection Target
            => target as TransitionLibrarySelection;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            var target = Target;
            if (target == null || !target.Validate())
                return;

            EditorGUI.BeginChangeCheck();

            switch (target.Type)
            {
                case TransitionLibrarySelection.SelectionType.Library:
                    DoNestedEditorGUI(target.Selected as TransitionLibraryAsset, "Transition Library");
                    break;

                case TransitionLibrarySelection.SelectionType.FromTransition:
                case TransitionLibrarySelection.SelectionType.ToTransition:
                    DoTransitionGUI(target.Selected as TransitionAssetBase);
                    break;

                case TransitionLibrarySelection.SelectionType.Modifier:
                    DoModifierGUI(target, (TransitionModifierDefinition)target.Selected);
                    break;

                default:
                    target.Deselect();
                    break;
            }

            if (EditorGUI.EndChangeCheck())
                target.Window.Repaint();
        }

        /************************************************************************************************************************/
        #region Nested Editor
        /************************************************************************************************************************/

        [NonSerialized] private readonly CachedEditor NestedEditor = new();
        [NonSerialized] private readonly CachedEditor NestedEditor2 = new();

        /************************************************************************************************************************/

        /// <summary>Draws the <see cref="UnityEditor.Editor"/> for the `target`.</summary>
        private void DoNestedEditorGUI<T>(T target, string referenceLabel)
            where T : Object
        {
            using (new EditorGUI.DisabledScope(true))
                AnimancerGUI.DoObjectFieldGUI(referenceLabel, target, false);

            var editor = NestedEditor.GetEditor(target);
            if (editor != null)
                editor.OnInspectorGUI();
        }

        /************************************************************************************************************************/

        /// <summary>Cleans up any nested editors.</summary>
        protected virtual void OnDestroy()
        {
            NestedEditor.Dispose();
            NestedEditor2.Dispose();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Transitions
        /************************************************************************************************************************/

        /// <summary>Draws the GUI for the `transition`.</summary>
        private void DoTransitionGUI(
            TransitionAssetBase transition)
        {
            DoTransitionNameGUI(transition);
            DoNestedEditorGUI(transition, "Transition Asset");
        }

        /************************************************************************************************************************/

        /// <summary>Draws a field for editing the name of the `transition`.</summary>
        private void DoTransitionNameGUI(
            TransitionAssetBase transition)
        {
            var isSubAsset = AssetDatabase.IsSubAsset(transition);
            var isMainAsset = !isSubAsset && AssetDatabase.IsMainAsset(transition);
            var label = isSubAsset
                ? "Sub-Asset Name"
                : isMainAsset
                ? "File Name"
                : "Name";

            EditorGUI.BeginChangeCheck();

            var name = TransitionModifierTableGUI.GetTransitionName(transition);
            name = EditorGUILayout.DelayedTextField(label, name);

            if (EditorGUI.EndChangeCheck() && transition != null)
            {
                transition.SetName(name);

                if (isSubAsset)
                {
                    AssetDatabase.SaveAssets();
                }
                else if (isMainAsset)
                {
                    AssetDatabase.RenameAsset(
                        AssetDatabase.GetAssetPath(transition),
                        name);
                }
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Modifiers
        /************************************************************************************************************************/

        private static readonly BoolPref
            IsFromExpanded = new($"{nameof(TransitionLibrarySelectionEditor)}.{nameof(IsFromExpanded)}"),
            IsToExpanded = new($"{nameof(TransitionLibrarySelectionEditor)}.{nameof(IsToExpanded)}");

        /************************************************************************************************************************/

        /// <summary>Draws the GUI for the `modifier`.</summary>
        private void DoModifierGUI(
            TransitionLibrarySelection selection,
            TransitionModifierDefinition modifier)
        {
            var library = selection.Window.Data;
            DoTransitionField(library, NestedEditor, IsFromExpanded, modifier.FromIndex, "From");
            DoTransitionField(library, NestedEditor2, IsToExpanded, modifier.ToIndex, "To");

            var area = AnimancerGUI.LayoutSingleLineRect();
            TransitionModifierTableGUI.DoFadeDurationGUI(
                area,
                selection.Window,
                modifier.FromIndex,
                modifier.ToIndex,
                "Fade Duration");
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI for a transition.</summary>
        private TransitionAssetBase DoTransitionField(
            TransitionLibraryDefinition library,
            CachedEditor cachedEditor,
            BoolPref isExpanded,
            int transitionIndex,
            string label)
        {
            library.TryGetTransition(transitionIndex, out var transition);

            var area = AnimancerGUI.LayoutSingleLineRect(AnimancerGUI.SpacingMode.After);
            var labelArea = area;
            labelArea.width = EditorGUIUtility.labelWidth;

            isExpanded.Value = EditorGUI.Foldout(labelArea, isExpanded, GUIContent.none, true);

            var enabled = GUI.enabled;
            GUI.enabled = false;

            AnimancerGUI.DoObjectFieldGUI(area, label, transition, false);

            GUI.enabled = enabled;

            if (isExpanded)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                var editor = cachedEditor.GetEditor(transition);
                editor.OnInspectorGUI();

                GUILayout.EndVertical();
            }

            return transition;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

