// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws a custom GUI for an object.</summary>
    /// <remarks>
    /// Every non-abstract type implementing this interface must have at least one <see cref="CustomGUIAttribute"/>.
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ICustomGUI
    /// 
    public interface ICustomGUI
    {
        /************************************************************************************************************************/

        /// <summary>The optional label to draw in front of the field.</summary>
        GUIContent Label { get; set; }

        /// <summary>The target object for which this GUI will be drawn.</summary>
        object Value { get; set; }

        /// <summary>Draws the GUI for the <see cref="Value"/>.</summary>
        void DoGUI();

        /************************************************************************************************************************/
    }

    /// <summary>[Editor-Only] Draws a custom GUI for an object.</summary>
    /// <remarks>
    /// Every non-abstract type inheriting from this class must have at least one <see cref="CustomGUIAttribute"/>.
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/CustomGUI_1
    /// 
    public abstract class CustomGUI<T> : ICustomGUI
    {
        /************************************************************************************************************************/

        /// <summary>The object for which this GUI will be drawn.</summary>
        public T Value { get; protected set; }

        /// <inheritdoc/>
        object ICustomGUI.Value
        {
            get => Value;
            set => Value = (T)value;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public GUIContent Label { get; set; } = GUIContent.none;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public abstract void DoGUI();

        /************************************************************************************************************************/
    }

    /// <summary>[Editor-Only] Extension methods for <see cref="ICustomGUI"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/CustomGUIExtensions
    /// 
    public static class CustomGUIExtensions
    {
        /************************************************************************************************************************/

        /// <summary>Sets the <see cref="ICustomGUI.Label"/>.</summary>
        public static void SetLabel(
            this ICustomGUI customGUI,
            string text,
            string tooltip = null,
            Texture image = null)
        {
            var label = customGUI.Label;
            if (label == null || label == GUIContent.none)
                customGUI.Label = label = new(text);

            label.text = text;
            label.tooltip = tooltip;
            label.image = image;
        }

        /************************************************************************************************************************/
    }
}

#endif

