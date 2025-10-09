// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;

namespace Animancer.Editor
{
    /// <summary>An <see cref="TransformTreeWindow{TTarget, TDefinition}"/> for editing spring definitions.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/WeightedMaskLayersDefinitionWindow
    public class WeightedMaskLayersDefinitionWindow :
        TransformTreeWindow<WeightedMaskLayers, WeightedMaskLayersDefinition>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override IList<Transform> Transforms
            => Data.Transforms;

        /// <inheritdoc/>
        public override WeightedMaskLayersDefinition SourceData
        {
            get => SourceObject.Definition;
            set => SourceObject.Definition = value;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override MultiColumnHeaderState.Column[] CreateColumns(float width)
        {
            const float TransformWidth = 300;
            const float GroupWidth = 100;

            var groupCount = Data.GroupCount;
            var oldColumns = HeaderState?.columns;

            var newColumns = new MultiColumnHeaderState.Column[groupCount + 2];

            int index;
            if (oldColumns == null)
            {
                var tooltip = "Select which objects to control the weight of";

                newColumns[0] = CreateColumn(
                    "Transform",
                    tooltip,
                    TransformWidth);

                var includedColumn = newColumns[1] = CreateColumn(
                    "?",
                    tooltip,
                    LineHeight + StandardSpacing);

                includedColumn.minWidth = includedColumn.maxWidth = includedColumn.width;

                index = 2;
            }
            else
            {
                var copyCount = Math.Min(oldColumns.Length, groupCount + 2);
                for (int i = 0; i < copyCount; i++)
                    newColumns[i] = oldColumns[i];

                index = copyCount;
            }

            for (int i = index; i < groupCount + 2; i++)
            {
                var groupIndex = i - 2;
                var name = "Group " + groupIndex.ToStringCached();
                var tooltip = "The weights for " + name;

                if (groupIndex == 0)
                {
                    name = "Default " + name;
                    tooltip += " (this group will be applied on startup by default)";
                }

                newColumns[i] = CreateColumn(
                    name,
                    tooltip,
                    GroupWidth);
            }

            return newColumns;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override Color GetRowColor(TreeViewItem item)
        {
            if (!TreeView.Transforms.TryGetObject(item.id, out var transform))
                return default;

            if (Transforms.IndexOf(transform) < 0)
                return default;

            return GetChainColor(transform, Transforms, 0.15f);
        }

        /// <summary>Returns a color based on the name of the `transform`'s highest included parent.</summary>
        public static Color GetChainColor(Transform transform, IList<Transform> transforms, float alpha)
        {
            transform = GetChainRoot(transform, transforms);
            return GetHashColor(transform.name.GetHashCode(), 1, 1, alpha);
        }

        /// <summary>Gets the highest parent of `transform` which is included in the `transforms`.</summary>
        public static Transform GetChainRoot(Transform transform, IList<Transform> transforms)
        {
            var parent = transform.parent;
            while (parent != null)
            {
                if (transforms.IndexOf(parent) >= 0)
                    transform = parent;

                parent = parent.parent;
            }

            return transform;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DrawCellGUI(Rect area, int column, int row, TreeViewItem item, ref bool isSelectionClick)
        {
            if (!TreeView.Transforms.TryGetObject(item.id, out var transform))
                return;

            var definitionIndex = GetDefinitionIndex(item.id);

            switch (column)
            {
                case 0:// Transform.
                    DrawTransformCellGUI(area, transform);
                    break;

                case 1:// Included.
                    DrawIsIncludedCellGUI(area, item.id, definitionIndex, ref isSelectionClick);
                    break;

                default:// Groups.
                    DrawWeightGUI(area, item.id, column - 2, definitionIndex);
                    break;

            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void SetIncluded(
            int treeItemID,
            int definitionIndex,
            bool isIncluded)
        {
            var data = RecordUndo();

            if (isIncluded)
            {
                if (definitionIndex < 0 &&
                    TreeView.Transforms.TryGetObject(treeItemID, out var transform))
                {
                    var groupCount = data.GroupCount;

                    data.AddTransform(transform);

                    if (groupCount != data.GroupCount)
                        CreateHeader();
                }
            }
            else
            {
                if (definitionIndex >= 0)
                {
                    data.RemoveTransform(definitionIndex);

                    if (data.GroupCount <= 0)
                        CreateHeader();
                }
            }
        }

        /************************************************************************************************************************/

        private void DrawWeightGUI(
            Rect area,
            int treeItemID,
            int groupIndex,
            int transformIndex)
        {
            if (transformIndex < 0)
                return;

            var weight = Data.GetWeight(groupIndex, transformIndex);

            if (float.IsNaN(weight))
                return;

            EditorGUI.BeginChangeCheck();

            weight = EditorGUI.FloatField(area, weight);

            if (EditorGUI.EndChangeCheck())
            {
                weight = Mathf.Clamp01(weight);

                SetValue(treeItemID, i => RecordUndo().SetWeight(groupIndex, i, weight));
            }
        }

        /************************************************************************************************************************/

        private static readonly GUIContent
            AddGroupLabel = new(
                "Add Group",
                "Add another weight group"),
            RemoveGroupLabel = new(
                "Remove Group",
                "Remove the last weight group");

        /// <inheritdoc/>
        protected override void DoFooterCenterGUI()
        {
            var hasTransforms = !Data.Transforms.IsNullOrEmpty();
            GUI.enabled = hasTransforms;

            if (GUILayout.Button(AddGroupLabel))
            {
                RecordUndo().GroupCount++;
                CreateHeader();
            }

            if (hasTransforms)
                GUI.enabled = Data.GroupCount > 1;

            if (GUILayout.Button(RemoveGroupLabel))
            {
                RecordUndo().GroupCount--;
                CreateHeader();
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Apply()
        {
            base.Apply();
            CreateHeader();
            SceneView.RepaintAll();
        }

        /// <inheritdoc/>
        public override void Revert()
        {
            base.Revert();
            CreateHeader();
            SceneView.RepaintAll();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override WeightedMaskLayersDefinition RecordUndo(string name = "Animancer")
        {
            SceneView.RepaintAll();
            return base.RecordUndo(name);
        }

        /************************************************************************************************************************/
    }
}

#endif

