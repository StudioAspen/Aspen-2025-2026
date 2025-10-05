// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Icon textures used throughout Animancer.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerIcons
    public static class AnimancerIcons
    {
        /************************************************************************************************************************/

        /// <summary>A standard icon for information.</summary>
        public static readonly Texture Info = Load("console.infoicon");

        /// <summary>A standard icon for warnings.</summary>
        public static readonly Texture Warning = Load("console.warnicon");

        /// <summary>A standard icon for errors.</summary>
        public static readonly Texture Error = Load("console.erroricon");

        /************************************************************************************************************************/

        private static Texture _ScriptableObject;

        /// <summary>The icon for <see cref="UnityEngine.ScriptableObject"/>.</summary>
        public static Texture ScriptableObject
        {
            get
            {

                if (_ScriptableObject == null)
                {
                    _ScriptableObject = Load("d_ScriptableObject Icon");

                    if (_ScriptableObject == null)
                        _ScriptableObject = AssetPreview.GetMiniTypeThumbnail(typeof(StringAsset));
                }

                return _ScriptableObject;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Loads an icon texture.</summary>
        public static Texture Load(string name, FilterMode filterMode = FilterMode.Bilinear)
        {
            var icon = EditorGUIUtility.Load(name) as Texture;
            if (icon != null)
                icon.filterMode = filterMode;
            return icon;
        }

        /// <summary>Loads an icon `texture` if it was <c>null</c>.</summary>
        public static Texture Load(ref Texture texture, string name, FilterMode filterMode = FilterMode.Bilinear)
            => texture != null
            ? texture
            : texture = Load(name, filterMode);

        /************************************************************************************************************************/

        private static GUIContent
            _PlayIcon,
            _PauseIcon,
            _StepBackwardIcon,
            _StepForwardIcon,
            _AddIcon,
            _ClearIcon,
            _CopyIcon;

        /// <summary><see cref="IconContent(ref GUIContent, string, string)"/> for a play button.</summary>
        public static GUIContent PlayIcon
            => IconContent(ref _PlayIcon, "PlayButton");

        /// <summary><see cref="IconContent(ref GUIContent, string, string)"/> for a pause button.</summary>
        public static GUIContent PauseIcon
            => IconContent(ref _PauseIcon, "PauseButton");

        /// <summary><see cref="IconContent(ref GUIContent, string, string)"/> for a step backward button.</summary>
        public static GUIContent StepBackwardIcon
            => IconContent(ref _StepBackwardIcon, "Animation.PrevKey");

        /// <summary><see cref="IconContent(ref GUIContent, string, string)"/> for a step forward button.</summary>
        public static GUIContent StepForwardIcon
            => IconContent(ref _StepForwardIcon, "Animation.NextKey");

        /// <summary><see cref="IconContent(ref GUIContent, string, string)"/> for an add button.</summary>
        public static GUIContent AddIcon(string tooltip = "Add")
            => IconContent(ref _AddIcon, "Toolbar Plus", tooltip);

        /// <summary><see cref="IconContent(ref GUIContent, string, string)"/> for a clear button.</summary>
        public static GUIContent ClearIcon(string tooltip = "Clear")
            => IconContent(ref _ClearIcon, "Grid.EraserTool", tooltip);

        /// <summary><see cref="IconContent(ref GUIContent, string, string)"/> for a copy button.</summary>
        public static GUIContent CopyIcon(string tooltip = "Copy to clipboard")
            => IconContent(ref _CopyIcon, "UnityEditor.ConsoleWindow", tooltip);

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="EditorGUIUtility.IconContent(string)"/> if the `content` was null.</summary>
        public static GUIContent IconContent(ref GUIContent content, string name, string tooltip = "")
        {
            content ??= EditorGUIUtility.IconContent(name);
            content.tooltip = tooltip;
            return content;
        }

        /************************************************************************************************************************/
    }
}

#endif

