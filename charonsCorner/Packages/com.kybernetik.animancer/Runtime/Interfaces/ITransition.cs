// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

namespace Animancer
{
    /// <summary>An object which can create an <see cref="AnimancerState"/> and set its details.</summary>
    /// <remarks>
    /// Transitions are generally used as arguments for <see cref="AnimancerLayer.Play(ITransition)"/>.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">
    /// Transitions</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ITransition
    public interface ITransition : IHasKey, IPolymorphic
    {
        /************************************************************************************************************************/

        /// <summary>The amount of time this transition should take (in seconds).</summary>
        float FadeDuration { get; }

        /// <summary>
        /// The <see cref="Animancer.FadeMode"/> which should be used when this transition is passed into
        /// <see cref="AnimancerLayer.Play(ITransition)"/>.
        /// </summary>
        FadeMode FadeMode { get; }

        /// <summary>Creates and returns a new <see cref="AnimancerState"/> defuned by this transition.</summary>
        /// <remarks>
        /// The first time a transition is used on an object, this method creates a state
        /// which is registered in the internal dictionary using the <see cref="IHasKey.Key"/>
        /// so that it can be reused later on.
        /// <para></para>
        /// Methods like <see cref="AnimancerLayer.Play(ITransition)"/> will also call <see cref="Apply"/>,
        /// so if you call this method manually you may want to call that method as well.
        /// Or you can just use <see cref="AnimancerUtilities.CreateStateAndApply"/>.
        /// </remarks>
        AnimancerState CreateState();

        /// <summary>Applies the details of this transition to the `state`.</summary>
        /// <remarks>This method is called by every <see cref="AnimancerLayer.Play(ITransition)"/>.</remarks>
        void Apply(AnimancerState state);

        /************************************************************************************************************************/
    }

    /// <summary>An <see cref="ITransition"/> which creates a specific type of <see cref="AnimancerState"/>.</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">
    /// Transitions</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ITransition_1
    public interface ITransition<out TState> : ITransition
        where TState : AnimancerState
    {
        /************************************************************************************************************************/

        /// <summary>
        /// The state that was created by this object. Specifically, this is the state that was most recently
        /// passed into <see cref="ITransition.Apply"/> (usually by <see cref="AnimancerLayer.Play(ITransition)"/>).
        /// </summary>
        TState State { get; }

        /************************************************************************************************************************/

        /// <summary>Creates and returns a new <typeparamref name="TState"/>.</summary>
        new TState CreateState();

        /************************************************************************************************************************/
    }
}

