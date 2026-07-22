using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace uPalette.Editor.Core.PaletteEditor
{
    [Serializable]
    internal sealed class GradientPaletteEditorWindowContentsView : PaletteEditorWindowContentsView<Gradient>
    {
        protected override PaletteEditorTreeView<Gradient> CreateTreeView(TreeViewState<int> state)
        {
            return new GradientPaletteEditorTreeView(state);
        }
    }
}
