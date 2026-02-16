// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity.Effects.Core
{

    /// <summary>
    /// Main class to that holds effects
    /// </summary>
    [System.Serializable]
    public abstract class EffectScriptableBase : ScriptableObject, IEffect
    {
        public abstract string TagID { get; set; }
        public virtual void Initialize()
        {

        }
    }

}