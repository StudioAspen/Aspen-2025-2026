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
    class RandomPositionData
    {
        public float amplitude = 1;
        public bool progressIndexWithTime = false;
    }

    [UnityEngine.Scripting.Preserve]
    [CreateAssetMenu(menuName = ScriptablePaths.EFFECT_STATES_DIRECT+"Random Position", fileName = "Random Position Effect")]
    sealed class RandomPositionEffect : ManagedEffectScriptable<RandomPositionEffectState, RandomPositionData>
    {
        protected override RandomPositionEffectState CreateState(RandomPositionData parameters)
            => new RandomPositionEffectState(parameters.amplitude, parameters.progressIndexWithTime);
    }
}