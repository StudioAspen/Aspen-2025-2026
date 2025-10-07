// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A wrapper for modifying a serialized field to allow it to be properly saved and undone.
    /// </summary>
    /// <remarks>
    /// <strong>Example:</strong>
    /// <code>
    /// using (new ModifySerializedField(target))// Undo gets captured.
    /// {
    ///     // Modify values on the target.
    /// }// Target gets flagged for saving.
    /// </code></remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ModifySerializedField
    public readonly struct ModifySerializedField : IDisposable
    {
        /************************************************************************************************************************/

        /// <summary>The object being modified.</summary>
        public readonly Object Target;

        /// <summary>Prefab modifications are more complicated to track due to the possibility of nested prefabs.</summary>
        public readonly bool MightBePrefab;

        /************************************************************************************************************************/

        /// <summary>Captures the state of the target as an undo step.</summary>
        public ModifySerializedField(
            Object target,
            string name = "Inspector",
            bool mightBePrefab = false)
        {
            Target = target;
            MightBePrefab = mightBePrefab;

            if (!string.IsNullOrEmpty(name))
                Undo.RecordObject(Target, name);
        }

        /************************************************************************************************************************/

        /// <summary>Flags the target as modified so that it will get saved by Unity.</summary>
        public void Dispose()
        {
            if (MightBePrefab)
                PrefabUtility.RecordPrefabInstancePropertyModifications(Target);

            EditorUtility.SetDirty(Target);

            AnimancerReflection.TryInvoke(Target, "OnValidate");
        }

        /************************************************************************************************************************/
    }
}

#endif

