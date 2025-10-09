// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using UnityEditor;
using UnityEngine;

namespace Animancer.Units.Editor
{
    /// <summary>[Editor-Only]
    /// A <see cref="PropertyDrawer"/> for fields with an <see cref="AnimationSpeedAttributeDrawer"/>
    /// which displays them using an 'x' suffix.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units.Editor/AnimationSpeedAttributeDrawer
    [CustomPropertyDrawer(typeof(AnimationSpeedAttribute), true)]
    public class AnimationSpeedAttributeDrawer : UnitsAttributeDrawer
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override int GetLineCount(SerializedProperty property, GUIContent label)
            => 1;

        /************************************************************************************************************************/
    }
}

#endif

