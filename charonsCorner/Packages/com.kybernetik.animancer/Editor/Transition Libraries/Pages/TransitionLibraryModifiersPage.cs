// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only]
    /// A <see cref="TransitionLibraryWindowPage"/> for editing transition modifiers.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibraryModifiersPage
    [Serializable]
    public class TransitionLibraryModifiersPage : TransitionLibraryWindowPage
    {
        /************************************************************************************************************************/

        [SerializeField]
        private TransitionModifierTableGUI _TableGUI;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string DisplayName
            => "Transition Modifiers";

        /// <inheritdoc/>
        public override string HelpTooltip
            => "Modifiers allow you to replace the usual fade duration for specific combinations of transitions.";

        /// <inheritdoc/>
        public override int Index
            => 0;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnGUI(Rect area)
        {
            _TableGUI ??= new();

            if (Window.Data.Transitions.Length == 0)
            {
                area = new Rect(
                    area.x + AnimancerGUI.StandardSpacing,
                    area.y + AnimancerGUI.StandardSpacing,
                    area.width - AnimancerGUI.StandardSpacing * 2,
                    AnimancerGUI.LineHeight);

                GUI.Label(
                    area,
                    "Library contains no Transitions." +
                    " Drag and Drop Transition Assets into this window or use the Create Transition button.");

                AnimancerGUI.NextVerticalArea(ref area);

                if (GUI.Button(area, "Create Transition"))
                    TransitionLibraryOperations.CreateTransition(Window);
            }
            else
            {
                _TableGUI.DoGUI(area, Window);
            }

            TransitionLibraryOperations.HandleBackgroundInput(area, Window);
        }

        /************************************************************************************************************************/
    }
}

#endif

