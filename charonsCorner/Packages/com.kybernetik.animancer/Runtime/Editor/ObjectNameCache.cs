// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_ASSERTIONS

//#define ANIMANCER_DISABLE_NAME_CACHE

using System.Runtime.CompilerServices;
using UnityEngine;

namespace Animancer
{
    /// <summary>[Assert-Only]
    /// A simple system for caching <see cref="Object.name"/> since it allocates garbage every time it's accessed.
    /// </summary>
    public static class NameCache
    {
        /************************************************************************************************************************/

        private static readonly ConditionalWeakTable<Object, string>
            ObjectToName = new();

        /************************************************************************************************************************/

        /// <summary>Caches and returns the <see cref="Object.name"/>.</summary>
        public static string GetCachedName(this Object obj)
        {
#if ANIMANCER_DISABLE_NAME_CACHE
            return obj.name;
#else
            if (obj == null)
            {
                if (obj is not null)
                    ObjectToName.Remove(obj);

                return null;
            }

            if (!ObjectToName.TryGetValue(obj, out var name))
            {
                name = obj.name;
                ObjectToName.Add(obj, name);
            }

            return name;
#endif
        }

        /************************************************************************************************************************/

        /// <summary>Tries to get the <see cref="Object.name"/> or <see cref="object.ToString"/>.</summary>
        public static bool TryToString(object obj, out string name)
        {
            if (obj == null)
            {
                name = null;
                return false;
            }

            if (obj is Object unityObject)
            {
                if (unityObject != null)
                {
                    name = unityObject.GetCachedName();
                }
                else
                {
                    name = null;
                    return false;
                }
            }
            else
            {
                name = obj.ToString();
            }

            return !string.IsNullOrEmpty(name);
        }

        /************************************************************************************************************************/

        /// <summary>Clears all cached names so they will be re-gathered when next accessed.</summary>
        public static void Clear()
            => ObjectToName.Clear();

        /************************************************************************************************************************/

        /// <summary>Sets the <see cref="Object.name"/> and caches it.</summary>
        public static void SetName(this Object obj, string name)
        {
            obj.name = name;
            ObjectToName.AddOrUpdate(obj, name);
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        private class Cleaner : UnityEditor.AssetPostprocessor
        {
            /************************************************************************************************************************/

            private static void OnPostprocessAllAssets(
                string[] importedAssets,
                string[] deletedAssets,
                string[] movedAssets,
                string[] movedFromAssetPaths,
                bool didDomainReload)
            {
                Clear();
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}

#endif

