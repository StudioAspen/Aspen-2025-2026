// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor.Tools
{
    /// <summary>[Editor-Only] [Pro-Only]
    /// An <see cref="EditorWindow"/> with various utilities for managing sprites and generating animations.
    /// </summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/tools">
    /// Animancer Tools</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Tools/AnimancerToolsWindow
    /// 
    public sealed partial class AnimancerToolsWindow : EditorWindow
    {
        /************************************************************************************************************************/

        /// <summary>The display name of this window.</summary>
        public const string Name = "Animancer Tools";

        /// <summary>The singleton instance of this window.</summary>
        public static AnimancerToolsWindow Instance { get; private set; }

        [SerializeReference] private List<Tool> _Tools;

        [SerializeField] private Vector2 _Scroll;

        [SerializeField] private int _CurrentTool = -1;

        /************************************************************************************************************************/

        private SerializedObject _SerializedObject;

        private SerializedObject SerializedObject
            => _SerializedObject ??= new(this);

        /// <summary>Returns the <see cref="SerializedProperty"/> which represents the specified `tool`.</summary>
        public SerializedProperty FindSerializedPropertyForTool(Tool tool)
        {
            var index = _Tools.IndexOf(tool);
            var property = SerializedObject.FindProperty(nameof(_Tools));
            return property.GetArrayElementAtIndex(index);
        }

        /************************************************************************************************************************/

        private void OnEnable()
        {
            titleContent = new(Name);
            Instance = this;

            InitializeTools();

            Undo.undoRedoPerformed += Repaint;

            OnSelectionChange();
        }

        /************************************************************************************************************************/

        private void InitializeTools()
        {
            AnimancerEditorUtilities.InstantiateDerivedTypes(ref _Tools);

            for (int i = 0; i < _Tools.Count; i++)
                _Tools[i].OnEnable(i);
        }

        /************************************************************************************************************************/

        private int IndexOfTool(Type type)
        {
            for (int i = 0; i < _Tools.Count; i++)
                if (_Tools[i].GetType() == type)
                    return i;

            return -1;
        }

        /************************************************************************************************************************/

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;

            for (int i = 0; i < _Tools.Count; i++)
                _Tools[i].OnDisable();

            if (_SerializedObject != null)
            {
                _SerializedObject.Dispose();
                _SerializedObject = null;
            }
        }

        /************************************************************************************************************************/

        private void OnSelectionChange()
        {
            for (int i = 0; i < _Tools.Count; i++)
                _Tools[i].OnSelectionChanged();

            Repaint();
        }

        /************************************************************************************************************************/

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = Mathf.Min(EditorGUIUtility.labelWidth, position.width * 0.5f);
            EditorGUIUtility.wideMode = true;

            _Scroll = GUILayout.BeginScrollView(_Scroll);
            GUILayout.BeginVertical();
            GUILayout.EndVertical();
            for (int i = 0; i < _Tools.Count; i++)
                _Tools[i].DoGUI();
            GUILayout.EndScrollView();
        }

        /************************************************************************************************************************/

        /// <summary>Causes the <see cref="Instance"/> to redraw its GUI.</summary>
        public static new void Repaint()
        {
            if (Instance != null)
                ((EditorWindow)Instance).Repaint();
        }

        /// <summary>Calls <see cref="Undo.RecordObject(Object, string)"/> for this window.</summary>
        public static void RecordUndo()
            => Undo.RecordObject(Instance, Name);

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="EditorGUI.BeginChangeCheck"/>.</summary>
        public static void BeginChangeCheck()
            => EditorGUI.BeginChangeCheck();

        /// <summary>Calls <see cref="EditorGUI.EndChangeCheck"/> and <see cref="RecordUndo"/> if it returned true.</summary>
        public static bool EndChangeCheck()
        {
            if (!EditorGUI.EndChangeCheck())
                return false;

            RecordUndo();
            return true;

        }

        /// <summary>Calls <see cref="EndChangeCheck"/> and sets the <c>field = value</c> if it returned true.</summary>
        public static bool EndChangeCheck<T>(ref T field, T value)
        {
            if (!EndChangeCheck())
                return false;

            field = value;
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>Creates and initializes a new <see cref="ReorderableList"/>.</summary>
        public static ReorderableList CreateReorderableList<T>(
            List<T> list,
            string name,
            ReorderableList.ElementCallbackDelegate drawElementCallback,
            bool showFooter = false)
        {
            var reorderableList = new ReorderableList(list, typeof(T))
            {
                drawHeaderCallback = (area) => GUI.Label(area, name),
                drawElementCallback = drawElementCallback,
                elementHeight = AnimancerGUI.LineHeight + AnimancerGUI.StandardSpacing,
            };

            if (!showFooter)
            {
                reorderableList.footerHeight = 0;
                reorderableList.displayAdd = false;
                reorderableList.displayRemove = false;
            }

            return reorderableList;
        }

        /************************************************************************************************************************/

        /// <summary>Creates and initializes a new <see cref="ReorderableList"/> for <see cref="Sprite"/>s.</summary>
        public static ReorderableList CreateReorderableObjectList<T>(
            List<T> objects,
            string name,
            bool showFooter = false)
            where T : Object
        {
            var reorderableList = CreateReorderableList(objects, name, (area, index, isActive, isFocused) =>
            {
                area.y = Mathf.Ceil(area.y + AnimancerGUI.StandardSpacing * 0.5f);
                area.height = AnimancerGUI.LineHeight;

                BeginChangeCheck();
                var obj = AnimancerGUI.DoObjectFieldGUI(area, "", objects[index], false);
                if (EndChangeCheck())
                {
                    objects[index] = obj;
                }
            }, showFooter);

            if (showFooter)
            {
                reorderableList.onAddCallback = (list) => list.list.Add(null);
            }

            return reorderableList;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="ReorderableList"/> for <see cref="string"/>s.</summary>
        public static ReorderableList CreateReorderableStringList(
            List<string> strings,
            string name,
            Func<Rect, int, string> doElementGUI)
        {
            return CreateReorderableList(strings, name, (area, index, isActive, isFocused) =>
            {
                area.y = Mathf.Ceil(area.y + AnimancerGUI.StandardSpacing * 0.5f);
                area.height = AnimancerGUI.LineHeight;

                BeginChangeCheck();
                var str = doElementGUI(area, index);
                if (EndChangeCheck())
                {
                    strings[index] = str;
                }
            });
        }

        /// <summary>Creates a new <see cref="ReorderableList"/> for <see cref="string"/>s.</summary>
        public static ReorderableList CreateReorderableStringList(
            List<string> strings,
            string name)
        {
            return CreateReorderableStringList(strings, name, (area, index) =>
            {
                return EditorGUI.TextField(area, strings[index]);
            });
        }

        /************************************************************************************************************************/

        /// <summary>Opens the <see cref="AnimancerToolsWindow"/>.</summary>
        [MenuItem(Strings.AnimancerToolsMenuPath)]
        public static void Open()
            => GetWindow<AnimancerToolsWindow>();

        /// <summary>Opens the <see cref="AnimancerToolsWindow"/> showing the specified `tool`.</summary>
        public static void Open(Type toolType)
        {
            var window = GetWindow<AnimancerToolsWindow>();
            window._CurrentTool = AnimancerEditorUtilities.IndexOfType(window._Tools, toolType);
        }

        /************************************************************************************************************************/
    }
}

#endif

