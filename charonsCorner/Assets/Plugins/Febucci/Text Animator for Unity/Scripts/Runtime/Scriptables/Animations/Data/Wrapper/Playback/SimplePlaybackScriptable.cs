// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorCore.BuiltIn;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity.Effects
{
    [System.Serializable]
    [CreateAssetMenu(menuName = ScriptablePaths.PLAYBACKS_PATH+"Simple", fileName = "Simple Playback")]
    sealed class SimplePlaybackScriptable : CoreLibraryPlaybackScriptable
    {
        [SerializeField] SimplePlayback playback;

        protected override IEffectPlayback Playback => playback;
    }
}