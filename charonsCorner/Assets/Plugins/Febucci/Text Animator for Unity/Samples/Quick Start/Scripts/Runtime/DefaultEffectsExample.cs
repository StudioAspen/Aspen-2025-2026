// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System.Text;
using Febucci.Parsing;
using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorCore.Settings;
using Febucci.TextAnimatorForUnity;
using UnityEngine;

namespace Febucci.UI.Examples
{
    [AddComponentMenu("")]
    public class DefaultEffectsExample : MonoBehaviour
    {
        public TypewriterComponent typewriter;
        TextAnimatorSettings settings;

        private void Awake()
        {
            // TODO check if user has databases - but at first launch it should be ok

            StringBuilder builder = new StringBuilder();

            builder.Append("Here are the currently recognized (global) effects: \n\n");

            void OpenTag(ParsingInfo parsing, string tagId)
            {
                builder.Append(parsing.openingBracket);
                if (!char.IsWhiteSpace(parsing.middleSymbol) && parsing.middleSymbol != '\n' && parsing.middleSymbol != TextParser.NONE_CHARACTER)
                    builder.Append(parsing.middleSymbol);
                builder.Append(tagId);
                builder.Append(parsing.closingBracket);
            }

            void CloseTag(ParsingInfo parsing, string tagId)
            {
                builder.Append(parsing.openingBracket);
                builder.Append('/');
                if (!char.IsWhiteSpace(parsing.middleSymbol) && parsing.middleSymbol != '\n' && parsing.middleSymbol != TextParser.NONE_CHARACTER)
                    builder.Append(parsing.middleSymbol);
                builder.Append(tagId);
                builder.Append(parsing.closingBracket);
            }


            var globalSettings = TextAnimatorSettings.Instance.Settings;
            foreach (var effect in globalSettings.GlobalEffectsDatabase.Database)
            {
                string tagId = effect.Key;
                OpenTag(globalSettings.parsingAppearances, tagId);
                OpenTag(globalSettings.parsingBehaviors, tagId);
                OpenTag(globalSettings.parsingDisappearances, tagId);

                builder.Append(tagId);

                CloseTag(globalSettings.parsingAppearances, tagId);
                CloseTag(globalSettings.parsingBehaviors, tagId);
                CloseTag(globalSettings.parsingDisappearances, tagId);

                builder.Append(' ');
            }

            builder.Append("\n\nHave fun customizing them or create yours both from the inspector or c#!");
            typewriter.ShowText(builder.ToString());
            typewriter.StartShowingText();

        }
    }

}