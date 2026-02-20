// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================


namespace Febucci.TextAnimatorForUnity.Parsing
{
    struct UITkTagInfo
    {
        public readonly string tagOpening;
        public readonly bool increasesTextLength;

        public UITkTagInfo(string tagOpening, bool increasesTextLength = false)
        {
            this.tagOpening = tagOpening;
            this.increasesTextLength = increasesTextLength;
        }
    }
}
