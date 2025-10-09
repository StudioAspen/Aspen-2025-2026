// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A <see cref="ICustomGUI"/> for <see cref="float"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/FloatGUI
    /// 
    [CustomGUI(typeof(float))]
    public class FloatGUI : CustomGUI<float>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI()
            => Value = EditorGUILayout.FloatField(Label, Value);

        /************************************************************************************************************************/
    }

    /// <summary>[Editor-Only] A <see cref="ICustomGUI"/> for <see cref="int"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/IntGUI
    /// 
    [CustomGUI(typeof(int))]
    public class IntGUI : CustomGUI<int>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI()
            => Value = EditorGUILayout.IntField(Label, Value);

        /************************************************************************************************************************/
    }

    /// <summary>[Editor-Only] A <see cref="ICustomGUI"/> for <see cref="string"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/StringGUI
    /// 
    [CustomGUI(typeof(string))]
    public class StringGUI : CustomGUI<string>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI()
            => Value = EditorGUILayout.TextField(Label, Value);

        /************************************************************************************************************************/
    }

    /// <summary>[Editor-Only] A <see cref="ICustomGUI"/> for <see cref="Object"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ObjectGUI_1
    /// 
    [CustomGUI(typeof(Object))]
    public class ObjectGUI<T> : CustomGUI<T>
        where T : Object
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI()
            => Value = AnimancerGUI.DoObjectFieldGUI(Label, Value, true);

        /************************************************************************************************************************/
    }
}

#endif

