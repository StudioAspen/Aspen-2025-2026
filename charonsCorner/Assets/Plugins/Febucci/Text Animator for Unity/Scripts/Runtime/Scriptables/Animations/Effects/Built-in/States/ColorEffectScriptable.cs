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
    class ColorData
    {
        public Febucci.TextAnimatorCore.BuiltIn.ColorMode mode = TextAnimatorCore.BuiltIn.ColorMode.Full;
        public Color color = new Color(1, 0, 0, 1);
    }

    [UnityEngine.Scripting.Preserve]
    [CreateAssetMenu(menuName = ScriptablePaths.EFFECT_STATES_DIRECT+"Color", fileName = "Color Effect")]
    sealed class ColorEffectScriptable : ManagedEffectScriptable<ColorEffectState, ColorData>
    {
        protected override ColorEffectState CreateState(ColorData parameters)
            => new ColorEffectState(parameters.color, parameters.mode);
    }
}