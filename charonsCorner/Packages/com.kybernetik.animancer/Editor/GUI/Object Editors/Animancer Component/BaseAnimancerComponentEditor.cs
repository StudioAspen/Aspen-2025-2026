// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A custom Inspector for <see cref="IAnimancerComponent"/>s.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/BaseAnimancerComponentEditor
    public abstract class BaseAnimancerComponentEditor : UnityEditor.Editor
    {
        /************************************************************************************************************************/

        [NonSerialized]
        private IAnimancerComponent[] _Targets;

        /// <summary><see cref="UnityEditor.Editor.targets"/> casted to <see cref="IAnimancerComponent"/>.</summary>
        public IAnimancerComponent[] Targets
            => _Targets;

        /************************************************************************************************************************/

        /// <summary>Initializes this <see cref="UnityEditor.Editor"/>.</summary>
        protected virtual void OnEnable()
        {
            var targets = this.targets;
            _Targets = new IAnimancerComponent[targets.Length];
            GatherTargets();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Copies the <see cref="UnityEditor.Editor.targets"/> into the <see cref="_Targets"/> array.
        /// </summary>
        private void GatherTargets()
        {
            for (int i = 0; i < _Targets.Length; i++)
                _Targets[i] = (IAnimancerComponent)targets[i];
        }

        /************************************************************************************************************************/

        /// <summary>Called by the Unity editor to draw the custom Inspector GUI elements.</summary>
        public override void OnInspectorGUI()
        {
            // Normally the targets wouldn't change after OnEnable, but the trick AnimancerComponent.Reset uses to
            // swap the type of an existing component when a new one is added causes the old target to be destroyed.
            GatherTargets();

            serializedObject.Update();

            DoSerializedFieldsGUI();

            serializedObject.ApplyModifiedProperties();
        }

        /************************************************************************************************************************/

        /// <summary>Draws the rest of the Inspector fields after the Animator field.</summary>
        protected void DoSerializedFieldsGUI()
        {
            var property = serializedObject.GetIterator();

            if (!property.NextVisible(true))
                return;

            do
            {
                var path = property.propertyPath;
                if (path == "m_Script")
                    continue;

                using (var label = PooledGUIContent.Acquire(property))
                {
                    // Let the target try to override.
                    if (DoOverridePropertyGUI(path, property, label))
                        continue;

                    // Otherwise draw the property normally.
                    EditorGUILayout.PropertyField(property, label, true);
                }
            }
            while (property.NextVisible(false));
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Draws any custom GUI for the `property`.
        /// The return value indicates whether the GUI should replace the regular call to
        /// <see cref="EditorGUILayout.PropertyField(SerializedProperty, GUIContent, bool, GUILayoutOption[])"/>. 
        /// True = GUI was drawn, so don't draw the regular GUI. 
        /// False = Draw the regular GUI.
        /// </summary>
        protected virtual bool DoOverridePropertyGUI(string path, SerializedProperty property, GUIContent label)
            => false;

        /************************************************************************************************************************/
    }
}

#endif

