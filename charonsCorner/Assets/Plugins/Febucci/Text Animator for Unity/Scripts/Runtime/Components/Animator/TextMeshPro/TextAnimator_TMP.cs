// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore.Text;
using TMPro;
using UnityEngine;
using Febucci.Parsing.Core;

namespace Febucci.TextAnimatorForUnity.TextMeshPro
{
    /// <summary>
    /// Animates a TMP text component, both UI or World.
    /// See <see cref="TextAnimatorComponentBase"/> for the base class information.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("Febucci/TextAnimator/Text Animator - Text Mesh Pro")]
    // ReSharper disable once InconsistentNaming
    public sealed class TextAnimator_TMP : TextAnimatorComponentBase
    {
        /// <summary>
        /// The TextMeshPro text component linked to this Text Animator
        /// </summary>
        public TMP_Text TMProComponent
        {
            get
            {
                if (tmpComponent) return tmpComponent;
                CacheComponentsOnce();
                return tmpComponent;
            }
        }

        TMP_Text tmpComponent;
        TMP_InputField inputField;
        TMProTextProvider generator;


        bool componentsCached;
        bool isUI;
        void CacheComponentsOnce()
        {
            if(componentsCached) return;

            if (!gameObject.TryGetComponent(out tmpComponent))
            {
                Debug.LogError($"TextAnimator_TMP {name} requires a TMP_Text component to work.", gameObject);
            }

            gameObject.TryGetComponent(out inputField);
            componentsCached = true;
            isUI = tmpComponent is TextMeshProUGUI;
            generator = new TMProTextProvider(tmpComponent, inputField);
        }

        protected override bool IsReady() => componentsCached && (!isUI || tmpComponent.canvas);

        protected override ITextGenerator GetTextGenerator()
        {
            CacheComponentsOnce();
            return generator;
        }

        protected override bool IsUpPositive() => true;

        protected override TagParserBase[] GetExtraParsers()
        {
            return new TagParserBase[]
            {
                // TPro tag provider
                new TMPTagParser(tmpComponent.richText, '<', '/', '>')
            };
        }

    }
}