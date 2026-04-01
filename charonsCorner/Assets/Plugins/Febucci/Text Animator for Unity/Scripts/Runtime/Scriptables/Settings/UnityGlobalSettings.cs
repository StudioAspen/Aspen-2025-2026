// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorCore.Data;
using Febucci.TextAnimatorCore.Settings;
using Febucci.TextAnimatorCore.Styles;
using Febucci.TextAnimatorCore.Typing;
using Febucci.TextAnimatorForUnity.Actions;
using Febucci.TextAnimatorForUnity.Effects;
using Febucci.TextAnimatorForUnity.Styles;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity
{
    [System.Serializable]
    public class UnityGlobalSettings : GlobalSettingsBase
    {
        // PSA fields must have the same name of the interface implementation,
        // but with the first character lowercase,
        // otherwise the custom inspector will break

        [SerializeField] EffectPlaybackScriptableBase fallbackPlayback;
        public override IEffectPlayback FallbackPlayback => fallbackPlayback;

        [SerializeField] EffectCurveScriptableBase fallbackStateCurve;
        public override IEffectCurve FallbackStateCurve => fallbackStateCurve;

        [SerializeField] AnimationsDatabase globalEffectsDatabase;
        public override IDatabaseProvider<IEffect> GlobalEffectsDatabase => globalEffectsDatabase;

        [SerializeField] ActionDatabase globalActionsDatabase;
        public override IDatabaseProvider<ITypewriterAction> GlobalActionsDatabase => globalActionsDatabase;

        [SerializeField] StyleSheetScriptable globalStyleSheet;
        public override IDatabaseProvider<Style> GlobalStyleSheet => globalStyleSheet;


        [SerializeField] PlaybacksDatabase globalPlaybacksDatabase;
        public override IDatabaseProvider<IEffectPlayback> GlobalPlaybacksDatabase => globalPlaybacksDatabase;
    }
}