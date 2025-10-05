// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A custom Inspector for <see cref="StringAsset"/> fields.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/StringAssetDrawer
    [CustomPropertyDrawer(typeof(StringAsset), true)]
    public class StringAssetDrawer : PropertyDrawer
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => LineHeight;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(area, label, property);

            property.objectReferenceValue = DrawGUI(
                area,
                label,
                property,
                out var exitGUI);

            if (exitGUI)
            {
                property.serializedObject.ApplyModifiedProperties();
                GUIUtility.ExitGUI();
            }

            EditorGUI.EndProperty();
        }

        /************************************************************************************************************************/

        private static readonly Func<Object[]> GetCurrentPropertyValues =
            () => _CurrentProperty != null ? Serialization.GetValues<Object>(_CurrentProperty) : null;

        private static SerializedProperty _CurrentProperty;

        /// <summary>Draws the GUI for a <see cref="StringAsset"/>.</summary>
        public static Object DrawGUI(
            Rect area,
            GUIContent label,
            SerializedProperty property,
            out bool exitGUI)
        {
            var showMixedValue = EditorGUI.showMixedValue;
            if (property != null && property.hasMultipleDifferentValues)
                EditorGUI.showMixedValue = true;

            _CurrentProperty = property;
            var value = DrawGUI(
                area,
                label,
                property?.objectReferenceValue,
                property?.serializedObject?.targetObject,
                out exitGUI,
                GetCurrentPropertyValues);
            _CurrentProperty = null;

            EditorGUI.showMixedValue = showMixedValue;

            return value;
        }

        /************************************************************************************************************************/

        private static readonly int ButtonHash = "Button".GetHashCode();
        private static readonly int ObjectFieldHash = "s_ObjectFieldHash".GetHashCode();

        /// <summary>Draws the GUI for a <see cref="StringAsset"/>.</summary>
        public static Object DrawGUI(
            Rect area,
            GUIContent label,
            Object value,
            Object context,
            out bool exitGUI,
            Func<Object[]> getAllValues = null)
        {
            var currentEvent = Event.current;
            if (currentEvent.type == EventType.Repaint &&
                !area.Contains(currentEvent.mousePosition) &&
                !IsDraggingStringAsset())
            {
                GUIUtility.GetControlID(ButtonHash, FocusType.Passive);// Button.
                GUIUtility.GetControlID(ButtonHash, FocusType.Passive);// Button.
                GUIUtility.GetControlID(DragHint, FocusType.Passive, area);// DragAndDrop.

                var iconSize = EditorGUIUtility.GetIconSize();
                var newIconSize = LineHeight * 2 / 3;
                EditorGUIUtility.SetIconSize(new(newIconSize, newIconSize));

                var controlID = GUIUtility.GetControlID(ObjectFieldHash, FocusType.Keyboard, area);// Object.

                var valueArea = EditorGUI.PrefixLabel(area, controlID, label);

                using (var content = PooledGUIContent.Acquire())
                {
                    if (value != null)
                    {
                        content.text = value.name;
                        content.image = AnimancerIcons.ScriptableObject;
                    }
                    else
                    {
                        content.text = "";
                        content.image = AssetPreview.GetMiniTypeThumbnail(typeof(StringAsset));
                    }

                    EditorStyles.objectField.Draw(valueArea, content, false, false, false, false);
                }

                EditorGUIUtility.SetIconSize(iconSize);

                exitGUI = false;
                return value;
            }

            if (value == null)
            {
                var buttonArea = StealFromRight(ref area, area.height, StandardSpacing);

                var content = AnimancerIcons.AddIcon("Create and save a new String Asset");
                if (GUI.Button(buttonArea, content, NoPaddingButtonStyle))
                {
                    exitGUI = true;
                    return CreateNewInstance(label.text, context);
                }

                GUIUtility.GetControlID(ButtonHash, FocusType.Passive);
            }
            else
            {
                var clearArea = StealFromRight(ref area, area.height, StandardSpacing);
                var copyArea = StealFromRight(ref area, area.height, StandardSpacing);

                var content = AnimancerIcons.CopyIcon("Copy string to clipboard");
                if (GUI.Button(copyArea, content, NoPaddingButtonStyle))
                    GUIUtility.systemCopyBuffer = value.name;

                content = AnimancerIcons.ClearIcon("Clear reference");
                if (GUI.Button(clearArea, content, NoPaddingButtonStyle))
                    value = null;
            }

            HandleDragAndDrop(area, currentEvent, value, getAllValues);

            exitGUI = false;
            return EditorGUI.ObjectField(area, label, value, typeof(StringAsset), false);
        }

        /************************************************************************************************************************/

        private static readonly int DragHint = "Drag".GetHashCode();

        private static void HandleDragAndDrop(
            Rect area,
            Event currentEvent,
            Object value,
            Func<Object[]> getAllValues)
        {
            var id = GUIUtility.GetControlID(DragHint, FocusType.Passive, area);

            switch (currentEvent.type)
            {
                // Drag out of object field.
                case EventType.MouseDrag:
                    if (GUIUtility.keyboardControl == id + 1 &&
                        currentEvent.button == 0 &&
                        area.Contains(currentEvent.mousePosition) &&
                        value != null)
                    {
                        var values = getAllValues?.Invoke() ?? new Object[] { value };
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.objectReferences = values;
                        DragAndDrop.StartDrag("Objects");
                        currentEvent.Use();
                    }
                    break;
            }
        }

        /// <summary>Is a <see cref="StringAsset"/> currently being dragged?</summary>
        private static bool IsDraggingStringAsset()
        {
            var dragging = DragAndDrop.objectReferences;
            if (dragging.IsNullOrEmpty())
                return false;

            for (int i = 0; i < dragging.Length; i++)
                if (dragging[i] is not StringAsset)
                    return false;

            return true;
        }

        /************************************************************************************************************************/

        private const string FolderPathKey = nameof(StringAsset) + ".FolderPath";

        /// <summary>Asks where to save a new <see cref="StringAsset"/>.</summary>
        private static Object CreateNewInstance(string name, Object targetObject)
        {
            var folderPath = GetSaveFolder(targetObject);

            var path = EditorUtility.SaveFilePanelInProject(
                "Create String Asset",
                name,
                "asset",
                "Where yould you like to save the new String Asset?",
                folderPath);

            if (string.IsNullOrEmpty(path))
                return null;

            EditorPrefs.SetString(FolderPathKey, Path.GetDirectoryName(path));

            var instance = ScriptableObject.CreateInstance<StringAsset>();

            AssetDatabase.CreateAsset(instance, path);

            return instance;
        }

        private static string GetSaveFolder(Object targetObject)
        {
            var getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod(
                "GetActiveFolderPath",
                AnimancerReflection.StaticBindings,
                null,
                Type.EmptyTypes,
                null);
            if (getActiveFolderPath != null &&
                getActiveFolderPath.ReturnType == typeof(string))
            {
                var activeFolderPath = getActiveFolderPath.Invoke(null, Array.Empty<object>())?.ToString();
                if (!string.IsNullOrEmpty(activeFolderPath))
                    return activeFolderPath;
            }

            var folderPath = AssetDatabase.GetAssetPath(targetObject);
            if (!string.IsNullOrEmpty(folderPath))
                folderPath = Path.GetDirectoryName(folderPath);

            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = EditorPrefs.GetString(FolderPathKey);
                if (folderPath == null || !folderPath.StartsWith("Assets/"))
                    folderPath = "Assets/";
            }

            return folderPath;
        }

        /************************************************************************************************************************/
    }
}

#endif

