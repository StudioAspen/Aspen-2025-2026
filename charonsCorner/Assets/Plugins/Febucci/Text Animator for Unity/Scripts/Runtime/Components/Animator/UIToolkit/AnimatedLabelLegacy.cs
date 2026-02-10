// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

/*
#if !UNITY_6000_3_OR_NEWER

#if UNITY_2023_2_OR_NEWER
#define CAN_USE_UXML_DECORATORS
#endif

using System;
using System.Collections.Generic;
using Febucci.TextAnimatorCore.Data;
using Febucci.Parsing;
using Febucci.Parsing.Core;
using Febucci.Parsing.Regions;
using Febucci.TextAnimatorForUnity.Actions;
using UnityEngine;
using UnityEngine.UIElements;
using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorForUnity.Parsing;

// DO NOT EVER CHANGE THIS OR people's project will break
namespace Febucci.TextAnimatorForUnity
{
    #if CAN_USE_UXML_DECORATORS
    [UxmlElement]
    #endif
    public sealed partial class AnimatedLabel : AnimatedLabelBase
    {
        /*

        #region UXML
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<AnimatedLabel, UxmlTraits> { }

        [UnityEngine.Scripting.Preserve]
        public new class UxmlTraits : TextElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription { name = "text", defaultValue = "" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                if(ve is not AnimatedLabel ate) return;

                string text = m_Text.GetValueFromBag(bag, cc);
                ate.UpdateText(text);
            }
        }
        #endregion


        List<VisualElement> wordsChildren;
        Label[] lettersLabels;
        readonly UIToolkitLabelTagParser uiToolkitParser;
        List<TagParserBase> additionalParsers;


        string _text;
        // FOR < UNITY6 Must expose your element class to a { get; set; } property that has the same name
        // as the name you set in your UXML attribute description with the camel case format
        public override string text
        {
            get => _text;
            set => UpdateText(value);
        }


        public AnimatedLabel() : base()
        {
            lettersLabels = Array.Empty<Label>();
            wordsChildren = new List<VisualElement>();


            PrepareLabel(this, 0);

            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            PrepareLabel(this, 0);
            style.paddingBottom = style.paddingLeft = style.paddingRight = style.paddingTop = new StyleLength(StyleKeyword.None);
            style.flexWrap = new StyleEnum<Wrap>(Wrap.Wrap);

            AddToClassList("unity-text-element");
            AddToClassList("unity-label");

            uiToolkitParser = new UIToolkitLabelTagParser
            (true,
                openingBracket:'<',
                closingTagSymbol:'/',
                closingBracket:'>'); // official Unity brackets

            additionalParsers = new List<TagParserBase>() { uiToolkitParser };
        }

        protected override List<TagParserBase> GetAdditionalParsers() => additionalParsers;

        protected override void OnTextChanged(string parsedText)
        {
            // --- clears previous state ---
            foreach (var child in wordsChildren)
            {
                if(!Contains(child))
                    continue;

                Remove(child);
            }

            wordsChildren.Clear();


            // --- creates letters ---
            lettersLabels = new Label[charactersLength];

            VisualElement CreateWordContainer()
            {
                var result = new VisualElement();
                result.style.flexDirection = FlexDirection.Row;
                wordsChildren.Add(result);
                Add(result);
                return result;
            }

            VisualElement wordContainer = CreateWordContainer();

            for (int i = 0; i < charactersLength; i++)
            {
                characters[i].ResetInfo(i, true);
                characters[i].info.character = parsedText[i];

                // one label per character
                string character = characters[i].info.character.ToString();
                var newLabel = new Label(character);

                bool isWhiteSpace = character == " ";
                if (isWhiteSpace)
                {
                    // creates new
                    wordContainer = CreateWordContainer();
                }

                PrepareLabel(newLabel, 0);

                foreach (var result in uiToolkitParser.results)
                    character = result.data.EncapsulateText(character, i, result);
                newLabel.text = character;

                wordContainer.Add(newLabel);
                lettersLabels[i] = newLabel;

                characters[i].source.transform.position = newLabel.transform.position;
                characters[i].source.transform.scale = newLabel.transform.scale;
                characters[i].source.transform.rotation = newLabel.transform.rotation;
                characters[i].source.color = newLabel.style.color.keyword == StyleKeyword.None || newLabel.style.color.keyword == StyleKeyword.Null? Color.black : newLabel.style.color.value;
                if (isWhiteSpace)
                {
                    wordContainer = CreateWordContainer(); // space has its own
                }
            }

            isReadyForAnimation = true;
        }

        void ApplyStylesFromSource(IStyle sourceStyle, VisualElement targetElement)
        {
            // Ensure both source and target are not null
            if (sourceStyle == null || targetElement == null) return;

            // The target's style interface
            IStyle targetStyle = targetElement.style;

            #region Flexbox

            targetStyle.alignContent = sourceStyle.alignContent;
            targetStyle.alignItems = sourceStyle.alignItems;
            targetStyle.alignSelf = sourceStyle.alignSelf;
            targetStyle.flexBasis = sourceStyle.flexBasis;
            targetStyle.flexDirection = sourceStyle.flexDirection;
            targetStyle.flexGrow = sourceStyle.flexGrow;
            targetStyle.flexShrink = sourceStyle.flexShrink;
            targetStyle.flexWrap = sourceStyle.flexWrap;
            targetStyle.justifyContent = sourceStyle.justifyContent;

            #endregion

            #region Sizing

            targetStyle.width = sourceStyle.width;
            targetStyle.height = sourceStyle.height;
            targetStyle.minWidth = sourceStyle.minWidth;
            targetStyle.minHeight = sourceStyle.minHeight;
            targetStyle.maxWidth = sourceStyle.maxWidth;
            targetStyle.maxHeight = sourceStyle.maxHeight;

            #endregion

            #region Margin & Padding

            targetStyle.marginLeft = sourceStyle.marginLeft;
            targetStyle.marginTop = sourceStyle.marginTop;
            targetStyle.marginRight = sourceStyle.marginRight;
            targetStyle.marginBottom = sourceStyle.marginBottom;

            targetStyle.paddingLeft = sourceStyle.paddingLeft;
            targetStyle.paddingTop = sourceStyle.paddingTop;
            targetStyle.paddingRight = sourceStyle.paddingRight;
            targetStyle.paddingBottom = sourceStyle.paddingBottom;

            #endregion

            #region Positioning

            targetStyle.position = sourceStyle.position;
            targetStyle.left = sourceStyle.left;
            targetStyle.top = sourceStyle.top;
            targetStyle.right = sourceStyle.right;
            targetStyle.bottom = sourceStyle.bottom;

            #endregion

            #region Background

            targetStyle.backgroundColor = sourceStyle.backgroundColor;
            targetStyle.backgroundImage = sourceStyle.backgroundImage;
            targetStyle.backgroundPositionX = sourceStyle.backgroundPositionX;
            targetStyle.backgroundPositionY = sourceStyle.backgroundPositionY;
            targetStyle.backgroundRepeat = sourceStyle.backgroundRepeat;
            targetStyle.backgroundSize = sourceStyle.backgroundSize;

            #endregion

            #region Border

            targetStyle.borderTopColor = sourceStyle.borderTopColor;
            targetStyle.borderRightColor = sourceStyle.borderRightColor;
            targetStyle.borderBottomColor = sourceStyle.borderBottomColor;
            targetStyle.borderLeftColor = sourceStyle.borderLeftColor;

            targetStyle.borderTopWidth = sourceStyle.borderTopWidth;
            targetStyle.borderRightWidth = sourceStyle.borderRightWidth;
            targetStyle.borderBottomWidth = sourceStyle.borderBottomWidth;
            targetStyle.borderLeftWidth = sourceStyle.borderLeftWidth;

            targetStyle.borderTopLeftRadius = sourceStyle.borderTopLeftRadius;
            targetStyle.borderTopRightRadius = sourceStyle.borderTopRightRadius;
            targetStyle.borderBottomLeftRadius = sourceStyle.borderBottomLeftRadius;
            targetStyle.borderBottomRightRadius = sourceStyle.borderBottomRightRadius;

            #endregion

            #region Text

            if (targetElement is TextElement)
            {
                targetStyle.color = sourceStyle.color;
                targetStyle.fontSize = sourceStyle.fontSize;
                targetStyle.unityFont = sourceStyle.unityFont;
                targetStyle.unityFontDefinition = sourceStyle.unityFontDefinition;
                targetStyle.unityFontStyleAndWeight = sourceStyle.unityFontStyleAndWeight;
                targetStyle.unityParagraphSpacing = sourceStyle.unityParagraphSpacing;
                targetStyle.unityTextAlign = sourceStyle.unityTextAlign;
                targetStyle.unityTextOutlineColor = sourceStyle.unityTextOutlineColor;
                targetStyle.unityTextOutlineWidth = sourceStyle.unityTextOutlineWidth;
                targetStyle.textOverflow = sourceStyle.textOverflow;
                targetStyle.whiteSpace = sourceStyle.whiteSpace;
            }

            #endregion

            #region Visibility & Interaction

            targetStyle.display = sourceStyle.display;
            targetStyle.overflow = sourceStyle.overflow;
            targetStyle.visibility = sourceStyle.visibility;
            targetStyle.opacity = sourceStyle.opacity;
            targetStyle.cursor = sourceStyle.cursor;

            #endregion

            #region Transform

            targetStyle.rotate = sourceStyle.rotate;
            targetStyle.scale = sourceStyle.scale;
            targetStyle.transformOrigin = sourceStyle.transformOrigin;
            targetStyle.translate = sourceStyle.translate;

            #endregion
        }

        void PrepareLabel(VisualElement element, float margin)
        {
            ApplyStylesFromSource(style, element);

            element.style.marginBottom
                = element.style.marginLeft
                    = element.style.marginRight
                        = element.style.marginTop
                            = margin;


            element.style.paddingBottom
                = element.style.paddingLeft
                    = element.style.paddingRight
                        = element.style.paddingTop
                            = margin;
        }

        protected override void OnCharacterProcessed(int i, CharacterData character)
        {
            base.OnCharacterProcessed(i, character);

            var label = lettersLabels[i];
            var pos = character.current.transform.position;
            pos.y *= -1; // UI tk has up = -1, but effects were initially made
            // for TMPro which is Legacy UI -> up = 1
            label.transform.position = pos;
            label.transform.rotation = character.current.transform.rotation;
            label.transform.scale = character.current.transform.scale;
            label.style.color = character.current.color;
        }

        // <---here
        public override void CopyMeshFromSource(ref CharacterData[] characters, int charactersCount)
        {
            throw new NotImplementedException();
        }
        public override void PasteMeshToSource(CharacterData[] characters, int charactersCount)
        {
            throw new NotImplementedException();
        }
        public override string GetStrippedTextWithoutAnyTags()
        {
            throw new NotImplementedException();
        }
        public override string GetFullText()
        {
            throw new NotImplementedException();
        }
        public override int GetCharactersCount()
        {
            throw new NotImplementedException();
        }
        public override bool HasChangedMeshRenderingSettings()
        {
            throw new NotImplementedException();
        }
    }
}
#endif
*/