// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore.Typing;

namespace Febucci.TextAnimatorForUnity.Actions.Core
{
    [System.Serializable]
    public abstract class ActionScriptableBase : UnityEngine.ScriptableObject, ITypewriterAction
    {
        public abstract IActionState CreateActionFrom(ActionMarker marker, object typewriter);

        public abstract string TagID { get;  set; }
    }
}