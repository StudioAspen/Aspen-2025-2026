// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System;
using Febucci.TextAnimatorCore;

namespace Febucci.TextAnimatorForUnity.Effects.Core
{

    /// <summary>
    /// Main class to hold effects data and their related curve
    /// </summary>
    [System.Serializable]
    public abstract class ManagedEffectScriptableBase : EffectScriptableBase, IEffectManaged
    {
        public abstract EffectPresetSettings Settings { get; }

        public abstract IEffectContent Appearance { get; }
        public abstract IEffectContent Disappearance { get; }
        public abstract IEffectContent Persistent { get; }

        public event Action OnValueChanged;
        protected void NotifyValueChanged()
        {
            OnValueChanged?.Invoke();
        }
    }

}