// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A static reference to a persistent setting stored in <see cref="AnimancerSettings"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerSettingsGroup_1
    public static class AnimancerSettingsGroup<T>
        where T : AnimancerSettingsGroup, new()
    {
        /************************************************************************************************************************/

        /// <summary>Gets or creates a <typeparamref name="T"/> in the <see cref="AnimancerSettings"/> asset.</summary>
        public static T Instance
            => AnimancerSettings.GetOrCreateData<T>();

        /************************************************************************************************************************/
    }

    /// <summary>Base class for groups of fields that can be serialized inside <see cref="AnimancerSettings"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerSettingsGroup
    [Serializable, InternalSerializableType]
    public abstract class AnimancerSettingsGroup : IComparable<AnimancerSettingsGroup>
    {
        /************************************************************************************************************************/

        private int _DataIndex = -1;
        private string _BasePropertyPath;

        /************************************************************************************************************************/

        /// <summary>The user-firendly name to display in the Inspector.</summary>
        public abstract string DisplayName { get; }

        /// <summary>The index to display this data at in the Inspector.</summary>
        public abstract int Index { get; }

        /************************************************************************************************************************/

        /// <summary>Sets the index used to find <see cref="SerializedProperty"/> instances for this group.</summary>
        internal void SetDataIndex(int index)
        {
            if (_DataIndex == index)
                return;

            _DataIndex = index;
            _BasePropertyPath = null;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a <see cref="SerializedProperty"/> relative to the base of this group.</summary>
        protected SerializedProperty GetSerializedProperty(string propertyPath)
            => AnimancerSettings.GetSerializedProperty(_DataIndex, ref _BasePropertyPath, propertyPath);

        /************************************************************************************************************************/

        /// <summary>
        /// Draws a <see cref="EditorGUILayout.PropertyField(SerializedProperty, GUILayoutOption[])"/> for a
        /// property in this group.
        /// </summary>
        protected SerializedProperty DoPropertyField(string propertyPath)
        {
            var property = GetSerializedProperty(propertyPath);
            EditorGUILayout.PropertyField(property, true);
            return property;
        }

        /************************************************************************************************************************/

        /// <summary>Compares the <see cref="Index"/>.</summary>
        public int CompareTo(AnimancerSettingsGroup other)
            => Index.CompareTo(other.Index);

        /************************************************************************************************************************/
    }
}

#endif

