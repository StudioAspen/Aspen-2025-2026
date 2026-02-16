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
    class PositionData
    {
        [SerializeField] public Vector3 direction = Vector3.up;
        [SerializeField] public float amplitude = 1;
    }

    [UnityEngine.Scripting.Preserve]
    [CreateAssetMenu(menuName = ScriptablePaths.EFFECT_STATES_DIRECT+"Position", fileName = "Position Effect")]
    sealed class PositionEffectScriptable : ManagedEffectScriptable<PositionEffectState, PositionData>
    {
        protected override PositionEffectState CreateState(PositionData parameters)
            => new PositionEffectState(parameters.direction * parameters.amplitude);
    }
}