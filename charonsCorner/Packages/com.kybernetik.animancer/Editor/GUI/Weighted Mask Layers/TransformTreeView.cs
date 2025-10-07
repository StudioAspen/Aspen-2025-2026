// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //
// FlexiMotion // https://kybernetik.com.au/flexi-motion // Copyright 2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Animancer.Editor
//namespace FlexiMotion.Editor
{
    /// <summary>An object that provides data to a <see cref="TransformTreeView"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ITransformTreeViewSource
    /// https://kybernetik.com.au/flexi-motion/api/FlexiMotion.Editor/ITransformTreeViewSource
    public interface ITransformTreeViewSource
    {
        /************************************************************************************************************************/

        /// <summary>The object at the top of the target hierarchy.</summary>
        Transform Root { get; }

        /// <summary>The objects to show in the view.</summary>
        IList<Transform> Transforms { get; }

        /// <summary>Adds the items to be displayed in the view.</summary>
        void AddItems(ref int id, TreeViewItem root);

        /// <summary>Adds an item for the `transform` to be displayed in the view.</summary>
        TreeViewItem AddItem(ref int id, TreeViewItem parent, Transform transform);

        /// <summary>Called before a row is drawn.</summary>
        void BeforeRowGUI(Rect area, TreeViewItem item);

        /// <summary>Draws a cell in the <see cref="TreeView"/>.</summary>
        void DrawCellGUI(Rect area, int column, int row, TreeViewItem item, ref bool isSelectionClick);

        /************************************************************************************************************************/
    }

    /// <summary>A <see cref="TreeView"/> for displaying <see cref="Transform"/>s alongside other data.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TransformTreeView
    /// https://kybernetik.com.au/flexi-motion/api/FlexiMotion.Editor/TransformTreeView
    public class TransformTreeView : TreeView, IDisposable
    {
        /************************************************************************************************************************/

        /// <summary>The ID of the root item.</summary>
        public const int RootID = 0;

        /// <summary>The object which defines what to show in this view.</summary>
        public readonly ITransformTreeViewSource Source;

        /// <summary>The field used to filter this view.</summary>
        public readonly SearchField Search = new();

        /// <summary>The <see cref="Transform"/> of each row in this view.</summary>
        public readonly List<Transform> Transforms = new();

        /************************************************************************************************************************/
        #region Initialization
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TransformTreeView"/>.</summary>
        public TransformTreeView(
            TreeViewState state,
            MultiColumnHeader header,
            ITransformTreeViewSource source)
            : base(state, header)
        {
            Source = source;

            Selection.selectionChanged += OnObjectSelectionChanged;
        }

        /************************************************************************************************************************/

        /// <summary>Cleans up this view.</summary>
        public void Dispose()
        {
            Selection.selectionChanged -= OnObjectSelectionChanged;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override TreeViewItem BuildRoot()
        {
            Transforms.Clear();

            var id = RootID;
            var root = CreateItem(ref id, -1, "");
            Transforms.Add(null);

            Source.AddItems(ref id, root);

            return root;
        }

        /************************************************************************************************************************/

        /// <summary>Adds a new item for the `transform` as a child of the `parent` and increments the `id`.</summary>
        public virtual TreeViewItem AddItem(ref int id, TreeViewItem parent, Transform transform)
        {
            Transforms.Add(transform);

            var item = CreateItem(ref id, parent.depth + 1, transform.name);
            parent.AddChild(item);
            return item;
        }

        /// <summary>Adds a new item for each child of the `transform` recursively.</summary>
        public void AddItemRecursive(ref int id, TreeViewItem parent, Transform transform)
        {
            parent = Source.AddItem(ref id, parent, transform);

            foreach (Transform child in transform)
                AddItemRecursive(ref id, parent, child);
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TreeViewItem"/> and increments the `id`.</summary>
        public static TreeViewItem CreateItem(ref int id, int depth, string displayName)
            => new()
            {
                id = id++,
                depth = depth,
                displayName = displayName,
            };

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region GUI
        /************************************************************************************************************************/

        /// <summary>Draws the <see cref="SearchField"/> for filtering this view.</summary>
        public void DrawSearchField(Rect area)
        {
            searchString = Search.OnGUI(area, searchString);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void RowGUI(RowGUIArgs args)
        {
            Source.BeforeRowGUI(args.rowRect, args.item);

            var currentEvent = Event.current;
            var isClick =
                currentEvent.type == EventType.MouseDown &&
                args.rowRect.Contains(currentEvent.mousePosition);

            var visibleColumnCount = args.GetNumVisibleColumns();
            for (int i = 0; i < visibleColumnCount; ++i)
            {
                var area = args.GetCellRect(i);
                if (i == 0)
                    area.xMin += (args.item.depth + 1) * depthIndentWidth;

                Source.DrawCellGUI(area, args.GetColumn(i), args.row, args.item, ref isClick);
            }

            if (isClick && currentEvent.type == EventType.Used)
                SelectionClick(args.item, false);
        }

        /************************************************************************************************************************/

        private static readonly List<GameObject>
            SelectedObjects = new();

        /// <summary>Called whenever the selected rows change.</summary>
        public event Action<IList<int>> OnSelectionChanged;

        /// <inheritdoc/>
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            SelectedObjects.Clear();

            for (int i = 0; i < selectedIds.Count; i++)
                if (Transforms.TryGetObject(selectedIds[i], out var transform))
                    SelectedObjects.Add(transform.gameObject);

            Selection.objects = SelectedObjects.ToArray();

            OnSelectionChanged?.Invoke(selectedIds);
        }

        /************************************************************************************************************************/

        private static readonly List<int>
            SelectedIDs = new();

        /// <summary>Called whenever the <see cref="Selection.objects"/> change.</summary>
        public void OnObjectSelectionChanged()
        {
            var selectedObjects = Selection.objects;

            SelectedIDs.Clear();
            SelectedIDs.AddRange(GetSelection());

            // Remove IDs that aren't in the Selection.
            for (int i = SelectedIDs.Count - 1; i >= 0; i--)
            {
                if (!Transforms.TryGetObject(SelectedIDs[i], out var transform) ||
                    Array.IndexOf(selectedObjects, transform.gameObject) < 0)
                {
                    SelectedIDs.RemoveAt(i);
                }
            }

            // If no selected rows correspond to a selected object, add all rows of that object.
            foreach (var selected in selectedObjects)
            {
                if (!AnimancerUtilities.TryGetTransform(selected, out var selectedTransform) ||
                    IsAlreadySelected(selectedTransform))
                    continue;

                var index = Transforms.IndexOf(selectedTransform);
                if (index >= 0)
                {
                    SelectedIDs.Add(index);

                    while (Transforms.TryGetObject(++index, out var transform) &&
                        transform == selectedTransform)
                        SelectedIDs.Add(index);
                }
            }

            SetSelection(SelectedIDs);
        }

        private bool IsAlreadySelected(Transform transform)
        {
            for (int i = 0; i < SelectedIDs.Count; i++)
            {
                if (!Transforms.TryGetObject(SelectedIDs[i], out var selectedTransform))
                    continue;

                if (selectedTransform == transform)
                    return true;
            }

            return false;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

