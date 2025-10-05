// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using Animancer.TransitionLibraries;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only]
    /// Additional data for a <see cref="TransitionLibraryAsset"/> which is excluded from Runtime Builds.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibraryEditorData
    [AnimancerHelpUrl(typeof(TransitionLibraryEditorData))]
    public partial class TransitionLibraryEditorData : ScriptableObject
    {
        /************************************************************************************************************************/

        private static readonly Dictionary<TransitionLibraryAsset, TransitionLibraryEditorData>
            LibraryToEditorData = new();

        /************************************************************************************************************************/

        [SerializeField, HideInInspector]
        private TransitionLibraryAsset _Library;

        /// <summary>The library this data is associated with.</summary>
        public TransitionLibraryAsset Library
        {
            get => _Library;
            private set
            {
                if (_Library == value)
                    return;

                if (_Library != null)
                    LibraryToEditorData.Remove(_Library);

                _Library = value;
                EditorUtility.SetDirty(this);

                if (_Library != null)
                    LibraryToEditorData.Add(_Library, this);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Registers this data for the <see cref="Library"/>.</summary>
        protected virtual void OnEnable()
        {
            if (_Library != null)
                LibraryToEditorData[_Library] = this;
        }

        /// <summary>Un-registers this data for the <see cref="Library"/>.</summary>
        protected virtual void OnDisable()
        {
            if (_Library != null)
                LibraryToEditorData.Remove(_Library);
        }

        /************************************************************************************************************************/

        /// <summary>Tries to get the `data` associated with the `library`.</summary>
        private static bool TryGet(
            TransitionLibraryAsset library,
            out TransitionLibraryEditorData data)
        {
            if (!LibraryToEditorData.TryGetValue(library, out data))
                return false;

            if (data != null)
            {
                data.Library = library;
                return true;
            }

            LibraryToEditorData.Remove(library);
            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the <see cref="TransitionLibraryEditorData"/> sub-asset of the `library` if one exists.
        /// </summary>
        public static TransitionLibraryEditorData GetEditorData(TransitionLibraryAsset library)
        {
            if (TryGet(library, out var data))
                return data;

            var assetPath = AssetDatabase.GetAssetPath(library);
            if (string.IsNullOrEmpty(assetPath))
                return null;

            var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            for (int i = 0; i < subAssets.Length; i++)
            {
                if (subAssets[i] is TransitionLibraryEditorData editorData)
                {
                    editorData.Library = library;
                    return editorData;
                }
            }

            return null;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the <see cref="TransitionLibraryEditorData"/> sub-asset of the `library` if one exists.
        /// Otherwise, creates and saves a new one.
        /// </summary>
        public static TransitionLibraryEditorData GetOrCreateEditorData(TransitionLibraryAsset library)
        {
            var data = library.GetEditorData();
            if (data != null)
                return data;

            data = CreateInstance<TransitionLibraryEditorData>();
            data.name = "Editor Data";
            data.hideFlags = HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
            data.Library = library;

            EditorApplication.CallbackFunction addSubAsset = null;

            addSubAsset = () =>
            {
                if (AssetDatabase.Contains(library))
                {
                    EditorApplication.update -= addSubAsset;

                    AssetDatabase.AddObjectToAsset(data, library);
                    AssetDatabase.SaveAssets();
                }
            };

            EditorApplication.update += addSubAsset;

            return data;
        }

        /************************************************************************************************************************/
    }

    /// <summary>[Editor-Only] Extension methods for <see cref="TransitionLibraryEditorData"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibraryEditorDataExtensions
    public static class TransitionLibraryEditorDataExtensions
    {
        /************************************************************************************************************************/

        /// <summary><see cref="TransitionLibraryEditorData.GetEditorData"/></summary>
        public static TransitionLibraryEditorData GetEditorData(this TransitionLibraryAsset library)
            => TransitionLibraryEditorData.GetEditorData(library);

        /// <summary><see cref="TransitionLibraryEditorData.GetOrCreateEditorData"/></summary>
        public static TransitionLibraryEditorData GetOrCreateEditorData(this TransitionLibraryAsset library)
            => TransitionLibraryEditorData.GetOrCreateEditorData(library);

        /************************************************************************************************************************/
    }
}

#endif

