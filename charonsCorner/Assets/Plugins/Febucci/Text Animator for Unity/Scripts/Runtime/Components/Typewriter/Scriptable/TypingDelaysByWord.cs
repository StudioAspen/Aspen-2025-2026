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
    [CreateAssetMenu(menuName = ScriptablePaths.TYPEWRITER_PATH+"By Word", fileName = "Typewriter By Word")]
    [System.Serializable]
    public class TypingDelaysByWord: TypingsTimingsScriptableBase
    {
        [SerializeField, Attributes.CharsDisplayTime] public float waitForNormalWord = 0.3f;
        [FormerlySerializedAs("waitForWordWithPuntuaction")] [SerializeField, Attributes.CharsDisplayTime] public float waitForWordWithPunctuation = 0.5f;
        [SerializeField, Attributes.CharsDisplayTime] public float disappearanceDelay = 0.5f;

        bool IsCharInsideAnyWord(CharacterData character)
        {
            return character.wordIndex >= 0;
        }

        public override float GetWaitAppearanceTimeOf(CharacterData chardata, TextAnimator textAnimator)
        {
            if (!IsCharInsideAnyWord(chardata) && textAnimator.LatestCharacterShown.index>0)
            {
                int latestWordShownIndex = textAnimator.Characters[textAnimator.LatestCharacterShown.index-1].wordIndex;
                if (latestWordShownIndex >= 0 && latestWordShownIndex < textAnimator.WordsCount)
                {
                    var word = textAnimator.Words[latestWordShownIndex];
                    return char.IsPunctuation(textAnimator.Characters[word.lastCharacterIndex].info.character)
                        ? waitForWordWithPunctuation
                        : waitForNormalWord;
                }

                return waitForNormalWord;
            }

            return 0;
        }

        public override float GetWaitDisappearanceTimeOf(CharacterData charIndex, TextAnimator animator)
        {
            return !IsCharInsideAnyWord(charIndex) ? disappearanceDelay : 0;
        }
    }
}