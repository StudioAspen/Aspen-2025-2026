// Animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A system which gathers information about <see cref="SerializeReference"/> fields to detect when multiple fields
    /// are referencing the same object.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/SharedReferenceCache
    public class SharedReferenceCache :
        IEnumerable<KeyValuePair<object, List<SharedReferenceCache.Field>>>
    {
        /************************************************************************************************************************/
        #region Static Caching
        /************************************************************************************************************************/

        private static readonly Dictionary<SerializedObject, SharedReferenceCache>
            SerializedObjectToCache = new();

        /// <summary>Returns a cached <see cref="SharedReferenceCache"/> for the `serializedObject`.</summary>
        public static SharedReferenceCache Get(SerializedObject serializedObject)
        {
            CheckFlush(serializedObject);

            if (!SerializedObjectToCache.TryGetValue(serializedObject, out var cache))
                SerializedObjectToCache.Add(serializedObject, cache = new(serializedObject));

            return cache;
        }

        /************************************************************************************************************************/

        private static readonly HashSet<SerializedObject>
            NotRecentlyUsed = new();

        private const double
            FlushInterval = 5;

        private static double
            _LastFlushTime;

        /// <summary>Discards any caches not used during the last <see cref="FlushInterval"/> when it elapses.</summary>
        private static void CheckFlush(SerializedObject serializedObject)
        {
            var currentTime = EditorApplication.timeSinceStartup;

            if (currentTime >= _LastFlushTime + FlushInterval)
            {
                _LastFlushTime = currentTime;

                foreach (var unused in NotRecentlyUsed)
                    SerializedObjectToCache.Remove(unused);

                NotRecentlyUsed.Clear();
                NotRecentlyUsed.UnionWith(SerializedObjectToCache.Keys);
            }

            NotRecentlyUsed.Remove(serializedObject);
        }

        /************************************************************************************************************************/

        /// <summary>The number of editor updates that have occurred since startup.</summary>
        public static ulong FrameCount { get; private set; }

        static SharedReferenceCache()
        {
            EditorApplication.update += () => FrameCount++;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <summary>Information about a field.</summary>
        public struct Field
        {
            /************************************************************************************************************************/

            /// <summary>The <see cref="Serialization.GetFriendlyPath"/> of the field.</summary>
            public string path;

            /// <summary>The area where the field was last drawn.</summary>
            public Rect area;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="Field"/>.</summary>
            public Field(string path)
            {
                this.path = path;
                area = default;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        private readonly SerializedObject
            SerializedObject;

        private readonly Dictionary<object, List<Field>>
            ObjectToReferences = new();

        private ulong
            _LastGatherFrameCount;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="SharedReferenceCache"/>.</summary>
        public SharedReferenceCache(SerializedObject serializedObject)
        {
            SerializedObject = serializedObject;
        }

        /************************************************************************************************************************/

        /// <summary>Should <see cref="GatherReferences"/> be called?</summary>
        public bool ShouldGather
            => _LastGatherFrameCount != FrameCount;

        /// <summary>Updates the cached reference info.</summary>
        public void GatherReferences()
        {
            _LastGatherFrameCount = FrameCount;

            ObjectToReferences.Clear();

            var property = SerializedObject.GetIterator();
            while (property.Next(true))
            {
                if (property.propertyType != SerializedPropertyType.ManagedReference)
                    continue;

                var reference = property.managedReferenceValue;
                if (reference == null)
                    continue;

                if (!ObjectToReferences.TryGetValue(reference, out var paths))
                    ObjectToReferences.Add(reference, paths = new());

                paths.Add(new(property.GetFriendlyPath()));
            }
        }

        /************************************************************************************************************************/

        /// <summary>Tries to get the info about all fields containing the `reference`.</summary>
        public bool TryGetInfo(object reference, out List<Field> references)
        {
            if (ShouldGather)
                GatherReferences();

            return ObjectToReferences.TryGetValue(reference, out references);
        }

        /************************************************************************************************************************/

        /// <summary>Returns an enumerator for all references and their info.</summary>
        public Dictionary<object, List<Field>>.Enumerator GetEnumerator()
        {
            if (ShouldGather)
                GatherReferences();

            return ObjectToReferences.GetEnumerator();
        }

        /// <summary>Returns an enumerator for all references and their info.</summary>
        IEnumerator<KeyValuePair<object, List<Field>>> IEnumerable<KeyValuePair<object, List<Field>>>.GetEnumerator()
            => GetEnumerator();

        /// <summary>Returns an enumerator for all references and their info.</summary>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /************************************************************************************************************************/
    }
}

#endif
