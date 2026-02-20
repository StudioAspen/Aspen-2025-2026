// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System;
using Febucci.TextAnimatorCore.Typing;
using Febucci.TextAnimatorForUnity.Actions.Core;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
#endif

namespace Febucci.TextAnimatorForUnity.Actions
{
    //PSA must be a class or the reference would be lost
    class UnityInputWrapper : IActionState
    {

#if ENABLE_INPUT_SYSTEM
        bool inputSystemPassed;
        IDisposable eventListener;
#endif

        public UnityInputWrapper(bool _)
        {
            #if ENABLE_INPUT_SYSTEM
            inputSystemPassed = false;
            eventListener = null;
            eventListener = InputSystem.onAnyButtonPress.CallOnce(PassInput);
            #endif
        }

#if ENABLE_INPUT_SYSTEM
        void PassInput(InputControl control)
        {
            this.inputSystemPassed = true;
        }
#endif

        public ActionStatus Progress(float deltaTime, ref TextAnimatorCore.Typing.TypingInfo typingInfo)
        {
            #if !ENABLE_LEGACY_INPUT_MANAGER && !ENABLE_INPUT_SYSTEM
            Debug.LogWarning($"No input system is enabled (?) - skipping this {nameof(WaitAnyInputAction)} action.");
            return ActionStatus.Finished;
            #endif

            #if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.anyKeyDown)
                return ActionStatus.Finished;
            #endif

            #if ENABLE_INPUT_SYSTEM
            if (inputSystemPassed)
                return ActionStatus.Finished;
            #endif

            return ActionStatus.Running;
        }

        public void Cancel()
        {
#if ENABLE_INPUT_SYSTEM
            eventListener?.Dispose();
#endif
        }
    }

    [System.Serializable]
    [CreateAssetMenu(menuName = ScriptablePaths.ACTIONS_PATH + "Wait Any Input", fileName = "Wait Any Input Action")]
    [TagInfo("waitinput")]
    public sealed class WaitAnyInputAction : ActionScriptableBase
    {
        [SerializeField] string tagID;

        public override string TagID
        {
            get => tagID;
            set => tagID = value;
        }

        public override IActionState CreateActionFrom(ActionMarker marker, object typewriter)
        {
            // TODO optimize + scriptable callbacks
            return new UnityInputWrapper(true);
        }
    }
}