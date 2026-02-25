// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System.Text;

namespace Febucci.TextAnimatorForUnity.TextMeshPro
{
    class TMPTagParser : Febucci.Parsing.Core.TagParserBase
    {
        readonly bool richTagsEnabled;

        //PS no "noparse" since it's already checked
        static readonly TMPTagInfo[] lookups = new[]
        {
            new TMPTagInfo("<align="), new TMPTagInfo("</align>"),
            new TMPTagInfo("<allcaps>"), new TMPTagInfo("</allcaps>"),
            new TMPTagInfo("<alpha="), new TMPTagInfo("</alpha>"),
            new TMPTagInfo("<b>"), new TMPTagInfo("</b>"),
            new TMPTagInfo("<color="),new TMPTagInfo( "</color>"),
            new TMPTagInfo( "</color="),
            new TMPTagInfo("<cspace="), new TMPTagInfo("</cspace>"),
            new TMPTagInfo("<font="), new TMPTagInfo("</font>"),
            new TMPTagInfo("<font-weight="), new TMPTagInfo("</font-weight>"),
            new TMPTagInfo("<gradient="), new TMPTagInfo("</gradient>"),
            new TMPTagInfo("<i>"),new TMPTagInfo( "</i>"),
            new TMPTagInfo("<indent="),new TMPTagInfo( "</indent>"),
            new TMPTagInfo("<line-height="),new TMPTagInfo( "</line-height>"),
            new TMPTagInfo("<line-indent="),new TMPTagInfo( "</line-indent>"),
            new TMPTagInfo("<link="), new TMPTagInfo("</link>"),
            new TMPTagInfo("<link>"), new TMPTagInfo("</link>"),
            new TMPTagInfo("<lowercase>"), new TMPTagInfo("</lowercase>"),
            new TMPTagInfo("<margin="), new TMPTagInfo( "</margin>"), new TMPTagInfo( "<margin-left>"), new TMPTagInfo( "<margin-right>"),
            new TMPTagInfo("<mark="), new TMPTagInfo( "</mark>"),
            new TMPTagInfo("<mspace="), new TMPTagInfo("</mspace>"),
            new TMPTagInfo("<nobr>"), new TMPTagInfo("</nobr>"),
            new TMPTagInfo("<page>"),
            new TMPTagInfo("<pos="),
            new TMPTagInfo("<rotate="), new TMPTagInfo("</rotate>"),
            new TMPTagInfo("<s>"), new TMPTagInfo("</s>"),
            new TMPTagInfo("<size="), new TMPTagInfo("</size>"),
            new TMPTagInfo("<smallcaps>"), new TMPTagInfo("</smallcaps>"),
            new TMPTagInfo("<space="),
            new TMPTagInfo("<sprite", true), new TMPTagInfo("<sprite ", true),
            new TMPTagInfo("<style="), new TMPTagInfo( "</style>"),
            new TMPTagInfo("<sub>"), new TMPTagInfo( "</sub>"),
            new TMPTagInfo("<sup>"), new TMPTagInfo( "</sup>"),
            new TMPTagInfo("<u>"), new TMPTagInfo( "</u>"),
            new TMPTagInfo("<uppercase>"), new TMPTagInfo( "</uppercase>"),
            new TMPTagInfo("<voffset="), new TMPTagInfo( "</voffset>"),
            new TMPTagInfo("<width="), new TMPTagInfo( "</width>"),
            new TMPTagInfo("<br>", true)
        };

        struct TMPTagInfo
        {
            public readonly string tagOpening;
            public readonly bool increasesTextLength;

            public TMPTagInfo(string tagOpening, bool increasesTextLength = false)
            {
                this.tagOpening = tagOpening;
                this.increasesTextLength = increasesTextLength;
            }
        }

        public TMPTagParser(bool richTagsEnabled, char openingBracket, char closingTagSymbol, char closingBracket)
            : base(openingBracket, closingTagSymbol, closingBracket)
        {
            this.richTagsEnabled = richTagsEnabled;
        }

        public override bool TryProcessingTag(string textInsideBrackets, int tagLength, ref int realTextIndex, StringBuilder finalTextBuilder, int internalOrder)
        {
            if (!richTagsEnabled) return false;

            string fullTag = OpeningBracket + textInsideBrackets + ClosingBracket; //TODO improve

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