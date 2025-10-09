// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

namespace Animancer.Editor
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/PlayableAssetStateDrawer
    [CustomGUI(typeof(PlayableAssetState))]
    public class PlayableAssetStateDrawer : AnimancerStateDrawer<PlayableAssetState>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string MainObjectName
            => "Playable Asset";

        /************************************************************************************************************************/
    }

}

#endif

