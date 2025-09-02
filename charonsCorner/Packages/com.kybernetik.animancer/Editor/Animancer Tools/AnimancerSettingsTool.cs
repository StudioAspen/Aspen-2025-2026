// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;

namespace Animancer.Editor.Tools
{
    /// <summary>[Editor-Only] Displays the <see cref="AnimancerSettings"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Tools/AnimancerSettingsTool
    [Serializable]
    public class AnimancerSettingsTool : AnimancerToolsWindow.Tool
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override int DisplayOrder => int.MaxValue;

        /// <inheritdoc/>
        public override string Name => "Animancer Settings";

        /// <inheritdoc/>
        public override string Instructions => null;

        /// <inheritdoc/>
        public override string HelpURL
            => $"{Strings.DocsURLs.APIDocumentation}.{nameof(Editor)}/{nameof(AnimancerSettings)}";

        /************************************************************************************************************************/

        [NonSerialized] private readonly CachedEditor SettingsEditor = new();

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnDisable()
        {
            base.OnDisable();
            SettingsEditor.Dispose();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoBodyGUI()
        {
            var settings = AnimancerSettings.Instance;
            if (settings == null)
                return;

            AnimancerSettings.Editor.HideNextInfo = true;

            SettingsEditor.GetEditor(settings).OnInspectorGUI();
        }

        /************************************************************************************************************************/
    }
}

#endif

