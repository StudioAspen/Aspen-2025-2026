// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.Parsing.Regions;
using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorCore.Text;
namespace Febucci.TextAnimatorForUnity
{
    public interface ITextAnimatorProvider
    {
        public void TryInitializingOnce();
        internal TextAnimator TextAnimator { get; }

        public int CharactersCount { get; }
        public CharacterData[] Characters { get; }


        public int WordsCount { get; }
        public WordInfo[] Words { get; }


        /// <summary>
        /// Handles the very first character allowed to be visible in the text.
        /// </summary>
        /// <remarks>
        /// Be aware that in order to visible, a character also needs to also have its own "visible" flag set to true. See <see cref="SetVisibilityChar"/> and <see cref="CharacterData.isVisible"/>
        /// </remarks>
        public int FirstVisibleCharacter { get; set; }

        /// <summary>
        /// Handles the very last character allowed to be visible in the text.
        /// </summary>
        /// <remarks>
        /// Be aware that in order to visible, a character also needs to also have its own "visible" flag set to true. See <see cref="SetVisibilityChar"/> and <see cref="CharacterData.isVisible"/>
        /// </remarks>
        public int MaxVisibleCharacters { get; set; }


        /// <summary>
        /// All the behavior effects that are applied to the current text.
        /// </summary>
        public TextRegion<IEffectPlayer>[] Behaviors { get; }

        /// <summary>
        /// All the appearance effects that are applied to the current text.
        /// </summary>
        public TextRegion<IEffectPlayer>[] Appearances { get; }

        /// <summary>
        /// All the disappearance effects that are applied to the current text.
        /// </summary>
        public TextRegion<IEffectPlayer>[] Disappearances { get; }


        #region Characters


        /// <summary>
        /// Sets a character visibility.
        /// </summary>
        /// <param name="index">Character's index. See <see cref="CharactersCount"/> and the <see cref="Characters"/> array.</param>
        /// <param name="isVisible">Controls if the character should be visible</param>
        /// <param name="canPlayEffects"></param>

        public void SetVisibilityChar(int index, bool isVisible, bool canPlayEffects);


        /// <summary>
        /// Sets a word visibility.
        /// </summary>
        /// <param name="index">Word's index. See <see cref="WordsCount"/> and the <see cref="Words"/> array.</param>
        /// <param name="isVisible">Controls if the word should be visible</param>
        /// <param name="canPlayEffects"></param>
        public void SetVisibilityWord(int index, bool isVisible, bool canPlayEffects);

        /// <summary>
        /// Sets the visibility of the entire text, also allowing to play or skip effects.
        /// </summary>
        /// <param name="isVisible"></param>
        /// <param name="canPlayEffects"></param>
        public void SetVisibilityEntireText(bool isVisible, bool canPlayEffects = true);



        #endregion


        #region Texts


        /// <summary>
        /// Sets the text to Text Animator, parsing its rich text tags.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="hideText"></param>
        public void SetText(string text, bool hideText = false);

        /// <summary>
        /// Changes the text to Text Animator with a new one, keeping the current visibility
        /// </summary>
        /// <param name="text"></param>
        public void SwapText(string text);


        /// <summary>
        /// Adds text to the already existing one, parsing its rich text tags.
        /// </summary>
        /// <param name="appendedText">New text that you want to append</param>
        /// <param name="hideText"></param>
        public void AppendText(string appendedText, bool hideText = false);

        #endregion

    }
}