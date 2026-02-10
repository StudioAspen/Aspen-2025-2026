// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorCore.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Febucci.TextAnimatorForUnity
{
    [CreateAssetMenu(menuName = ScriptablePaths.TYPEWRITER_PATH+"By Character", fileName = "Typewriter By Character")]
    [System.Serializable]
    public class TypingDelaysByCharacter : TypingsTimingsScriptableBase
    {
        [SerializeField, Attributes.CharsDisplayTime, Tooltip("Wait time for normal letters")] public float waitForNormalChars = .03f;
        [SerializeField, Attributes.CharsDisplayTime, Tooltip("Wait time for ! ? .")] public float waitLong = .6f;
        [SerializeField, Attributes.CharsDisplayTime, Tooltip("Wait time for ; : ) - ,")] public float waitMiddle = .2f;

        [System.Obsolete("Typo, please use 'avoidMultiplePunctuationWait' instead.")]
        public bool avoidMultiplePunctuactionWait => avoidMultiplePunctuationWait;

        [FormerlySerializedAs("avoidMultiplePunctuactionWait")]
        [SerializeField, Tooltip("-True: only the last punctuation on a sequence waits for its category time.\n-False: each punctuation will wait, regardless if it's in a sequence or not")] public bool avoidMultiplePunctuationWait = false;

        [SerializeField, Tooltip("True if you want the typewriter to wait for new line characters")] public bool waitForNewLines = true;

        [SerializeField, Tooltip("True if you want the typewriter to wait for all characters, false if you want to skip waiting for the last one")] public bool waitForLastCharacter = true;

        [SerializeField, Tooltip("True if you want to use the same typewriter's wait times for the disappearance progression, false if you want to use a different wait time")] public bool useTypewriterWaitForDisappearances = true;
        [SerializeField, Attributes.CharsDisplayTime, Tooltip("Wait time for characters in the disappearance progression")] float disappearanceWaitTime = .015f;
        [SerializeField, Attributes.MinValue(0.1f), Tooltip("How much faster/slower is the disappearance progression compared to the typewriter's typing speed")] public float disappearanceSpeedMultiplier = 1;

        public override float GetWaitAppearanceTimeOf(CharacterData characterData, TextAnimator animator)
        {
            int charIndex = characterData.index;
            char character = characterData.info.character;

            //avoids waiting for the last character
            if (!waitForLastCharacter && animator.AllLettersShown)
                return 0;

            //avoids waiting for multiple times if there are puntuactions near each other
            if (avoidMultiplePunctuationWait && char.IsPunctuation(character)) //curr char is punctuation
            {
                //next char is punctuation too, so skips this one
                if (charIndex < animator.CharactersCount - 1
                    && char.IsPunctuation(animator.Characters[charIndex + 1].info
                        .character))
                {
                    return waitForNormalChars;
                }
            }

            //avoids waiting for new lines
            if (!waitForNewLines && !characterData.info.isRendered)
            {
                bool IsUnicodeNewLine(ulong unicode) //Returns true if the unicode value represents a new line
                {
                    return unicode == 10 || unicode == 13;
                }

                //skips waiting for a new line
                if (IsUnicodeNewLine(System.Convert.ToUInt64(character)))
                    return 0; //TODO test
            }

            //character is not before another punctuaction
            switch (character)
            {
                case ';':
                case ':':
                case ')':
                case '-':
                case ',': return waitMiddle;

                case '!':
                case '?':
                case '.':
                    return waitLong;
            }

            return waitForNormalChars;

        }

        public override float GetWaitDisappearanceTimeOf(CharacterData characterData, TextAnimator animator)
        {
            return useTypewriterWaitForDisappearances ? GetWaitAppearanceTimeOf(characterData, animator) * (1/disappearanceSpeedMultiplier) : disappearanceWaitTime;
        }
    }
}