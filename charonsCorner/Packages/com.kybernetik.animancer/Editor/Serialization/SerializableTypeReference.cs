// Animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A <see cref="SerializableAttribute"/> reference to a <see cref="Type"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/SerializableTypeReference
    [Serializable]
    public struct SerializableTypeReference : ISerializationCallbackReceiver
    {
        /************************************************************************************************************************/

        [SerializeField]
        private string _QualifiedName;

        private Type _Type;

        /************************************************************************************************************************/

        /// <summary>[<see cref="SerializeField"/>] The <see cref="Type.AssemblyQualifiedName"/>.</summary>
        public string QualifiedName
        {
            readonly get => _QualifiedName;
            set
            {
                if (_QualifiedName == value)
                    return;

                _QualifiedName = value;
                _Type = null;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The referenced type.</summary>
        public Type Type
        {
            get
            {
                if (_Type == null && !string.IsNullOrEmpty(_QualifiedName))
                    _Type = Type.GetType(_QualifiedName);

                return _Type;
            }
            set
            {
                if (_Type == value)
                    return;

                _QualifiedName = value?.AssemblyQualifiedName;
                _Type = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="SerializableTypeReference"/>.</summary>
        public SerializableTypeReference(string qualifiedName)
        {
            _QualifiedName = qualifiedName;
            _Type = null;
        }

        /// <summary>Creates a new <see cref="SerializableTypeReference"/>.</summary>
        public SerializableTypeReference(Type type)
        {
            _QualifiedName = type?.AssemblyQualifiedName;
            _Type = type;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public readonly void OnBeforeSerialize() { }

        /// <inheritdoc/>
        public void OnAfterDeserialize()
            => _Type = null;

        /************************************************************************************************************************/
        #region Drawer
        /************************************************************************************************************************/

        /// <summary>[Editor-Only] A <see cref="PropertyDrawer"/> for <see cref="SerializableTypeReference"/>.</summary>
        [CustomPropertyDrawer(typeof(SerializableTypeReference))]
        public class Drawer : PropertyDrawer
        {
            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                => AnimancerGUI.LineHeight;

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
            {
                var name = property.FindPropertyRelative(nameof(_QualifiedName));

                var spacing = AnimancerGUI.StandardSpacing;
                var pickerArea = AnimancerGUI.StealFromRight(ref area, area.height + spacing, spacing);

                name.stringValue = EditorGUI.TextField(area, label, name.stringValue);

                var indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                var picked = EditorGUI.ObjectField(pickerArea, null, typeof(Object), true);
                if (picked != null)
                    name.stringValue = picked.GetType().AssemblyQualifiedName;

                EditorGUI.indentLevel = indentLevel;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif
