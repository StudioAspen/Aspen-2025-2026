// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System.Text;
using UnityEngine.TextCore.Text;
using TextElement = UnityEngine.UIElements.TextElement;

namespace Febucci.TextAnimatorForUnity.Parsing
{
    class UIToolkitLabelTagParser : Febucci.Parsing.Core.TagParserBase
    {
        readonly UnityEngine.UIElements.TextElement attachedTextElement;

        static readonly UITkTagInfo[] lookups = new[]
        {
            new UITkTagInfo("<a"), new UITkTagInfo("</a>"),
            new UITkTagInfo("<align="), new UITkTagInfo("</align>"),
            new UITkTagInfo("<allcaps>"), new UITkTagInfo("</allcaps>"),
            new UITkTagInfo("<alpha="), new UITkTagInfo("</alpha>"),
            new UITkTagInfo("<b>"), new UITkTagInfo("</b>"),
            new UITkTagInfo("<color="), new UITkTagInfo("</color>"),
            new UITkTagInfo("</color="),
            new UITkTagInfo("<cspace="), new UITkTagInfo("</cspace>"),
            new UITkTagInfo("<font="), new UITkTagInfo("</font>"),
            new UITkTagInfo("<font-weight="), new UITkTagInfo("</font-weight>"),
            new UITkTagInfo("<gradient="), new UITkTagInfo("</gradient>"),
            new UITkTagInfo("<i>"), new UITkTagInfo("</i>"),
            new UITkTagInfo("<indent="), new UITkTagInfo("</indent>"),
            new UITkTagInfo("<line-height="), new UITkTagInfo("</line-height>"),
            new UITkTagInfo("<line-indent="), new UITkTagInfo("</line-indent>"),
            new UITkTagInfo("<link="), new UITkTagInfo("</link>"),
            new UITkTagInfo("<link>"), new UITkTagInfo("</link>"),
            new UITkTagInfo("<lowercase>"), new UITkTagInfo("</lowercase>"),
            new UITkTagInfo("<margin="), new UITkTagInfo("</margin>"),
            new UITkTagInfo("<margin-left="), new UITkTagInfo("<margin-right="),
            new UITkTagInfo("<mark="), new UITkTagInfo("</mark>"),
            new UITkTagInfo("<mspace="), new UITkTagInfo("</mspace>"),
            new UITkTagInfo("<nobr>"), new UITkTagInfo("</nobr>"),
            new UITkTagInfo("<noparse>"), new UITkTagInfo("</noparse>"),
            new UITkTagInfo("<pos="),
            new UITkTagInfo("<rotate="), new UITkTagInfo("</rotate>"),
            new UITkTagInfo("<s>"), new UITkTagInfo("</s>"),
            new UITkTagInfo("<size="), new UITkTagInfo("</size>"),
            new UITkTagInfo("<smallcaps>"), new UITkTagInfo("</smallcaps>"),
            new UITkTagInfo("<space=", true),
            new UITkTagInfo("<sprite", true), new UITkTagInfo("<sprite ", true),
            new UITkTagInfo("<style="), new UITkTagInfo("</style>"),
            new UITkTagInfo("<sub>"), new UITkTagInfo("</sub>"),
            new UITkTagInfo("<sup>"), new UITkTagInfo("</sup>"),
            new UITkTagInfo("<u>"), new UITkTagInfo("</u>"),
            new UITkTagInfo("<uppercase>"), new UITkTagInfo("</uppercase>"),
            new UITkTagInfo("<voffset="), new UITkTagInfo("</voffset>"),
            new UITkTagInfo("<width="), new UITkTagInfo("</width>"),
            new UITkTagInfo("<br>", true)
        };

        public UIToolkitLabelTagParser(TextElement attachedTextElement, char openingBracket, char closingTagSymbol, char closingBracket)
            : base(openingBracket, closingTagSymbol, closingBracket)
        {
            this.attachedTextElement = attachedTextElement;
        }

        public override bool TryProcessingTag(string textInsideBrackets, int tagLength, ref int realTextIndex, StringBuilder finalTextBuilder, int internalOrder)
        {
            // cannot optimize 'cos it might be made null by the user :/
            if (attachedTextElement == null) return false;
            if (!attachedTextElement.enableRichText) return false;

            string fullTag = OpeningBracket + textInsideBrackets + ClosingBracket;

            foreach (var lookupTag in lookups)
            {
                if (fullTag.StartsWith(lookupTag.tagOpening, true, System.Globalization.CultureInfo.InvariantCulture))
                {
                    finalTextBuilder.Append(fullTag);
                    if (lookupTag.increasesTextLength) realTextIndex++;
                    return true;
                }
            }

            return false;
        }
    }
}