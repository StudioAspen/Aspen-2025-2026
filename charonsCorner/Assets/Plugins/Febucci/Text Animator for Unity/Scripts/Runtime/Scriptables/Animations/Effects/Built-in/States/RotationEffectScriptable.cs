// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore.BuiltIn;
using UnityEngine;
using Vector3 = Febucci.Numbers.Vector3;

namespace Febucci.TextAnimatorForUnity.Effects
{
    [System.Serializable]
    class RotationData
    {
        public float loopDegrees = 45;
        public float oscillationDegrees = 45;
        [Tooltip("1 to lock the rotation to the side of a character, e.g. y = 1 seems like a pendulum, -1 makes it from the bottom. Go beyond 1 to have crazier effects, and 0 to disable")] public Vector2 customPivot = new Vector2(0, 0);
    }

    [UnityEngine.Scripting.Preserve]
    [CreateAssetMenu(menuName = ScriptablePaths.EFFECT_STATES_DIRECT+"Continuous Rotation", fileName = "Continuous Rotation Effect")]
    [EffectInfo("rot", EffectCategory.Behaviors)]
    sealed class RotationEffectScriptable : ManagedEffectScriptable<RotationEffectState, RotationData>
    {
        protected override RotationEffectState CreateState(RotationData parameters)
            => new RotationEffectState(parameters.loopDegrees, parameters.oscillationDegrees, new Vector3(parameters.customPivot.x, parameters.customPivot.y, 0));
    }

}