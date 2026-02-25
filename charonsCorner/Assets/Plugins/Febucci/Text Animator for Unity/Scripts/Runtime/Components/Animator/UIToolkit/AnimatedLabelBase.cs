// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

#if UNITY_2023_2_OR_NEWER
#define CAN_USE_UXML_DECORATORS
#endif

using System;
using Febucci.Parsing;
using Febucci.Parsing.Core;
using Febucci.Parsing.Regions;
using Febucci.TextAnimatorCore.Settings;
using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorForUnity.Actions;
using Febucci.TextAnimatorForUnity.Parsing;
using UnityEngine;
using UnityEngine.UIElements;
using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorCore.Data;
using Febucci.TextAnimatorCore.Typing;
using Febucci.TextAnimatorForUnity.Styles;

namespace Febucci.TextAnimatorForUnity.UIToolkit
{
    #if CAN_USE_UXML_DECORATORS
    [UxmlElement]
    #endif
#if UNITY_6000_3_OR_NEWER
    public abstract partial class AnimatedLabelBase : Label,
        ITextGenerator,
        ISettingsProvider<TypewriterSettings>,
        ISettingsProvider<AnimatorSettings>,
        ITextAnimatorProvider
#else
    public abstract partial class AnimatedLabelBase : VisualElement, ITextGenerator, ISettingsProvider
#endif
    {
        protected bool isValid { get; private set; }
        protected TextAnimator animator { get; private set; }

        public TypewriterCore Typewriter
        {
            get;
            private set;
        }


        protected int CharactersCountWithoutTAnimTags { get; private set; }

        TextAnimator ITextAnimatorProvider.TextAnimator => animator;


        /// <inheritdoc cref="ITextAnimatorProvider.CharactersCount"/>
        public int CharactersCount => animator.CharactersCount;

        /// <inheritdoc cref="ITextAnimatorProvider.Characters"/>
        public CharacterData[] Characters => animator.Characters;

        /// <inheritdoc cref="ITextAnimatorProvider.WordsCount"/>
        public int WordsCount => animator.WordsCount;

        /// <inheritdoc cref="ITextAnimatorProvider.Words"/>
        public WordInfo[] Words => animator.Words;

        #if CAN_USE_UXML_DECORATORS
        [UxmlAttribute]
        #endif
        // PSA this is invoked by UI toolkit in *A LOT* of occasions, outside playmode and also
        // when the label itself is not initialized yet - so we need to make sure
        // - a: we're assigning stuff (or it'd get lost) BUT updating it only when necessary
        // - b: we refresh the text only if in playmode and valid state
        public AnimatorSettingsScriptable AnimationSettings
        {
            get => animationSettings;
            set
            {
                animationSettings = value;
                if(animator==null)return;
                animator.LocalSettingsProvider = animationSettings;
                RefreshText();
            }
        }

        AnimatorSettings ISettingsProvider<AnimatorSettings>.Settings => animationSettings != null ? animationSettings.Settings : null;

        AnimatorSettingsScriptable animationSettings;


        TypingsTimingsScriptableBase timingsSettings;
        #if CAN_USE_UXML_DECORATORS
        [UxmlAttribute]
        #endif
        public TypingsTimingsScriptableBase TimingSettings
        {

            get => timingsSettings;
            set
            {
                timingsSettings = value;
                if (Typewriter == null) return;
                Typewriter.timingsProvider = timingsSettings;
            }
        }

        TypewriterSettingsScriptable typewriterSettings;

        TypewriterSettings ISettingsProvider<TypewriterSettings>.Settings => settings;

        #if CAN_USE_UXML_DECORATORS
        [UxmlAttribute]
        #endif
        public TypewriterSettingsScriptable TypewriterSettings
        {
            get => typewriterSettings;
            set
            {
                typewriterSettings = value;
                if (Typewriter == null) return;
            }
        }


        #if CAN_USE_UXML_DECORATORS
        [UxmlAttribute, TextArea]
        #endif
        public string Text
        {
            get => animator.TextFull;
            set
            {
                string processedValue = value;
                #if UNITY_6000_3_OR_NEWER
                // Convert escape sequences if parseEscapeSequences is enabled (Label property, Unity 6.3+)
                // Reference: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UIElements.Label-parseEscapeSequences.html
                if (parseEscapeSequences)
                {
                    processedValue = processedValue.Replace(@"\n", "\n");
                }
                #endif
                text = processedValue;
                animator?.SetText(processedValue);
            }
        }

        AnimationsDatabase _behaviorsDatabase;
        #if CAN_USE_UXML_DECORATORS
        [UxmlAttribute]
        #endif
        // PSA this is invoked by UI toolkit in *A LOT* of occasions, outside playmode and also
        // when the label itself is not initialized yet - so we need to make sure
        // - a: we're assigning stuff (or it'd get lost) BUT updating it only when necessary
        // - b: we refresh the text only if in playmode and valid state
        public AnimationsDatabase BehaviorsDatabase
        {
            get => _behaviorsDatabase;
            set
            {
                if(_behaviorsDatabase == value) return;
                _behaviorsDatabase = value;
                if(animator == null) return;
                animator.effectsDatabase = _behaviorsDatabase;
                RefreshText();
            }
        }


        StyleSheetScriptable _stylesDatabase;
        #if CAN_USE_UXML_DECORATORS
        [UxmlAttribute]
        #endif
        // PSA this is invoked by UI toolkit in *A LOT* of occasions, outside playmode and also
        // when the label itself is not initialized yet - so we need to make sure
        // - a: we're assigning stuff (or it'd get lost) BUT updating it only when necessary
        // - b: we refresh the text only if in playmode and valid state
        public StyleSheetScriptable StyleSheetDatabase
        {
            get => _stylesDatabase;
            set
            {
                if(_stylesDatabase == value) return;
                _stylesDatabase = value;
                if(animator == null) return;
                animator.stylesDatabase = _stylesDatabase;
                RefreshText();
            }
        }

        ActionDatabase _actionsDatabase;
        #if CAN_USE_UXML_DECORATORS
        [UxmlAttribute]
        #endif
        // PSA this is invoked by UI toolkit in *A LOT* of occasions, outside playmode and also
        // when the label itself is not initialized yet - so we need to make sure
        // - a: we're assigning stuff (or it'd get lost) BUT updating it only when necessary
        // - b: we refresh the text only if in playmode and valid state
        public ActionDatabase ActionsDatabase
        {
            get => _actionsDatabase;
            set
            {
                if(_actionsDatabase == value) return;
                _actionsDatabase = value;

                if(!Application.isPlaying)
                    return;

                if (Typewriter != null)
                {
                    Typewriter.ActionProviders = GetActionProviders();
                }
                RefreshText();
            }
        }

        IDatabaseProvider<ITypewriterAction>[] GetActionProviders()
        {
            var providers = new System.Collections.Generic.List<IDatabaseProvider<ITypewriterAction>>();

            bool isAttachedAndPlaying = panel != null && Application.isPlaying;

            #if UNITY_EDITOR
            if (isAttachedAndPlaying && GlobalActionComponentsDatabase.Instance != null)
                providers.Add(GlobalActionComponentsDatabase.Instance);

            if (_actionsDatabase != null)
                providers.Add(_actionsDatabase);
            #else
            if (_actionsDatabase != null)
                providers.Add(_actionsDatabase);
            if (isAttachedAndPlaying && GlobalActionComponentsDatabase.Instance != null)
                providers.Add(GlobalActionComponentsDatabase.Instance);
            #endif

            return providers.ToArray();
        }

        TextParser uitkParser;
        UIToolkitLabelTagParser uiToolkitLabelTagParser;
        public AnimatedLabelBase()
        {
            try
            {
                uiToolkitLabelTagParser = new UIToolkitLabelTagParser(this, '<', '/', '>');
            }
            catch(Exception e)
            {
                Debug.LogError($"Something went wrong initializing the UI Toolkit tag parser. Please make sure the setup is correct, or contact support with the stack trace and we'll fix it as soon as possible. Error: {e}");
                return;
            }

            uitkParser = new TextParser(false, '<', '>');

            try
            {
                animator = new TextAnimator(
                    pasteNoParseTag: false,
                    openingBracket: '<',
                    closingTagSymbol:'/',
                    closingBracket:'>',
                    UnityEngineProvider.Instance,
                    animatorSettingsProvider: this,
                    globalSettingsProvider: TextAnimatorSettings.Instance,
                    textGenerator: this,
                    caller: this,
                    isUpPositive: false, //UITk has y = down,
                    extraParsers: new TagParserBase[]{uiToolkitLabelTagParser}
                );

                animator.OnStartedParsing += textFull =>
                {
                    Logger.Debug($"Updating AnimatedLabel text from parsing start to: {textFull}");
                    text = textFull;
                };
                isValid = true;
            }
            catch(Exception e)
            {
                Debug.LogError($"Something went wrong initializing TextAnimator for UI Toolkit. Please make sure the setup is correct, or contact support with the stack trace and we'll fix it as soon as possible. Error: {e}");
                return;
            }

            try
            {
                Typewriter = new TypewriterCore(
                    animator,
                    this,
                    timingsSettings,
                    TextAnimatorSettings.Instance,
                    localTypewriterSettingsProvider:this,
                    GetActionProviders());
            }
            catch(Exception e)
            {
                Debug.LogError($"Unable to create typewriter on {this}. Skipping. - {e}");
                return;
            }

            animator.OnTextParsed += OnTextParsed;
            animator.effectsDatabase = BehaviorsDatabase;
            animator.stylesDatabase = StyleSheetDatabase;

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            if (Application.isPlaying && Typewriter != null)
            {
                Typewriter.ActionProviders = GetActionProviders();
            }
        }

        public void TryInitializingOnce()
        {
            // nothing - already done in the constructor
        }


        /// <inheritdoc cref="ITextAnimatorProvider.SetText"/>
        public void SetText(string text, bool hideText = false) => animator.SetText(text,  hideText ? ShowTextMode.Hidden : ShowTextMode.Shown);

        /// <inheritdoc cref="ITextAnimatorProvider.SwapText"/>
        public void SwapText(string text) => animator.SwapText(text);

        /// <inheritdoc cref="ITextAnimatorProvider.AppendText"/>
        public void AppendText(string appendedText, bool hideText = false) => animator.AppendText(appendedText, hideText);

        public TextRegion<IEffectPlayer>[] Disappearances { get; }
        /// <inheritdoc cref="ITextAnimatorProvider.SetVisibilityChar"/>
        public void SetVisibilityChar(int index, bool isVisible, bool canPlayEffects) => animator.SetVisibilityChar(index, isVisible, canPlayEffects);

        /// <inheritdoc cref="ITextAnimatorProvider.SetVisibilityWord"/>
        public void SetVisibilityWord(int index, bool isVisible, bool canPlayEffects) => animator.SetVisibilityWord(index, isVisible, canPlayEffects);

        /// <inheritdoc cref="ITextAnimatorProvider.SetVisibilityEntireText"/>
        public void SetVisibilityEntireText(bool isVisible, bool canPlayEffects = true) => animator.SetVisibilityEntireText(isVisible, canPlayEffects);

        /// <inheritdoc cref="ITextAnimatorProvider.FirstVisibleCharacter"/>
        public int FirstVisibleCharacter
        {
            get => animator.FirstVisibleCharacter;
            set => animator.FirstVisibleCharacter = value;
        }


        /// <inheritdoc cref="ITextAnimatorProvider.MaxVisibleCharacters"/>
        public int MaxVisibleCharacters
        {
            get => animator.MaxVisibleCharacters;
            set => animator.MaxVisibleCharacters = value;
        }
        public TextRegion<IEffectPlayer>[] Behaviors { get; }
        public TextRegion<IEffectPlayer>[] Appearances { get; }

        protected virtual void OnTextParsed()
        {
            text = animator.TextWithoutTextAnimatorTags;
            CharactersCountWithoutTAnimTags = text.Length;
            schedule.Execute(MarkDirtyRepaint);
            #if UNITY_6000_3_OR_NEWER
            schedule.Execute(MarkDirtyText);
            #endif
        }

        float GetCurrentTime()
        {
            #if UNITY_EDITOR
            if(!Application.isPlaying)
                return (float)UnityEditor.EditorApplication.timeSinceStartup;
            #endif

            return UnityEngine.Time.time;
        }

        void RefreshText()
        {
            if(!Application.isPlaying)
                return;

            animator.SetText(animator.TextFull);
        }

        float lastTime = 0;
        TypewriterSettings settings;

        protected void Animate()
        {
            float currentTime = GetCurrentTime();
            float delta = currentTime - lastTime;
            lastTime = currentTime;
            animator.Animate(delta);
        }

        public void SetTextToSource(string text)
        {

        }

        public abstract void CopyMeshFromSource(ref CharacterData[] characters, int charactersCount);
        public abstract void PasteMeshToSource(CharacterData[] characters, int charactersCount);

        public virtual void ForceMeshUpdate()
        {
            schedule.Execute(MarkDirtyRepaint);
        }

        public string GetStrippedTextWithoutAnyTags(string textWithoutTAnimTags)
        {
            return uitkParser.ParseText(textWithoutTAnimTags, uiToolkitLabelTagParser);
        }

        public abstract string GetFullText();

        public abstract int GetCharactersCount();
        public abstract bool HasChangedMeshRenderingSettings();

        public int GetFirstCharacterIndexInsidePage() => 0;
        public int GetRenderedCharactersCountInsidePage(int charactersCount) => int.MaxValue;
    }

}