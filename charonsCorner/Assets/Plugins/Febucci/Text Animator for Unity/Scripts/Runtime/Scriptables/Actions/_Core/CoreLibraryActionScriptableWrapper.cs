// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore.Typing;
using UnityEngine;
namespace Febucci.TextAnimatorForUnity.Actions.Core
{
    abstract class CoreLibraryActionScriptableWrapper<TState> : ActionScriptableBase where TState : IActionState
    {
        [SerializeField] string tagID;
        IActionState state;

        public override string TagID
        {
            get => tagID;
            set => tagID = value;
        }

        public override IActionState CreateActionFrom(ActionMarker marker, object typewriter)
        {
            state = CreateState(marker, typewriter);
            return state;
        }

        protected abstract TState CreateState(ActionMarker marker, object typewriter);

    }
}