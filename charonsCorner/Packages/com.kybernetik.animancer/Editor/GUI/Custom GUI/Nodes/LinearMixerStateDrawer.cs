// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using UnityEditor;

namespace Animancer.Editor
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/LinearMixerStateDrawer
    [CustomGUI(typeof(LinearMixerState))]
    public class LinearMixerStateDrawer : ParametizedAnimancerStateDrawer<LinearMixerState>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AddContextMenuFunctions(GenericMenu menu)
        {
            base.AddContextMenuFunctions(menu);

            menu.AddItem(new("Extrapolate Speed"), Value.ExtrapolateSpeed, () =>
            {
                Value.ExtrapolateSpeed = !Value.ExtrapolateSpeed;
            });
        }

        /************************************************************************************************************************/
    }
}

#endif

