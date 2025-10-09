// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

namespace Animancer.Editor
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ControllerStateDrawer
    [CustomGUI(typeof(ControllerState))]
    public class ControllerStateDrawer : ParametizedAnimancerStateDrawer<ControllerState>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string MainObjectName
            => "Animator Controller";

        /************************************************************************************************************************/
    }
}

#endif

