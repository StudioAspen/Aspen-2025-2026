// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Persistent settings used by Animancer.</summary>
    /// <remarks>
    /// This asset automatically creates itself when first accessed.
    /// <para></para>
    /// The default location is <em>Packages/com.kybernetik.animancer/Code/Editor</em>, but you can freely move it
    /// (and the whole Animancer folder) anywhere in your project.
    /// <para></para>
    /// These settings can also be accessed via the Settings in the <see cref="Tools.AnimancerToolsWindow"/>
    /// (<c>Window/Animation/Animancer Tools</c>).
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerSettings
    /// 
    [AnimancerHelpUrl(typeof(AnimancerSettings))]
    public class AnimancerSettings : ScriptableObject
    {
        /************************************************************************************************************************/

        private static AnimancerSettings _Instance;

        /// <summary>
        /// Loads an existing <see cref="AnimancerSettings"/> if there is one anywhere in your project.
        /// Otherwise, creates a new one and saves it in the Assets folder.
        /// </summary>
        public static AnimancerSettings Instance
        {
            get
            {
                if (_Instance != null)
                    return _Instance;

                _Instance = AnimancerEditorUtilities.FindAssetOfType<AnimancerSettings>();

                if (_Instance != null)
                    return _Instance;

                _Instance = CreateInstance<AnimancerSettings>();
                _Instance.name = "Animancer Settings";
                _Instance.hideFlags = HideFlags.DontSaveInBuild;

                var path = $"Assets/{_Instance.name}.asset";
                path = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(_Instance, path);

                return _Instance;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Finds an existing instance of this asset anywhere in the project.</summary>
        [InitializeOnLoadMethod]
        private static void FindExistingInstance()
        {
            if (_Instance == null)
                _Instance = AnimancerEditorUtilities.FindAssetOfType<AnimancerSettings>();
        }

        /************************************************************************************************************************/

        private SerializedObject _SerializedObject;

        /// <summary>The <see cref="SerializedProperty"/> representing the <see cref="Instance"/>.</summary>
        public static SerializedObject SerializedObject
            => Instance._SerializedObject ?? (Instance._SerializedObject = new(Instance));

        /************************************************************************************************************************/

        private readonly List<Dictionary<string, SerializedProperty>>
            SerializedProperties = new();

        private static SerializedProperty GetSerializedProperty(int index, string propertyPath)
        {
            while (index >= Instance.SerializedProperties.Count)
                Instance.SerializedProperties.Add(null);

            var properties = Instance.SerializedProperties[index];
            properties ??= Instance.SerializedProperties[index] = new();

            if (!properties.TryGetValue(propertyPath, out var property))
            {
                property = SerializedObject.FindProperty(propertyPath);
                properties.Add(propertyPath, property);
            }

            return property;
        }

        /// <summary>Returns a <see cref="SerializedProperty"/> relative to the data at the specified `index`.</summary>
        public static SerializedProperty GetSerializedProperty(
            int index,
            ref string basePropertyPath,
            string propertyPath)
        {
            if (string.IsNullOrEmpty(basePropertyPath))
                basePropertyPath =
                    $"{nameof(_Data)}{Serialization.ArrayDataPrefix}{index}{Serialization.ArrayDataSuffix}";

            if (string.IsNullOrEmpty(propertyPath))
                propertyPath = basePropertyPath;
            else
                propertyPath = $"{basePropertyPath}.{propertyPath}";

            return GetSerializedProperty(index, propertyPath);
        }

        /************************************************************************************************************************/

        [SerializeReference]
        private List<AnimancerSettingsGroup> _Data;

        /// <summary>Returns a stored item of the specified type or creates a new one if necessary.</summary>
        public static T GetOrCreateData<T>()
            where T : AnimancerSettingsGroup, new()
        {
            ref var data = ref Instance._Data;
            data ??= new();

            var index = AnimancerEditorUtilities.IndexOfType(Instance._Data, typeof(T));
            if (index >= 0)
                return (T)data[index];

            var newT = new T();
            newT.SetDataIndex(data.Count);
            data.Add(newT);
            SetDirty();
            return newT;
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="EditorUtility.SetDirty"/> on the <see cref="Instance"/>.</summary>
        public static new void SetDirty()
            => EditorUtility.SetDirty(_Instance);

        /************************************************************************************************************************/

        /// <summary>
        /// Ensures that there is an instance of each class derived from <see cref="AnimancerSettingsGroup"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            AnimancerEditorUtilities.InstantiateDerivedTypes(ref _Data);

            for (int i = 0; i < _Data.Count; i++)
                _Data[i].SetDataIndex(i);
        }

        /************************************************************************************************************************/

        /// <summary>A custom Inspector for <see cref="AnimancerSettings"/>.</summary>
        [CustomEditor(typeof(AnimancerSettings), true), CanEditMultipleObjects]
        public class Editor : UnityEditor.Editor
        {
            /************************************************************************************************************************/

            [NonSerialized]
            private SerializedProperty _Data;

            /************************************************************************************************************************/

            /// <summary>Called when this object is first loaded.</summary>
            protected virtual void OnEnable()
            {
                _Data = serializedObject.FindProperty(nameof(AnimancerSettings._Data));
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void OnInspectorGUI()
            {
                DoInfoGUI();

                DoOptionalWarningsGUI();

                serializedObject.Update();

                var count = _Data.arraySize;
                for (int i = 0; i < count; i++)
                    DoDataGUI(_Data.GetArrayElementAtIndex(i), i);

                serializedObject.ApplyModifiedProperties();
            }

            /************************************************************************************************************************/

            /// <summary>
            /// If <c>true</c>, the next <see cref="OnInspectorGUI"/> will skip drawing the info panel.
            /// </summary>
            public static bool HideNextInfo { get; set; }

            private void DoInfoGUI()
            {
                if (HideNextInfo)
                {
                    HideNextInfo = false;
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Feel free to move this asset anywhere in your project." +
                        "\n\nIt should generally not be in the Animancer folder" +
                        " so that if you ever update Animancer you can delete that folder" +
                        " without losing these settings." +
                        "\n\nIf this asset is deleted, it will be automatically recreated" +
                        " with default values when something needs it.",
                        MessageType.Info);
                }
            }

            /************************************************************************************************************************/

            private void DoDataGUI(SerializedProperty property, int index)
            {
                if (property.managedReferenceValue is AnimancerSettingsGroup value)
                {
                    DoHeading(value.DisplayName);

                    var first = true;
                    var depth = property.depth;
                    while (property.NextVisible(first) && property.depth > depth)
                    {
                        first = false;

                        EditorGUILayout.PropertyField(property, true);
                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();

                    DoHeading("Missing Type");

                    if (GUILayout.Button("X", AnimancerGUI.MiniButtonStyle))
                    {
                        var count = _Data.arraySize;
                        _Data.DeleteArrayElementAtIndex(index);
                        if (count == _Data.arraySize)
                            _Data.DeleteArrayElementAtIndex(index);

                        serializedObject.ApplyModifiedProperties();
                        GUIUtility.ExitGUI();
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            /************************************************************************************************************************/

            private void DoOptionalWarningsGUI()
            {
                DoHeading("Optional Warnings");

                EditorGUILayout.BeginHorizontal();

                using (var label = PooledGUIContent.Acquire("Disabled Warnings"))
                {
                    EditorGUI.BeginChangeCheck();
                    var value = EditorGUILayout.EnumFlagsField(label, Validate.PermanentlyDisabledWarnings);
                    if (EditorGUI.EndChangeCheck())
                        Validate.PermanentlyDisabledWarnings = (OptionalWarning)value;
                }

                if (GUILayout.Button("Help", EditorStyles.miniButton, AnimancerGUI.DontExpandWidth))
                    Application.OpenURL(Strings.DocsURLs.OptionalWarning);

                EditorGUILayout.EndHorizontal();
            }

            /************************************************************************************************************************/

            private static GUIStyle _HeadingStyle;

            /// <summary>Draws a heading label.</summary>
            public static void DoHeading(string text)
                => GUILayout.Label(text, _HeadingStyle ??= new(EditorStyles.largeLabel)
                {
                    fontSize = 18,
                });

            /************************************************************************************************************************/

            /// <summary>Creates the Project Settings page.</summary>
            [SettingsProvider]
            public static SettingsProvider CreateSettingsProvider()
            {
                UnityEditor.Editor editor = null;

                return new("Project/" + Strings.ProductName, SettingsScope.Project)
                {
                    keywords = new HashSet<string>() { Strings.ProductName },
                    guiHandler = searchContext =>
                    {
                        if (editor == null)
                            editor = CreateEditor(Instance);

                        HideNextInfo = true;

                        editor.OnInspectorGUI();
                    },
                };
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
}

#endif
