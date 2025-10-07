// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>
    /// A <see cref="PlayableBehaviour"/> which executes <see cref="IUpdatable.Update"/>
    /// on each item in an <see cref="IUpdatable.List"/> every frame.
    /// </summary>
    public class UpdatableListPlayable : PlayableBehaviour
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Since <see cref="ScriptPlayable{T}.Create(PlayableGraph, int)"/> needs to clone an existing instance,
        /// we keep a static template to avoid allocating an extra garbage one every time.
        /// This also means the fields can't be readonly because field initializers don't run on the clone.
        /// </summary>
        private static readonly UpdatableListPlayable Template = new();

        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerGraph"/> this behaviour is connected to.</summary>
        private AnimancerGraph _Graph;

        /// <summary>Objects to be updated before time advances.</summary>
        private IUpdatable.List _Updatables;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="UpdatableListPlayable"/>.</summary>
        public static ScriptPlayable<UpdatableListPlayable> Create(
            AnimancerGraph graph,
            int inputCount,
            IUpdatable.List updatables)
        {
            var playable = ScriptPlayable<UpdatableListPlayable>.Create(graph._PlayableGraph, Template, inputCount);
            var instance = playable.GetBehaviour();
            instance._Graph = graph;
            instance._Updatables = updatables;
            return playable;
        }

        /************************************************************************************************************************/

        /// <summary>[Internal] Calls <see cref="IUpdatable.Update"/> on everything added to this list.</summary>
        /// <remarks>
        /// Called by the <see cref="PlayableGraph"/> after the rest of the <see cref="Playable"/>s are evaluated.
        /// </remarks>
        public override void PrepareFrame(Playable playable, FrameData info)
            => _Graph.UpdateAll(
                _Updatables,
                info.deltaTime * info.effectiveParentSpeed,
                info.frameId);

        /************************************************************************************************************************/
    }
}

