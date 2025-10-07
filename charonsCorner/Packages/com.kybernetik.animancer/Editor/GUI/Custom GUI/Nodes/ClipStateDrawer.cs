// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

namespace Animancer.Editor
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ClipStateDrawer
    [CustomGUI(typeof(ClipState))]
    public class ClipStateDrawer : AnimancerStateDrawer<ClipState>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string MainObjectName
            => "Clip";

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AddContextMenuFunctions(UnityEditor.GenericMenu menu)
        {
            menu.AddDisabledItem(new(
                $"{DetailsPrefix}Animation Type: {AnimationBindings.GetAnimationType(Value.Clip)}"));

            base.AddContextMenuFunctions(menu);

            AnimancerNodeBase.AddContextMenuIK(menu, Value);
        }

        /************************************************************************************************************************/
    }
}

#endif

