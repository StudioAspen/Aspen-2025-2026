#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HierarchyDesigner
{
    internal static class HD_Texture
    {
        #region Properties
        #region Struct
        private readonly struct TextureKey
        {
            public readonly Color Color;
            public readonly int Width;
            public readonly int Height;

            public TextureKey(Color color, int width, int height)
            {
                Color = color;
                Width = width;
                Height = height;
            }

            public override bool Equals(object obj)
            {
                return obj is TextureKey key && Color.Equals(key.Color) && Width == key.Width && Height == key.Height;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Color, Width, Height);
            }
        }
        #endregion

        #region Cache
        private static Texture2D clearTextureCache;
        private static readonly Font fallbackFont;
        private static readonly Texture2D fallbackTexture;
        private static readonly Dictionary<string, Font> fontCache = new();
        private static readonly Dictionary<TextureKey, Texture2D> textureCache = new();
        private static readonly Dictionary<string, Texture2D> loadedTextureCache = new();
        #endregion
        #endregion

        #region Initializer
        static HD_Texture()
        {
            #region Fallback Font
            if (!HD_Session.instance.FallbackFont)
            {
                fallbackFont = GetFallbackFont();
                HD_Session.instance.FallbackFont = fallbackFont;
            }
            else
            {
                fallbackFont = HD_Session.instance.FallbackFont;
            }
            #endregion

            #region Fallback Texture
            if (!HD_Session.instance.FallbackTexture)
            {
                fallbackTexture = CreateFallbackTexture();
                HD_Session.instance.FallbackTexture = fallbackTexture;
            }
            else
            {
                fallbackTexture = HD_Session.instance.FallbackTexture;
            }
            #endregion
        }
        #endregion

        #region Accessors
        public static Texture2D ClearTexture
        {
            get
            {
                if (clearTextureCache == null)
                {
                    clearTextureCache = CreateTexture(2, 2, Color.clear);
                }
                return clearTextureCache;
            }
        }
        #endregion

        #region Methods
        public static Font LoadFont(string fontName)
        {
            if (string.IsNullOrEmpty(fontName))
            {
                Debug.LogWarning("LoadFont called with null or empty fontName. Loading fallback font.");
                return fallbackFont;
            }

            if (fontCache.TryGetValue(fontName, out Font cachedFont))
            {
                return cachedFont;
            }

            Font loadedFont = Resources.Load<Font>(fontName);
            if (loadedFont != null)
            {
                fontCache[fontName] = loadedFont;
                return loadedFont;
            }

            Debug.LogWarning($"Font '{fontName}' not found. Loading fallback font.");
            return fallbackFont;
        }

        public static Texture2D LoadTexture(string textureName)
        {
            if (string.IsNullOrEmpty(textureName))
            {
                Debug.LogWarning("LoadTexture called with null or empty textureName. Returning fallback texture.");
                return fallbackTexture;
            }

            if (loadedTextureCache.TryGetValue(textureName, out Texture2D cachedTexture))
            {
                return cachedTexture;
            }

            Texture2D loadedTexture = Resources.Load<Texture2D>(textureName);
            if (loadedTexture != null)
            {
                loadedTextureCache[textureName] = loadedTexture;
                return loadedTexture;
            }

            Debug.LogWarning($"Texture '{textureName}' was not found in Resources. Using fallback texture.");
            return fallbackTexture;
        }

        public static Texture2D CreateTexture(int width, int height, Color color)
        {
            TextureKey key = new(color, width, height);

            if (textureCache.TryGetValue(key, out Texture2D existingTexture) && existingTexture != null)
            {
                return existingTexture;
            }

            Texture2D newTexture = new(width, height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.DontSave
            };

            Color32 color32 = color;
            Color32[] pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color32;
            }

            newTexture.SetPixels32(pixels);
            newTexture.Apply();
            textureCache[key] = newTexture;

            return newTexture;
        }

        public static Texture2D CreateGradientTexture(int width, int height, Color[] colors)
        {
            if (colors == null || colors.Length < 2)
            {
                throw new ArgumentException("At least two colors are required for gradient.");
            }

            Texture2D gradientTexture = new(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };

            int numSegments = colors.Length - 1;
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1);
                int segmentIndex = Mathf.Min((int)(t * numSegments), numSegments - 1);

                float t0 = (float)segmentIndex / numSegments;
                float t1 = (float)(segmentIndex + 1) / numSegments;
                float localT = (t - t0) / (t1 - t0);
                localT = localT * localT * (3f - 2f * localT);

                Color color = Color.Lerp(colors[segmentIndex], colors[segmentIndex + 1], localT);
                for (int x = 0; x < width; x++)
                {
                    gradientTexture.SetPixel(x, y, color);
                }
            }

            gradientTexture.Apply();
            return gradientTexture;
        }

        #region Helpers
        private static Font GetFallbackFont()
        {
            #if UNITY_2022_1_OR_NEWER
            Font fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            #else
            Font fallbackFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            #endif
            if (fallbackFont == null)
            {
                Debug.LogError($"Fallback font {fallbackFont} could not be loaded. Please ensure it exists.");
            }
            return fallbackFont;
        }

        private static Texture2D CreateFallbackTexture()
        {
            Texture2D fallback = new(2, 2, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.DontSave
            };

            Color32 black32 = Color.black;
            Color32[] pixels = new Color32[4] { black32, black32, black32, black32 };
            fallback.SetPixels32(pixels);
            fallback.Apply();

            return fallback;
        }
        #endregion
        #endregion
    }
}
#endif