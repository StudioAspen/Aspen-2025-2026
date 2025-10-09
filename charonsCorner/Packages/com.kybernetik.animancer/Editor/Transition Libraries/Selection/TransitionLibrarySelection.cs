// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using Animancer.TransitionLibraries;
using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only]
    /// A dummy object for tracking the selection within the <see cref="TransitionLibraryWindow"/>
    /// and showing its details in the Inspector.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibrarySelection
    [AnimancerHelpUrl(typeof(TransitionLibrarySelection))]
    public class TransitionLibrarySelection : ScriptableObject
    {
        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Types of objects can be selected.</summary>
        public enum SelectionType
        {
            /// <summary>Nothing selected.</summary>
            None,

            /// <summary>The main library.</summary>
            Library,

            /// <summary>A from-transition.</summary>
            FromTransition,

            /// <summary>A to-transition.</summary>
            ToTransition,

            /// <summary>A fade duration modifier.</summary>
            Modifier,
        }

        /************************************************************************************************************************/

        [SerializeField] private TransitionLibraryWindow _Window;
        [SerializeField] private SelectionType _Type;
        [SerializeField] private int _FromIndex = -1;
        [SerializeField] private int _ToIndex = -1;
        [SerializeField] private int _Version;

        /// <summary>The window this selection is associated with.</summary>
        public TransitionLibraryWindow Window
            => _Window;

        /// <summary>The type of selected object.</summary>
        public SelectionType Type
            => _Type;

        /// <summary>The index of the <see cref="FromTransition"/>.</summary>
        public int FromIndex
            => _FromIndex;

        /// <summary>The index of the <see cref="ToTransition"/>.</summary>
        public int ToIndex
            => _ToIndex;

        /// <summary>The number of times this selection has been changed.</summary>
        public int Version
            => _Version;

        /************************************************************************************************************************/

        /// <summary>The transition the current selection is coming from.</summary>
        public TransitionAssetBase FromTransition { get; private set; }

        /// <summary>The transition the current selection is going to.</summary>
        public TransitionAssetBase ToTransition { get; private set; }

        /// <summary>The <see cref="ITransition.FadeDuration"/> of the current selection.</summary>
        public float FadeDuration { get; private set; }

        /// <summary>Does the current selection have a modified <see cref="FadeDuration"/>?</summary>
        public bool HasModifier { get; private set; }

        /************************************************************************************************************************/

        [NonSerialized] private object _Selected;

        /// <summary>The currently selected object.</summary>
        public object Selected
        {
            get
            {
                Validate();
                return _Selected;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Deselects the current object if it isn't valid.</summary>
        public bool Validate()
        {
            if (IsValid())
                return true;

            Deselect();
            return false;
        }

        /// <summary>Is the current selection valid?</summary>
        public bool IsValid()
        {
            if (this == null ||
                _Window == null ||
                Selection.activeObject != this)
                return false;

            var library = _Window.SourceObject;
            if (library == null)
                return false;

            FromTransition = null;
            ToTransition = null;
            FadeDuration = float.NaN;
            HasModifier = false;

            switch (_Type)
            {
                case SelectionType.Library:
                    name = "Transition Library";
                    _Selected = library;
                    return library != null;

                case SelectionType.FromTransition:
                    name = "From Transition";
                    if (!_Window.Data.Transitions.TryGet(_FromIndex, out var transition))
                        return false;

                    FromTransition = transition;
                    FadeDuration = transition.TryGetFadeDuration();
                    _Selected = transition;
                    return true;

                case SelectionType.ToTransition:
                    name = "To Transition";
                    if (!_Window.Data.Transitions.TryGet(_ToIndex, out transition))
                        return false;

                    ToTransition = transition;
                    FadeDuration = transition.TryGetFadeDuration();
                    _Selected = transition;
                    return true;

                case SelectionType.Modifier:
                    name = "Transition Modifier";

                    var hasTransitions = _Window.Data.TryGetTransition(_FromIndex, out transition);
                    FromTransition = transition;

                    hasTransitions |= _Window.Data.TryGetTransition(_ToIndex, out transition);
                    ToTransition = transition;

                    if (_Window.Data.TryGetModifier(_FromIndex, _ToIndex, out var modifier))
                    {
                        HasModifier = true;
                    }
                    else if (hasTransitions)
                    {
                        modifier = modifier.WithFadeDuration(transition.TryGetFadeDuration());
                    }
                    else
                    {
                        return false;
                    }

                    FadeDuration = modifier.FadeDuration;
                    _Selected = modifier;
                    return true;

                default:
                    return false;
            };
        }

        /************************************************************************************************************************/

        /// <summary>Sets the <see cref="Selected"/> object.</summary>
        /// <remarks>
        /// We can't simply set the <see cref="Selection.activeObject"/>
        /// because it might not be a <see cref="UnityEngine.Object"/>
        /// and if it is then we don't want the Project window to move to it.
        /// <para></para>
        /// So instead, we select this dummy object and <see cref="TransitionLibrarySelectionEditor"/>
        /// draws a custom Inspector for the target object.
        /// </remarks>
        public void Select(
            TransitionLibraryWindow window,
            object select,
            int index,
            SelectionType type)
        {
            switch (type)
            {
                case SelectionType.Library:
                    _FromIndex = -1;
                    _ToIndex = -1;
                    break;

                case SelectionType.FromTransition:
                    _FromIndex = index;
                    _ToIndex = -1;
                    break;

                case SelectionType.ToTransition:
                    _FromIndex = -1;
                    _ToIndex = index;
                    break;

                case SelectionType.Modifier:
                    if (select is TransitionModifierDefinition modifier)
                    {
                        _FromIndex = modifier.FromIndex;
                        _ToIndex = modifier.ToIndex;
                        break;
                    }
                    else
                    {
                        Deselect();
                        return;
                    }

                default:
                    Deselect();
                    throw new ArgumentException($"Unhandled {nameof(SelectionType)}", nameof(type));
            }

            _Window = window;
            _Type = type;
            _Selected = select;
            _Version++;

            Selection.activeObject = this;

            Validate();
        }

        /************************************************************************************************************************/

        /// <summary>Clears the <see cref="Selected"/> object.</summary>
        public void Deselect()
        {
            _Window = null;
            _Type = default;
            _FromIndex = -1;
            _ToIndex = -1;
            _Selected = null;
            _Version++;

            if (Selection.activeObject == this)
                Selection.activeObject = null;
        }

        /************************************************************************************************************************/

        /// <summary>Handles selection changes.</summary>
        public void OnSelectionChange()
        {
            if (Selection.activeObject == this)
                return;

            Deselect();

            if (_Window != null)
                _Window.Repaint();
        }

        /************************************************************************************************************************/

        /// <summary>Selects this object if it contains a valid selection.</summary>
        protected virtual void OnEnable()
        {
            if (Selected != null)
                Selection.activeObject = this;
        }

        /************************************************************************************************************************/
    }
}

#endif

