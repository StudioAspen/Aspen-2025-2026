// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A default <see cref="ICustomGUI"/> which simply draws the <see cref="object.ToString"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/LabelGUI
    [CustomGUI(typeof(object))]
    public class LabelGUI : CustomGUI<object>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI()
        {
            string text;
            try
            {
                text = Value != null
                    ? Value.ToString()
                    : "Null";
            }
            catch (Exception exception)
            {
                text = exception.ToString();
            }

            using (var value = PooledGUIContent.Acquire(text))
                EditorGUILayout.LabelField(Label, value);
        }

        /************************************************************************************************************************/
    }
}

#endif

