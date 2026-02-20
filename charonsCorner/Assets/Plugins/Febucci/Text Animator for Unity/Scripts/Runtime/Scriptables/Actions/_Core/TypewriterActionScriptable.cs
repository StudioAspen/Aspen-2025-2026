// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System.Collections;
using Febucci.TextAnimatorCore.Typing;
using Febucci.TextAnimatorForUnity.Actions.Core;
using Febucci.TextAnimatorForUnity.Core;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity.Actions
{
    /// <summary>
    /// Use this class to create custom typewriter actions using Scriptable Objects.
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
    /// 2. Create the Scriptable Object that inherits from <see cref="TypewriterActionScriptable"/>
    /// and links/creates our ExampleState
    /// <code>
    /// [System.Serializable]
    /// [CreateAssetMenu(menuName = "Create Example Action")]
    /// class ExampleActionScriptable : TypewriterActionScriptable
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
    /// Override PerformAction for coroutine-based actions (uses typewriter MonoBehaviour as runner):
    /// <code>
    /// [System.Serializable]
    /// [CreateAssetMenu(menuName = "Create Delay Action")]
    /// class DelayActionScriptable : TypewriterActionScriptable
    /// {
    ///     [SerializeField] float delay = 1f;
    ///
    ///     protected override IEnumerator PerformAction()
    ///     {
    ///         yield return new WaitForSeconds(delay);
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class TypewriterActionScriptable : ActionScriptableBase
    {
        [SerializeField] string tagID;

        public override string TagID
        {
            get => tagID;
            set => tagID = value;
        }

        class CoroutineState : IActionState
        {
            readonly TypewriterActionScriptable scriptable;
            readonly MonoBehaviour runner;
            Coroutine activeCoroutine;
            bool hasFinished;

            public CoroutineState(TypewriterActionScriptable scriptable, MonoBehaviour runner)
            {
                this.scriptable = scriptable;
                this.runner = runner;
            }

            public ActionStatus Progress(float deltaTime, ref TypingInfo typingInfo)
            {
                if (runner == null || !runner.isActiveAndEnabled)
                    return ActionStatus.Finished;

                if (activeCoroutine == null)
                {
                    activeCoroutine = runner.StartCoroutine(PerformCoroutineWrapper());
                }

                return hasFinished ? ActionStatus.Finished : ActionStatus.Running;
            }

            IEnumerator PerformCoroutineWrapper()
            {
                yield return scriptable.PerformAction();
                hasFinished = true;
            }

            public void Cancel()
            {
                if (activeCoroutine != null && runner != null)
                {
                    runner.StopCoroutine(activeCoroutine);
                    activeCoroutine = null;
                }
                hasFinished = true;
            }
        }

        public override IActionState CreateActionFrom(ActionMarker marker, object typewriter)
        {
            OnActionCreated(marker, typewriter);

            var customState = CreateCustomState(marker, typewriter);
            if (customState != null)
                return customState;

            if (typewriter is MonoBehaviour mb)
            {
                return new CoroutineState(this, mb);
            }

            var dispatcher = MonoDispatcher.Instance;
            if (dispatcher != null)
            {
                return new CoroutineState(this, dispatcher);
            }

            Debug.LogWarning($"TypewriterActionScriptable '{name}': Neither CreateCustomState nor PerformAction was overridden, or typewriter is not a MonoBehaviour. Action will not execute.", this);
            return null;
        }

        /// <summary>
        /// Called when this action is created from a tag in text.
        /// Override to initialize from marker parameters.
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
        protected virtual IEnumerator PerformAction()
        {
            yield break;
        }
    }
}