// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] [Internal]
    /// A utility for drawing empty [<see cref="SerializeReference"/>] <see cref="IInvokable"/> fields.
    /// </summary>
    /// <remarks>
    /// Used to draw empty slots in <see cref="AnimancerEvent.Sequence.Serializable.Callbacks"/>
    /// which don't actually have a <see cref="SerializedProperty"/> of their own because
    /// the array is compacted to trim any <c>null</c> items from the end. 
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/DummyInvokableDrawer
    internal class DummyInvokableDrawer : ScriptableObject
    {
        /************************************************************************************************************************/

        [SerializeReference, Polymorphic]
        private IInvokable[] _Invokable;

        /************************************************************************************************************************/

        private static SerializedProperty _InvokableProperty;

        /// <summary>[Editor-Only] A static dummy <see cref="IInvokable"/>.</summary>
        private static SerializedProperty InvokableProperty
        {
            get
            {
                if (_InvokableProperty == null)
                {
                    var instance = CreateInstance<DummyInvokableDrawer>();

                    instance.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                    var serializedObject = new SerializedObject(instance);
                    _InvokableProperty = serializedObject.FindProperty(nameof(_Invokable));

                    AssemblyReloadEvents.beforeAssemblyReload += () =>
                    {
                        serializedObject.Dispose();
                        DestroyImmediate(instance);
                    };
                }

                return _InvokableProperty;
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only] The GUI height required by <see cref="DoGUI"/>.</summary>
        public static float Height
            => AnimancerGUI.LineHeight;

        /************************************************************************************************************************/

        private static int _LastControlID;
        private static int _PropertyIndex;

        /// <summary>[Editor-Only] Draws the <see cref="InvokableProperty"/> GUI.</summary>
        public static bool DoGUI(
            ref Rect area,
            GUIContent label,
            SerializedProperty property,
            out object invokable)
        {
            var controlID = GUIUtility.GetControlID(FocusType.Passive);

            if (_LastControlID >= controlID)
                _PropertyIndex = 0;

            _LastControlID = controlID;

            var invokablesProperty = InvokableProperty;
            if (invokablesProperty.arraySize <= _PropertyIndex)
                invokablesProperty.arraySize = _PropertyIndex + 1;

            var invokableProperty = invokablesProperty.GetArrayElementAtIndex(_PropertyIndex);
            invokableProperty.prefabOverride = property.prefabOverride;

            _PropertyIndex++;

            label = EditorGUI.BeginProperty(area, label, property);

            EditorGUI.PropertyField(area, invokableProperty, label, false);

            EditorGUI.EndProperty();

            invokable = invokableProperty.managedReferenceValue;
            if (invokable == null)
                return false;

            invokableProperty.managedReferenceValue = null;
            invokableProperty.serializedObject.ApplyModifiedProperties();
            return true;
        }

        /************************************************************************************************************************/
    }
}

#endif

