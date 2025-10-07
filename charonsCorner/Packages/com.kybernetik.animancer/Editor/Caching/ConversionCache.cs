// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //
// Inspector Gadgets // https://kybernetik.com.au/inspector-gadgets // Copyright 2017-2024 Kybernetik //

#if UNITY_EDITOR

//#define LOG_CONVERSION_CACHE

using System;
using System.Collections.Generic;
using UnityEngine;

// Shared File Last Modified: 2023-09-02
namespace Animancer.Editor
//namespace InspectorGadgets.Editor
{
    /// <summary>[Editor-Only]
    /// A simple system for converting objects and storing the results so they can be reused to minimise the need for
    /// garbage collection, particularly for string construction.
    /// </summary>
    /// <remarks>This class doesn't use any Editor-Only functionality, but it's unlikely to be useful at runtime.</remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ConversionCache_2
    /// https://kybernetik.com.au/inspector-gadgets/api/InspectorGadgets.Editor/ConversionCache_2
    /// 
    public class ConversionCache<TKey, TValue>
    {
        /************************************************************************************************************************/

        private class CachedValue
        {
            public int lastFrameAccessed;
            public TValue value;
        }

        /************************************************************************************************************************/

        private readonly Dictionary<TKey, CachedValue>
            Cache = new();
        private readonly List<TKey>
            Keys = new();
        private readonly Func<TKey, TValue>
            Converter;

        private int _LastCleanupFrame;

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="ConversionCache{TKey, TValue}"/> which uses the specified delegate to convert values.
        /// </summary>
        public ConversionCache(Func<TKey, TValue> converter)
            => Converter = converter;

        /************************************************************************************************************************/

        /// <summary>
        /// If a value has already been cached for the specified `key`, return it. Otherwise create a new one using
        /// the delegate provided in the constructor and cache it.
        /// <para></para>
        /// If the `key` is <c>null</c>, this method returns the default <typeparamref name="TValue"/>.
        /// </summary>
        /// <remarks>This method also periodically removes values that have not been used recently.</remarks>
        public TValue Convert(TKey key)
        {
            if (key == null)
                return default;

            CachedValue cached;

            // The next time a value is retrieved after at least 100 frames, clear out any old ones.
            var frame = Time.frameCount;
            if (_LastCleanupFrame + 100 < frame)
            {

                for (int i = Keys.Count - 1; i >= 0; i--)
                {
                    var checkKey = Keys[i];
                    if (!Cache.TryGetValue(checkKey, out cached) ||
                        cached.lastFrameAccessed <= _LastCleanupFrame)
                    {
                        Cache.Remove(checkKey);
                        Keys.RemoveAt(i);
                    }
                }

                _LastCleanupFrame = frame;

            }

            if (!Cache.TryGetValue(key, out cached))
            {
                Cache.Add(key, cached = new() { value = Converter(key) });
                Keys.Add(key);

            }

            cached.lastFrameAccessed = frame;

            return cached.value;
        }

        /************************************************************************************************************************/
    }

    /// <summary>[Editor-Only] Utilities for <see cref="ConversionCache{TKey, TValue}"/>.</summary>
    /// <remarks>This class doesn't use any Editor-Only functionality, but it's unlikely to be useful at runtime.</remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ConversionCache
    /// https://kybernetik.com.au/inspector-gadgets/api/InspectorGadgets.Editor/ConversionCache
    /// 
    public static class ConversionCache
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Creates a <see cref="ConversionCache{TKey, TValue}"/> for calculating the GUI width occupied by text using
        /// the specified `style`.
        /// </summary>
        public static ConversionCache<string, float> CreateWidthCache(GUIStyle style)
            => new(style.CalculateWidth);

        /************************************************************************************************************************/

        // The "g" format gives a lower case 'e' for exponentials instead of upper case 'E'.
        private static readonly ConversionCache<float, string>
            FloatToString = new((value) => $"{value:g}");

        /// <summary>[Animancer Extension]
        /// Calls <see cref="float.ToString(string)"/> using <c>"g"</c> as the format and caches the result.
        /// </summary>
        public static string ToStringCached(this float value)
            => FloatToString.Convert(value);

        /************************************************************************************************************************/
    }
}

#endif

