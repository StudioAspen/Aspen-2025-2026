// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> which holds a <see cref="StringReference"/>
    /// based on its <see cref="Object.name"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/StringAsset
    [AnimancerHelpUrl(typeof(StringAsset))]
    [CreateAssetMenu(
        menuName = Strings.MenuPrefix + "String Asset",
        order = Strings.AssetMenuOrder + 3)]
    public class StringAsset : ScriptableObject,
        IComparable<StringAsset>,
        IConvertable<StringReference>,
        IConvertable<string>,
        IHasKey
    {
        /************************************************************************************************************************/

        private StringReference _Name;

        /// <summary>A <see cref="StringReference"/> to the <see cref="Object.name"/>.</summary>
        /// <remarks>
        /// This value is gathered when first accessed, but will not be automatically updated after that
        /// because doing so causes some garbage allocation (except in the Unity Editor for convenience).
        /// </remarks>
        public StringReference Name
        {
#if UNITY_EDITOR
            // Don't do this at runtime because it allocates garbage every time.
            // But in the Unity Editor things could get renamed at any time.
            get => _Name = this ? name : "";
#else
            get => _Name ??= name;
#endif
            set => _Name = name = value;
        }

        /// <inheritdoc/>
        public object Key
            => Name;

        /************************************************************************************************************************/
        #region Comparison
        /************************************************************************************************************************/

        /// <summary>Compares the <see cref="StringReference.String"/>s.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(StringAsset a, StringAsset b)
            => a == b
            ? 0
            : a
            ? a.CompareTo(b)
            : -1;

        /// <summary>Compares the <see cref="StringReference.String"/>s.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(StringAsset other)
            => other
            ? Name.String.CompareTo(other.Name.String)
            : 1;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Conversion
        /************************************************************************************************************************/

        /// <summary>Returns the <see cref="Name"/>.</summary>
        public override string ToString()
            => Name;

        /// <inheritdoc/>
        StringReference IConvertable<StringReference>.Convert()
            => Name;

        /// <inheritdoc/>
        string IConvertable<string>.Convert()
            => Name;

        /************************************************************************************************************************/

        /// <summary>Returns the <see cref="Name"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(StringAsset key)
            => key?.Name;

        /// <summary>Returns the <see cref="Name"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringReference(StringAsset key)
            => key?.Name;

        /************************************************************************************************************************/

        /// <summary>Creates a new array containing the <see cref="Name"/>s.</summary>
        public static StringReference[] ToStringReferences(params StringAsset[] keys)
        {
            if (keys == null)
                return null;

            if (keys.Length == 0)
                return Array.Empty<StringReference>();

            var strings = new StringReference[keys.Length];
            for (int i = 0; i < keys.Length; i++)
                strings[i] = keys[i];
            return strings;
        }

        /// <summary>Creates a new array containing the <see cref="Name"/>s.</summary>
        public static string[] ToStrings(params StringAsset[] keys)
        {
            if (keys == null)
                return null;

            if (keys.Length == 0)
                return Array.Empty<string>();

            var strings = new string[keys.Length];
            for (int i = 0; i < keys.Length; i++)
                strings[i] = keys[i];
            return strings;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        [Tooltip("An unused Editor-Only field where you can explain what this asset is used for")]
        [SerializeField, TextArea(2, 25)]
        private string _EditorComment;

        /// <summary>[Editor-Only] [<see cref="SerializeField"/>]
        /// An unused Editor-Only field where you can explain what this asset is used for.
        /// </summary>
        public ref string EditorComment
            => ref _EditorComment;

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Returns a <see cref="StringAsset"/> with the specified `name` if one exists in the project.
        /// </summary>
        /// <remarks>If multiple assets have the same `name`, any one of them will be returned.</remarks>
        public static StringAsset Find(
            StringReference name,
            out string path)
        {
            var filter = $"{name} t:{nameof(StringAsset)}";
            var guids = UnityEditor.AssetDatabase.FindAssets(filter);

            for (int i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<StringAsset>(path);
                if (asset != null && asset.Name == name)
                    return asset;
            }

            path = null;
            return null;
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Saves a new <see cref="StringAsset"/> in the `directory` and returns it.</summary>
        /// <remarks>If no `directory` is specified, this method will ask the user to select a directory manually.</remarks>
        public static StringAsset Create(
            StringReference name,
            ref string directory,
            out string path)
        {
            if (string.IsNullOrEmpty(directory))
            {
                directory = UnityEditor.EditorUtility.SaveFolderPanel(
                    $"Select Folder to save String Asset - {name}",
                    "Assets",
                    "");

                if (string.IsNullOrEmpty(directory))
                {
                    path = null;
                    return null;
                }
            }

            var newAsset = CreateInstance<StringAsset>();
            newAsset.name = name;

            var workingDirectory = Environment.CurrentDirectory.Replace('\\', '/');
            if (directory.StartsWith(workingDirectory))
                directory = directory[(workingDirectory.Length + 1)..];

            path = System.IO.Path.Combine(directory, name + ".asset");

            UnityEditor.AssetDatabase.CreateAsset(newAsset, path);

            Debug.Log($"Created {nameof(StringAsset)}: {path}", newAsset);

            return newAsset;
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// If a <see cref="StringAsset"/> exists with the specified `name`, this method returns it.
        /// If multiple assets have the same name, any one of them will be returned.
        /// Otherwise, a new asset will be saved in the `createDirectory` and returned.
        /// </summary>
        /// <remarks>
        /// If no `createDirectory` is specified, this method will ask the user to select a directory manually.
        /// </remarks>
        public static StringAsset FindOrCreate(
            StringReference name,
            string createDirectory,
            out string path)
        {
            var asset = Find(name, out path);
            return asset != null
                ? asset
                : Create(name, ref createDirectory, out path);
        }

        /************************************************************************************************************************/
#endif
    }
}
