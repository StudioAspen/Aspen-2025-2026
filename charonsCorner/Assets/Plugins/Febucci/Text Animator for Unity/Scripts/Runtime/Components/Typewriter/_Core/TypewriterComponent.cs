// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System;
using System.Collections.Generic;
using Febucci.TextAnimatorCore.Data;
using Febucci.TextAnimatorCore.Settings;
using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorCore.Typing;
using Febucci.TextAnimatorForUnity.Actions;
using UnityEngine;
using UnityEngine.Events;

namespace Febucci.TextAnimatorForUnity
{
    /// <summary>
    /// Base class for all Typewriters. <br/>
    /// - Manual: <see href="https://www.febucci.com/text-animator-unity/docs/typewriters/">Typewriters</see>.<br/>
    /// </summary>
    /// <remarks>
    /// If you want to use the built-in Typewriter, see: <see cref="TypingDelaysByCharacter"/> or <see cref="TypingDelaysByWord"/><br/>
    /// <br/>
    /// You can also create custom typewriters by inheriting from this class. <br/>
    /// Manual: <see href="https://www.febucci.com/text-animator-unity/docs/writing-custom-typewriters-c-sharp/">Writing Custom Typewriters (C#)</see>
    /// </remarks>
    [DisallowMultipleComponent]
    public class TypewriterComponent : MonoBehaviour, ITypewriterProvider, ISettingsProvider<TypewriterSettings>, IDatabaseProvider<ITypewriterAction>
    {
        internal ITextAnimatorProvider textAnimatorProvider;

        [SerializeField] TypingsTimingsScriptableBase timingsScriptableBase;
        TypewriterCore _wrapper;
        TypewriterCore Wrapper
        {
            get
            {
                InitializeOnce();
                return _wrapper;
            }
        }

        [SerializeField] ActionDatabase actionsDatabase;

        /// <summary>
        /// Local Actions Database, cached at runtime / awake
        /// </summary>
        public Dictionary<string, ITypewriterAction> Database { get; private set; }

        void Awake()
        {
            InitializeOnce();
        }

        [SerializeField] public UnityTypewriterSettings localSettings;
        [SerializeField] public TypewriterSettingsScriptable sharedSettings;

        TypewriterSettings ISettingsProvider<TypewriterSettings>.Settings
        {
            get => sharedSettings != null ? sharedSettings.Settings : localSettings;
        }

        bool initialized = false;
        void InitializeOnce()
        {
            if(initialized) return;

            if (textAnimatorProvider == null && !TryGetComponent(out textAnimatorProvider))
                return;

            Logger.Debug($"Initializing typewriter {name}", gameObject);
            Database = new Dictionary<string, ITypewriterAction>();

            var localActions = GetComponents<ITypewriterAction>();
            foreach (var localAction in localActions)
                Database.TryAdd(localAction.TagID, localAction);

            textAnimatorProvider.TryInitializingOnce();
            var animator = textAnimatorProvider.TextAnimator;

            _wrapper = new TypewriterCore(animator,this,
                timingsScriptableBase,
                TextAnimatorSettings.Instance,
                this,
                new IDatabaseProvider<ITypewriterAction>[]
                {
                    this, // local actions have a priority
                    GlobalActionComponentsDatabase.Instance, // monobehaviors later
                    actionsDatabase, //finally - stateless ones
                },
                OnBeforeShowingCharacter: OnBeforeShowingCharacter,
                OnAfterWaitingCharacter: OnAfterWaitingCharacter);

            _wrapper.OnTextShowed += () => onTextShowed?.Invoke();
            _wrapper.OnTextDisappeared += () => onTextDisappeared?.Invoke();
            _wrapper.OnTypewriterStart += () => onTypewriterStart?.Invoke();
            _wrapper.OnMessage += (x) => onMessage?.Invoke(x);
            _wrapper.OnCharacterWaitStarted += (character, mode) => onCharacterWaitStarted?.Invoke(character, mode);
            _wrapper.OnCharacterWaitFinished += (character, mode) => onCharacterWaitFinished?.Invoke(character, mode);
            _wrapper.OnCharacterVisible += (x) => onCharacterVisible?.Invoke(x);

            // TODO make sure if "start from show text" works
            _wrapper.ShowText(animator.TextFull); // re-parses with typewriter actions to prevent ignoring them (if they're already present)
            initialized = true;
        }

        internal void AssignAnimator(ITextAnimatorProvider animator, ActionDatabase actionsDatabase, TypingsTimingsScriptableBase timingsScriptableBase)
        {
            if(textAnimatorProvider == animator) return;

            this.actionsDatabase = actionsDatabase;
            textAnimatorProvider = animator;
            _wrapper?.Dispose();
            initialized = false;
            InitializeOnce();
        }

        #region Variables

        #region Management Variables

        TextAnimatorComponentBase _textAnimator;

        /// <summary>
        /// The TextAnimator Component linked to this typewriter
        /// </summary>
        public TextAnimatorComponentBase TextAnimator
        {
            get
            {
                if (_textAnimator != null)
                    return _textAnimator;

                if(!TryGetComponent(out _textAnimator))
                {
                    Debug.LogError($"TextAnimator: Text Animator component is null on GameObject {gameObject.name}. Please add a component that inherits from TAnimCore");
                }

                _textAnimator.TryInitializingOnce();

                return _textAnimator;
            }
        }

        #endregion

        #region Typewriter settings

        /// <summary>
        /// <c>true</c> if the typewriter is enabled
        /// </summary>

        [System.Obsolete("Please access localSettings or the referenced scriptable instead")]
        public bool useTypeWriter => localSettings.useTypeWriter;

        [System.Obsolete("Please access localSettings or the referenced scriptable instead")]
        public StartTypewriterMode startTypewriterMode => localSettings.startTypewriterMode;

        #region Typewriter Skip

        [System.Obsolete("Please access localSettings or the referenced scriptable instead")]
        public bool hideAppearancesOnSkip => localSettings.hideAppearancesOnSkip;

        [System.Obsolete("Please access localSettings or the referenced scriptable instead")]
        public bool hideDisappearancesOnSkip => localSettings.hideDisappearancesOnSkip;
        #endregion


        [System.Obsolete("Please access localSettings or the referenced scriptable instead")]
        public DisappearanceOrientation disappearanceOrientation => localSettings.disappearanceOrientation;

        #endregion

        /// <summary>
        /// <c>true</c> if you want to wait for every single character to animate before invoking <see cref="onTextShowed"/>.
        /// Otherwise, you might have that event invoked even if the very last character(s) are animating.
        /// </summary>
        /// <remarks>
        /// Usually users don't want to wait for the very last letter(s), similar to punctuation. That said, this option might come useful in cases like you have very slow letters.
        /// </remarks>
        [Tooltip(
            "True if you want to wait for every single character appearance to finish before triggering 'onTextShowed'. Default to false, as effects are usually fast enough and make the letters visible, and users are able to read them instantly.")]

        [System.Obsolete("Please access localSettings or the referenced scriptable instead")]
        public bool triggerShowedAfterEffectsEnd => localSettings.triggerDisappearedAfterEffectsEnd;

        /// <summary>
        /// <c>true</c> if you want to wait for every single character to animate before invoking <see cref="onTextDisappeared"/>.
        /// Otherwise, you might have that event invoked even if the very last character(s) are animating.
        /// </summary>
        /// <remarks>
        /// Usually users don't want to wait for the very last letter(s), similar to punctuation. That said, this option might come useful in cases like you have very slow letters.
        /// </remarks>
        [Tooltip("True if you want to wait for every single character disappearance to finish before triggering 'onTextDisappeared'. Default to false, as effects are usually fast enough")]


        [System.Obsolete("Please access localSettings or the referenced scriptable instead")]
        public bool triggerDisappearedAfterEffectsEnd => localSettings.triggerDisappearedAfterEffectsEnd;

        #endregion

        #region Events
        /// <summary>
        /// Called once the text is completely shown. <br/>
        /// If the typewriter is enabled, this event is called once it has ended showing all letters.
        /// </summary>
        public UnityEvent onTextShowed = new UnityEvent();

        /// <summary>
        /// Called once the typewriter starts showing text.<br/>
        /// </summary>
        /// <remarks>
        /// It is only invoked when the typewriter is enabled.
        /// </remarks>
        public UnityEvent onTypewriterStart = new UnityEvent();

        /// <summary>
        /// Called once the typewriter has completed hiding all the letters.
        /// </summary>
        /// <remarks>
        /// It is only invoked when the typewriter is enabled.
        /// </remarks>
        public UnityEvent onTextDisappeared = new UnityEvent();

        /// <summary>
        /// Called once a character has been shown by the typewriter.<br/>
        /// </summary>
        /// <remarks>
        /// It is only invoked when the typewriter is enabled.
        /// </remarks>
        public CharacterEvent onCharacterVisible = new CharacterEvent();

        /// <summary>
        /// Invoked when the typewriter is starting to wait for a character
        /// </summary>
        public CharacterWaitEvent onCharacterWaitStarted = new CharacterWaitEvent();

        /// <summary>
        /// Invoked after the typewriter has finished waiting for a character
        /// </summary>
        public CharacterWaitEvent onCharacterWaitFinished = new CharacterWaitEvent();

        /// <summary>
        /// Called once an event has been shown by the typewriter.<br/>
        /// See the <a href="https://www.febucci.com/text-animator-unity/docs/triggering-events-while-typing/">Events Manual</a> for more info.
        /// </summary>
        /// <remarks>
        /// It is only invoked when the typewriter is enabled.
        /// </remarks>
        public MessageEvent onMessage = new MessageEvent();
        #endregion

        #region Public Methods


        /// <inheritdoc cref="ITypewriterProvider.ShowText"/>
        public void ShowText(string text) => Wrapper.ShowText(text);


        /// <inheritdoc cref="ITypewriterProvider.SkipTypewriter"/>
        public void SkipTypewriter() => Wrapper.SkipTypewriter();


        #region Typewriter

        #region Appearing


        /// <inheritdoc cref="ITypewriterProvider.IsShowingText"/>
        public bool IsShowingText => Wrapper.IsShowingText;


        /// <inheritdoc cref="ITypewriterProvider.StartShowingText"/>
        [ContextMenu("Start Showing Text")]
        public void StartShowingText() => StartShowingText(false);

        /// <inheritdoc cref="ITypewriterProvider.StartShowingText"/>
        public void StartShowingText(bool restart)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) //prevents from firing in edit mode from the context menu
                return;
#endif
            Wrapper.StartShowingText(restart);
        }

        /// <inheritdoc cref="ITypewriterProvider.StopShowingText"/>
        [ContextMenu("Stop Showing Text")]
        public void StopShowingText()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) //prevents from firing in edit mode from the context menu
                return;
#endif

            Wrapper?.StopShowingText();
        }

        #endregion

        #region Disappearing

        /// <inheritdoc cref="ITypewriterProvider.IsHidingText"/>
        public bool IsHidingText => Wrapper.IsHidingText;


        /// <inheritdoc cref="ITypewriterProvider.StartDisappearingText"/>
        [ContextMenu("Start Disappearing Text")]
        public void StartDisappearingText()
        {
            #if UNITY_EDITOR
            if(!Application.isPlaying)
                return;
            #endif

            Wrapper?.StartDisappearingText();
        }

        Coroutine hideRoutine;
        Coroutine nestedHideRoutine;


        /// <inheritdoc cref="ITypewriterProvider.StopDisappearingText"/>
        [ContextMenu("Stop Disappearing Text")]
        public void StopDisappearingText()
        {
            #if UNITY_EDITOR
            if(!Application.isPlaying)
                return;
            #endif

            Wrapper?.StopDisappearingText();
        }

        #endregion


        /// <inheritdoc cref="ITypewriterProvider.SetTypewriterSpeed"/>
        public void SetTypewriterSpeed(float value) => Wrapper.SetTypewriterSpeed(value);

        #endregion

        #endregion

        #region Utilities

        /// <inheritdoc cref="ITypewriterProvider.TriggerRemainingEvents"/>
        public void TriggerRemainingEvents() => Wrapper.TriggerRemainingEvents();

        /// <inheritdoc cref="ITypewriterProvider.TriggerVisibleEvents"/>
        public void TriggerVisibleEvents() => Wrapper.TriggerVisibleEvents();
        #endregion


        #region Virtual

        /// <summary>
        /// Used to "block" the typewriter before progressing (waiting for the character)
        /// </summary>
        /// <param name="character">Current character being elaborated</param>
        /// <returns><see cref="ActionStatus.Running"/> to keep holding, otherwise <see cref="ActionStatus.Finished"/></returns>
        /// <seealso cref="OnAfterWaitingCharacter"/>
        /// <example>
        ///
        /// <code>
        ///
        /// protected override ActionStatus OnBeforeWaitingCharacter(CharacterData character)
        /// {
        ///     if (YourGameComponent.IsDoingSomething)
        ///         return ActionStatus.Running;
        ///
        ///     return base.OnBeforeWaitingCharacter(character);
        /// }
        /// </code>
        /// </example>
        /// <remarks>This is 99% used in custom integrations for custom typewriters. If you want to implement different wait times for characters
        /// instead, please use "Custom Typewriter Waits" and create a custom scriptable:
        /// https://docs.febucci.com/text-animator-unity/writing-custom-classes/writing-custom-typing-waits-c</remarks>
        protected virtual ActionStatus OnBeforeShowingCharacter(CharacterData character)
            => ActionStatus.Finished;

        /// <summary>
        /// Used to "block" the typewriter before progressing (going to the next character)
        /// </summary>
        /// <param name="character">Current character being elaborated</param>
        /// <returns><see cref="ActionStatus.Running"/> to keep holding, otherwise <see cref="ActionStatus.Finished"/></returns>
        /// <seealso cref="OnBeforeShowingCharacter"/>
        /// <example>
        ///
        /// <code>
        ///
        /// protected override ActionStatus OnAfterWaitingCharacter(CharacterData character)
        /// {
        ///     if (YourGameComponent.IsDoingSomething)
        ///         return ActionStatus.Running;
        ///
        ///     return base.OnBeforeWaitingCharacter(character);
        /// }
        /// </code>
        /// </example>
        /// <remarks>This is 99% used in custom integrations for custom typewriters. If you want to implement different wait times for characters
        /// instead, please use "Custom Typewriter Waits" and create a custom scriptable:
        /// https://docs.febucci.com/text-animator-unity/writing-custom-classes/writing-custom-typing-waits-c</remarks>
        protected virtual ActionStatus OnAfterWaitingCharacter(CharacterData character)
            => ActionStatus.Finished;

        #endregion

        /// <summary>
        /// Unity's default MonoBehavior 'OnEnable' callback.
        /// </summary>
        /// <remarks>
        /// P.S. If you're overriding this method, don't forget to invoke the base one.
        /// </remarks>
        protected virtual void OnEnable()
        {
            if(!initialized) return;

            if (!localSettings.useTypeWriter)
                return;

            if (!localSettings.startTypewriterMode.HasFlag(StartTypewriterMode.OnEnable))
                return;

            StartShowingText();
        }

        // ReSharper disable once Unity.RedundantEventFunction
        /// <summary>
        /// Unity's default MonoBehavior 'OnDisable' callback.
        /// </summary>
        /// <remarks>
        /// P.S. If you're overriding this method, don't forget to invoke the base one.
        /// </remarks>
        protected virtual void OnDisable()
        {
            // for backwards compatibility
        }

        // ReSharper disable once Unity.RedundantEventFunction
        /// <summary>
        /// Unity's default MonoBehavior 'OnDestroy' callback.
        /// </summary>
        /// <remarks>
        /// P.S. If you're overriding this method, don't forget to invoke the base one.
        /// </remarks>
        protected virtual void OnDestroy()
        {
            _wrapper?.Dispose();
            initialized = false;
        }

        #region BACKWARDS COMPATIBILITY

        [Obsolete("Use IsHidingText instead")]
        // ReSharper disable once InconsistentNaming
        public bool isHidingText => IsHidingText;

        [Obsolete("Please use IsShowingText")]
        // ReSharper disable once InconsistentNaming
        public bool isShowingText => IsShowingText;

        #endregion

    }

}