// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A custom Inspector for <see cref="StringAsset"/>s.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/StringAssetEditor
    [CustomEditor(typeof(StringAsset), true)]
    public class StringAssetEditor : UnityEditor.Editor
    {
        /************************************************************************************************************************/

        private const string InfoMessage = "This is a String Asset." +
            "\n\nThe name of this asset is what differentiates it from others" +
            " so it should be unique to avoid conflicts." +
            "\n\nThe Editor Comment field isn't used for anything and is excluded from runtime builds." +
            " It's recommended to explain what you're using this key for.";

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(InfoMessage, MessageType.Info);

            DrawDefaultInspector();
        }

        /************************************************************************************************************************/
    }
}

#endif

