// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //
// FlexiMotion // https://kybernetik.com.au/flexi-motion // Copyright 2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A <see cref="SerializedComponentDataEditorWindow{TObject, TData}"/>
    /// which displays a <see cref="TransformTreeView"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TransformTreeWindow_2
    /// https://kybernetik.com.au/flexi-motion/api/FlexiMotion.Editor/TransformTreeWindow_2
    public abstract class TransformTreeWindow<TObject, TData> :
        SerializedComponentDataEditorWindow<TObject, TData>,
        ITransformTreeViewSource
        where TObject : Component
        where TData : class, ICopyable<TData>, IEquatable<TData>, new()
    {
        /************************************************************************************************************************/

        [SerializeField] private MultiColumnHeaderState _HeaderState;
        [SerializeField] private TreeViewState _GUIState;

        [NonSerialized] private TransformTreeView _TreeView;

        /************************************************************************************************************************/

        /// <summary>The header of the tree view.</summary>
        public MultiColumnHeaderState HeaderState => _HeaderState;

        /// <summary>The view used to display the <see cref="SerializedDataEditorWindow{TObject, TData}.Data"/>.</summary>
        public TransformTreeView TreeView => _TreeView;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            minSize = new Vector2(400, 200);

            Initialize();

            Undo.undoRedoPerformed += ReloadTreeView;
            Selection.selectionChanged += Repaint;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            base.OnDisable();

            Undo.undoRedoPerformed -= ReloadTreeView;
            Selection.selectionChanged -= Repaint;

            _TreeView?.Dispose();

            SceneView.RepaintAll();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void CaptureData()
        {
            base.CaptureData();
            Initialize();
        }

        /// <summary>
        /// Initializes this window if the <see cref="SerializedDataEditorWindow{TObject, TData}.SourceObject"/>
        /// has been set.
        /// </summary>
        protected virtual void Initialize()
        {
            if (SourceObject == null)
                return;

            titleContent = new($"{SourceObject.name}: {SourceObject.GetType().Name}");

            var isNew = _GUIState == null;
            if (isNew)
                _GUIState = new();

            if (_TreeView == null)
            {
                _TreeView = new(_GUIState, null, this);
                _TreeView.Reload();
                _TreeView.OnObjectSelectionChanged();
            }

            CreateHeader();

            if (isNew)
                InitializeExpandedRows();
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="CreateColumns"/> and initializes the tree view header.</summary>
        protected void CreateHeader()
        {
            var serializedHeaderState = _HeaderState;
            _HeaderState = new(CreateColumns(position.width - LineHeight));
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(serializedHeaderState, _HeaderState))
                MultiColumnHeaderState.OverwriteSerializedFields(serializedHeaderState, _HeaderState);

            _TreeView.multiColumnHeader = new MultiColumnHeader(_HeaderState);
        }

        /// <summary>Creates the columns for the <see cref="TreeView"/> to use.</summary>
        protected abstract MultiColumnHeaderState.Column[] CreateColumns(float width);

        /// <summary>Creates a column for the <see cref="TreeView"/> to use.</summary>
        protected static MultiColumnHeaderState.Column CreateColumn(string name, string tooltip, float width)
            => new()
            {
                headerContent = new GUIContent(name, tooltip),
                width = width,
                allowToggleVisibility = false,
                canSort = false,
            };

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void AddItems(ref int id, TreeViewItem root)
        {
            var rootTransform = Root;
            TreeView.AddItemRecursive(ref id, root, rootTransform);

            var transforms = Transforms;
            var count = transforms.Count;
            for (int i = 0; i < count; i++)
            {
                var transform = transforms[i];
                if (!transform.IsChildOf(rootTransform))
                {
                    AddItem(ref id, root, transform);
                }
            }
        }

        /// <inheritdoc/>
        public virtual TreeViewItem AddItem(ref int id, TreeViewItem parent, Transform transform)
            => TreeView.AddItem(ref id, parent, transform);

        /************************************************************************************************************************/

        private void InitializeExpandedRows()
        {
            var includedTransforms = Transforms;
            if (includedTransforms.Count == 0)// If there are no springs, expand everything.
            {
                _TreeView.SetExpandedRecursive(TransformTreeView.RootID, true);
            }
            else// Otherwise, only expand to show all springs.
            {
                var allTransforms = _TreeView.Transforms;
                for (int i = 0; i < allTransforms.Count; i++)
                {
                    var transform = allTransforms[i];
                    if (includedTransforms.Contains(transform))
                    {
                        while (transform.parent != null)
                        {
                            transform = transform.parent;
                            var index = allTransforms.LastIndexOf(transform);
                            if (index >= 0)
                                _TreeView.SetExpanded(index, true);
                        }
                    }
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws the GUI of this window.</summary>
        protected virtual void OnGUI()
        {
            if (_TreeView == null ||
                Data == null ||
                SourceObject == null)
            {
                Close();
                return;
            }

            GUILayout.FlexibleSpace();
            var area = GUILayoutUtility.GetLastRect();
            area.width = position.width;

            var searchArea = area;
            searchArea.xMin += StandardSpacing;
            searchArea.y += StandardSpacing;
            searchArea.height = LineHeight;

            area.yMin = searchArea.yMax + StandardSpacing;

            _TreeView.DrawSearchField(searchArea);

            _TreeView.OnGUI(area);

            DoFooterGUI();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual Transform Root
            => AnimancerUtilities.FindRoot(SourceObject.gameObject);

        /// <inheritdoc/>
        public abstract IList<Transform> Transforms { get; }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void BeforeRowGUI(Rect area, TreeViewItem item)
        {
            var color = GetRowColor(item);
            if (color.a > 0)
                EditorGUI.DrawRect(area, color);
        }

        /// <summary>Gets the color of a row in the <see cref="TreeView"/>.</summary>
        protected virtual Color GetRowColor(TreeViewItem item)
            => default;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public abstract void DrawCellGUI(Rect area, int column, int row, TreeViewItem item, ref bool isSelectionClick);

        /************************************************************************************************************************/

        /// <summary>Draws a <see cref="Transform"/> cell.</summary>
        public void DrawTransformCellGUI(Rect area, Transform transform)
        {
            var enabled = GUI.enabled;
            if (Event.current.type != EventType.Repaint)
                GUI.enabled = false;

            DoObjectFieldGUI(area, GUIContent.none, transform.gameObject, true);

            GUI.enabled = enabled;
        }

        /************************************************************************************************************************/

        /// <summary>Draws a cell to toggle whether a particular item is included or not.</summary>
        public void DrawIsIncludedCellGUI(
            Rect area,
            int treeItemID,
            int definitionIndex,
            ref bool isSelectionClick)
        {
            EditorGUI.BeginChangeCheck();

            var isIncluded = definitionIndex >= 0;
            isIncluded = GUI.Toggle(area, isIncluded, "");

            if (EditorGUI.EndChangeCheck())
            {
                SetIncludedWithSelection(treeItemID, isIncluded);
                isSelectionClick = false;
            }
        }

        /************************************************************************************************************************/

        private static readonly List<int>
            TreeItemIDs = new();

        private List<int> GetSelectedIDsWith(int treeItemID)
        {
            TreeItemIDs.Clear();
            TreeItemIDs.AddRange(TreeView.GetSelection());
            if (!TreeItemIDs.Contains(treeItemID))
                TreeItemIDs.Add(treeItemID);
            TreeItemIDs.Sort();
            return TreeItemIDs;
        }

        /************************************************************************************************************************/

        private void SetIncludedWithSelection(
            int treeItemID,
            bool isIncluded)
        {
            RecordUndo();

            var selected = GetSelectedIDsWith(treeItemID);
            for (int i = selected.Count - 1; i >= 0; i--)
            {
                treeItemID = selected[i];
                var definitionIndex = GetDefinitionIndex(treeItemID);
                SetIncluded(treeItemID, definitionIndex, isIncluded);
                EditorGUIUtility.editingTextField = false;
            }

            TreeView.Reload();
            GUIUtility.ExitGUI();
        }

        /// <summary>Adds or removes an item from the <see cref="SerializedDataEditorWindow{TObject, TData}.Data"/>.</summary>
        protected abstract void SetIncluded(
            int treeItemID,
            int definitionIndex,
            bool isIncluded);

        /************************************************************************************************************************/

        /// <summary>Sets the value of a field for all selected items.</summary>
        protected void SetValue(
            int treeItemID,
            Action<int> setValue)
        {
            RecordUndo();

            var selected = GetSelectedIDsWith(treeItemID);
            for (int i = selected.Count - 1; i >= 0; i--)
            {
                var definitionIndex = GetDefinitionIndex(selected[i]);
                if (definitionIndex >= 0)
                    setValue(definitionIndex);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Gets the index in the <see cref="SerializedDataEditorWindow{TObject, TData}.Data"/>
        /// corresponding to a row in the <see cref="TreeView"/>.
        /// Returns -1 if the row isn't included.
        /// </summary>
        protected virtual int GetDefinitionIndex(int treeItemID)
        {
            if (_TreeView.Transforms.TryGetObject(treeItemID, out var transform))
                return Transforms.IndexOf(transform);
            else
                return -1;
        }

        /************************************************************************************************************************/

        private static readonly GUIContent
            RevertLabel = new(
                "Revert",
                "Undo all changes made in this window"),
            ApplyLabel = new(
                "Apply",
                "Apply all changes made in this window to the source object"),
            AutoApplyLabel = new(
                "Auto Apply",
                "Immediately apply all changes made in this window to the source object?");

        /// <summary>Draws the GUI at the bottom of this window.</summary>
        protected virtual void DoFooterGUI()
        {
            GUILayout.Space(StandardSpacing);

            var area = GUILayoutUtility.GetRect(0, 0);
            area.y -= 1;
            area.height = 1;
            EditorGUI.DrawRect(area, Grey(0.5f, 0.5f));

            GUILayout.BeginHorizontal();

            using (new EditorGUI.DisabledScope(Event.current.type != EventType.Repaint))
                DoObjectFieldGUI("", SourceObject, true);

            GUILayout.FlexibleSpace();

            DoFooterCenterGUI();

            GUILayout.FlexibleSpace();

            DoApplyRevertGUI();

            GUILayout.EndHorizontal();
        }

        /// <summary>Draws additional GUI controls in the center of the footer.</summary>
        protected virtual void DoFooterCenterGUI() { }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Revert()
        {
            base.Revert();

            _TreeView.Reload();
        }

        /************************************************************************************************************************/

        private void ReloadTreeView()
            => TreeView.Reload();

        /************************************************************************************************************************/
    }
}

#endif

