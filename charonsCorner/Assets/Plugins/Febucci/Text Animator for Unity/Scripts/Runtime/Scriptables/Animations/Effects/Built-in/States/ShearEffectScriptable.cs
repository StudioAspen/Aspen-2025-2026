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
    class ShearData
    {
        public HorizontalShearType horizontal = HorizontalShearType.None;
        public VerticalShearType vertical = VerticalShearType.AllSides;
        public float amplitude = 1;
    }

    [UnityEngine.Scripting.Preserve]
    [CreateAssetMenu(menuName = ScriptablePaths.EFFECT_STATES_DIRECT+"Shear", fileName = "Shear Effect")]
    sealed class ShearEffectScriptable : ManagedEffectScriptable<ShearEffectState, ShearData>
    {
        protected override ShearEffectState CreateState(ShearData parameters)
            => new ShearEffectState(parameters.amplitude, parameters.vertical, parameters.horizontal);
    }
}