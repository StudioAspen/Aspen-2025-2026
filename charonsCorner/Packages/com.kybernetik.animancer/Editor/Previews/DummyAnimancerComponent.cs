// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEngine;

namespace Animancer.Editor.Previews
{
    /// <summary>[Editor-Only]
    /// An <see cref="IAnimancerComponent"/> which isn't actually a <see cref="Component"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Previews/DummyAnimancerComponent
    public class DummyAnimancerComponent : IAnimancerComponent
    {
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="DummyAnimancerComponent"/>.</summary>
        public DummyAnimancerComponent(Animator animator, AnimancerGraph playable)
        {
            Animator = animator;
            Graph = playable;
            InitialUpdateMode = animator.updateMode;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public bool enabled => true;

        /// <inheritdoc/>
        public GameObject gameObject => Animator.gameObject;

        /// <inheritdoc/>
        public Animator Animator { get; set; }

        /// <inheritdoc/>
        public AnimancerGraph Graph { get; private set; }

        /// <inheritdoc/>
        public bool IsGraphInitialized => true;

        /// <inheritdoc/>
        public bool ResetOnDisable => false;

        /// <inheritdoc/>
        public AnimatorUpdateMode UpdateMode
        {
            get => Animator.updateMode;
            set => Animator.updateMode = value;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public object GetKey(AnimationClip clip) => clip;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public string AnimatorFieldName => null;

        /// <inheritdoc/>
        public string ActionOnDisableFieldName => null;

        /// <inheritdoc/>
        public AnimatorUpdateMode? InitialUpdateMode { get; private set; }

        /************************************************************************************************************************/

        /// <summary>Describes this and the <see cref="Animator"/>.</summary>
        public override string ToString()
            => $"{nameof(DummyAnimancerComponent)}({(Animator != null ? Animator.name : "Destroyed")})";

        /************************************************************************************************************************/
    }
}

#endif

