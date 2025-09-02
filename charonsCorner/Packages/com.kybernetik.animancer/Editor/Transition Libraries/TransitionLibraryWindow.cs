// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using Animancer.TransitionLibraries;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;
using Object = UnityEngine.Object;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only]
    /// An <see cref="EditorWindow"/> for configuring <see cref="TransitionLibraryAsset"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibraryWindow
    public class TransitionLibraryWindow :
        SerializedDataEditorWindow<TransitionLibraryAsset, TransitionLibraryDefinition>
    {
        /************************************************************************************************************************/

        /// <summary>Opens a window for the `library`.</summary>
        public static TransitionLibraryWindow Open(TransitionLibraryAsset library)
            => Open<TransitionLibraryWindow>(library, true, typeof(SceneView));

        /************************************************************************************************************************/

        /// <summary>
        /// Double clicking a <see cref="TransitionLibraryAsset"/>
        /// opens it in the <see cref="TransitionLibraryWindow"/>.
        /// </summary>
        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            var library = EditorUtility.InstanceIDToObject(instanceID) as TransitionLibraryAsset;
            if (library == null)
                return false;

            Open(library);
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>The current window instance.</summary>
        public static TransitionLibraryWindow Instance { get; private set; }

        /// <summary>Is a window currently showing the `library`.</summary>
        public static bool IsShowing(Object library)
            => Instance != null
            && Instance.SourceObject == library;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override TransitionLibraryDefinition SourceData
        {
            get => SourceObject.Definition;
            set => SourceObject.Definition = value;
        }

        /************************************************************************************************************************/

        [SerializeField]
        private TransitionLibrarySelection _Selection;

        /// <summary>Manages the objects which can be selected within a library.</summary>
        public TransitionLibrarySelection Selection
            => AnimancerEditorUtilities.FindOrCreate(ref _Selection);

        /************************************************************************************************************************/

        [SerializeReference]
        private List<TransitionLibraryWindowPage> _Pages;

        [SerializeField]
        private int _CurrentPage;

        /// <summary>The currently selected page.</summary>
        public TransitionLibraryWindowPage CurrentPage
        {
            get
            {
                _CurrentPage = Mathf.Clamp(_CurrentPage, 0, _Pages.Count - 1);
                return _Pages[_CurrentPage];
            }
        }

        /************************************************************************************************************************/

        /// <summary>Object highlight manager.</summary>
        public readonly TransitionLibraryWindowHighlighter
            Highlighter = new();

        /************************************************************************************************************************/

        /// <summary>Called when an object is selected.</summary>
        private void OnSelectionChange()
        {
            if (_Selection != null)
                _Selection.OnSelectionChange();

            var library = UnityEditor.Selection.activeObject as TransitionLibraryAsset;
            if (library != null && library != SourceObject)
                SetAndCaptureSource(library);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            Instance = this;
            wantsMouseMove = true;

            // MainStageView, CanvasGroup Icon, GridLayoutGroup Icon.
            titleContent = EditorGUIUtility.IconContent("CanvasGroup Icon");
            titleContent.text = "Transition Library";

            AnimancerEditorUtilities.InstantiateDerivedTypes(ref _Pages);

            for (int i = 0; i < _Pages.Count; i++)
                _Pages[i].Window = this;

            OnSelectionChange();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (Instance == this)
                Instance = null;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyImmediate(_Selection);
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI of this window.</summary>
        protected virtual void OnGUI()
        {
            if (SourceObject == null)
            {
                GUILayout.Label("No Transition Library has been selected");
                return;
            }

            DoHeaderGUI();
            DoBodyGUI();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void CaptureData()
        {
            base.CaptureData();
            Data.SortAliases();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Apply()
        {
            base.Apply();

            for (int i = 0; i < Data.Transitions.Length; i++)
            {
                var transition = Data.Transitions[i];
                if (EditorUtility.IsPersistent(transition))
                    continue;

                AssetDatabase.AddObjectToAsset(transition, SourceObject);
            }
        }

        /************************************************************************************************************************/

        private static ButtonGroupStyles _ApplyRevertStyles;

        /// <summary>Draws the header GUI.</summary>
        private void DoHeaderGUI()
        {
            if (_ApplyRevertStyles.left == null)
                _ApplyRevertStyles = new(
                    EditorStyles.toolbarButton,
                    EditorStyles.toolbarButton,
                    EditorStyles.toolbarButton);

            GUILayout.BeginHorizontal();

            var style = EditorStyles.toolbar;

            var applyRevertWidth = CalculateApplyRevertWidth(_ApplyRevertStyles) - StandardSpacing - 1;

            var area = GUILayoutUtility.GetRect(position.width, style.fixedHeight);

            var currentEvent = Event.current;
            if (currentEvent.type == EventType.Repaint)
                style.Draw(area, false, false, false, false);

            var pageArea = StealFromLeft(ref area, PageSelectionWidth);
            var applyRevertArea = StealFromRight(ref area, applyRevertWidth);
            var pathArea = area;

            DoPageSelectionDropdown(pageArea);
            DoAssetPathButton(pathArea, currentEvent);

            DoApplyRevertGUI(applyRevertArea, _ApplyRevertStyles);

            GUILayout.EndHorizontal();
        }

        /************************************************************************************************************************/

        [NonSerialized]
        private float _PageSelectionWidth;

        private float PageSelectionWidth
        {
            get
            {
                if (_PageSelectionWidth == 0)
                {
                    for (int i = 0; i < _Pages.Count; i++)
                    {
                        _PageSelectionWidth = Math.Max(
                            _PageSelectionWidth,
                            EditorStyles.toolbarDropDown.CalculateWidth(_Pages[i].DisplayName));
                    }
                }

                return _PageSelectionWidth;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws a dropdown button for selecting the <see cref="CurrentPage"/>.</summary>
        private void DoPageSelectionDropdown(Rect area)
        {
            using (var label = PooledGUIContent.Acquire(CurrentPage.DisplayName, CurrentPage.HelpTooltip))
                if (!EditorGUI.DropdownButton(area, label, FocusType.Passive, EditorStyles.toolbarDropDown))
                    return;

            var menu = new GenericMenu();

            for (int i = 0; i < _Pages.Count; i++)
            {
                var index = i;
                var page = _Pages[index];
                menu.AddItem(
                    new(page.DisplayName),
                    _CurrentPage == index,
                    () => _CurrentPage = index);
            }

            menu.AddSeparator("");
            menu.AddItem(
                new("Documentation"),
                false,
                () => Application.OpenURL(Strings.DocsURLs.TransitionLibraries));

            menu.ShowAsContext();
        }

        /************************************************************************************************************************/

        private static GUIStyle _AssetPathStyle;

        private readonly GUIContent AssetPath = new();

        /// <summary>Draws the asset path of the target library and selects it if clicked.</summary>
        private void DoAssetPathButton(Rect area, Event currentEvent)
        {
            _AssetPathStyle ??= new(EditorStyles.toolbarButton)
            {
                richText = true,
                alignment = TextAnchor.MiddleRight,
                fontStyle = FontStyle.Italic,
                fontSize = (int)(EditorStyles.toolbarButton.fontSize * 0.8f),
            };

            if (currentEvent.type == EventType.Repaint)
            {
                var assetPath = AssetDatabase.GetAssetPath(SourceObject);
                if (string.IsNullOrEmpty(assetPath))
                {
                    AssetPath.text = "The target Transition Library isn't saved as an asset.";
                    AssetPath.tooltip = null;
                }
                else if (AssetPath.tooltip != assetPath)
                {
                    AssetPath.tooltip = assetPath;

                    var directory = Path.GetDirectoryName(assetPath).Replace('\\', '/');
                    var file = Path.GetFileNameWithoutExtension(assetPath);
                    assetPath = $"{directory}/<b>{file}</b>";

                    AssetPath.text = assetPath;
                }
            }

            if (GUI.Button(area, AssetPath, _AssetPathStyle))
            {
                if (Selection.Selected != (object)SourceObject)
                    Selection.Select(this, SourceObject, -1, TransitionLibrarySelection.SelectionType.Library);
                else
                    EditorGUIUtility.PingObject(SourceObject);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws the <see cref="CurrentPage"/>.</summary>
        private void DoBodyGUI()
        {
            GUILayout.FlexibleSpace();
            var area = GUILayoutUtility.GetLastRect();
            area.width = position.width;

            EditorGUI.DrawRect(area, Grey(0.2f, 0.5f));

            if (_Pages.Count > 0)
            {
                Highlighter.BeginGUI(area);

                CurrentPage?.OnGUI(area);

                Highlighter.EndGUI(this);
            }
        }

        /************************************************************************************************************************/
    }
}

#endif

