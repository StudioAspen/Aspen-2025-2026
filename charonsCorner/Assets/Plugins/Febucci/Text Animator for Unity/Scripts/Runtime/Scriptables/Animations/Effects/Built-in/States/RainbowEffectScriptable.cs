// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore.BuiltIn;

using UnityEngine;

namespace Febucci.TextAnimatorForUnity.Effects
{
    [System.Serializable]
    class RainbowData
    {

    }

    [UnityEngine.Scripting.Preserve]
    [CreateAssetMenu(menuName = ScriptablePaths.EFFECT_STATES_DIRECT+"Rainbow", fileName = "Rainbow Effect")]
    sealed class RainbowEffectScriptable : ManagedEffectScriptable<RainbowColorEffectState, RainbowData>
    {
        protected override RainbowColorEffectState CreateState(RainbowData parameters)
            => new RainbowColorEffectState(true);
    }
}