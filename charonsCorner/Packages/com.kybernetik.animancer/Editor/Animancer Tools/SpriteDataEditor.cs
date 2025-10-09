// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

#if UNITY_2D_SPRITE
using UnityEditor.U2D.Sprites;
#else
#pragma warning disable CS0618 // Type or member is obsolete.
#endif

namespace Animancer.Editor.Tools
{
    /// <summary>A wrapper around the '2D Sprite' package features for editing Sprite data.</summary>
    public class SpriteDataEditor
    {
        /************************************************************************************************************************/
#if UNITY_2D_SPRITE
        /************************************************************************************************************************/

        private static SpriteDataProviderFactories _Factories;

        private static SpriteDataProviderFactories Factories
        {
            get
            {
                if (_Factories == null)
                {
                    _Factories = new();
                    _Factories.Init();
                }

                return _Factories;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The data provider for the target.</summary>
        public readonly ISpriteEditorDataProvider DataProvider;

        private SpriteRect[] _SpriteRects;

        /************************************************************************************************************************/

        /// <summary>The number of sprites in the target data.</summary>
        /// <remarks>Setting this value clears all sprites.</remarks>
        public int SpriteCount
        {
            get => _SpriteRects.Length;
            set
            {
                // Unity might have given a UnityEditor.U2D.Sprites.SpriteDataExt[].
                // We need to ensure it's a base SpriteRect[] so we can new() them.

                if (_SpriteRects == null ||
                    _SpriteRects.Length != value ||
                    _SpriteRects.GetType() != typeof(SpriteRect[]))
                    _SpriteRects = new SpriteRect[value];

                for (int i = 0; i < _SpriteRects.Length; i++)
                    _SpriteRects[i] = new();
            }
        }

        /// <summary>Returns the name of the sprite at the specified `index`.</summary>
        public string GetName(int index) => _SpriteRects[index].name;

        /// <summary>Sets the name of the sprite at the specified `index`.</summary>
        public void SetName(int index, string name) => _SpriteRects[index].name = name;

        /// <summary>Returns the rect of the sprite at the specified `index`.</summary>
        public Rect GetRect(int index) => _SpriteRects[index].rect;

        /// <summary>Sets the rect of the sprite at the specified `index`.</summary>
        public void SetRect(int index, Rect rect) => _SpriteRects[index].rect = rect;

        /// <summary>Returns the pivot of the sprite at the specified `index`.</summary>
        public Vector2 GetPivot(int index) => _SpriteRects[index].pivot;

        /// <summary>Sets the pivot of the sprite at the specified `index`.</summary>
        public void SetPivot(int index, Vector2 pivot)
        {
            _SpriteRects[index].pivot = pivot;
            _SpriteRects[index].alignment = GetSpriteAlignment(pivot);
        }

        /// <summary>Returns the alignment of the sprite at the specified `index`.</summary>
        public SpriteAlignment GetAlignment(int index) => _SpriteRects[index].alignment;

        /// <summary>Sets the alignment of the sprite at the specified `index`.</summary>
        public void SetAlignment(int index, SpriteAlignment alignment) => _SpriteRects[index].alignment = alignment;

        /// <summary>Returns the border of the sprite at the specified `index`.</summary>
        public Vector4 GetBorder(int index) => _SpriteRects[index].border;

        /// <summary>Sets the border of the sprite at the specified `index`.</summary>
        public void SetBorder(int index, Vector4 border) => _SpriteRects[index].border = border;

        /// <summary>References the sprite at the specified `index`.</summary>
        public ref SpriteRect RefSprite(int index) => ref _SpriteRects[index];

        /************************************************************************************************************************/
#else
        /************************************************************************************************************************/

        private SpriteMetaData[] _SpriteSheet;

        /************************************************************************************************************************/

        /// <summary>The number of sprites in the target data.</summary>
        public int SpriteCount
        {
            get => _SpriteSheet.Length;
            set => System.Array.Resize(ref _SpriteSheet, value);
        }

        /// <summary>Returns the name of the sprite at the specified `index`.</summary>
        public string GetName(int index) => _SpriteSheet[index].name;

        /// <summary>Sets the name of the sprite at the specified `index`.</summary>
        public void SetName(int index, string name) => _SpriteSheet[index].name = name;

        /// <summary>Returns the rect of the sprite at the specified `index`.</summary>
        public Rect GetRect(int index) => _SpriteSheet[index].rect;

        /// <summary>Sets the rect of the sprite at the specified `index`.</summary>
        public void SetRect(int index, Rect rect) => _SpriteSheet[index].rect = rect;

        /// <summary>Returns the pivot of the sprite at the specified `index`.</summary>
        public Vector2 GetPivot(int index) => _SpriteSheet[index].pivot;

        /// <summary>Sets the pivot of the sprite at the specified `index`.</summary>
        public void SetPivot(int index, Vector2 pivot)
        {
            _SpriteSheet[index].pivot = pivot;
            _SpriteSheet[index].alignment = (int)GetSpriteAlignment(pivot);
        }

        /// <summary>Returns the alignment of the sprite at the specified `index`.</summary>
        public SpriteAlignment GetAlignment(int index) => (SpriteAlignment)_SpriteSheet[index].alignment;

        /// <summary>Sets the alignment of the sprite at the specified `index`.</summary>
        public void SetAlignment(int index, SpriteAlignment alignment) => _SpriteSheet[index].alignment = (int)alignment;

        /// <summary>Returns the border of the sprite at the specified `index`.</summary>
        public Vector4 GetBorder(int index) => _SpriteSheet[index].border;

        /// <summary>Sets the border of the sprite at the specified `index`.</summary>
        public void SetBorder(int index, Vector4 border) => _SpriteSheet[index].border = border;

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/

        /// <summary>Returns the appropriate alignment for the given `pivot`.</summary>
        public static SpriteAlignment GetSpriteAlignment(Vector2 pivot)
        {
            switch (pivot.x)
            {
                case 0:
                    switch (pivot.y)
                    {
                        case 0: return SpriteAlignment.BottomLeft;
                        case 0.5f: return SpriteAlignment.LeftCenter;
                        case 1: return SpriteAlignment.TopLeft;
                    }
                    break;
                case 0.5f:
                    switch (pivot.y)
                    {
                        case 0: return SpriteAlignment.BottomCenter;
                        case 0.5f: return SpriteAlignment.Center;
                        case 1: return SpriteAlignment.TopCenter;
                    }
                    break;
                case 1:
                    switch (pivot.y)
                    {
                        case 0: return SpriteAlignment.BottomRight;
                        case 0.5f: return SpriteAlignment.RightCenter;
                        case 1: return SpriteAlignment.TopRight;
                    }
                    break;
            }

            return SpriteAlignment.Custom;
        }

        /************************************************************************************************************************/

        private readonly TextureImporter Importer;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="SpriteDataEditor"/>.</summary>
        public SpriteDataEditor(TextureImporter importer)
        {
            Importer = importer;

#if UNITY_2D_SPRITE
            DataProvider = Factories.GetSpriteEditorDataProviderFromObject(importer);
            DataProvider.InitSpriteEditorDataProvider();

            _SpriteRects = DataProvider.GetSpriteRects();
#else
            _SpriteSheet = importer.spritesheet;
#endif
        }

        /************************************************************************************************************************/

        /// <summary>Tries to find the index of the data matching the `sprite`.</summary>
        /// <remarks>
        /// Returns -1 if there is no data matching the <see cref="UnityEngine.Object.name"/>.
        /// <para></para>
        /// Returns -2 if there is more than one data matching the <see cref="UnityEngine.Object.name"/> but no
        /// <see cref="Sprite.rect"/> match.
        /// </remarks>
        public int IndexOf(Sprite sprite)
        {
            var nameMatchIndex = -1;

            var count = SpriteCount;
            for (int i = 0; i < count; i++)
            {
                if (GetName(i) == sprite.name)
                {
                    if (GetRect(i) == sprite.rect)
                        return i;

                    if (nameMatchIndex == -1)// First name match.
                        nameMatchIndex = i;
                    else
                        nameMatchIndex = -2;// Already found 2 name matches.
                }
            }

            if (nameMatchIndex == -1)
            {
                Debug.LogError($"No {nameof(SpriteMetaData)} for '{sprite.name}' was found.", sprite);
            }
            else if (nameMatchIndex == -2)
            {
                Debug.LogError($"More than one {nameof(SpriteMetaData)} for '{sprite.name}' was found" +
                    $" but none of them matched the {nameof(Sprite)}.{nameof(Sprite.rect)}." +
                    $" If the texture's Max Size is smaller than its actual size, increase the Max Size before performing this" +
                    $" operation so that the {nameof(Rect)}s can be used to identify the correct data.", sprite);
            }

            return nameMatchIndex;
        }

        /************************************************************************************************************************/

        /// <summary>Logs an error and returns false if the data at the specified `index` is out of the texture bounds.</summary>
        public bool ValidateBounds(int index, Sprite sprite)
        {
            var rect = GetRect(index);
            if (rect.xMin >= 0 &&
                rect.yMin >= 0 &&
                rect.xMax <= sprite.texture.width &&
                rect.yMax <= sprite.texture.height)
                return true;

            var path = AssetDatabase.GetAssetPath(sprite);

            // The Max Texture Size import setting may cause the loaded texture to be smaller than the actual image.
            // Sprite dimensions are defined against the actual image though, so we need to check those bounds.
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.GetSourceTextureWidthAndHeight(out var width, out var height);
            if (rect.xMin >= 0 &&
                rect.yMin >= 0 &&
                rect.xMax <= width &&
                rect.yMax <= height)
                return true;

            Debug.LogError(
                $"This modification would put '{sprite.name}' at {rect}" +
                $" which is outside of the texture ({width}x{height})" +
                $" so '{path}' was not modified.",
                sprite);

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>Applies any modifications to the target asset.</summary>
        public void Apply()
        {
#if UNITY_2D_SPRITE
            DataProvider.SetSpriteRects(_SpriteRects);
            DataProvider.Apply();
#else
            Importer.spritesheet = _SpriteSheet;
            EditorUtility.SetDirty(Importer);
#endif

            Importer.SaveAndReimport();
        }

        /************************************************************************************************************************/
    }
}

#endif

