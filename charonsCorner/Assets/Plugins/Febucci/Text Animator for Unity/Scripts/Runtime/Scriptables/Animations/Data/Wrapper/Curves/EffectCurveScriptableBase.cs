// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity.Effects
{

    // THIS IS EMPTY because it's the only way to serialize
    // abstract / generic types in the inspector from scriptable objects
    [System.Serializable]
    public abstract class EffectCurveScriptableBase : ScriptableObject, IEffectCurve
    {
        public abstract float Evaluate01(float time);
        public abstract float EvaluateRange(float time);
        public virtual void Initialize() { }
        public abstract int BakeResolution { get; }
    }

}