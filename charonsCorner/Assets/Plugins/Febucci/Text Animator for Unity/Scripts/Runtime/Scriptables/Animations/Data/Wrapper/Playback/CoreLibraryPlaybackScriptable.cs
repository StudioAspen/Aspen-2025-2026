// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System;
using Febucci.TextAnimatorCore;

using Febucci.Parsing;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity.Effects
{
    [System.Serializable]
    public abstract class CoreLibraryPlaybackScriptable : EffectPlaybackScriptableBase
    {
        [SerializeField] string tagID;
        public override string TagID => tagID;

        IEffectPlayback playback;
        protected abstract IEffectPlayback Playback { get; }

        public override void UpdateParameters(RegionParameters parameters) => playback?.UpdateParameters(parameters);

        void OnEnable()
        {
        }

        public override void Initialize()
        {
            playback = Playback; // gets once

            if (playback == null)
                throw new NullReferenceException($"Playback is null in {name}");

            playback.Initialize();
        }

        public override float GetTotalDuration() => playback?.GetTotalDuration() ?? 0;
        public override void CalculateIntensity01(float time, out float intensity, out bool hasFinishedEffect)
        {
            if (playback != null)
                playback.CalculateIntensity01(time, out intensity, out hasFinishedEffect);
            else
            {
                intensity = 0;
                hasFinishedEffect = true;
            }
        }
        public override IEffectPhase Phase { get; }
    }

}