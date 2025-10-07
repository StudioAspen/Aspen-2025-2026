// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Collections.Generic;
using UnityEngine;

namespace Animancer.TransitionLibraries
{
    /// <summary>[Pro-Only]
    /// A <see cref="ScriptableObject"/> which serializes a <see cref="TransitionLibraryDefinition"/>
    /// and creates a <see cref="TransitionLibrary"/> from it at runtime.
    /// </summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/libraries">
    /// Transition Libraries</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.TransitionLibraries/TransitionLibraryAsset
    [CreateAssetMenu(
        menuName = Strings.MenuPrefix + "Transition Library",
        order = Strings.AssetMenuOrder + 0)]
    [AnimancerHelpUrl(typeof(TransitionLibraryAsset))]
    public class TransitionLibraryAsset : ScriptableObject,
        IAnimationClipCollection
    {
        /************************************************************************************************************************/

        [SerializeField]
        private TransitionLibraryDefinition _Definition;

        /// <summary>[<see cref="SerializeField"/>]
        /// The serialized data which will be used to initialize the <see cref="Library"/> at runtime.
        /// </summary>
        /// <remarks>
        /// If you modify the contents of this reference, either re-assign this property
        /// or call <see cref="OnDefinitionModified"/> to apply any changes to the <see cref="Library"/>.
        /// </remarks>
        public TransitionLibraryDefinition Definition
        {
            get => _Definition;
            set
            {
                _Definition = value ?? new();
                OnDefinitionModified();
            }
        }

#if UNITY_EDITOR
        /// <summary>[Editor-Only] [Internal]
        /// The name of the field which stores the <see cref="Definition"/>.
        /// </summary>
        internal const string DefinitionField = nameof(_Definition);
#endif

        /************************************************************************************************************************/

        /// <summary>The runtime <see cref="TransitionLibrary"/> created from the <see cref="Definition"/>.</summary>
        public TransitionLibrary Library { get; private set; }

        /************************************************************************************************************************/

        /// <summary>Initializes the <see cref="Library"/>.</summary>
        protected virtual void OnEnable()
        {
            _Definition ??= new();

            Library = new();
            Library.Initialize(_Definition);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Adds the contents of the <see cref="Definition"/>
        /// to the <see cref="Library"/> if it was already initialized.
        /// </summary>
        /// <remarks>
        /// Call this after modifying the contents of the <see cref="Definition"/>
        /// to ensure that the <see cref="Library"/> reflects any changes.
        /// <para></para>
        /// Note that this doesn't remove anything from the <see cref="Library"/>,
        /// it only adds or replaces values.
        /// </remarks>
        public void OnDefinitionModified()
        {
            Library.Initialize(_Definition);
        }

        /************************************************************************************************************************/

        /// <summary>Gathers all the animations in the <see cref="Definition"/> and <see cref="Library"/>.</summary>
        public virtual void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            clips.GatherFromSource(_Definition);
            clips.GatherFromSource(Library);
        }

        /************************************************************************************************************************/
    }
}
