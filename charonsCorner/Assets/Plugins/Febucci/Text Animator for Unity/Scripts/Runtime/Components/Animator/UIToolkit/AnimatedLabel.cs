// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

#if UNITY_6000_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorCore.Data;
using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorForUnity.UIToolkit;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;

// DO NOT EVER CHANGE THIS OR people's project will break
namespace Febucci.TextAnimatorForUnity // direct namespace for the UITK builder
{
    [UxmlElement]
    public sealed partial class AnimatedLabel : AnimatedLabelBase
    {
        MeshData[] sources = new MeshData[0];
        MeshData[] currents = new MeshData[0];

        int targetCharacters = 0;
        int currentCapacity = 0;

        bool shouldBeCopied = true;
        const int targetHz = 60;
        const int CAPACITY_GROWTH_FACTOR = 2;
        IVisualElementScheduledItem animationJob;

        [UxmlAttribute]
        public bool EnableInEditMode = false;

        public AnimatedLabel() : base()
        {
            shouldBeCopied = true;
            PostProcessTextVertices += OnPostProcessTextVertices;
            schedule.Execute(AnimateLabelSafe);

            animationJob = schedule.Execute(Animate).Every(1000 / targetHz);
            animationJob.Resume();
        }

        void AnimateLabelSafe()
        {
            if(!Application.isPlaying && !EnableInEditMode)
                return;

            MarkDirtyRepaint();
        }

        [UxmlAttribute]
        public bool IsGeometryUpdated = true;

        public override string GetFullText() => text;

        public override int GetCharactersCount() => animator.TextWithoutAnyTag.Length;


        protected override void OnTextParsed()
        {
            base.OnTextParsed();
        }

        public override void CopyMeshFromSource(ref CharacterData[] characters, int charactersCount)
        {
            targetCharacters = charactersCount;
            shouldBeCopied = true;

            if(!onSyncedContext) schedule.Execute(AnimateLabelSafe);
        }

        void CopyGlyphs(TextElement.GlyphsEnumerable glyphs, bool hasAdvancedTextGeneration)
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
            int glyphIndex = 0;
            float fontSize = resolvedStyle.fontSize;
            float lastY = 0;
            float heightThreshold = 0;

            // Skip leading whitespace/newlines before first glyph
            while (charIndex < targetCharacters && char.IsWhiteSpace(text[charIndex]))
            {
                charIndex++;
            }

            foreach (var glyph in glyphs)
            {
                float currentY = glyph.vertices[0].position.y;
                SetupCharIndexUITkShenanigans(ref charIndex,
                    glyphIndex,
                    hasAdvancedTextGeneration,
                    currentY,
                    lastY,
                    heightThreshold);

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
                charInfo.pointSize = fontSize;
                charInfo.character = animator.TextWithoutAnyTag[charIndex];
                charInfo.isRendered = true;

                copy.info = charInfo;

                if (charIndex == 0)
                {
                    lastY = glyph.vertices[0].position.y;
                    heightThreshold = Mathf.Abs((glyph.vertices[1].position.y - lastY)/2f);
                    //Logger.Debug($"Height threshold: {heightThreshold}");
                }
                else
                {
                    // went on a new line
                    if (!ShouldSkipCharacter(copy.info.character)
                        && Mathf.Abs(glyph.vertices[0].position.y - lastY) >= heightThreshold)
                    {
                        //Logger.Debug($"Character '{copy.info.character}' at index {charIndex} went on a new line."
                        //    + $" Position: {copy.source.positions[0].Y} instead of {lastY}");
                        lastY = copy.source.positions[0].Y;
                    }
                }

                charIndex++;
                glyphIndex++;

            }
        }


        public override void PasteMeshToSource(CharacterData[] characters, int charactersCount)
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

            if(!onSyncedContext) schedule.Execute(MarkDirtyRepaint);
        }

        bool onSyncedContext;
        // copy TANIM -> copy glyph || paste tanim -> paste glyph

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool ShouldSkipCharacter(char character)
            => char.IsWhiteSpace(character);

        void SetupCharIndexUITkShenanigans(ref int charIndex, int glyphIndex, bool hasAdvancedTextGeneration,
            float currentY, float lastY, float heightThreshold)
        {
            if (!hasAdvancedTextGeneration)
            {
                // UITK provides ONLY glyphs for rendered words, so we need to skip spaces
                while (charIndex < CharactersCountWithoutTAnimTags
                       && ShouldSkipCharacter(text[charIndex]))
                {
                    charIndex++;
                }
            }
            else // Advanced text generation mode
            {

                // PS UIToolkit doesn't generate glyphs for newlines, trailing spaces, or leading spaces after newlines
                // DDDDDD:

                // skips trailing spaces before newlines
                while (charIndex < targetCharacters && text[charIndex] == ' ')
                {
                    int nextNonSpace = charIndex;
                    while (nextNonSpace < targetCharacters && text[nextNonSpace] == ' ')
                        nextNonSpace++;

                    if (nextNonSpace < targetCharacters && text[nextNonSpace] == '\n')
                    {
                        charIndex = nextNonSpace; // Jump to newline position
                        break;
                    }

                    // not before newline - check if wrapped space
                    if (glyphIndex > 0 && Mathf.Abs(currentY - lastY) >= heightThreshold)
                    {
                        charIndex++;
                    }
                    else
                    {
                        break; // Regular space with glyph
                    }
                }

                // skip all consecutive newlines
                bool foundNewline = false;
                while (charIndex < targetCharacters && text[charIndex] == '\n')
                {
                    charIndex++;
                    foundNewline = true;
                }

                // skip leading spaces after newlines
                if (foundNewline)
                {
                    while (charIndex < targetCharacters && text[charIndex] == ' ')
                    {
                        charIndex++;
                    }
                }
            }
        }


        void PasteGlyphs(TextElement.GlyphsEnumerable glyphs , bool hasAdvancedTextGeneration)
        {
            if (currents.Length == 0) return;

            int charIndex = 0;
            int glyphIndex = 0;
            float lastY = 0;
            float heightThreshold = 0;

            // Skip leading whitespace/newlines before first glyph
            while (charIndex < currents.Length && charIndex < CharactersCountWithoutTAnimTags && char.IsWhiteSpace(text[charIndex]))
            {
                charIndex++;
            }

            //Logger.Debug($"Glyphs: {glyphs.Count} - Characters: {CharactersCount}");

            foreach (var glyph in glyphs)
            {
                float currentY = glyph.vertices[0].position.y;

                if (glyphIndex == 0)
                {
                    lastY = currentY;
                    heightThreshold = Mathf.Abs((glyph.vertices[1].position.y - lastY) / 2f);
                }

                SetupCharIndexUITkShenanigans(ref charIndex, glyphIndex, hasAdvancedTextGeneration, currentY, lastY, heightThreshold);

                if (charIndex >= currents.Length) break;

                ref var animChar = ref currents[charIndex];
                //Logger.Log($"Pasting: {charIndex} - {animator.Characters[charIndex].info.character}");

                lastY = currentY;

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
                glyphIndex++;
            }
        }

        int latestPostProcessFrame = -1;
        double latestPostProcessTime = -1;
        void OnPostProcessTextVertices(TextElement.GlyphsEnumerable glyphs)
        {
            if(!isValid) return;
            if(!IsGeometryUpdated) return;
            if(!Application.isPlaying && !EnableInEditMode) return;
            if (string.IsNullOrEmpty(text) || targetCharacters == 0) return;

            #if UNITY_EDITOR
            if (!Application.isPlaying && EnableInEditMode)
            {
                double currentTime = UnityEditor.EditorApplication.timeSinceStartup;
                if (System.Math.Abs(currentTime - latestPostProcessTime) < 0.001) return;
                latestPostProcessTime = currentTime;
            }
            else
            #endif
            {
                int currentFrame = Time.frameCount;
                if(currentFrame == latestPostProcessFrame) return;
                latestPostProcessFrame = currentFrame;
            }

            onSyncedContext = true;
            Animate();

            bool hasAdvancedTextGeneration = resolvedStyle.unityTextGenerator == TextGeneratorType.Advanced;
            if (shouldBeCopied)
            {
                CopyGlyphs(glyphs, hasAdvancedTextGeneration);
                shouldBeCopied = false;
            }

            PasteGlyphs(glyphs, hasAdvancedTextGeneration);
            onSyncedContext = false;
        }

        public override void ForceMeshUpdate()
        {
            base.ForceMeshUpdate();
            shouldBeCopied = true;
        }

        public override bool HasChangedMeshRenderingSettings() => true;

        public void SetEffectsDatabase(IDatabaseProvider<IEffect> database)
        {
            if (animator != null)
            {
                animator.effectsDatabase = database;
            }
        }
    }
}

#endif