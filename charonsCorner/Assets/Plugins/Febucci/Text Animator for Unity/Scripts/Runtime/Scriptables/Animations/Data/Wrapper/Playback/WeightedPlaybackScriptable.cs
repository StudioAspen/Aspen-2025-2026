// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore.BuiltIn;
using Febucci.TextAnimatorCore;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity.Effects
{

    [System.Serializable]
    public class WeightedPlaybackWrapper : WeightedPlayback
    {
        [SerializeField, Range(0, 1)] float intensity01;

        protected override float Intensity01 => intensity01;
    }

    [System.Serializable]
    [CreateAssetMenu(menuName = ScriptablePaths.PLAYBACKS_PATH+"Weighted", fileName = "Weighted Playback")]
    public sealed class WeightedPlaybackScriptable : CoreLibraryPlaybackScriptable
    {
        [SerializeField] WeightedPlaybackWrapper playback;
        protected override IEffectPlayback Playback => playback;
    }
}