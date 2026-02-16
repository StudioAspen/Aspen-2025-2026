// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore.Data;
using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorForUnity.Parsing;
using Febucci.TextAnimatorForUnity;
using UnityEngine.UIElements;
using System;
using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorCore.Settings;
using Febucci.Parsing.Core;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Integrations.UIToolkit.Scripts.Runtime
{
    /*
    // NOT PUBLIC YET
    internal class TextElementAnimator : ITextGenerator, ISettingsProvider, IDisposable
    {
        TextElement textElement;
        bool isValid;
        TextAnimator animator;
        readonly UIToolkitLabelTagParser uiToolkitLabelTagParser;

        AnimatorSettings fallbackSettings = AnimatorSettings.CreateDefault();
        public AnimatorSettingsScriptable settings;
        public AnimatorSettings Settings => settings ? settings.Settings : fallbackSettings;
        AnimationsDatabase effectsDatabase;
        public AnimationsDatabase AnimationsDatabase
        {
            get => effectsDatabase;
            set
            {
                effectsDatabase = value;
                animator.effectsDatabase = value;
                RefreshText();
            }
        }


        public TextElementAnimator(TextElement element)
        {
            if (element == null)
            {
                Debug.LogError("Text Element is null. Skipping");
                return;
            }

            this.textElement = element;
            try
            {
                uiToolkitLabelTagParser = new UIToolkitLabelTagParser(element, '<', '/', '>');
                animator = new TextAnimator(
                    false,
                    '<',
                    '>',
                    '/',
                    UnityEngineProvider.Instance,
                    settingsProvider: this,
                    textGenerator: this,
                    caller: this,
                    isUpPositive: false, //UITk has y = down,
                    extraParsers: new TagParserBase[] { uiToolkitLabelTagParser }
                );
                isValid = true;
            }
            catch (Exception e)
            {
                Debug.LogError(
                    "Something went wrong initializing TextAnimator for UI Toolkit. Please make sure the setup is correct, or contact support with the stack trace and we'll fix it as soon as possible. Thanks!");
                throw;
            }

            animator.effectsDatabase = effectsDatabase;
            textElement.PostProcessTextVertices += OnPostProcessTextVertices;

            element.RegisterValueChangedCallback((x) =>
            {
                if(isUpdatingText) return;
                UpdateText(x.newValue);
            });

            element.RegisterCallback<AttachToPanelEvent>(x => RegisterAnimationCallback());
            element.RegisterCallback<DetachFromPanelEvent>(x => ClearAnimationCallback());

            UpdateText(element.text);
        }

        void RegisterAnimationCallback()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= Animate;
            UnityEditor.EditorApplication.update += Animate;
            #endif

            lastTime = GetCurrentTime();
        }

        float GetCurrentTime()
        {
            #if UNITY_EDITOR
            if(!Application.isPlaying)
                return (float)UnityEditor.EditorApplication.timeSinceStartup;
            #endif

            return Time.time;
        }

        void ClearAnimationCallback()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= Animate;
            #endif
        }


        public void RefreshText() => animator.SetText(animator.TextFull);

        bool isUpdatingText = false;
        void UpdateText(string newValue)
        {
            isUpdatingText = true;
            animator.SetText(newValue);
            Debug.Log($"Setting text: {newValue}");
            isUpdatingText = false;
        }

        float lastTime = 0;

        public void Animate()
        {
            float currentTime = GetCurrentTime();
            float delta = currentTime - lastTime;
            lastTime = currentTime;
            animator.Animate(delta);
        }

        public void SetTextToSource(string text)
        {
            textElement.text = text;
        }

        public void ForceMeshUpdate()
        {
            shouldBeCopied = true;
            textElement.schedule.Execute(() => textElement.MarkDirtyText());
        }

        public int GetFirstCharacterIndexInsidePage() => 0;
        public int GetRenderedCharactersCountInsidePage(int charactersCount) => int.MaxValue;

        MeshData[] sources = new MeshData[0];
        MeshData[] currents = new MeshData[0];

        int targetCharacters = 0;
        int currentCapacity = 0;

        bool shouldBeCopied = true;
        const int CAPACITY_GROWTH_FACTOR = 2;

        public string GetStrippedTextWithoutAnyTags() => textElement.text;
        public string GetFullText() => animator.TextWithoutTextAnimatorTags;

        public int GetCharactersCount() => textElement.text.Length;

        public void CopyMeshFromSource(ref CharacterData[] characters, int charactersCount)
        {
            targetCharacters = charactersCount;
            shouldBeCopied = true;
            textElement.schedule.Execute(() => textElement.MarkDirtyText());
        }


        void CopyGlyphs(TextElement.GlyphsEnumerable glyphs)
        {
            if (targetCharacters > currentCapacity)
            {
                int newCapacity = Math.Max(targetCharacters, currentCapacity * CAPACITY_GROWTH_FACTOR);
                int initial = sources.Length;
                Array.Resize(ref sources, newCapacity);
                for (int i = initial; i < newCapacity; i++)
                    sources[i] = new MeshData(true);
                currentCapacity = newCapacity;
            }

            int charIndex = 0;
            float fontSize = textElement.resolvedStyle.fontSize * 10;
            foreach (var glyph in glyphs)
            {
                if (charIndex >= targetCharacters) break;

                ref var charTran = ref sources[charIndex];
                var verts = glyph.vertices;

                unsafe
                {
                    // only way to prevent 9325829135823985 calls and buffer.memcpy etc.
                    Vertex* vertPtr = (Vertex*)verts.GetUnsafeReadOnlyPtr();
                    for (int i = 0; i < CharacterData.VERTICES_PER_CHAR; i++)
                    {
                        ref var pos = ref charTran.positions[i];
                        pos.X = vertPtr[i].position.x;
                        pos.Y = vertPtr[i].position.y;
                        pos.Z = vertPtr[i].position.z;

                        ref var col = ref charTran.colors[i];
                        col.R = vertPtr[i].tint.r;
                        col.G = vertPtr[i].tint.g;
                        col.B = vertPtr[i].tint.b;
                        col.A = vertPtr[i].tint.a;
                    }
                }

                ref var copy = ref animator.Characters[charIndex];
                ref var copySource = ref copy.source;

                for (int i = 0; i < CharacterData.VERTICES_PER_CHAR; i++)
                {
                    ref var targetPos = ref copySource.positions[i];
                    ref var sourcePos = ref charTran.positions[i];
                    targetPos.X = sourcePos.X;
                    targetPos.Y = sourcePos.Y;
                    targetPos.Z = sourcePos.Z;

                    ref var targetCol = ref copySource.colors[i];
                    ref var sourceCol = ref charTran.colors[i];
                    targetCol.R = sourceCol.R;
                    targetCol.G = sourceCol.G;
                    targetCol.B = sourceCol.B;
                    targetCol.A = sourceCol.A;
                }

                ref var charInfo = ref copy.info;
                charInfo.isRendered = true;
                charInfo.pointSize = fontSize;
                charInfo.character = textElement.text[charIndex];

                copy.info = charInfo;

                charIndex++;
            }
        }


        public void PasteMeshToSource(CharacterData[] characters, int charactersCount)
        {
            if (shouldBeCopied) return;

            if (currents.Length < currentCapacity)
            {
                int initial = currents.Length;
                Array.Resize(ref currents, currentCapacity);
                for (int i = initial; i < currentCapacity; i++)
                    currents[i] = new MeshData(true);
            }

            int count = Math.Min(charactersCount, currentCapacity);
            for (int i = 0; i < count; i++)
            {
                ref var target = ref currents[i];
                ref var source = ref characters[i].current;

                for (int j = 0; j < CharacterData.VERTICES_PER_CHAR; j++)
                {
                    ref var targetPos = ref target.positions[j];
                    ref var sourcePos = ref source.positions[j];
                    targetPos.X = sourcePos.X;
                    targetPos.Y = sourcePos.Y;
                    targetPos.Z = sourcePos.Z;

                    ref var targetCol = ref target.colors[j];
                    ref var sourceCol = ref source.colors[j];
                    targetCol.R = sourceCol.R;
                    targetCol.G = sourceCol.G;
                    targetCol.B = sourceCol.B;
                    targetCol.A = sourceCol.A;
                }
            }

            textElement.schedule.Execute(() => textElement.MarkDirtyRepaint());
        }


        // copy TANIM -> copy glyph || paste tanim -> paste glyph

        void PasteGlyphs(TextElement.GlyphsEnumerable glyphs)
        {
            if (currents.Length == 0) return;

            int charIndex = 0;

            foreach (var glyph in glyphs)
            {
                if (charIndex >= currents.Length) break;

                ref var animChar = ref currents[charIndex];

                var verts = glyph.vertices;
                int vertCount = Math.Min(verts.Length, CharacterData.VERTICES_PER_CHAR);

                // only way to prevent 9325829135823985 calls and buffer.memcpy etc.
                unsafe
                {
                    Vertex* vertPtr = (Vertex*)verts.GetUnsafePtr();
                    for (int i = 0; i < vertCount; i++)
                    {
                        ref var sourcePos = ref animChar.positions[i];
                        vertPtr[i].position.x = sourcePos.X;
                        vertPtr[i].position.y = sourcePos.Y;
                        vertPtr[i].position.z = sourcePos.Z;

                        ref var sourceCol = ref animChar.colors[i];
                        vertPtr[i].tint.r = sourceCol.R;
                        vertPtr[i].tint.g = sourceCol.G;
                        vertPtr[i].tint.b = sourceCol.B;
                        vertPtr[i].tint.a = sourceCol.A;
                    }
                }

                charIndex++;
            }
        }

        void OnPostProcessTextVertices(TextElement.GlyphsEnumerable glyphs)
        {
            if(!isValid) return;
            if (string.IsNullOrEmpty(textElement.text) || targetCharacters == 0) return;

            if (shouldBeCopied)
            {
                CopyGlyphs(glyphs);
                shouldBeCopied = false;
            }

            PasteGlyphs(glyphs);
        }

        public bool HasChangedMeshRenderingSettings() => true;
        public void Dispose()
        {
            animator?.Dispose();
            if(textElement == null) return;
            textElement.PostProcessTextVertices -= OnPostProcessTextVertices;
        }
    }*/
}