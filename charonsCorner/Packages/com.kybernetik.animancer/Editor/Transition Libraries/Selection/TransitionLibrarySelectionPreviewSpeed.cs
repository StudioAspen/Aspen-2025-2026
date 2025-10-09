// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only]
    /// <see cref="ToggledSpeedSlider"/> for <see cref="TransitionLibrarySelectionPreview"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibrarySelectionPreviewSpeed
    public class TransitionLibrarySelectionPreviewSpeed : ToggledSpeedSlider
    {
        /************************************************************************************************************************/

        private const string
            SpeedPrefKey = nameof(TransitionLibrarySelectionPreviewSpeed) + "." + nameof(Speed);

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TransitionLibrarySelectionPreviewSpeed"/>.</summary>
        public TransitionLibrarySelectionPreviewSpeed()
            : base(nameof(TransitionLibrarySelectionPreviewSpeed) + ".Show")
        {
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnSetSpeed(float speed)
        {
            EditorPrefs.SetFloat(SpeedPrefKey, speed);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool DoToggleGUI(Rect area, GUIStyle style)
        {
            if (float.IsNaN(Speed))
                Speed = EditorPrefs.GetFloat(SpeedPrefKey, 1);

            return base.DoToggleGUI(area, style);
        }

        /************************************************************************************************************************/
    }
}

#endif

