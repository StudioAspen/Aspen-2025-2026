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
    /// A custom Inspector for <see cref="TransitionLibraryAsset"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibraryAssetEditor
    [CustomEditor(typeof(TransitionLibraryAsset), true)]
    public class TransitionLibraryAssetEditor : UnityEditor.Editor
    {
        /************************************************************************************************************************/

        private static GUIStyle _HeaderStyle;

        /// <summary>Style for section headers.</summary>
        public static GUIStyle HeaderStyle
            => _HeaderStyle ??= new(EditorStyles.label)
            {
                fontSize = EditorStyles.label.fontSize * 2,
            };

        /************************************************************************************************************************/

        [NonSerialized]
        private SerializedProperty _AliasAllTransitions;

        /************************************************************************************************************************/

        /// <summary>Called when a <see cref="TransitionLibraryAsset"/> is selected.</summary>
        protected virtual void OnEnable()
        {
            _AliasAllTransitions = serializedObject.FindProperty(
                 TransitionLibraryAsset.DefinitionField + "." + TransitionLibraryDefinition.AliasAllTransitionsField);
        }

        /************************************************************************************************************************/

        /// <summary>Called when a <see cref="TransitionLibraryAsset"/> is deselected.</summary>
        protected virtual void OnDestroy()
        {
            NestedEditor.Dispose();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            var library = target as TransitionLibraryAsset;
            if (library == null)
                return;

            DoMainButtonsGUI(library);
            DoSettingsGUI(library);
            DoEditorDataGUI(library);
            DoSubAssetWarningGUI(library);
        }

        /************************************************************************************************************************/

        /// <summary>Draws several buttons with utility functions.</summary>
        private void DoMainButtonsGUI(TransitionLibraryAsset library)
        {
            var editLabel = TransitionLibraryWindow.IsShowing(library)
                ? "Currently Editing"
                : "Edit";
            if (GUILayout.Button(editLabel))
                TransitionLibraryWindow.Open(library);

            using (var label = PooledGUIContent.Acquire("Documentation", Strings.DocsURLs.TransitionLibraries))
                if (GUILayout.Button(label))
                    Application.OpenURL(Strings.DocsURLs.TransitionLibraries);
        }

        /************************************************************************************************************************/

        /// <summary>Draws the `library`'s main settings.</summary>
        private void DoSettingsGUI(TransitionLibraryAsset library)
        {
            GUILayout.Space(AnimancerGUI.LineHeight);
            GUILayout.Label("Settings", HeaderStyle);

            EditorGUILayout.PropertyField(_AliasAllTransitions);
        }

        /************************************************************************************************************************/

        [NonSerialized] private readonly CachedEditor NestedEditor = new();

        /// <summary>Draws the `library`'s <see cref="TransitionLibraryEditorData"/>.</summary>
        private void DoEditorDataGUI(TransitionLibraryAsset library)
        {
            GUILayout.Space(AnimancerGUI.LineHeight);
            GUILayout.Label("Editor-Only Settings", HeaderStyle);

            var data = library.GetOrCreateEditorData();
            var editor = NestedEditor.GetEditor(data);
            editor.OnInspectorGUI();
        }

        /************************************************************************************************************************/

        /// <summary>Draws warnings about any sub-assets which aren't actually referenced by the `library`.</summary>
        private void DoSubAssetWarningGUI(TransitionLibraryAsset library)
        {
            var assetPath = AssetDatabase.GetAssetPath(library);
            if (string.IsNullOrEmpty(assetPath))
                return;

            var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < subAssets.Length; i++)
                DoSubAssetWarningGUI(library, assetPath, subAssets[i]);
        }

        /// <summary>Draws a warning about the `subAsset` if it isn't actually referenced by the `library`.</summary>
        private void DoSubAssetWarningGUI(
            TransitionLibraryAsset library,
            string assetPath,
            Object subAsset)
        {
            switch (subAsset)
            {
                case TransitionAssetBase transition:
                    if (Array.IndexOf(library.Definition.Transitions, transition) < 0)
                        break;

                    return;

                case StringAsset alias:
                    var aliases = library.Definition.Aliases;
                    for (int i = 0; i < aliases.Length; i++)
                        if (aliases[i].Name == alias)
                            return;

                    break;

                default:
                    return;
            }

            EditorGUILayout.HelpBox(
                $"Sub-Asset '{subAsset.name}' isn't referenced by this Transition Library." +
                $" Click to ping. Shift + Click to delete.",
                MessageType.Warning);

            if (AnimancerGUI.TryUseClickEventInLastRect(0))
            {
                if (Event.current.shift)
                {
                    if (EditorUtility.DisplayDialog("Delete Sub-Asset",
                        $"Are you sure you want to delete '{subAsset.name}'" +
                        $" inside {assetPath}?" +
                        $"\n\nThis operation cannot be undone.",
                        "Delete",
                        "Cancel"))
                        AnimancerEditorUtilities.DeleteSubAsset(subAsset);
                }
                else
                {
                    EditorGUIUtility.PingObject(subAsset);
                }
            }
        }

        /************************************************************************************************************************/
    }
}

#endif

