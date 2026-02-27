// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System.Collections;
using Febucci.TextAnimatorCore.Typing;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity.Actions
{
    /// <summary>
    /// Use this class to create custom typewriter actions that are instantiated from the inspector.
    /// Supports both stateless and coroutine-based patterns.
    /// </summary>
    /// <example>
    /// In all occasions, import the right namespaces
    /// <code>
    /// using Febucci.TextAnimatorForUnity.Actions;
    /// using Febucci.TextAnimatorCore.Typing;
    /// using UnityEngine;
    /// </code>
    /// -----STATELESS PATTERN -----
    /// <br/>
    /// 1. Create the State that inherits from <see cref="IActionState"/> and that will perform the action.
    /// <code>
    /// struct ExampleState : IActionState
    /// {
    ///     float timePassed;
    ///     readonly float timeToWait;
    ///
    ///     public ExampleState(float timeToWait)
    ///     {
    ///         timePassed = 0;
    ///         this.timeToWait = timeToWait;
    ///     }
    ///
    ///     public ActionStatus Progress(float deltaTime, ref TypingInfo typingInfo)
    ///     {
    ///         timePassed += deltaTime;
    ///         return timePassed >= timeToWait
    ///             ? ActionStatus.Finished
    ///             : ActionStatus.Running;
    ///     }
    ///
    ///     public void Cancel()
    ///     {
    ///         // does nothing
    ///     }
    /// }
    /// </code>
    ///
    /// 2. Create the Component that inherits from <see cref="TypewriterActionComponent"/>
    /// and links/creates our ExampleState
    /// <code>
    /// [AddComponentMenu("Example Action")]
    /// class ExampleActionComponent : TypewriterActionComponent
    /// {
    ///     [SerializeField] float timeToWait;
    ///
    ///     protected override IActionState CreateCustomState(ActionMarker marker, object typewriter)
    ///         => new ExampleState(timeToWait);
    /// }
    /// </code>
    ///
    /// --------- COROUTINE PATTERN --------
    /// <br/>
    /// Override PerformAction for coroutine-based actions:
    /// <code>
    /// [AddComponentMenu("Play Sound Action")]
    /// class PlaySoundAction : TypewriterActionComponent
    /// {
    ///     [SerializeField] AudioSource source;
    ///
    ///     protected override IEnumerator PerformAction()
    ///     {
    ///         if (source != null &amp;&amp; source.clip != null)
    ///         {
    ///             source.Play();
    ///             yield return new WaitForSeconds(source.clip.length);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class TypewriterActionComponent : MonoBehaviour, ITypewriterAction
    {
        [SerializeField] string tagID;
        public string TagID
        {
            get => tagID;
            set => tagID = value;
        }


        [SerializeField] bool makeAvailableGlobally;
        void Awake()
        {
            OnAwake();
        }

        protected virtual void OnAwake(){}

        void OnEnable()
        {
            if(makeAvailableGlobally) GlobalActionComponentsDatabase.Instance?.Register(this);
            OnEnableCalled();
        }

        protected virtual void OnEnableCalled() { }

        void OnDisable()
        {
            if(makeAvailableGlobally) GlobalActionComponentsDatabase.Instance?.Unregister(this);
            OnDisableCalled();
        }

        protected virtual void OnDisableCalled(){ }

        class CoroutineState : IActionState
        {
            TypingInfo typingInfo;
            readonly TypewriterActionComponent component;
            Coroutine activeCoroutine;
            bool hasFinished;

            public CoroutineState(TypewriterActionComponent component)
            {
                this.component = component;
            }

            public ActionStatus Progress(float deltaTime, ref TypingInfo typingInfo)
            {
                if (component == null || !component.isActiveAndEnabled)
                    return ActionStatus.Finished;

                if (activeCoroutine == null)
                {
                    this.typingInfo = typingInfo;
                    activeCoroutine = component.StartCoroutine(PerformCoroutineWrapper());
                }

                return hasFinished ? ActionStatus.Finished : ActionStatus.Running;
            }

            IEnumerator PerformCoroutineWrapper()
            {
                yield return component.PerformAction(typingInfo);
                hasFinished = true;
            }

            public void Cancel()
            {
                if (activeCoroutine != null && component != null)
                {
                    component.StopCoroutine(activeCoroutine);
                    activeCoroutine = null;
                }
                hasFinished = true;
            }
        }

        public IActionState CreateActionFrom(ActionMarker marker, object typewriter)
        {
            OnActionCreated(marker, typewriter);

            var customState = CreateCustomState(marker, typewriter);
            if (customState != null)
                return customState;

            return new CoroutineState(this);
        }

        /// <summary>
        /// Called when this action is created from a tag in text.
        /// Override to initialize the component from marker parameters.
        /// </summary>
        /// <param name="marker">Parsed action marker with parameters</param>
        /// <param name="typewriter">Reference to typewriter component</param>
        protected virtual void OnActionCreated(ActionMarker marker, object typewriter) { }

        /// <summary>
        /// Override to create a custom stateless IActionState.
        /// </summary>
        /// <param name="marker">Parsed action marker with parameters</param>
        /// <param name="typewriter">Reference to typewriter component</param>
        /// <returns>Custom state instance, or null to use coroutine pattern</returns>
        protected virtual IActionState CreateCustomState(ActionMarker marker, object typewriter)
        {
            return null;
        }

        /// <summary>
        /// Override to create a coroutine-based action.
        /// Only called if CreateCustomState returns null.
        /// </summary>
        protected virtual IEnumerator PerformAction(TypingInfo typingInfo)
        {
            yield break;
        }
    }
}