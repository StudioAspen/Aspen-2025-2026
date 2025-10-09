// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] <see cref="GUIStyle"/>s for a group of connected buttons.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ButtonGroupStyles
    public struct ButtonGroupStyles
    {
        /************************************************************************************************************************/

        /// <summary>The style for the button on the far left.</summary>
        public GUIStyle left;

        /// <summary>The style for any buttons in the middle.</summary>
        public GUIStyle middle;

        /// <summary>The style for the button on the far right.</summary>
        public GUIStyle right;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="ButtonGroupStyles"/>.</summary>
        public ButtonGroupStyles(
            GUIStyle left,
            GUIStyle middle,
            GUIStyle right)
        {
            this.left = left;
            this.middle = middle;
            this.right = right;
        }

        /************************************************************************************************************************/

        /// <summary>Copies any <c>null</c> values from another group.</summary>
        public void CopyMissingStyles(ButtonGroupStyles copyFrom)
        {
            left ??= copyFrom.left;
            middle ??= copyFrom.middle;
            right ??= copyFrom.right;
        }

        /************************************************************************************************************************/

        /// <summary>The default styles for a mini button.</summary>
        public static ButtonGroupStyles MiniButton => new(
            EditorStyles.miniButtonLeft,
            EditorStyles.miniButtonMid,
            EditorStyles.miniButtonRight);

        /************************************************************************************************************************/

        private static ButtonGroupStyles _Button;

        /// <summary>The default styles for a button.</summary>
        public static ButtonGroupStyles Button
        {
            get
            {
                _Button.left ??= MiniToRegularButtonStyle(EditorStyles.miniButtonLeft);
                _Button.middle ??= MiniToRegularButtonStyle(EditorStyles.miniButtonMid);
                _Button.right ??= MiniToRegularButtonStyle(EditorStyles.miniButtonRight);
                return _Button;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Creates a copy of the `style` with the size of a regular button.</summary>
        public static GUIStyle MiniToRegularButtonStyle(GUIStyle style)
            => new(style)
            {
                fixedHeight = 0,
                padding = GUI.skin.button.padding,
                stretchWidth = false,
            };

        /************************************************************************************************************************/
    }
}

#endif

