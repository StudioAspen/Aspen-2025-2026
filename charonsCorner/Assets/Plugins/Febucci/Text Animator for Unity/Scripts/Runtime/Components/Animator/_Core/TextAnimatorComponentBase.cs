// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System;
using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorCore.Settings;
using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorCore.Time;
using Febucci.Parsing.Regions;
using Febucci.Parsing.Core;
using Febucci.TextAnimatorForUnity.Styles;
using UnityEngine;
using UnityEngine.Serialization;

namespace Febucci.TextAnimatorForUnity
{

    [DisallowMultipleComponent]
    [HelpURL("https://www.febucci.com/text-animator-unity/docs/how-to-add-effects-to-your-texts/")]
    public abstract class TextAnimatorComponentBase : MonoBehaviour, ISettingsProvider<AnimatorSettings>, ITextAnimatorProvider
    {
        [SerializeField] public AnimatorSettings localSettings = new AnimatorSettings();
        [SerializeField] public AnimatorSettingsScriptable sharedSettings;
        public AnimatorSettings Settings
        {
            get => sharedSettings != null ? sharedSettings.Settings : localSettings;
        }

        // ReSharper disable once InconsistentNaming
        TextAnimator _wrapper;

        // mandatory 'cos we have no assurance that the user
        // is initializing first and THEN invoking methods in their game
        TextAnimator Wrapper
        {
            get
            {
                if (initialized) return _wrapper;

                TryInitializingOnce();
                return _wrapper;
            }
        }

        TextAnimator ITextAnimatorProvider.TextAnimator
        {
            get => _wrapper;
        }

        TagParserBase[] extraParsers;
        ITextGenerator generator;

        protected abstract ITextGenerator GetTextGenerator();

        #region  Variables
        bool initialized;

        #region Options


        /// <summary>
        /// Controls when Text Animator should update its effects. Set it to <see cref="AnimationLoop.Script"/> if you want to control the animations from your own loop, invoking the <see cref="Animate(float)"/> method.
        /// </summary>
        [Tooltip("Controls when this TextAnimator component should update its effects. Defaults in the 'Update' Loop.\nSet it to 'Manual' if you want to control the animations from your own loop instead.")]
        public AnimationLoop animationLoop = AnimationLoop.Update;

        /// <summary>
        /// Chooses which Time Scale to use when automatically animating effects (in other words, when the Update Mode is not set to Script). Set it to <see cref="TimeScale.Unscaled"/> if you want to animate effects even when the game is paused.
        /// </summary>
        [Tooltip("Chooses which Time Scale to use when animating effects.\nSet it to 'Unscaled' if you want to animate effects even when the game is paused.")]
        public TimeScale timeScale => localSettings.timeScale;

        #endregion

        #region Text



        public string textFull => Wrapper.TextFull;

        /// <summary>
        /// The text without any Text Animator tag
        /// </summary>
        /// <remarks>
        /// PS. this might still contain other tags from different supported plugins, like "color" from TMPro.
        /// To get the full stripped text, see <see cref="textWithoutAnyTag"/>.
        /// </remarks>
        public string textWithoutTextAnimTags => Wrapper.TextWithoutTextAnimatorTags;
        public string textWithoutAnyTag => Wrapper.TextWithoutAnyTag;

        public CharacterData latestCharacterShown => Wrapper.LatestCharacterShown;


        /// <summary>
        /// <c>true</c> if the text is entirely visible, including waiting for appearance effects to finish
        /// (as they might still hide a character until the very end)
        /// </summary>
        /// <remarks>
        /// You can use this to check if all the letters have been shown.
        /// </remarks>
        public bool allLettersShown => Wrapper.AllLettersShown;


        /// <summary>
        /// <c>true</c> if any letter is still visible in the text
        /// </summary>
        /// <remarks>
        /// You can use this to check if the disappearance effects are still running.
        /// </remarks>
        public bool anyLetterVisible => Wrapper.AnyLetterVisible;

        public int CharactersCount => Wrapper.CharactersCount;
        public CharacterData[] Characters => Wrapper.Characters;

        /// <summary>
        /// Number of words in the text
        /// </summary>
        public int WordsCount => Wrapper.WordsCount;

        public WordInfo[] Words => Wrapper.Words;

        //---CHARS SIZE/INTENSITY---

        /// <summary>
        /// True if you want the animations to be uniform/consistent across different font sizes. Default/Suggested to leave this as true, and change <see cref="referenceFontSize"/>. Otherwise, effects will move more when the text is smaller (requires less space on screen).
        /// </summary>
        [Tooltip("True if you want the animations to be uniform/consistent across different font sizes. Default/Suggested to leave this as true, and change the 'Reference Font Size'.\nOtherwise, effects will move more when the text is smaller (requires less space on screen)")]
        public bool useDynamicScaling => localSettings.useDynamicScaling;

        /// <summary>
        /// Font size that will be used as reference to keep animations consistent/uniform at different scales. Works only if <see cref="useDynamicScaling"/> is set to true.
        /// </summary>
        [Tooltip("Font size that will be used as reference to keep animations consistent/uniform at different scales.")]
        public float referenceFontSize => localSettings.referenceFontSize;


        //---OTHERS---

        /// <summary>
        /// True if you want the animator's time to be reset on new text.
        /// </summary>
        [Tooltip("True if you want the animator's time to be reset on new text.")]
        public bool isResettingTimeOnNewText => localSettings.isResettingTimeOnNewText;

        #endregion

        #region Effects and Databases

        // ----------------
        // -- Databases --
        // ----------------
        [FormerlySerializedAs("databaseBehaviors")]
        [SerializeField] AnimationsDatabase databaseEffects;
        /// <summary>
        /// Behaviors Database used by this Text Animator, in addition to the global ones found in <see cref="TextAnimatorSettings"/> asset.
        /// </summary>
        public AnimationsDatabase DatabaseEffects
        {
            get => databaseEffects;
            set
            {
                databaseEffects = value;
                if(_wrapper!=null) _wrapper.RequiresMeshUpdate = true;
            }
        }

        // ----------------
        // -- Styles --
        // ----------------

        [SerializeField] StyleSheetScriptable styleSheet;

        public StyleSheetScriptable StyleSheet
        {
            get => styleSheet;
            set
            {
                styleSheet = value;

                if (Wrapper != null)
                {
                    _wrapper.stylesDatabase = styleSheet;
                    _wrapper.RequiresMeshUpdate = true;
                }
            }
        }

        // ----------------
        // -- Effects --
        // ----------------

        /// <inheritdoc cref="ITextAnimatorProvider.Behaviors"/>
        public TextRegion<IEffectPlayer>[] Behaviors => Wrapper.BehaviorRegions;

        /// <inheritdoc cref="ITextAnimatorProvider.Appearances"/>
        public TextRegion<IEffectPlayer>[] Appearances => Wrapper.AppearanceRegions;


        /// <inheritdoc cref="ITextAnimatorProvider.Disappearances"/>
        public TextRegion<IEffectPlayer>[] Disappearances => Wrapper.DisappearanceRegions;

        #endregion

        #endregion

        /// <summary>
        /// Called once when the component is initialized.
        /// </summary>
        protected virtual void OnInitialized() { }

        void Awake()
        {
            TryInitializingOnce();
        }

        protected abstract bool IsUpPositive();

        public void TryInitializingOnce()
        {
            if (initialized) return;

            initialized = true;

            AnimUtils.Initialize();

            generator = GetTextGenerator();
            var parsers = GetExtraParsers();

            databaseEffects?.ForceBuildRefresh();
            styleSheet?.ForceBuildRefresh();

            _wrapper = new TextAnimator(
                true,
                openingBracket:
                '<',
                closingTagSymbol: '/',
                closingBracket: '>',
                UnityEngineProvider.Instance,
                generator,
                animatorSettingsProvider: this,
                globalSettingsProvider: TextAnimatorSettings.Instance,
                extraParsers: parsers,
                caller: this,
                isUpPositive: IsUpPositive());

            OnInitialized();

            _wrapper.effectsDatabase = databaseEffects;
            _wrapper.stylesDatabase = styleSheet;
            _wrapper.OnDisposed += () => initialized = false;

            // Sets initial text, if any
            generator.SetTextToSource(generator.GetFullText());
        }

        /// <summary>
        /// Contains TextAnimator's current time values.
        /// </summary>
        [HideInInspector] public TimeData time => _wrapper.Time;


        /// <summary>
        /// Controls how default tags should be applied.\n"Fallback" will apply the effects only to characters that don't have any.\n"Constant" will apply the default effects to all the characters, even if they already have other tags via text.
        /// </summary>
        [Tooltip(
            "Controls how default tags should be applied.\n\"Fallback\" will apply the effects only to characters that don't have any.\n\"Constant\" will apply the default effects to all the characters, even if they already have other tags via text.")]
        public DefaultEffectsMode defaultTagsMode
        {
            get => localSettings.defaultEffectsMode;
            set => localSettings.defaultEffectsMode = value;
        }

        #region Text

        protected virtual TagParserBase[] GetExtraParsers(){ return Array.Empty<TagParserBase>(); }

        TextAnimatorSettings globalSettings;

        /// <summary>
        /// Sets the text to Text Animator, parsing its rich text tags.
        /// </summary>
        /// <param name="text">Full text that you want to paste, including rich text tags.</param>
        /// <remarks>This method shows the text instantly. To control if it should be hidden instead, please see <see cref="SetText(string, bool)"/>. </remarks>
        public void SetText(string text)
            => Wrapper.SetText(text);

        /// <inheritdoc cref="ITextAnimatorProvider.SwapText"/>
        public void SwapText(string text)
            => Wrapper.SwapText(text);

        /// <inheritdoc cref="ITextAnimatorProvider.SetText"/>
        public void SetText(string text, bool hideText)
            => Wrapper.SetText(text, hideText ? ShowTextMode.Hidden : ShowTextMode.Shown);

        /// <inheritdoc cref="ITextAnimatorProvider.AppendText"/>
        public void AppendText(string appendedText, bool hideText = false)
            => Wrapper.AppendText(appendedText, hideText);

        /// <inheritdoc cref="ITextAnimatorProvider.SetVisibilityChar"/>
        public void SetVisibilityChar(int index, bool isVisible, bool canPlayEffects = true)
            => Wrapper.SetVisibilityChar(index, isVisible, canPlayEffects);

        /// <inheritdoc cref="ITextAnimatorProvider.SetVisibilityWord"/>
        public void SetVisibilityWord(int index, bool isVisible, bool canPlayEffects = true)
            => Wrapper.SetVisibilityWord(index, isVisible, canPlayEffects);

        /// <inheritdoc cref="ITextAnimatorProvider.SetVisibilityEntireText"/>
        public void SetVisibilityEntireText(bool isVisible, bool canPlayEffects = true)
            => Wrapper.SetVisibilityEntireText(isVisible, canPlayEffects);

        #endregion

        #region Typing


        /// <inheritdoc cref="ITextAnimatorProvider.FirstVisibleCharacter"/>
        public int FirstVisibleCharacter
        {
            get => Wrapper.FirstVisibleCharacter;
            set => Wrapper.FirstVisibleCharacter = value;
        }


        /// <inheritdoc cref="ITextAnimatorProvider.MaxVisibleCharacters"/>
        public int MaxVisibleCharacters
        {
            get => Wrapper.MaxVisibleCharacters;
            set => Wrapper.MaxVisibleCharacters = value;
        }


        #endregion


        #region Animation
        private void Update()
        {
            if(!IsReady()) return;

            //--Animates in Core Loop--
            if (animationLoop == AnimationLoop.Update)
                Animate(timeScale == TimeScale.Unscaled ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        void LateUpdate()
        {
            if (animationLoop == AnimationLoop.LateUpdate)
                Animate(UnityEngineProvider.Instance.GetCurrentDeltaTime(timeScale));
        }

        protected abstract bool IsReady();

        /// <summary>
        /// Proceeds the text animation with the given deltaTime value.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <example>
        /// You could use this if <see cref="animationLoop"/> is set to <see cref="AnimationLoop.Script"/> and you want to control when to animate the text.
        /// </example>
        public void Animate(float deltaTime)
        {
            if(!initialized) return;
            if(!IsReady()) return;

            //called here as well since this method be called from outside
            TryInitializingOnce();

            _wrapper.Animate(deltaTime);
        }

        #endregion

        /// <summary>
        /// Schedules that a mesh refresh is required as soon as possible, which will be applied before the next animation loop starts.
        /// </summary>
        public void ScheduleMeshRefresh() => Wrapper.RequiresMeshUpdate = true;

        public void ForceDatabaseRefresh()
        {
            if(DatabaseEffects) DatabaseEffects.ForceBuildRefresh();
            if(StyleSheet) StyleSheet.ForceBuildRefresh();

            _wrapper?.SetText(_wrapper.TextFull, ShowTextMode.Refresh);
        }

        public void SetBehaviorsActive(bool isCategoryEnabled) => localSettings.isAnimatingBehaviors = isCategoryEnabled;
        public void SetAppearancesActive(bool isCategoryEnabled) => localSettings.isAnimatingAppearances = isCategoryEnabled;

        #region Callbacks

        protected virtual void OnEnable() // things might have changed when disabled, e.g. autoSize etc.
        {
            Wrapper.RequiresMeshUpdate = true;
            Animate(0);
        }
        #endregion

        protected void OnDisable()
        {

        }

        protected virtual void OnDestroy()
        {
            _wrapper?.Dispose();
        }

        /// <summary>
        /// INTERNAL
        /// </summary>
        public void ResetState()
        {
            initialized = false;
            TryInitializingOnce();
        }

        /// <summary>
        /// <inheritdoc cref="TextAnimator.RefreshAllEffectStates"/>
        /// </summary>
        [ContextMenu("Refresh All Effect States")]
        public void RefreshAllEffectStates()
        {
            #if UNITY_EDITOR
            if(!Application.isPlaying)return;
            #endif

            _wrapper?.RefreshAllEffectStates();
        }


        #region Obsolete
        // Just for compatibility with older versions

        /// <summary>
        /// Turns all characters visible at the end of the frame (i.e. "a typewriter skip")
        /// </summary>
        /// <param name="skipAppearanceEffects">Set this to true if you want all letters to appear instantly (without any appearance effect)</param>
        [System.Obsolete("Use SetVisibilityEntireText instead")]
        public void ShowAllCharacters(bool skipAppearanceEffects) => SetVisibilityEntireText(true, skipAppearanceEffects);

        [System.Obsolete("Use 'Animate' instead.")]
        public void UpdateEffects() => Animate(timeScale == TimeScale.Unscaled ? Time.unscaledDeltaTime : Time.deltaTime);

        [System.Obsolete("Use 'ScheduleMeshRefresh' instead")]
        public void ForceMeshRefresh() => ScheduleMeshRefresh();


        [System.Obsolete("To restart TextAnimator's time, please use 'time.RestartTime()'. To skip appearances effects please set 'SetVisibilityEntireText(true, false)' instead")]
        public void ResetEffectsTime(bool skipAppearances)
        {
            time.RestartTime();

            if(skipAppearances) SetVisibilityEntireText(true, false);
        }

        [System.Obsolete("Please use 'isResettingTimeOnNewText' instead")]
        public bool isResettingEffectsOnNewText => isResettingTimeOnNewText;

        [System.Obsolete("Please use 'animationLoop' instead")]
        public AnimationLoop updateMode => animationLoop;


        [System.Obsolete("Please use 'textFull' instead")]
        public string text => textFull;

        [System.Obsolete("Please change 'referenceFontSize' instead")]
        public float effectIntensityMultiplier
        {
            get => referenceFontSize;
            set => localSettings.referenceFontSize = value;
        }


        #endregion
    }
}