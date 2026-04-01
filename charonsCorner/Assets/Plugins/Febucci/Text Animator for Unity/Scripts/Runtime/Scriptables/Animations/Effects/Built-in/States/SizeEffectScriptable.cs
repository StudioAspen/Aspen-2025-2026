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
    class SizeData
    {
        public Vector3 scale = Vector3.one;
    }

    [UnityEngine.Scripting.Preserve]
    [CreateAssetMenu(menuName = ScriptablePaths.EFFECT_STATES_DIRECT+"Scale", fileName = "Scale Effect")]
    sealed class SizeEffectScriptable : ManagedEffectScriptable<SizeEffectState, SizeData>
    {

        protected override SizeEffectState CreateState(SizeData parameters)
            => new SizeEffectState(parameters.scale);
    }
}